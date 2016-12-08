'b站挂瓜子的脚本而已

'转载部分的声明
'Author: Beining --<i@cnbeining.com>
'Co-op: SuperFashi
'Purpose: Auto grab silver of Bilibili
'Created: 10/22/2015
'Last modified: 12/8/2015
' https://www.cnbeining.com/
' https://github.com/cnbeining

'source code : python -> vb .net
Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports VBUtil.Utils.NetUtils
Imports VBUtil.Utils
Imports VBUtil
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.InteropServices

''' <summary>
''' 哼，我就要瓜子，你来咬我啊
''' </summary>
''' <remarks></remarks>

Public Module Variables
    <DllImport("AspriseOCR.dll", EntryPoint:="OCR", CallingConvention:=CallingConvention.Cdecl)>
    Public Function OCR(file As String, type As Integer) As IntPtr

    End Function
    <DllImport("AspriseOCR.dll", EntryPoint:="OCRpart", CallingConvention:=CallingConvention.Cdecl)>
    Public Function OCRpart(file As String, type As Integer, startX As Integer, startY As Integer, width As Integer, height As Integer) As IntPtr

    End Function
    <DllImport("AspriseOCR.dll", EntryPoint:="OCRBarCodes", CallingConvention:=CallingConvention.Cdecl)>
    Public Function OCRBarCodes(file As String, type As Integer) As IntPtr

    End Function
    <DllImport("AspriseOCR.dll", EntryPoint:="OCRpartBarCodes", CallingConvention:=CallingConvention.Cdecl)>
    Public Function OCRpartBarCodes(file As String, type As Integer, startX As Integer, startY As Integer, width As Integer, height As Integer) As IntPtr

    End Function
End Module
Public Class guazi
    Public Event DebugOutput(ByVal msg As String)
    Public Event FinishedGrabbing()

    Private _workThd As Thread
    Private _startTime As Integer
    Private _timeStart As UInteger
    Private _timeEnd As UInteger
    Private _RoomId As Integer
    Private _RoomURL As Integer
    Private _RoomInfo As JObject

    'grabbing silver module
    Private Function get_request_option() As NetStream
        Dim ret As New NetStream
        ret.Timeout = 15000
        ret.RetryTimes = 1000

        Return ret
    End Function

    Private _expireTime As Date
    Public Event RefreshClock(ByVal expireTime As Date, ByVal silver As Integer)
    ''' <summary>
    ''' 领取瓜子线程回调函数
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GuaziCallBack()
        Try
            _expireTime = Date.MinValue

            Dim je As JObject = Nothing
            '循环领取瓜子,直到领取结束
            Do
                Dim silver As Integer = 0

                '获取新的任务
                If _expireTime = Date.MinValue Then

                    Do
                        Try
                            je = get_new_task_time_and_award()
                        Catch ex As Exception
                            RaiseEvent DebugOutput(ex.ToString)
                            Trace.TraceError(ex.ToString)
                        End Try
                        Dim code As Integer = je.Value(Of Integer)("code")
                        If code = -10017 Then
                            RaiseEvent DebugOutput("本日瓜子已领完， 欢迎下次再来XD")
                            RaiseEvent FinishedGrabbing()
                            Exit Try
                        End If
                    Loop While je Is Nothing OrElse je.Value(Of Integer)("code") <> 0

                    '计算时间
                    Dim minutes As Integer = je("data").Value(Of Integer)("minute")
                    silver = je("data").Value(Of Integer)("silver")
                    _timeStart = je("data").Value(Of UInteger)("time_start")
                    _timeEnd = je("data").Value(Of UInteger)("time_end")
                    _expireTime = Now.AddMinutes(minutes) '这里用回原来的计时方法是为了避免本地计算机时间的误差造成的领取错误（某些电脑快了或者慢了大半天什么的早醉了）
                    '_expireTime = FromUnixTimeStamp(je("data").Value(Of ULong)("time_end"))
                End If

                RaiseEvent RefreshClock(_expireTime, silver)
                Thread.Sleep(_expireTime - Now)

                '领取瓜子
                Dim getsilver, total_silver As Integer
                Try
                    je = get_award()
                    If je.Value(Of Integer)("code") = 0 Then

                        getsilver = je("data").Value(Of Integer)("awardSilver")
                        total_silver = je("data").Value(Of Integer)("silver")

                        If getsilver > 0 Then
                            RaiseEvent DebugOutput("领取成功!得到" & getsilver & "个银瓜子(总" & total_silver & "个)")
                            _expireTime = Date.MinValue
                        End If
                    Else
                        RaiseEvent DebugOutput("领取错误")
                        _expireTime = Date.MinValue
                    End If
                Catch ex As Exception
                    RaiseEvent DebugOutput(ex.ToString)
                    Trace.TraceError(ex.ToString)
                End Try

            Loop
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] 抛出异常 " & vbCrLf & ex.ToString)
            Trace.TraceError(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 获取房间的信息，用于投票
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub get_room_info()
        If _RoomURL <= 0 Then
            _RoomInfo = New JObject
            Return
        End If
        'room url -> room id
        _RoomId = get_roomid_from_url(_RoomURL)

        Dim req As NetStream = get_request_option()

        Dim info_url As String = "http://live.bilibili.com/live/getInfo"
        Dim param As New Parameters
        param.Add("roomid", _RoomId)
        req.HttpGet(info_url, , param)

        Dim str As String = req.ReadResponseString

        Trace.TraceInformation("Get room #" & _RoomURL & "(" & _RoomId & ") info succeeded, response returned value")
        Trace.TraceInformation("    " & str)

        req.Close()

        _RoomInfo = JsonConvert.DeserializeObject(str)

        _startTime = Int(Utils.Others.ToUnixTimestamp(Now))

    End Sub
    ''' <summary>
    ''' 从url处获取roomid
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    Private Function get_roomid_from_url(ByVal url As Integer) As Integer
        If url <= 0 Then Return 0
        If _listUrlIdReflect Is Nothing Then _listUrlIdReflect = New Dictionary(Of UInteger, UInteger)
        If _listUrlIdReflect.ContainsKey(url) Then Return _listUrlIdReflect(url)

        Dim room_url As String = "http://live.bilibili.com/" & url
        Dim req As New NetStream
        req.Timeout = 30000
        req.RetryTimes = 10
        req.HttpGet(room_url)

        Dim str As String = req.ReadResponseString
        req.Close()

        Dim reg As Match = Regex.Match(str, "var\s+ROOMID\s+=\s+(\d+);")
        If reg.Success = False Then Throw New ArgumentException("Can Not get RoomID")
        Dim roomId As Integer = Integer.Parse(reg.Result("$1"))
        _listUrlIdReflect.Add(url, roomId)
        Return roomId
    End Function
    '加了个缓存roomurl到roomid的字典
    Private _listUrlIdReflect As Dictionary(Of UInteger, UInteger)
    ''' <summary>
    ''' 获取新的任务
    ''' </summary>
    ''' <returns>请求后返回的JSON对象</returns>
    ''' <remarks></remarks>
    Private Function get_new_task_time_and_award() As JObject
        Dim url As String = "http://live.bilibili.com/FreeSilver/getCurrentTask"

        Dim req As NetStream = get_request_option()
        req.HttpGet(url)
        Dim str As String = req.ReadResponseString

        Trace.TraceInformation("Get New tasks succeeded, response returned value:")
        Trace.TraceInformation("    " & str)

        req.Close()

        Dim ret As JObject = JsonConvert.DeserializeObject(str)

        Return ret
    End Function
    ''' <summary>
    ''' 获取宝箱的验证计算图
    ''' </summary>
    ''' <returns></returns>
    Private Function get_captcha() As Image
        Dim url As String = "http://live.bilibili.com/FreeSilver/getCaptcha?ts=" & Int(ToUnixTimestamp(Now))
        Dim req As NetStream = get_request_option()

        req.HttpGet(url)
        Dim mm As New MemoryStream()
        Dim r As Integer = 0
        Dim buf(4095) As Byte
        Do
            r = req.Stream.Read(buf, 0, 4096)
            mm.Write(buf, 0, r) 'transform http stream to local memory stream
        Loop Until r = 0
        mm.Position = 0
        Return Image.FromStream(mm)
    End Function
    ''' <summary>
    ''' 分析并计算宝箱的验证图
    ''' </summary>
    ''' <param name="img">图片</param>
    ''' <returns></returns>
    Private Function analyse_captcha(ByVal img As Image) As Integer
        'Dim ocr As New asprise_ocr_api.AspriseOCR
        'Dim ocr As Patagames.Ocr.OcrApi = Patagames.Ocr.OcrApi.Create
        'OCR.StartEngine("eng", asprise_ocr_api.AspriseOCR.SPEED_SLOW)
        'OCR.Init(Patagames.Ocr.Enums.Languages.English)

        Dim tempFile As String = "temp_" & Int(ToUnixTimestamp(Now)) & ".jpg"
        img.Save(tempFile)
        Dim result As String = Marshal.PtrToStringAnsi(OCR(tempFile, -1)) 'OCR.Recognize(tempFile, -1, -1, -1, -1, -1, asprise_ocr_api.AspriseOCR.RECOGNIZE_TYPE_ALL, asprise_ocr_api.AspriseOCR.OUTPUT_FORMAT_PLAINTEXT)
        'Dim result As String = ocr.GetTextFromImage(img)
        Trace.TraceInformation("OCR Image Result: " & result)
        'ocr.StopEngine()
        If File.Exists(tempFile) Then File.Delete(tempFile)
        'computing
        Dim mat As Match = Regex.Match(result, "^\s*(?<first>\d+)\s*(?<operator>[+-])\s*(?<second>\d+)\s*$")
        If mat.Success Then
            Dim first As Integer = Integer.Parse(mat.Result("${first}"))
            Dim second As Integer = Integer.Parse(mat.Result("${second}"))
            Dim _operator As String = mat.Result("${operator}")
            Select Case _operator
                Case "+"
                    Return first + second
                Case "-"
                    Return first - second
                Case Else
                    Return -1 'failed : operator invalid
            End Select
        Else
            Return -1 'failed : string invalid
        End If

    End Function
    ''' <summary>
    ''' 领取瓜子
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function get_award() As JObject
        Dim url As String = "http://live.bilibili.com/FreeSilver/getAward"
        Dim req As NetStream = get_request_option()
        Dim param As New Parameters
        param.Add("time_start", _timeStart)
        param.Add("time_end", _timeEnd)

        'captcha
        Dim captcha As Image

        Dim answer As Integer = -1
        Do
            Try
                captcha = get_captcha()
                answer = analyse_captcha(captcha)

            Catch ex As Exception

            End Try
        Loop While answer = -1

        param.Add("captcha", answer)

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Trace.TraceInformation("Get award succeeded, response returned value:")
        Trace.TraceInformation("    " & str)

        Dim a As JObject = JsonConvert.DeserializeObject(str)
        req.Close()

        If a.Value(Of Integer)("code") <> 0 Then
            RaiseEvent DebugOutput(a.Value(Of String)("message"))
        End If

        RaiseEvent UserInfoChanged()

        Return a
    End Function

    ''' <summary>
    ''' 领取瓜子前的请求(前戏？)
    ''' </summary>
    ''' <returns>额外的分钟：>=0</returns>
    ''' <remarks></remarks>
    Private Function award_requests() As Integer
        Dim url As String = "http://live.bilibili.com/mobile/freeSilverSurplus"
        Dim req As New NetStream
        req.ReadWriteTimeout = 5000
        req.Timeout = 5000
        Dim param As New Parameters
        'param.Add("appkey", APPKEY)
        param.Add("platform", "ios")
        'param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Trace.TraceInformation("Get award request succeeded, response returned value:")
        Trace.TraceInformation("    " & str)

        Dim a As JObject = JsonConvert.DeserializeObject(str)
        Dim statuscode As HttpStatusCode = req.HTTP_Response.StatusCode
        req.Close()

        Return If(a.Value(Of Integer)("code") = 0, a("data").Value(Of Integer)("surplus"), 0)
    End Function

    ''' <summary>
    ''' 每日签到函数
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function daily_sign() As JObject
        Dim url As String = "http://live.bilibili.com/sign/doSign"
        Dim status_url As String = "http://live.bilibili.com/sign/GetSignInfo"

        Dim http_req As NetStream = get_request_option()
        http_req.HttpGet(url)
        Dim rep As String = http_req.ReadResponseString
        http_req.Close()

        Trace.TraceInformation("Daily sign succeeded, response returned value:")
        Trace.TraceInformation("    " & rep)

        Dim ret As JObject = JsonConvert.DeserializeObject(rep)
        Dim sign_status As Integer = ret.Value(Of Integer)("code")

        If sign_status = 0 Then
            http_req.HttpGet(status_url)
            rep = ReadToEnd(http_req.Stream)
            http_req.Close()
            Dim obj As JObject = JsonConvert.DeserializeObject(rep)
            If obj.Value(Of Integer)("code") = 0 Then
                RaiseEvent DebugOutput("签到成功，本月已签到 " & obj("data").Value(Of Integer)("hadSignDays") & " 天")
            Else
                RaiseEvent DebugOutput("签到成功")
            End If
        ElseIf sign_status = -500 Then
            RaiseEvent DebugOutput("今天已签到")
        End If

        RaiseEvent UserInfoChanged()
        RaiseEvent UserBaggageChanged()
        Return ret
    End Function

    ''' <summary>
    ''' 获得周期性赠送的道具
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function get_send_gift() As JObject
        Dim url As String = "http://live.bilibili.com/giftBag/sendDaily"
        Dim url2 As String = "http://live.bilibili.com/giftBag/getSendGift"
        Dim http_req As NetStream = get_request_option()
        http_req.HttpGet(url)
        Dim rep As String = http_req.ReadResponseString
        http_req.Close()

        http_req.HttpGet(url2)
        Dim rep2 As String = http_req.ReadResponseString
        http_req.Close()

        Trace.TraceInformation("Get send gift succeeded, response returned value:")
        Trace.TraceInformation("    " & rep & vbCrLf & "    " & rep2)

        RaiseEvent UserBaggageChanged()
        Return JsonConvert.DeserializeObject(rep)
    End Function

    ''' <summary>
    ''' 获取用户道具列表，并确定是否自动送出
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SyncGetPlayerBag(Optional ByVal auto_send As Boolean = False) As JObject
        Dim url As String = "http://live.bilibili.com/gift/playerBag"
        Dim http_req As NetStream = get_request_option()
        http_req.HttpGet(url)
        Dim rep As String = ReadToEnd(http_req.Stream)

        Trace.TraceInformation("Get player bag succeeded, response returned value:")
        Trace.TraceInformation("    " & rep)

        http_req.Close()

        Dim ret As JObject = JsonConvert.DeserializeObject(rep)

        If auto_send AndAlso _RoomId > 0 Then
            Dim arr As JArray = ret.Value(Of JArray)("data")
            '获取道具名称
            For Each item As JObject In arr
                Dim gift_id As Integer = item.Value(Of Integer)("gift_id")
                Dim gift_num As Integer = item.Value(Of Integer)("gift_num")
                Dim id As Integer = item.Value(Of Integer)("id")

                SyncSendGift(gift_id, id, gift_num)
            Next

            '重置返回值
            If arr.Count Then
                http_req.HttpGet(url)
                rep = ReadToEnd(http_req.Stream)
                http_req.Close()
                ret = JsonConvert.DeserializeObject(rep)
            End If

            Return ret
        Else
            Return ret
        End If
    End Function

    ''' <summary>
    ''' 赠送道具
    ''' </summary>
    ''' <param name="giftid">道具id</param>
    ''' <param name="bagid">道具在背包内的id</param>
    ''' <param name="giftnum">数量</param>
    ''' <returns></returns>
    Public Function SyncSendGift(ByVal giftid As Integer, ByVal bagid As Integer, ByVal giftnum As Integer) As Boolean
        If _RoomId <= 0 Then Return False
        Try

            Dim http_req As New NetStream
            http_req.ReadWriteTimeout = 5000
            http_req.Timeout = 5000
            'HTTP请求
            Dim req_param As New Parameters
            Dim gift_send_url As String = "http://live.bilibili.com/giftBag/send"
            req_param.Add("giftId", giftid)
            req_param.Add("roomid", _RoomId)
            req_param.Add("ruid", _RoomInfo("data").Value(Of Integer)("MASTERID"))
            req_param.Add("num", giftnum)
            req_param.Add("coinType", "silver")
            req_param.Add("Bag_id", bagid)
            req_param.Add("timestamp", CInt(ToUnixTimestamp(Now)))
            req_param.Add("rnd", CInt(ToUnixTimestamp(Now)))
            req_param.Add("token", DefaultCookieContainer.GetCookies(New Uri(gift_send_url))("LIVE_LOGIN_DATA").Value)

            Dim header_param As New Parameters
            header_param.Add("X-Requested-With", "XMLHttpRequest")
            header_param.Add("Origin", "http://live.bilibili.com")
            header_param.Add("Referer", "http://live.bilibili.com/" & _RoomURL)
            http_req.HttpPost(gift_send_url, req_param,, header_param)

            Dim post_result As String = http_req.ReadResponseString
            http_req.Close()

            Trace.TraceInformation("Sending gifts succeeded, response returned value:")
            Trace.TraceInformation("    " & post_result)

            Dim post_result_ds As JObject = JsonConvert.DeserializeObject(post_result)
            Dim post_result_code As Integer = post_result_ds.Value(Of Integer)("code")
            If post_result_code = 0 Then
                RaiseEvent DebugOutput("送出道具成功(道具编号:" & giftid & ",数量:" & giftnum & ")")
            Else
                RaiseEvent DebugOutput("送出道具失败，返回数据:" & vbCrLf & post_result_ds.ToString)
            End If

            RaiseEvent UserBaggageChanged()
            RaiseEvent UserInfoChanged()

            Return True
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR]" & ex.ToString)
            Trace.TraceError(ex.ToString)
            Return False
        End Try
    End Function
    'public functions
    ''' <summary>
    ''' 构造函数，roomid之前是用来送道具的
    ''' </summary>
    ''' <param name="roomid">房间id(目前支持id和cid)</param>
    ''' <remarks></remarks>
    Public Sub New(Optional ByVal roomid As Integer = 0)
        _RoomURL = roomid
        _RoomInfo = Nothing
        _DownloadManager = New HTTP_Stream_Manager
        _expireTime = Date.MinValue

        Dim suc As Boolean = False
        Do
            Try
                get_room_info()
                suc = True
            Catch ex As Exception
                Throw ex
            End Try
        Loop Until suc
    End Sub

    '开始领取瓜子
    Public Sub AsyncStartGrabbingSilver()
        If _workThd Is Nothing OrElse (_workThd.ThreadState = ThreadState.Stopped Or _workThd.ThreadState = ThreadState.Aborted) Then
            _workThd = New Thread(AddressOf GuaziCallBack)
            _workThd.IsBackground = True
            _workThd.Name = "Bili Live Auto Grabbing Silver Thread"
        End If

        If (_workThd.ThreadState And ThreadState.Unstarted) Then
            _workThd.Start()
        End If
    End Sub
    '停止领取瓜子
    Public Sub AsyncEndGrabbingSilver()
        If _workThd Is Nothing Then Return
        Debug.Print(_workThd.ThreadState.ToString)
        If (_workThd.ThreadState Or ThreadState.Running) Or (_workThd.ThreadState Or ThreadState.WaitSleepJoin) Then
            _workThd.Abort()
        End If
    End Sub
    '获得每日道具
    Public Sub AsyncGetDailyGift()
        Dim thd As New Thread(
            Sub()
                Dim suc = False
                Do
                    Try
                        '领取道具
                        Dim gift_rep As JObject = get_send_gift()
                        If gift_rep.Value(Of Integer)("code") = 0 Then
                            RaiseEvent DebugOutput("领取每日道具成功")
                            RaiseEvent GetDailyGiftFinished()
                            suc = True
                        Else
                            RaiseEvent DebugOutput("领取道具失败，返回数据:" & vbCrLf & gift_rep.ToString)
                        End If
                    Catch ex As Exception
                        RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
                    End Try
                Loop Until suc
            End Sub)

        thd.Name = "Get Daily Gift Thread"
        thd.IsBackground = True
        thd.Start()
    End Sub
    Public Event GetDailyGiftFinished()
    '赠送每日道具
    Public Sub AsyncSendDailyGift()
        If _RoomId <= 0 Then Return
        Dim thd As New Thread(
            Sub()
                Try
                    SyncGetPlayerBag(True)
                    RaiseEvent SendDailyGiftFinished()
                Catch ex As Exception
                    RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
                End Try
            End Sub)

        thd.Name = "Send Daily Gift Thread"
        thd.IsBackground = True
        thd.Start()
    End Sub
    Public Event SendDailyGiftFinished()
    '签到
    Public Sub AsyncDoSign()
        Dim thd As New Thread(
            Sub()
                Try

                    '签到
                    Dim dosign As JObject = daily_sign()
                    Dim sign_state As Integer = dosign.Value(Of Integer)("code")

                    Select Case sign_state
                        Case 0
                            RaiseEvent DoSignSucceeded()
                        Case Else
                            'RaiseEvent DebugOutput("未知错误:[" & sign_state & "]" & dosign.Value(Of String)("msg"))
                    End Select

                Catch ex As Exception
                    RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
                End Try
            End Sub)

        thd.Name = "Daily Sign Thread"
        thd.IsBackground = True
        thd.Start()
    End Sub
    Public Event DoSignSucceeded()
    Public Property RoomURL() As Integer
        Get
            Return _RoomURL
        End Get
        Set(value As Integer)
            If _RoomURL = value Then Return
            RaiseEvent DebugOutput("进入房间:" & value & "成功")
            _RoomURL = value
            Dim suc As Boolean = False
            Do
                Try
                    get_room_info()
                    suc = True
                Catch ex As Exception
                End Try
            Loop Until suc

            AsyncStopDownloadStream()
            Dim recv As Boolean = _isReceivingComment
            AsyncStopReceiveComment()
            ThreadPool.QueueUserWorkItem(
                Sub()
                    If recv Then
                        _CommentThd.Join()
                        AsyncStartReceiveComment()
                    End If
                End Sub)
        End Set
    End Property
    Public ReadOnly Property RoomID() As Integer
        Get
            Return _RoomId
        End Get
    End Property

    '录播
    Private Const DEFAULT_VIDEO_URL As String = "http://live.bilibili.com/api/playurl"
    Private WithEvents _DownloadManager As HTTP_Stream_Manager
    Public Event DownloadStarted()
    Public Event DownloadStopped()
    Private Sub OnDownloadStatusChange(ByVal name As String, ByVal fromstatus As VBUtil.HTTP_Stream_Manager.StreamStatus, ByVal tostatus As VBUtil.HTTP_Stream_Manager.StreamStatus) Handles _DownloadManager.StatusUpdate
        If tostatus = HTTP_Stream_Manager.StreamStatus.STATUS_WORK Then
            RaiseEvent DownloadStarted()
        ElseIf tostatus = HTTP_Stream_Manager.StreamStatus.STATUS_STOP Then
            RaiseEvent DownloadStopped()
        End If
    End Sub
    Public Event DownloadSpeed(ByVal speed As Integer)
    Private Sub OnSpeedChange() Handles _DownloadManager.SpeedUpdate
        RaiseEvent DownloadSpeed(_DownloadManager.GetSpeed())
    End Sub
    Public Sub AsyncStartDownloadStream(ByVal path As String)
        If _RoomId <= 0 Then Return

        Try

            Dim param As New Parameters
            param.Add("cid", _RoomId)
            param.Add("player", 1)
            param.Add("quality", 0)

            Dim xml_document As New Xml.XmlDocument
            Dim req As NetStream = get_request_option()

            req.HttpGet(DEFAULT_VIDEO_URL, , param)
            Dim xml_str As String = ReadToEnd(req.Stream)
            req.Close()
            xml_document.LoadXml(xml_str)

            Dim url As String = xml_document("video")("durl")("url").InnerText

            _DownloadManager.AddDownloadTaskAndStart(path, url, "Video")

        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
            RaiseEvent DownloadStopped()
        End Try
    End Sub

    Public Sub AsyncStopDownloadStream()
        Try
            _DownloadManager.StopTask("Video")
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)

        End Try
    End Sub


    '弹幕
    Private _CommentThd As Thread
    Private _CommentHeartBeat As Thread
    Private Const DEFAULT_COMMENT_HOST As String = "dm.live.bilibili.com"
    Private Const DEFAULT_COMMENT_PORT As Integer = 788
    Private _CommentSocket As Socket
    Private _isReceivingComment As Boolean
    Private _sckrwLck As ReaderWriterLock
    Private Sub CommentHeartBeatCallBack()
        Dim next_update_time As Date = Now.AddSeconds(10)

        Do
            Dim ts As TimeSpan = next_update_time - Now
            If ts.TotalMilliseconds > 0 Then
                Thread.Sleep(ts)
                next_update_time = next_update_time.AddSeconds(10)
            End If

            If _CommentSocket IsNot Nothing AndAlso _CommentSocket.Connected = True Then
                Try
                    If Not _needNextRecv Then
                        _sckrwLck.AcquireWriterLock(Timeout.Infinite)
                        Try
                            SendSocketHeartBeat(_CommentSocket)
                        Catch ex As Exception
                            Throw ex
                        Finally
                            _sckrwLck.ReleaseWriterLock()
                        End Try
                    End If
                Catch ex As Exception
                    Trace.TraceError(ex.ToString)
                    RaiseEvent DebugOutput("[ERR]" & ex.ToString)
                End Try
            End If
        Loop While _CommentSocket IsNot Nothing
    End Sub
    Private Sub CommentThdCallback()
        If _RoomId <= 0 Then Return
        Dim cmt_ns As String = ""
        'get from live
        Try
            Dim req = get_request_option()
            req.HttpGet("http://live.bilibili.com/api/player?id=cid:" & _RoomId & "&ts=" & Microsoft.VisualBasic.Hex(CInt(Others.ToUnixTimestamp(Now))))

            Dim response_str = req.ReadResponseString.Replace(vbCr, "").Replace(vbLf, "")
            req.Close()

            Dim match As Match = Regex.Match(response_str, "<server>(?<ns>.+)</server>")
            If match.Success Then cmt_ns = match.Result("${ns}")
            If (String.IsNullOrEmpty(cmt_ns)) Then Throw New ArgumentNullException("Can not get domain!")
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR]" & ex.ToString)
            Trace.TraceError(ex.ToString)

        End Try

        Dim ipaddr As IPAddress = Dns.GetHostAddresses(cmt_ns)(0)
        Dim ip_ed As IPEndPoint = New IPEndPoint(ipaddr, DEFAULT_COMMENT_PORT)

        _CommentSocket = New Sockets.Socket(Sockets.AddressFamily.InterNetwork, Sockets.SocketType.Stream, Sockets.ProtocolType.Tcp)
        _tempCommentData = New Byte() {}

        Dim buffer(65535) As Byte
        Dim length As Integer = 0
        Try
            _CommentSocket.Connect(ip_ed)

            Trace.TraceInformation("Socket: Sending User data")
            RaiseEvent DebugOutput("开始接收 " & _RoomURL & " 房间的弹幕信息")

            Dim param As New JObject
            param.Add("roomid", _RoomId)
            param.Add("uid", _RoomInfo("data").Value(Of ULong)("UID"))

            SendSocketData(_CommentSocket, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(param)))
            SendSocketHeartBeat(_CommentSocket)
            Do
                length = _CommentSocket.Receive(buffer)
                If length <> 0 Then
                    Try
                        _sckrwLck.AcquireWriterLock(Timeout.Infinite)
                        ParseSocketData(buffer, length)
                        _sckrwLck.ReleaseWriterLock()
                    Catch ex As Exception
                        RaiseEvent DebugOutput("[ERR]" & ex.ToString)
                        Trace.TraceError(ex.ToString)
                    End Try
                End If

            Loop

        Catch ex2 As ThreadAbortException

        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR]" & ex.ToString)
            Trace.TraceError(ex.ToString)
        Finally
            If _CommentSocket.Connected Then _CommentSocket.Disconnect(True)
            If _CommentHeartBeat IsNot Nothing Then
                _CommentHeartBeat.Abort()
                _CommentHeartBeat = Nothing
            End If
            _CommentSocket = Nothing
        End Try
    End Sub
    Private Sub SendSocketData(ByVal sck As Socket, ByVal data() As Byte)
        '套接字 v1.0
        Dim data_length As UInteger = 0
        If data IsNot Nothing Then data_length = data.Length
        Dim total_len As UInteger = 16 + data_length
        Dim head_len As UShort = 16
        Dim version As UShort = 1
        Dim request_type As UInteger = 7
        Dim param5 As UInteger = 1

        Dim buf As New MemoryStream
        WriteUI32(buf, total_len)
        WriteUI16(buf, head_len)
        WriteUI16(buf, version)
        WriteUI32(buf, request_type)
        WriteUI32(buf, param5)
        If data_length > 0 Then buf.Write(data, 0, data_length)
        buf.Position = 0
        Dim post_data(total_len - 1) As Byte
        buf.Read(post_data, 0, total_len)
        If sck IsNot Nothing Then
            sck.Send(post_data)
        End If
        buf.Close()
    End Sub
    Private Sub SendSocketHeartBeat(ByVal sck As Socket)

        Trace.TraceInformation("Socket: Sending Heartbeat data")

        Dim total_len As UInteger = 16
        Dim head_len As UShort = 16
        Dim version As UShort = 1
        Dim request_type As UInteger = 2
        Dim param5 As UInteger = 1

        Dim buf As New MemoryStream
        WriteUI32(buf, total_len)
        WriteUI16(buf, head_len)
        WriteUI16(buf, version)
        WriteUI32(buf, request_type)
        WriteUI32(buf, param5)

        buf.Position = 0
        Dim post_data(15) As Byte
        buf.Read(post_data, 0, 16)
        If sck IsNot Nothing Then
            sck.Send(post_data)
        End If
        buf.Close()

    End Sub
    Public Event ReceivedOnlinePeople(ByVal people As Integer)
    Public Event ReceivedComment(ByVal unixTimestamp As Long, ByVal username As String, ByVal msg As String)
    Public Event ReceivedGiftSent(ByVal unixTimestamp As Long, ByVal giftName As String, ByVal giftId As Integer, ByVal giftNum As Integer, ByVal user As String)
    Public Event ReceivedWelcome(ByVal admin As Boolean, ByVal vip As Boolean, ByVal name As String)
    Public Event ReceivedSystemMsg(ByVal msg As String, ByVal refer_url As String, ByVal roomid As UInteger)
    Public Event StatusPreparing()
    Public Event StatusLive()
    Public Event RoomInfoChanged()
    Public Event RoomBlockMsg(ByVal uid As Integer, ByVal uname As String)
    Public Event BetStarted()
    Public Event BetEnding()
    Public Event BetEnded()
    Public Event BetSealed()
    Public Event SpecialGiftStarted(ByVal roomid As UInteger, ByVal content As String, ByVal id As Integer, ByVal num As Integer, ByVal time As Integer, ByVal joined As Boolean)
    Public Event SpecialGiftEnded(ByVal roomid As UInteger)

    Public Structure BetStatus
        Public question As String
        Public answer() As String
        Public isInBet As Boolean
        Public isBet As Boolean
        Public id As UInteger
        Public uid As UInteger
        Public updateTime As String
        Public Structure AnswerData
            Public id As UInteger
            Public uid As UInteger
            Public betId As UInteger
            Public coin As UInteger
            Public coin_type As String
            Public times As Single
            Public amountCanBuy As UInteger
            Public amountTotal As UInteger
            Public amountCurrent As UInteger
            Public progress As UInteger
            Public referIndex As Integer '指向该竞猜数据的答案下标
        End Structure
        Public data() As AnswerData
    End Structure
    Public Event BetStatusChanged(ByVal status As BetStatus)

    Private _tempCommentData() As Byte
    Private _needNextRecv As Boolean
    Private Sub ParseSocketData(ByVal data() As Byte, ByVal length As Integer)
        '3: online people
        '5: msg
        Dim ms As New MemoryStream
        ms.Write(data, 0, length)
        ms.Position = 0

        Trace.TraceInformation("Socket: Received a data of " & length & " bytes")
        ' display raw data
        'Dim rawData(ms.Length - 1) As Byte
        'ms.Read(rawData, 0, length)
        'Trace.TraceInformation("Data Display: " & Hex(rawData))
        'ms.Position = 0

        While ms.Position < ms.Length

            '读取回上次的数据
            If _needNextRecv Then
                Dim buf(ms.Length - 1) As Byte
                ms.Position = 0
                ms.Read(buf, 0, ms.Length)

                ms.Position = 0
                ms.Write(_tempCommentData, 0, _tempCommentData.Length)
                ms.Write(buf, 0, buf.Length)
                ms.Position = 0
            End If

            Dim total_len As UInteger = ReadUI32(ms)
            Dim head_len As UShort = ReadUI16(ms)
            If head_len = 0 Then
                ms.Close()
                Return
            End If
            Dim version As UShort = ReadUI16(ms)
            Dim type As UInteger = ReadUI32(ms)
            Dim param5 As UInteger = ReadUI32(ms)
            Trace.TraceInformation("Parsing Socket data: [TotalLength: " & total_len & ", HeadLength: " & head_len & ", version: " & version & ", type: " & type & ", additional param: " & param5 & "]")

            Select Case type
                Case 3
                    Dim onlineUser As UInteger = ReadUI32(ms)
                    'Trace.TraceInformation("Parsing uint32:" & onlineUser)
                    RaiseEvent ReceivedOnlinePeople(onlineUser)

                Case 5
                    Dim buf(total_len - head_len - 1) As Byte
                    ms.Read(buf, 0, total_len - head_len)
                    Dim str As String = Encoding.UTF8.GetString(buf)

                    'trace for debug
                    Trace.TraceInformation("Parsing string:" & str)
                    'Trace.TraceInformation("[TRACE] string ends with }: " & str.EndsWith("}").ToString)
                    '判断是否需要分开读取
                    '标识符为最后一字节是否为}
                    If str.EndsWith("}") Then
                        _needNextRecv = False
                        _tempCommentData = New Byte() {}
                    Else
                        _needNextRecv = True
                        '不是的话将本次所有数据存入临时变量中，下次执行该函数再统一解析
                        ms.Position = 0
                        Dim origin_tempcomment_length As Integer = _tempCommentData.Length
                        Array.Resize(_tempCommentData, _tempCommentData.Length + length)
                        ms.Read(_tempCommentData, origin_tempcomment_length, length)
                        ms.Close()
                        Return
                    End If


                    Dim str_obj As JObject = Nothing
                    Try
                        str_obj = JsonConvert.DeserializeObject(str)
                    Catch ex As Exception
                        Trace.TraceWarning("An error occured while deserializing json data:")
                        Trace.TraceInformation("[TRACE] Origin String: " & str & vbCrLf)
                        Trace.TraceInformation("[TRACE] Exception Type: " & ex.ToString)
                        Exit Sub
                    End Try


                    Select Case str_obj.Value(Of String)("cmd")

                        Case "DANMU_MSG"
#Region "Example and remark"
                            '{"info":[[0,1,25,16777215,1469456771,907357409,0,"4d505c40",0],"摸摸阿一",[20940214,"半枫そう",0,0,0,10000],[4,"无常","小十里",22714,6606973],[20,226959,6215679],["ice-dust"]],"cmd":"DANMU_MSG"}

                            '[
                            '   [
                            '       弹幕开始时间(默认为0),
                            '       弹幕类型(默认为1 - 滚动弹幕),
                            '       弹幕大小(px)(默认为25),
                            '       弹幕颜色(rrggbb),
                            '       弹幕发送时间戳(unix),
                            '       (?) ,
                            '       弹幕池类型(默认为0 - 普通弹幕池),
                            '       用户代号,
                            '       (?) - (默认为0)
                            '   ],
                            '   弹幕信息,
                            '   [
                            '       用户id,
                            '       用户名称,
                            '       是否房管 ,
                            '       是否老爷,
                            '       是否月费老爷 ,
                            '       用户权限(默认为10000)
                            '   ], [ （此处可无）
                            '       勋章等级,
                            '       勋章名称,
                            '       勋章博主名称,
                            '       勋章房间id,
                            '       (?)
                            '   ], [
                            '       用户等级,
                            '       用户排名,
                            '       (?)
                            '   ], [ （此处可无）
                            '       用户头衔 -> "sign-one-month" : 月老; "ice-dust" : 钻石星尘
                            '   ]
                            ']
#End Region
                            '暂时先撸这么多参数吧

                            Dim color As UInteger = str_obj("info").Value(Of JArray)(0)(3)
                            Dim post_time As UInteger = str_obj("info").Value(Of JArray)(0)(4)
                            Dim msg As String = str_obj("info").Value(Of String)(1)
                            Dim user_name As String = str_obj("info").Value(Of JArray)(2)(1)
                            Dim user_hashid As String = str_obj("info").Value(Of JArray)(0)(7)
                            '勋章等级、名称以及来源up主
                            Dim medal_level As Integer
                            Dim medal_name As String
                            Dim medal_up_name As String

                            If str_obj("info").Value(Of JArray)(3).Count Then
                                medal_level = str_obj("info").Value(Of JArray)(3)(0)
                                medal_name = str_obj("info").Value(Of JArray)(3)(1)
                                medal_up_name = str_obj("info").Value(Of JArray)(3)(2)
                            End If

                            RaiseEvent ReceivedComment(post_time, user_name, msg)
                        Case "SEND_GIFT"
#Region "Example and remark"
                            '{"cmd":"SEND_GIFT","data":{"giftName":"\u8fa3\u6761","num":6,"uname":"\u841d\u00b7\u5bbe\u6c49","rcost":2793402,"uid":1974757,"top_list":[],"timestamp":1469457764,"giftId":1,"giftType":0,"action":"\u5582\u98df","super":0,"price":100,"rnd":"1469457648","newMedal":0,"medal":1,"capsule":[]},"roomid":22714}
                            ' no remark
#End Region

                            Dim gift_name As String = str_obj("data").Value(Of String)("giftName")
                            Dim gift_num As Integer = str_obj("data").Value(Of Integer)("num")
                            Dim user_name As String = str_obj("data").Value(Of String)("uname")
                            Dim rcost As Integer = str_obj("data").Value(Of Integer)("rcost")
                            Dim uid As Integer = str_obj("data").Value(Of Integer)("uid")
                            Dim top_list As JArray = str_obj("data").Value(Of JArray)("top_list")
                            Dim timestamp As Long = str_obj("data").Value(Of Long)("timestamp")
                            Dim gift_id As Integer = str_obj("data").Value(Of Integer)("giftId")
                            Dim gift_type As Integer = str_obj("data").Value(Of Integer)("giftType")
                            Dim action As String = str_obj("data").Value(Of String)("action")
                            Dim super As Integer = str_obj("data").Value(Of Integer)("super")
                            Dim price As Integer = str_obj("data").Value(Of Integer)("price")
                            'Dim rnd As String = str_obj("data").Value(Of String)("rnd")
                            Dim new_medal As Integer = str_obj("data").Value(Of Integer)("newMedal")
                            'Dim medal As Integer = str_obj("data").Value(Of Integer)("medal")
                            Dim room_id As Integer = str_obj.Value(Of Integer)("roomid")

                            RaiseEvent ReceivedGiftSent(timestamp, gift_name, gift_id, gift_num, user_name)
                        Case "WELCOME"
#Region "Example and remark"
                            '{"cmd":"WELCOME","data":{"uid":6011599,"uname":"\u96c1\u675e\u5357\u98de","isadmin":0,"vip":1},"roomid":22714}
                            ' no remark
#End Region
                            Dim is_admin As Integer = str_obj("data").Value(Of Integer)("isadmin")
                            Dim is_vip As Integer = str_obj("data").Value(Of Integer)("vip")
                            Dim uid As Integer = str_obj("data").Value(Of Integer)("uid")
                            Dim user_name As String = str_obj("data").Value(Of String)("uname")
                            Dim room_id As Integer = str_obj.Value(Of Integer)("roomid")

                            RaiseEvent ReceivedWelcome(is_admin, is_vip, user_name)
                        Case "SYS_MSG"
#Region "Example and remark"
                            '{"cmd":"SYS_MSG","msg":"\u606d\u559c:?\u3010\u94f6\u5723\u7433\u3011:?\u5728\u76f4\u64ad\u95f4:?\u3010240\u3011:?\u62bd\u5230 \u5927\u53f7\u5c0f\u7535\u89c6\u62b1\u6795\u4e00\u4e2a","rep":1,"styleType":2,"url":""}
                            ' no remark
#End Region
                            Dim msg As String = str_obj.Value(Of String)("msg")
                            Dim url As String = str_obj.Value(Of String)("url")
                            Dim roomid As UInteger = str_obj.Value(Of UInteger)("roomid")
                            RaiseEvent ReceivedSystemMsg(msg, url, roomid)
                        Case "SYS_GIFT"

#Region "Example and remark"
                            '{"cmd":"SYS_GIFT","msg":"\u6263\u5b50\u6316:? \u5728\u5e05\u70b8\u4e4c\u51ac\u7684:?\u76f4\u64ad\u95f4138:?\u5185\u8d60\u9001:?36:?\u5171100\u4e2a\uff0c\u89e6\u53d11\u6b21\u5228\u51b0\u96e8\u62bd\u5956\uff0c\u5feb\u53bb\u524d\u5f80\u62bd\u5
                            ' 注意这只是部分，还有一部分在下一次receive中返回
#End Region
                            Dim msg As String = str_obj.Value(Of String)("msg")
                            Dim url As String = str_obj.Value(Of String)("url")
                            Dim roomid As UInteger = str_obj.Value(Of UInteger)("roomid")
                            RaiseEvent ReceivedSystemMsg(msg, url, roomid)

                            '直播状态
                        Case "PREPARING"
                            RaiseEvent StatusPreparing()
                        Case "LIVE"
                            RaiseEvent StatusLive()

                            '竞猜 文化人这种东西不能叫赌
                        Case "BET_START"
                            RaiseEvent BetStarted()
                        Case "BET_BETTOR"
                            Dim eventarg As BetStatus = ParseBetJson(str_obj)
                            RaiseEvent BetStatusChanged(eventarg)
                        Case "BET_BANKER"
                            Dim eventarg As BetStatus = ParseBetJson(str_obj)
                            RaiseEvent BetStatusChanged(eventarg)
                        Case "BET_SEAL"
                            RaiseEvent BetSealed()
                        Case "BET_ENDING"
                            RaiseEvent BetEnding()
                        Case "BET_END"
                            RaiseEvent BetEnded()

                            '更换房间信息
                        Case "CHANGE_ROOM_INFO"
                            get_room_info()
                            RaiseEvent RoomInfoChanged()

                        Case "ROOM_BLOCK_MSG"
                            Dim uid As Integer = str_obj.Value(Of Integer)("uid")
                            Dim uname As String = str_obj.Value(Of String)("uname")
                            RaiseEvent RoomBlockMsg(uid, uname)

                        Case "SEND_TOP"
                            'todo

                            '节奏风暴
                        Case "SPECIAL_GIFT"
                            Dim action As String = str_obj("data")("39").Value(Of String)("action")
                            Dim roomid As UInteger = str_obj.Value(Of UInteger)("roomid")
                            Select Case action
                                Case "start"
                                    Dim id As Integer = Integer.Parse(str_obj("data").Value(Of String)("id"))
                                    Dim num As Integer = str_obj("data")("39").Value(Of Integer)("num")
                                    Dim time As Integer = str_obj("data")("39").Value(Of Integer)("time")
                                    Dim content As String = str_obj("data")("39").Value(Of String)("content")
                                    Dim joined As Boolean = str_obj("data")("39").Value(Of Boolean)("hadJoin")
                                    RaiseEvent SpecialGiftStarted(roomid, content, id, num, time, joined)
                                Case "end"
                                    RaiseEvent SpecialGiftEnded(roomid)
                                Case Else
                                    Throw New ArgumentException("读取节奏风暴参数错误：action")
                            End Select
                        Case Else
                            RaiseEvent DebugOutput("接收到未指定类型的弹幕消息: " & str)
                    End Select
            End Select
        End While

        ms.Close()
    End Sub
    Private Function ParseBetJson(ByVal json As JObject) As BetStatus

        Dim eventarg As New BetStatus

        Dim betdata As JObject = json("data").Value(Of JObject)("data")
        eventarg.question = betdata("bet").Value(Of String)("question")
        ReDim eventarg.answer(1)
        eventarg.answer(0) = betdata("bet").Value(Of String)("a")
        eventarg.answer(1) = betdata("bet").Value(Of String)("b")
        eventarg.isInBet = betdata.Value(Of Boolean)("isInBet")
        eventarg.isBet = betdata.Value(Of Boolean)("isBet")
        eventarg.id = betdata("bet").Value(Of UInteger)("id")
        eventarg.uid = betdata("bet").Value(Of UInteger)("uid")
        eventarg.updateTime = betdata("bet").Value(Of String)("update_time")

        Dim hasGoldBet As Boolean, hasSilverBet As Boolean
        If betdata("gold")("a").Value(Of UInteger)("c") <> 0 Or betdata("gold")("b").Value(Of UInteger)("c") <> 0 Then
            hasGoldBet = True
        End If
        If betdata("silver")("a").Value(Of UInteger)("c") <> 0 Or betdata("silver")("b").Value(Of UInteger)("c") <> 0 Then
            hasSilverBet = True
        End If

        Dim lsData As New List(Of BetStatus.AnswerData)
        If hasGoldBet Then
            Dim a, b As New BetStatus.AnswerData
            Dim gold_betdata As JObject = betdata.Value(Of JObject)("gold")
            a.id = gold_betdata("a").Value(Of UInteger)("id")
            a.uid = gold_betdata("a").Value(Of UInteger)("uid")
            a.coin = gold_betdata("a").Value(Of UInteger)("coin")
            a.coin_type = gold_betdata("a").Value(Of String)("coin_type")
            a.times = gold_betdata("a").Value(Of Single)("times")
            a.amountCanBuy = gold_betdata("a").Value(Of UInteger)("amount")
            a.amountTotal = gold_betdata("a").Value(Of UInteger)("total_amount")
            a.amountCurrent = gold_betdata("a").Value(Of UInteger)("c")
            a.progress = gold_betdata("a").Value(Of UInteger)("progress")
            a.referIndex = 0
            b.id = gold_betdata("b").Value(Of UInteger)("id")
            b.uid = gold_betdata("b").Value(Of UInteger)("uid")
            b.coin = gold_betdata("b").Value(Of UInteger)("coin")
            b.coin_type = gold_betdata("b").Value(Of String)("coin_type")
            b.times = gold_betdata("b").Value(Of Single)("times")
            b.amountCanBuy = gold_betdata("b").Value(Of UInteger)("amount")
            b.amountTotal = gold_betdata("b").Value(Of UInteger)("total_amount")
            b.amountCurrent = gold_betdata("b").Value(Of UInteger)("c")
            b.progress = gold_betdata("b").Value(Of UInteger)("progress")
            b.referIndex = 1
            lsData.Add(a)
            lsData.Add(b)
        End If
        If hasSilverBet Then
            Dim a, b As New BetStatus.AnswerData
            Dim silver_betdata As JObject = betdata.Value(Of JObject)("silver")
            a.id = silver_betdata("a").Value(Of UInteger)("id")
            a.uid = silver_betdata("a").Value(Of UInteger)("uid")
            a.coin = silver_betdata("a").Value(Of UInteger)("coin")
            a.coin_type = silver_betdata("a").Value(Of String)("coin_type")
            a.times = silver_betdata("a").Value(Of Single)("times")
            a.amountCanBuy = silver_betdata("a").Value(Of UInteger)("amount")
            a.amountTotal = silver_betdata("a").Value(Of UInteger)("total_amount")
            a.amountCurrent = silver_betdata("a").Value(Of UInteger)("c")
            a.progress = silver_betdata("a").Value(Of UInteger)("progress")
            a.referIndex = 0
            b.id = silver_betdata("b").Value(Of UInteger)("id")
            b.uid = silver_betdata("b").Value(Of UInteger)("uid")
            b.coin = silver_betdata("b").Value(Of UInteger)("coin")
            b.coin_type = silver_betdata("b").Value(Of String)("coin_type")
            b.times = silver_betdata("b").Value(Of Single)("times")
            b.amountCanBuy = silver_betdata("b").Value(Of UInteger)("amount")
            b.amountTotal = silver_betdata("b").Value(Of UInteger)("total_amount")
            b.amountCurrent = silver_betdata("b").Value(Of UInteger)("c")
            b.progress = silver_betdata("b").Value(Of UInteger)("progress")
            b.referIndex = 1
            lsData.Add(a)
            lsData.Add(b)
        End If

        eventarg.data = lsData.ToArray
        Return eventarg
    End Function
    Public Sub AsyncStartReceiveComment()
        If _RoomId <= 0 Then Return
        If _CommentThd Is Nothing OrElse (_CommentThd.ThreadState = ThreadState.Stopped Or _CommentThd.ThreadState = ThreadState.Aborted) Then

            _CommentThd = New Thread(AddressOf CommentThdCallback)
            _CommentThd.Name = "Bili Live Socket Thread"
            _CommentThd.IsBackground = True
            _CommentHeartBeat = New Thread(AddressOf CommentHeartBeatCallBack)
            _CommentHeartBeat.Name = "Bili Live Socket Heartbeat Thread"
            _CommentHeartBeat.IsBackground = True
        End If

        If (_CommentThd.ThreadState And ThreadState.Unstarted) Then
            _isReceivingComment = True
            _sckrwLck = New ReaderWriterLock()
            _CommentThd.Start()
            _CommentHeartBeat.Start()
        End If
    End Sub
    Public Sub AsyncStopReceiveComment()
        If _CommentThd IsNot Nothing AndAlso （_CommentThd.ThreadState And (ThreadState.Running Or ThreadState.Background)) Then
            _isReceivingComment = False
            _CommentSocket.Close()
            _CommentThd.Abort()
            _CommentHeartBeat.Abort()
        End If
    End Sub

    '挂机刷经验（爽爽爽 XD）
    Private _liveOnThd As Thread
    Private Sub liveOnThdCallback()
        Dim req As NetStream = get_request_option()
        If _RoomId <= 0 Then Return
        Dim url As String = "http://live.bilibili.com/User/userOnlineHeart"
        Do
            Try
                Dim next_time As Date = Now.AddMinutes(5)
                RaiseEvent NextOnlineHeartBeatTime(next_time)

                Dim httpHeader As New Parameters
                httpHeader.Add("Origin", "http://live.bilibili.com/")
                httpHeader.Add("X-Requested-With", "XMLHttpRequest")
                httpHeader.Add("Referer", "http://live.bilibili.com/" & _RoomId)

                req.HttpPost(url, New Byte() {}, "text/plain", httpHeader)

                Dim str As String = req.ReadResponseString

                Trace.TraceInformation("Sending Online Heartbeat succeeded, response returned value:")
                Trace.TraceInformation("    " & str)

                RaiseEvent UserInfoChanged()

                req.Close()
                Dim sleep_time As TimeSpan = next_time - Now
                If sleep_time.TotalMilliseconds > 0 Then Thread.Sleep(sleep_time)
            Catch ex2 As ThreadAbortException
                Exit Do
            Catch ex As Exception
                Trace.TraceError(ex.ToString)
            End Try
        Loop
    End Sub
    Public Event NextOnlineHeartBeatTime(ByVal time As Date)
    Public Sub AsyncBeginLiveOn()
        If _liveOnThd Is Nothing OrElse (_liveOnThd.ThreadState = ThreadState.Stopped Or _liveOnThd.ThreadState = ThreadState.Aborted) Then
            _liveOnThd = New Thread(AddressOf liveOnThdCallback)
            _liveOnThd.Name = "Bili Live Online Thread"
            _liveOnThd.IsBackground = True
        End If

        If (_liveOnThd.ThreadState And ThreadState.Unstarted) Then
            _liveOnThd.Start()
        End If
    End Sub
    Public Sub AsyncStopLiveOn()
        If _liveOnThd Is Nothing Then Return
        _liveOnThd.Abort()
    End Sub

    'b站限时活动 目前是领取红薯的活动，所以在活动结束后不要调用 :-D
    Private _timeLimitEventThd As Thread
    Private Sub EventThdCallback()
        If _RoomId <= 0 Then Return
        Dim req As NetStream = get_request_option()
        Dim url As String = "http://live.bilibili.com/eventRoom/heart"

        Dim continueGet As Boolean
        Try
            Dim preload_url As String = "http://live.bilibili.com/eventRoom/index?ruid=" & _RoomInfo("data").Value(Of Integer)("MASTERID")
            req.HttpGet(preload_url)
            Dim preload_str As String = req.ReadResponseString
            req.Close()
            Dim json As JObject = JsonConvert.DeserializeObject(preload_str)
            continueGet = json("data").Value(Of Boolean)("heart")

            Trace.TraceInformation("Get Summer Special Event succeeded, response returned value:")
            Trace.TraceInformation("    " & preload_str)

            If Not continueGet Then Return
        Catch ex As Exception
            Trace.TraceError(ex.ToString)
            Return
        End Try

        Do
            Try
                Dim next_time As Date = Now.AddMinutes(5)

                RaiseEvent NextEventGrabTime(next_time)

                Dim sleep_time As TimeSpan = next_time - Now
                If sleep_time.TotalMilliseconds > 0 Then Thread.Sleep(sleep_time)

                Dim httpHeader As New Parameters
                httpHeader.Add("X-Requested-With", "XMLHttpRequest")
                httpHeader.Add("Referer", "http://live.bilibili.com/" & _RoomURL)
                httpHeader.Add("Origin", "http://live.bilibili.com")

                Dim postData As New Parameters
                postData.Add("roomid", _RoomId)
                req.HttpPost(url, postData,, httpHeader)
                Dim ret_str As String = req.ReadResponseString

                Trace.TraceInformation("Sending Special Event Heartbeat succeeded, response returned value:")
                Trace.TraceInformation("    " & ret_str)

                req.Close()

                RaiseEvent UserBaggageChanged()

                Dim json As JObject = JsonConvert.DeserializeObject(ret_str)
                continueGet = json("data").Value(Of Boolean)("heart")

                If Not continueGet Then
                    RaiseEvent DebugOutput("本日红薯已领完")
                    Return
                End If
            Catch ex2 As ThreadAbortException
                Exit Do
            Catch ex As Exception
                Trace.TraceError(ex.ToString)
            End Try
        Loop
    End Sub
    Public Sub AsyncBeginTimeLimitedEvent()
        If _timeLimitEventThd Is Nothing OrElse (_timeLimitEventThd.ThreadState = ThreadState.Stopped Or _timeLimitEventThd.ThreadState = ThreadState.Aborted) Then
            _timeLimitEventThd = New Thread(AddressOf EventThdCallback)
            _timeLimitEventThd.Name = "Bili Live Special Event Thread"
            _timeLimitEventThd.IsBackground = True
        End If

        If (_timeLimitEventThd.ThreadState And ThreadState.Unstarted) Then
            _timeLimitEventThd.Start()
        End If
    End Sub
    Public Sub AsyncStopTimeLimitedEvent()
        If _timeLimitEventThd Is Nothing Then Return
        _timeLimitEventThd.Abort()
    End Sub
    Public Event NextEventGrabTime(ByVal time As Date)


    '获取用户信息
    Public Function SyncGetUserInfo() As JObject
        Dim netstr As NetStream = get_request_option()

        netstr.HttpGet("http://live.bilibili.com/User/getUserInfo?timestamp=" & Int(ToUnixTimestamp(Now) * 1000))

        Dim str As String = netstr.ReadResponseString
        netstr.Close()

        Trace.TraceInformation("Get User Info succeeded, response returned value:")
        Trace.TraceInformation("    " & str)

        Return JsonConvert.DeserializeObject(str)
    End Function

    '发送弹幕
    Public Sub SyncSendComment(ByVal msg As String, Optional ByVal color As Color = Nothing, Optional ByVal fontsize As UInteger = 25, Optional ByVal roomid As Integer = -1)
        If roomid = -1 Then roomid = _RoomId
        If color.IsEmpty Then color = Color.White
        If roomid <= 0 Then Return

        Dim req As NetStream = get_request_option()
        Dim req_param As New Parameters
        req_param.Add("color", color.ToArgb And &HFFFFFF)
        req_param.Add("fontsize", fontsize)
        req_param.Add("mode", 1)
        req_param.Add("msg", msg)
        req_param.Add("rnd", rand.Next)
        req_param.Add("roomid", roomid)
        Try
            req.HttpPost("http://live.bilibili.com/msg/send", req_param)
            Dim rep As String = req.ReadResponseString

            Trace.TraceInformation("Send comment succeeded, response returned value:")
            Trace.TraceInformation("    " & rep)
        Catch ex As Exception
            Trace.TraceError(ex.ToString)
        End Try
        req.Close()
    End Sub

    Public Event UserInfoChanged()
    Public Event UserBaggageChanged()

    Private _ActThd As Thread
    Private _is_listening_act As Boolean
    Private Structure _ActData
        Implements IComparable
        Implements IEqualityComparer
        Public roomid As Integer
        Public raffleId As Integer
        Public expireTime As Date
        Public actType As _ActType

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Return expireTime.CompareTo(CType(obj, _ActData).expireTime)
        End Function
        Public Shared Operator =(a As _ActData, b As _ActData)
            Dim valid As Boolean = True
            If a.roomid <> b.roomid Then valid = False
            If a.raffleId <> b.raffleId Then valid = False
            If a.actType <> b.actType Then valid = False
            Return valid
        End Operator
        Public Shared Operator <>(a As _ActData, b As _ActData)
            Return (Not a = b)
        End Operator

        Private Function IEqualityComparer_Equals(x As Object, y As Object) As Boolean Implements IEqualityComparer.Equals
            Return (CType(x, _ActData) = CType(y, _ActData))
        End Function
#Disable Warning BC40005
        Public Function GetHashCode(obj As Object) As Integer Implements IEqualityComparer.GetHashCode
            Return roomid.GetHashCode Xor raffleId.GetHashCode Xor expireTime.GetHashCode Xor actType.GetHashCode
        End Function
#Enable Warning BC40005
    End Structure
    Private Enum _ActType
        TYPE_SmallTV
        TYPE_Ice
    End Enum
    Private _lsActData As List(Of _ActData)
    Private _lsActDataLck As New Object
    Private Sub listActThdCallback()
        Dim ls_count As Integer
        Dim req As NetStream = get_request_option()

        Do

            SyncLock _lsActDataLck
                ls_count = _lsActData.Count
            End SyncLock

            '排序并返回首个元素
            Dim first_element As _ActData
            SyncLock _lsActDataLck
                _lsActData.Sort() '如果list.count=0会报错就加条if
                first_element = If(ls_count, _lsActData.First(), Nothing)
            End SyncLock

            '睡眠等待，可能会被唤醒然后重新计算睡眠时间
            Try
                If ls_count Then
                    Dim ts As TimeSpan = first_element.expireTime - Now
                    If ts.TotalMilliseconds > 0 Then
                        Thread.Sleep(ts)
                    End If
                Else
                    Thread.Sleep(Timeout.Infinite)
                End If
            Catch ex As ThreadInterruptedException
                Continue Do
            Catch ex As Exception
                Trace.TraceError(ex.ToString)
            End Try

            '到时间领取
            Select Case first_element.actType
                Case _ActType.TYPE_Ice
                    req.HttpGet("http://live.bilibili.com/summer/notice?roomid=" & first_element.roomid & "&raffleId=" & first_element.raffleId)

                    Dim str As String = req.ReadResponseString

                    Trace.TraceInformation("Getting Ice Activity award:")
                    Trace.TraceInformation("    " & str)

                    req.Close()
                    Dim json As JObject = JsonConvert.DeserializeObject(str)
                    If json.Value(Of Integer)("code") = 0 Then
                        RaiseEvent DebugOutput("刨冰抽奖结果:" & json.Value(Of String)("msg"))
                        RaiseEvent UserBaggageChanged()

                        '成功后才移除
                        SyncLock _lsActDataLck
                            _lsActData.Remove(first_element)
                        End SyncLock
                    Else
                        '否则延后10s再次领取
                        SyncLock _lsActDataLck
                            _lsActData.Remove(first_element)
                            first_element.expireTime = first_element.expireTime.AddSeconds(10)
                            _lsActData.Add(first_element)
                        End SyncLock
                    End If

                Case _ActType.TYPE_SmallTV
                    'todo: 抓包兼容

                    req.HttpGet("http://live.bilibili.com/SmallTV/getReward?id=" & first_element.raffleId)

                    Dim str As String = req.ReadResponseString
                    req.Close()

                    Trace.TraceInformation("Getting Small TV Activity award:")
                    Trace.TraceInformation("    " & str)

                    Dim json As JObject = JsonConvert.DeserializeObject(str)
                    If json.Value(Of Integer)("code") = 0 Then
                        RaiseEvent DebugOutput("小电视抽奖结果: 获得" & json("data")("reward").Value(Of Integer)("num") & "个道具")
                        RaiseEvent UserBaggageChanged()

                        '成功后才移除
                        SyncLock _lsActDataLck
                            _lsActData.Remove(first_element)
                        End SyncLock
                    Else
                        '否则延后10s再次领取
                        SyncLock _lsActDataLck
                            _lsActData.Remove(first_element)
                            first_element.expireTime = first_element.expireTime.AddSeconds(10)
                            _lsActData.Add(first_element)
                        End SyncLock
                    End If
            End Select
        Loop
    End Sub
    Private Sub onSysMsgRecv(ByVal msg As String, ByVal url As String, ByVal roomURL As UInteger) Handles Me.ReceivedSystemMsg
        If Not _is_listening_act Then Return
        If InStr(msg, "抽奖") <= 0 Then Return
        If _RoomURL <= 0 Then Return

        '另外启动线程参加抽奖活动
        ThreadPool.QueueUserWorkItem(
            Sub()
                Try
                    Dim roomid As Integer = get_roomid_from_url(roomURL)
                    If roomid <= 0 Then Return

                    Dim req As NetStream = get_request_option()

                    Dim headerParam As New Parameters
                    headerParam.Add("Referer", "http://live.bilibili.com/" & _RoomURL)
                    headerParam.Add("X-Requested-With", "XMLHttpRequest")

                    '手动延迟
                    Thread.Sleep(200)

                    If InStr(msg, "小电视") Then
                        req.HttpGet("http://live.bilibili.com/SmallTV/index?roomid=" & roomid & "_=" & Int(ToUnixTimestamp(Now) * 1000))
                        Dim str As String = req.ReadResponseString

                        Trace.TraceInformation("Checking SmallTV Activities succeeded:")
                        Trace.TraceInformation("    " & str)
                        req.Close()
                        Dim json As JObject = JsonConvert.DeserializeObject(str)
                        Dim act_data As JArray = json("data").Value(Of JArray)("unjoin")

                        For Each element As JObject In act_data
                            Dim data As New _ActData
                            data.actType = _ActType.TYPE_SmallTV
                            data.expireTime = Now.AddSeconds(element.Value(Of Integer)("dtime")).AddSeconds(80)
                            data.raffleId = element.Value(Of Integer)("id")
                            data.roomid = roomid

                            req.HttpGet("http://live.bilibili.com/SmallTV/join?roomid=" & roomid & "&_=" & Int(ToUnixTimestamp(Now) * 1000) & "&id=" & data.raffleId)

                            RaiseEvent DebugOutput("已参加 " & roomURL & " 房间的小电视抽奖活动")

                            Trace.TraceInformation("Joining Small TV Act succeeded:")
                            Trace.TraceInformation("    " & req.ReadResponseString)

                            SyncLock _lsActDataLck
                                _lsActData.Add(data)
                            End SyncLock
                        Next

                    ElseIf InStr(msg, "刨冰雨") Then
                        req.HttpGet("http://live.bilibili.com/summer/check?roomid=" & roomid, headerParam)
                        Dim str As String = req.ReadResponseString

                        Trace.TraceInformation("Checking Ice Activities succeeded:")
                        Trace.TraceInformation("    " & str)

                        req.Close()
                        Dim json As JObject = JsonConvert.DeserializeObject(str)
                        Dim act_data As JArray = json.Value(Of JArray)("data")

                        For Each element As JObject In act_data
                            Dim data As New _ActData
                            data.actType = _ActType.TYPE_Ice
                            data.expireTime = Now.AddSeconds(element.Value(Of Integer)("time")).AddSeconds(20)
                            data.raffleId = element.Value(Of Integer)("raffleId")
                            data.roomid = roomid

                            Dim has_data As Boolean = element.Value(Of Boolean)("status")

                            If Not has_data Then
                                req.HttpGet("http://live.bilibili.com/summer/join?roomid=" & data.roomid & "&raffleId=" & data.raffleId, headerParam)

                                RaiseEvent DebugOutput("已参加 " & roomURL & " 房间的刨冰抽奖活动")

                                Trace.TraceInformation("Joining Ice Activity succeeded:")
                                Trace.TraceInformation("    " & req.ReadResponseString)

                                req.Close()

                                SyncLock _lsActDataLck
                                    _lsActData.Add(data)
                                End SyncLock
                            End If
                        Next
                    Else
                        Return
                    End If

                Catch ex As Exception
                    Trace.TraceError(ex.ToString)
                Finally
                    _ActThd.Interrupt()
                End Try
            End Sub)
    End Sub
    Private Sub onSpecialGiftRecv(ByVal roomid As UInteger, ByVal content As String, ByVal id As Integer, ByVal num As Integer, ByVal time As Integer, ByVal joined As Boolean) Handles Me.SpecialGiftStarted
        If Not _is_listening_act Then Return
        ThreadPool.QueueUserWorkItem(
            Sub()
                Try
                    Trace.TraceInformation("Joining Special Gift...")
                    SyncSendComment(content,, roomid)
                    RaiseEvent DebugOutput("已参加 " & roomid & " 房间的节奏风暴")

                    RaiseEvent UserBaggageChanged()
                Catch ex As Exception
                    RaiseEvent DebugOutput("[ERR]" & ex.ToString)
                    Trace.TraceError(ex.ToString)
                End Try
            End Sub)
    End Sub
    '参加抽奖活动
    Public Sub AsyncListenAct()
        If Not _isReceivingComment Then
            'RaiseEvent DebugOutput("[ERR]需要先开启弹幕监听才能使用本功能")
            AsyncStartReceiveComment()
        End If

        _lsActData = New List(Of _ActData)
        _is_listening_act = True

        If _ActThd Is Nothing OrElse (_ActThd.ThreadState = ThreadState.Stopped Or _ActThd.ThreadState = ThreadState.Aborted) Then
            _ActThd = New Thread(AddressOf listActThdCallback)
            _ActThd.IsBackground = True
            _ActThd.Name = "Bili Activity Joining Thread"
        End If

        If (_ActThd.ThreadState And ThreadState.Unstarted) Then
            _ActThd.Start()
        End If
    End Sub
    Public Sub AsyncStopListeningAct()
        _is_listening_act = False
        If _ActThd Is Nothing Then Return
        _ActThd.Abort()
    End Sub
End Class
''' <summary>
''' b站登录函数[附带RSA加密模块]
''' </summary>
''' <remarks></remarks>
Public Module Bilibili_Login
    Public Const LOGIN_URL As String = "https://passport.bilibili.com/login/dologin"
    Public Const LOGOUT_URL As String = "https://passport.bilibili.com/login"
    Public Const BACKUP_LOGIN_URL As String = "https://passport.bilibili.com/ajax/miniLogin/login"
    Public Const LOGIN_PUBLIC_KEY As String = "https://passport.bilibili.com/login?act=getkey"
    Public Const BILIBILI_MAIN_PAGE As String = "http://www.bilibili.com"
    Public Const CAPTCHA_URL As String = "https://passport.bilibili.com/captcha"
    Public Const API_MY_INFO As String = "http://api.bilibili.com/myinfo"
    '2015/12/31  RSA 加密登录成功
    '2016/03/25  将域名 account.bilibili.com 换为 passport.bilibili.com ，登录成功

    ''' <summary>
    ''' 使用主站登录模块
    ''' </summary>
    ''' <param name="userid">用户ID</param>
    ''' <param name="pwd">密码</param>
    ''' <param name="captcha">验证码</param>
    ''' <returns>登录是否成功</returns>
    ''' <remarks></remarks>
    Public Function Login(ByVal userid As String, ByVal pwd As String, ByVal captcha As String, Optional ByRef login_result As String = Nothing) As Boolean
        Dim param As New Parameters
        param.Add("act", "login")
        param.Add("userid", userid)
        param.Add("keeptime", 604800)

        'Form1.DebugOutput("用户名: " & userid)

        'RSA加密
        Dim req As New NetStream
        req.Timeout = 15000
        req.RetryTimes = 20

        'Form1.DebugOutput("获取RSA公钥URL: " & LOGIN_PUBLIC_KEY)
        req.HttpGet(LOGIN_PUBLIC_KEY)
        Dim loginRequest As String = ReadToEnd(req.Stream)
        Dim loginRequest2 As JObject = JsonConvert.DeserializeObject(loginRequest)

        Dim rsaPublicKey As String = loginRequest2.Value(Of String)("key")
        Dim hash As String = loginRequest2.Value(Of String)("hash")
        req.Close()

        'Form1.DebugOutput("RSA公钥: " & rsaPublicKey)

        Dim rsa1 As New System.Security.Cryptography.RSACryptoServiceProvider
        rsa1.ImportParameters(RSA.ConvertFromPemPublicKey(rsaPublicKey))

        'Form1.DebugOutput("本地RSA XML数据: " & rsa1.ToXmlString(False))

        '哔站所谓的加密也不过如此嘛……第一次用了pwd=pwd+hash报错，第二次pwd=hash+pwd结果wwwww
        pwd = hash & pwd
        Dim tempPwd() As Byte = System.Text.Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(pwd)

        Dim password() As Byte = rsa1.Encrypt(tempPwd, False)

        pwd = Convert.ToBase64String(password)

        param.Add("pwd", pwd)

        'Form1.DebugOutput("加密后的密码: " & pwd)

        param.Add("vdcode", captcha)


        'Form1.DebugOutput("发送登录信息...")

        req.HttpPost(LOGIN_URL, param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        If login_result IsNot Nothing Then
            login_result = str
        End If
        'Form1.DebugOutput("返回数据: " & str)

        Return CheckLogin()
    End Function
    ''' <summary>
    ''' 获取验证码
    ''' </summary>
    ''' <returns>返回验证码图像</returns>
    ''' <remarks></remarks>
    Public Function GetCaptchaImage() As Image
        Dim req As New NetStream
        req.Timeout = 15000
        req.RetryTimes = 20
        req.HttpGet(CAPTCHA_URL)
        Dim img As Image = Image.FromStream(req.Stream)
        req.Close()
        Return img
    End Function
    ''' <summary>
    ''' 查看登录是否成功
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CheckLogin() As Boolean
        Dim req As New NetStream
        req.Timeout = 15000
        req.RetryTimes = 20
        req.HttpGet(API_MY_INFO)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()
        str = str.Replace(vbCr, "").Replace(vbLf, "")
        'Form1.DebugOutput("API请求: [登录信息]: " & API_MY_INFO & vbCrLf & JsonConvert.DeserializeObject(str).ToString)
        Return If(InStr(str, "-101"), False, True)
    End Function
    ''' <summary>
    ''' 退出登录
    ''' </summary>
    ''' <returns>返回当前是否登录</returns>
    ''' <remarks></remarks>
    Public Function LogOut() As Boolean
        Dim url As String = LOGOUT_URL
        Dim param As New Parameters
        param.Add("act", "exit")
        Dim req As New NetStream
        req.Timeout = 15000
        req.RetryTimes = 20

        'Form1.DebugOutput("发送注销信息...")

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        'Form1.DebugOutput("返回数据:" & str)

        Return CheckLogin()
    End Function
End Module
