Imports System.IO
Imports System.Net
Imports System.Threading
Imports VBUtil.Utils.NetUtils

Public Class HTTP_Stream_Manager
    Public Const DEFAULT_MAX_DOWNLOAD_THREAD As Integer = 5 '默认最大下载线程数
    Public Const DEFAULT_MAX_UPLOAD_THREAD As Integer = 2 '默认最大上传线程数
    Public Enum StreamStatus
        STATUS_WORK
        STATUS_PAUSE
        STATUS_STOP
        STATUS_IDLE
    End Enum
    Private Class StreamWorker
        Private _Thread As Thread '处理线程
        Private _Status As StreamStatus '数据流状态
        Private _LocalAddress As String '本地文件地址
        Private _HTTPAddress As String 'URL地址

        Private _NetStream As NetStream '网络数据流

        Private _pauseFlag As Boolean '线程暂停标识
        Private _stopFlag As Boolean '线程停止标识

        Private _Length As Long '文件长度
        Private _Position As Long '文件位置

        Private _parent As HTTP_Stream_Manager '父级管理类
        Public Const DEFAULT_BUFFER_SIZE As Integer = 4096 '默认文件缓存大小
        Public Const DEFAULT_TIMED_OUT_RETRY_TIMES As Integer = 3 '默认http请求超时的重试次数
        Private _name As String '名称，供父级回调
        Private _local_fs As FileStream '本地的文件数据流

        Private _is_download_thread As Boolean '是下载线程还是上传线程
        Private _upload_response_data As Byte() '上传文件后返回的数据
#Region "ReadOnlyProperty"
        Public ReadOnly Property Status As StreamStatus
            Get
                Return _Status
            End Get
        End Property
        Public ReadOnly Property LocalAddress As String
            Get
                Return _LocalAddress
            End Get
        End Property
        Public ReadOnly Property HTTPAddress As String
            Get
                Return _HTTPAddress
            End Get
        End Property
        Public ReadOnly Property ContentLength As Long
            Get
                Return _Length
            End Get
        End Property
        Public ReadOnly Property ContentPosition As Long
            Get
                Return _Position
            End Get
        End Property
        Public ReadOnly Property TaskName As String
            Get
                Return _name
            End Get
        End Property
        Public ReadOnly Property IsDownloadThread As Boolean
            Get
                Return _is_download_thread
            End Get
        End Property
#End Region
        '下载线程回调函数，支持部分断点
        Private Sub _thread_download_callback()
            _NetStream = New NetStream
            '本地文件输出流
            If _Status = StreamStatus.STATUS_PAUSE Then
                _local_fs = New FileStream(_LocalAddress, FileMode.Append, FileAccess.Write)
                '文件长度与当前不匹配或者不支持断点续传(Length = -1)
                If _local_fs.Length <> _Position OrElse _Length = -1 Then
                    _local_fs.Close()
                    _local_fs = New FileStream(_LocalAddress, FileMode.Create, FileAccess.Write)
                End If
            Else

                _local_fs = New FileStream(_LocalAddress, FileMode.Create, FileAccess.Write)
            End If

            '改变数据流状态
            RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_WORK)
            _Status = StreamStatus.STATUS_WORK

            'reading stream data
            Try
                _parent._sc.BeginCalculate(_name)

                '读取模块改成超时自动重试的 :D
                Dim timed_out_retry As Integer = 0
                Do

                    Try
                        _read_stream()
                        Exit Do
                    Catch ex As WebException
                        If ex.Message = "操作超时" Then
                            timed_out_retry += 1
                        Else
                            Throw ex
                        End If
                    Catch ex As Exception
                        Throw ex
                    End Try

                Loop While timed_out_retry <= DEFAULT_TIMED_OUT_RETRY_TIMES

            Catch ex As Exception
                Debug.Print(ex.ToString)
                RaiseEvent ErrorUpdate(_name, ex)
            Finally
                '无论有没有错误，都要释放以下资源
                _NetStream.Close()
                _local_fs.Close()
                _parent._sc.EndCalculate(_name)

                'Length = -1 : 因无法断点续传，所以改成stop
                If _Position = _Length Or _Length = -1 Then
                    RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_STOP)
                    _Status = StreamStatus.STATUS_STOP
                ElseIf _pauseFlag Then
                    '一般支持断点续传的
                    RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_PAUSE)
                    _Status = StreamStatus.STATUS_PAUSE
                End If
            End Try
        End Sub
        '上传线程回调函数，不支持断点
        Private Sub _thread_upload_callback()
            _NetStream = New NetStream
            '本地文件数据流
            Try
                _local_fs = New FileStream(_LocalAddress, FileMode.Open, FileAccess.Read)
                _Position = 0
                _Length = _local_fs.Length

            Catch ex As Exception
                Throw
            End Try


            '改变数据流状态
            RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_WORK)
            _Status = StreamStatus.STATUS_WORK

            '开始上传数据
            Try
                '开始计算速度
                _parent._sc.BeginCalculate(_name)

                Dim dst_stream As Stream = _NetStream.HttpPost(_HTTPAddress, _Length)

                Dim buffer(DEFAULT_BUFFER_SIZE - 1) As Byte
                Dim nRead As Integer = 0
                '上传数据
                Do
                    nRead = _local_fs.Read(buffer, 0, DEFAULT_BUFFER_SIZE)
                    dst_stream.Write(buffer, 0, nRead)

                    _Position += nRead
                    _parent._sc.UpdatePosition(_name, _Position)
                Loop Until (nRead = 0 Or _pauseFlag Or _stopFlag)

            Catch ex As Exception
                Debug.Print(ex.ToString)
                RaiseEvent ErrorUpdate(_name, ex)
            Finally
                _local_fs.Close()

                Try
                    _NetStream.HttpPostClose()

                    '获取返回的数据(失败下为null)
                    If _NetStream.Stream IsNot Nothing Then
                        Dim mm As New MemoryStream
                        Dim buffer(DEFAULT_BUFFER_SIZE - 1) As Byte
                        Dim nRead As Integer = 0
                        Do
                            nRead = _NetStream.Stream.Read(buffer, 0, DEFAULT_BUFFER_SIZE)
                            mm.Write(buffer, 0, nRead)
                        Loop Until (nRead = 0 Or _pauseFlag Or _stopFlag)
                        mm.Position = 0
                        ReDim _upload_response_data(mm.Length - 1)
                        mm.Write(_upload_response_data, 0, mm.Length)
                        mm.Close()
                    End If

                Catch ex As Exception
                End Try
                _NetStream.Close()
                _parent._sc.EndCalculate(_name)


                RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_STOP)
                _Status = StreamStatus.STATUS_STOP
            End Try

            RaiseEvent UploadComplete(_name, _upload_response_data)
        End Sub
        Private Sub _read_stream()
            Dim buf(DEFAULT_BUFFER_SIZE - 1) As Byte

            '如果是Length = -1的话，默认重新开始读取
            _NetStream.HttpGet(_HTTPAddress, , , If(_Length = -1, -1, _Position))

            _Length = _NetStream.HTTP_Response.ContentLength
            _Position = 0
            Dim nRead As Integer = 0
            Dim stream As Stream = _NetStream.Stream
            Do
                nRead = stream.Read(buf, 0, DEFAULT_BUFFER_SIZE)
                _local_fs.Write(buf, 0, nRead)

                _Position += nRead
                _parent._sc.UpdatePosition(_name, _Position)
            Loop Until (nRead = 0 Or _pauseFlag Or _stopFlag)
        End Sub
        Public Sub New(ByVal parent As HTTP_Stream_Manager, ByVal name As String, ByVal LocalAddr As String, ByVal HTTPAddr As String, ByVal download As Boolean)
            If download Then
                _Thread = New Thread(AddressOf _thread_download_callback)
            Else
                _Thread = New Thread(AddressOf _thread_upload_callback)
            End If
            _Thread.Name = "HTTP Worker Thread"
            _Status = StreamStatus.STATUS_PAUSE
            _LocalAddress = LocalAddr
            _HTTPAddress = HTTPAddr
            _parent = parent
            _name = name
            _is_download_thread = download
            If String.IsNullOrEmpty(LocalAddr) Then
                Throw New ArgumentNullException("本地文件地址不能为空")
            End If
            If String.IsNullOrEmpty(HTTPAddr) Then
                Throw New ArgumentNullException("网页地址不能为空")
            End If

            SetIdle()
        End Sub
        Public Sub Start()
            Select Case _Status
                Case StreamStatus.STATUS_WORK
                    Return
                Case StreamStatus.STATUS_PAUSE

                Case StreamStatus.STATUS_STOP
                    Return
                Case StreamStatus.STATUS_IDLE

            End Select

            If _Thread.ThreadState = ThreadState.Stopped Or _Thread.ThreadState = ThreadState.Aborted Then
                If _is_download_thread Then
                    _Thread = New Thread(AddressOf _thread_download_callback)
                Else
                    _Thread = New Thread(AddressOf _thread_upload_callback)
                End If
                _Thread.Name = "HTTP Worker Thread"
            End If
            _pauseFlag = False
            _Thread.Start()
        End Sub
        Public Sub SetIdle()
            RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_IDLE)
            _Status = StreamStatus.STATUS_IDLE
        End Sub
        Public Sub Pause()
            Select Case _Status
                Case StreamStatus.STATUS_WORK
                    _pauseFlag = True
                    _Thread.Join(100)
                Case StreamStatus.STATUS_IDLE
                    RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_PAUSE)
                    _Status = StreamStatus.STATUS_PAUSE

                Case StreamStatus.STATUS_PAUSE

                Case StreamStatus.STATUS_STOP
            End Select
        End Sub
        Public Sub [Stop]()
            Select Case _Status
                Case StreamStatus.STATUS_WORK
                    _stopFlag = True
                    'If _Thread.Join(100) = False Then _Thread.Abort()
                    _Thread.Join(100)
                Case StreamStatus.STATUS_PAUSE
                    RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_STOP)
                    _Status = StreamStatus.STATUS_STOP

                Case StreamStatus.STATUS_STOP
                    Return
                Case StreamStatus.STATUS_IDLE
                    RaiseEvent StatusUpdate(_name, _Status, StreamStatus.STATUS_STOP)
                    _Status = StreamStatus.STATUS_STOP

            End Select
        End Sub

        Public Event StatusUpdate(ByVal name As String, ByVal fromStatus As StreamStatus, ByVal toStatus As StreamStatus)
        Public Event ErrorUpdate(ByVal name As String, ByVal ex As Exception)
        Public Event UploadComplete(ByVal name As String, ByVal return_data As Byte())
    End Class

    Private _sc As Speed_Calculator

    Private _stream_list As Dictionary(Of String, StreamWorker)
    Public Sub New()
        _stream_list = New Dictionary(Of String, StreamWorker)
        _sc = New Speed_Calculator()
        AddHandler _sc.SpeedUpdated, AddressOf on_speed_update
    End Sub
    Private _syncLock As New Object
    Private _current_download_thread As Integer
    Private _current_upload_thread As Integer
    ''' <summary>
    ''' 添加下载任务并自动执行
    ''' </summary>
    ''' <param name="LocalAddr">本地保存文件的路径</param>
    ''' <param name="HTTPAddr">HTTP URL地址</param>
    ''' <param name="TaskName">任务名称(唯一标识)</param>
    ''' <returns>是否添加成功</returns>
    ''' <remarks></remarks>
    Public Function AddDownloadTaskAndStart(ByVal LocalAddr As String, ByVal HTTPAddr As String, ByVal TaskName As String) As Boolean
        Try
            '存在重名任务 -> 添加失败
            Dim exists As Boolean
            SyncLock _syncLock
                exists = _stream_list.ContainsKey(TaskName)
            End SyncLock
            If exists Then Return False

            '添加到列表中
            Dim temp As New StreamWorker(Me, TaskName, LocalAddr, HTTPAddr, True)
            AddHandler temp.StatusUpdate, AddressOf on_status_update
            AddHandler temp.UploadComplete, AddressOf on_upload_complete

            SyncLock _syncLock
                _stream_list.Add(TaskName, temp)
            End SyncLock
            '开始任务
            StartTask(TaskName)

            Return True
        Catch ex As Exception
            Debug.Print(ex.ToString)
            Return False
        End Try
    End Function
    Public Function AddUploadTaskAndStart(ByVal LocalAddr As String, ByVal HTTPAddr As String, ByVal TaskName As String) As Boolean
        Try
            '存在重名任务 -> 添加失败
            Dim exists As Boolean
            SyncLock _syncLock
                exists = _stream_list.ContainsKey(TaskName)
            End SyncLock
            If exists Then Return False

            '添加到列表中
            Dim temp As New StreamWorker(Me, TaskName, LocalAddr, HTTPAddr, False)
            AddHandler temp.StatusUpdate, AddressOf on_status_update
            AddHandler temp.UploadComplete, AddressOf on_upload_complete

            SyncLock _syncLock
                _stream_list.Add(TaskName, temp)
            End SyncLock
            '开始任务
            StartTask(TaskName)

            Return True
        Catch ex As Exception
            Debug.Print(ex.ToString)
            Return False
        End Try
    End Function
    ''' <summary>
    ''' 开始任务
    ''' </summary>
    ''' <param name="TaskName">任务名称</param>
    ''' <remarks></remarks>
    Public Sub StartTask(ByVal TaskName As String)
        SyncLock _syncLock
            If _stream_list.ContainsKey(TaskName) Then
                If _stream_list(TaskName).IsDownloadThread Then

                    If _current_download_thread < DEFAULT_MAX_DOWNLOAD_THREAD Then
                        _current_download_thread += 1
                        _stream_list(TaskName).Start()
                    Else
                        _stream_list(TaskName).SetIdle()
                    End If
                Else

                    If _current_upload_thread < DEFAULT_MAX_UPLOAD_THREAD Then
                        _current_upload_thread += 1
                        _stream_list(TaskName).Start()
                    Else
                        _stream_list(TaskName).SetIdle()
                    End If
                End If

            End If
        End SyncLock
    End Sub
    ''' <summary>
    ''' 暂停任务
    ''' </summary>
    ''' <param name="TaskName">任务名称</param>
    ''' <remarks></remarks>
    Public Sub PauseTask(ByVal TaskName As String)
        SyncLock _syncLock
            If _stream_list.ContainsKey(TaskName) Then
                _stream_list(TaskName).Pause()
            End If
        End SyncLock
    End Sub
    ''' <summary>
    ''' 停止任务
    ''' </summary>
    ''' <param name="TaskName">任务名称</param>
    ''' <remarks></remarks>
    Public Sub StopTask(ByVal TaskName As String)
        SyncLock _syncLock
            If _stream_list.ContainsKey(TaskName) Then
                _stream_list(TaskName).Stop()
                _stream_list.Remove(TaskName)
            End If
        End SyncLock
    End Sub
    Private Sub on_status_update(ByVal name As String, ByVal fromStatus As StreamStatus, ByVal toStatus As StreamStatus)
        If fromStatus = toStatus Then Return
        'Debug.Print("Status: " & fromStatus.ToString & " -> " & toStatus.ToString)
        If toStatus = StreamStatus.STATUS_STOP Then
            SyncLock _syncLock
                If fromStatus = StreamStatus.STATUS_WORK Then
                    If _stream_list(name).IsDownloadThread Then

                        _current_download_thread -= 1
                        _stream_list.Remove(name)
                        'auto start new tasks
                        For Each e In _stream_list
                            If _current_download_thread >= DEFAULT_MAX_DOWNLOAD_THREAD Then
                                Exit For
                            End If
                            If e.Value.Status = StreamStatus.STATUS_IDLE Then
                                _current_download_thread += 1
                                e.Value.Start()
                            End If
                        Next

                    Else
                        _current_upload_thread -= 1
                        _stream_list.Remove(name)
                        'auto start new tasks
                        For Each e In _stream_list
                            If _current_upload_thread >= DEFAULT_MAX_UPLOAD_THREAD Then
                                Exit For
                            End If
                            If e.Value.Status = StreamStatus.STATUS_IDLE Then
                                _current_upload_thread += 1
                                e.Value.Start()
                            End If
                        Next
                    End If
                End If
            End SyncLock
        End If
        RaiseEvent StatusUpdate(name, fromStatus, toStatus)
    End Sub
    Public Event StatusUpdate(ByVal name As String, ByVal fromStatus As StreamStatus, ByVal toStatus As StreamStatus)
    Private Sub on_speed_update()
        RaiseEvent SpeedUpdate()
    End Sub
    Public Event SpeedUpdate()
    Private Sub on_upload_complete(ByVal name As String, ByVal data() As Byte)
        RaiseEvent UploadCompleteData(name, data)
    End Sub
    Private Sub on_error_occured(ByVal name As String, ByVal ex As Exception)
        RaiseEvent ErrorOccured(name, ex)
    End Sub
    Public Event UploadCompleteData(ByVal name As String, ByVal data() As Byte)
    Public Event ErrorOccured(ByVal name As String, ByVal ex As Exception)
    ''' <summary>
    ''' 获取任务的下载速度
    ''' </summary>
    ''' <param name="name">任务名称</param>
    ''' <returns>任务的速度(BPS)</returns>
    ''' <remarks></remarks>
    Public Function GetSpeed(ByVal name As String) As Long
        Return _sc.GetSpeed(name)
    End Function
    ''' <summary>
    ''' 获取任务的总下载速度
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetSpeed() As Long
        'Return _sc.GetSpeed
        Dim ret As Long
        Dim ls As List(Of KeyValuePair(Of String, Long)) = _sc.GetSpeed()
        For Each e In ls
            ret += e.Value
        Next
        Return ret
    End Function
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    'calculate the download speed, using the other threads
    Private Class Speed_Calculator
        Public Const DEFAULT_INTERVAL As Integer = 1000
        Public Sub New(Optional ByVal interval As Integer = DEFAULT_INTERVAL)
            mCalculateList = New SortedList(Of String, Long)
            mOutputSpeedList = New List(Of Long)
            mLastCalculateList = New SortedList(Of String, Long)
            mThread = New Thread(AddressOf Calculator_callback)
            mInterval = interval
        End Sub
        Private Sub Calculator_callback()
            While mCalculateList.Count
                SyncLock mLocker
                    Dim i As Integer = 0
                    For Each x In mCalculateList
                        mOutputSpeedList(i) = x.Value - mLastCalculateList(x.Key)

                        i += 1
                    Next

                    mLastCalculateList = New SortedList(Of String, Long)(mCalculateList)
                End SyncLock
                RaiseEvent SpeedUpdated()
                Thread.Sleep(mInterval)
            End While
        End Sub
        Private mCalculateList As SortedList(Of String, Long)
        Private mLastCalculateList As SortedList(Of String, Long)
        Private mOutputSpeedList As List(Of Long)
        Private mLocker As New Object
        Private mInterval As Integer
        Private mThread As Thread

        Public Sub BeginCalculate(ByVal name As String)
            SyncLock mLocker
                mCalculateList.Add(name, 0)
                mLastCalculateList.Add(name, 0)
                mOutputSpeedList.Insert(mCalculateList.IndexOfKey(name), 0)
            End SyncLock

            If mThread.ThreadState = ThreadState.Stopped Or mThread.ThreadState = ThreadState.Aborted Then
                mThread = New Thread(AddressOf Calculator_callback)
                mThread.Name = "Speed Calculating Thread"
            End If

            If mThread.ThreadState = ThreadState.Unstarted Then
                mThread.Start()
            End If
        End Sub

        Public Sub EndCalculate(ByVal name As String)
            SyncLock mLocker
                Dim index As Integer = mCalculateList.IndexOfKey(name)
                If index >= 0 Then
                    mOutputSpeedList.RemoveAt(index)
                    mLastCalculateList.Remove(name)
                    mCalculateList.Remove(name)

                End If
            End SyncLock
        End Sub

        Public Sub UpdatePosition(ByVal name As String, ByVal pos As Long)
            SyncLock mLocker
                If mCalculateList.ContainsKey(name) Then
                    mCalculateList(name) = pos
                End If
            End SyncLock
        End Sub

        Public Function GetSpeed(ByVal name As String) As Long
            Dim ret As Long
            SyncLock mLocker
                Dim index As Integer = mCalculateList.IndexOfKey(name)
                If index = -1 Then ret = 0 Else ret = mOutputSpeedList(index)
            End SyncLock

            Return ret
        End Function
        Public Function GetSpeed() As List(Of KeyValuePair(Of String, Long))
            Dim ret As New List(Of KeyValuePair(Of String, Long))
            Dim i As Integer = 0
            SyncLock mLocker
                For Each x In mCalculateList
                    ret.Add(New KeyValuePair(Of String, Long)(x.Key, mOutputSpeedList(i)))
                Next
                Return ret

            End SyncLock
        End Function
        Public Event SpeedUpdated()
    End Class
End Class
