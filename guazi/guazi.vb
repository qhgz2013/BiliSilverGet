'b站挂瓜子的脚本而已

'Author: Beining --<i@cnbeining.com>
'Co-op: SuperFashi
'Purpose: Auto grab silver of Bilibili
'Created: 10/22/2015
'Last modified: 12/8/2015
' https://www.cnbeining.com/
' https://github.com/cnbeining

'source code : python ->  vb .net
'translator: pandasxd (4/3/2016)

Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports VBUtil.Utils.NetUtils
Imports VBUtil.Utils.StreamUtils
Imports VBUtil
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Net
Imports System.Net.Sockets

''' <summary>
''' 哼，我就要瓜子，你来咬我啊
''' </summary>
''' <remarks></remarks>

Public Module Variables
End Module
Public Class guazi
    Public Event DebugOutput(ByVal msg As String)
    Public Event FinishedGrabbing()

    Private _workThd As Thread
    Private _startTime As Integer
    Private _RoomId As Integer
    Private _RoomInfo As JObject

    Private Const APPKEY As String = "85eb6835b0a1034e" '"c1b107428d337928" 
    Private Const SECRETKEY As String = "2ad42749773c441109bdc0191257a664" '"49c0356b37c67b5524c3646b694e8a57" 
    Private Const VER As String = "0.98.86"

    Private Const DEBUG_RETURN_INFO As Boolean = False

    'grabbing silver module
    Private Function calc_sign(ByVal str As String) As String
        Dim md5 As New System.Security.Cryptography.MD5CryptoServiceProvider

        Return Utils.Others.Hex(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str))).ToLower
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

            Dim je As JObject
            '循环领取瓜子
            Do
                Dim silver As Integer = 0
                '+ : 把领奖时间设为成员变量，避免重新领取任务
                If _expireTime = Date.MinValue Then
                    je = get_new_task_time_and_award()

                    Dim code As Integer = je.Value(Of Integer)("code")
                    If code = -10017 Then
                        RaiseEvent DebugOutput("本日瓜子已领完，欢迎下次再来XD")
                        RaiseEvent FinishedGrabbing()
                        Exit Do
                    ElseIf code <> 0 Then
                        For i As Integer = 1 To 11
                            Thread.Sleep(1000)
                            RaiseEvent DebugOutput("发送领取请求错误，重试第" & i & "次")
                            je = get_new_task_time_and_award()
                            code = je.Value(Of Integer)("code")

                        Next
                    End If
                    Dim minutes As Integer = je("data").Value(Of Integer)("minute")
                    silver = je("data").Value(Of Integer)("silver")

                    _expireTime = Now.AddMinutes(minutes)

                    RaiseEvent RefreshClock(_expireTime, silver)
                End If


                While (_expireTime - Now).TotalSeconds > 0
                    send_heartbeat()
                    Dim sleep_time As Integer = (_expireTime - Now).Seconds * 1000 + (_expireTime - Now).Milliseconds
                    If sleep_time < 0 Then sleep_time = 0
                    Thread.Sleep(sleep_time)

                End While

                Dim times As Integer = award_requests()
                If times = -1 Then
                    Continue Do
                End If
                If times > 0 Then
                    _expireTime = Now.AddMinutes(times)
                    RaiseEvent RefreshClock(_expireTime, silver)
                    Thread.Sleep(times * 1000 * 60)
                End If

                Thread.Sleep(500) '等待0.5s以防bug掉

                Dim getsilver, total_silver As Integer
                je = get_award()
                If je.Value(Of Integer)("code") = 0 Then

                    getsilver = je("data").Value(Of Integer)("awardSilver")
                    total_silver = je("data").Value(Of Integer)("silver")

                    If getsilver > 0 Then
                        RaiseEvent DebugOutput("领取成功！得到" & getsilver & "个银瓜子(总" & total_silver & "个)")
                        _expireTime = Date.MinValue
                    End If
                Else
                    RaiseEvent DebugOutput("领取错误")
                    _expireTime = Date.MinValue
                End If

            Loop
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] 抛出异常: " & vbCrLf & ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' 获取房间的信息，用于投票
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub get_room_info()
        If _RoomId <= 0 Then
            _RoomInfo = New JObject
            Return
        End If
        Dim url As String = "http://live.bilibili.com/live/getInfo"
        Dim param As New Parameters
        param.Add("roomid", _RoomId)
        Dim req As New NetStream
        req.HttpGet(url, , param)

        Dim str As String = ReadToEnd(req.Stream)

        Debug.Print("Get room #" & _RoomId & " info succeeded, response returned value:")
        Debug.Print(str)

        req.Close()

        _RoomInfo = JsonConvert.DeserializeObject(str)

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("In get_room_info : " & _RoomInfo.ToString)
        End If

        _startTime = Int(Utils.Others.ToUnixTimestamp(Now))

    End Sub

    ''' <summary>
    ''' 获取新的任务
    ''' </summary>
    ''' <returns>请求后返回的JSON对象</returns>
    ''' <remarks></remarks>
    Private Function get_new_task_time_and_award() As JObject
        Dim url As String = "http://live.bilibili.com/mobile/freeSilverCurrentTask"


        Dim param As New Parameters
        param.Add("appkey", APPKEY)
        param.Add("platform", "ios")

        'sign calc
        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))

        Dim req As New NetStream
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Debug.Print("Get new tasks succeeded, response returned value:")
        Debug.Print(str)

        req.Close()

        Dim ret As JObject = JsonConvert.DeserializeObject(str)

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In get_new_task_time_and_award() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & ret.ToString)

        End If

        Return ret
    End Function

    ''' <summary>
    ''' 发送心跳包
    ''' </summary>
    ''' <returns>心跳包返回的状态码</returns>
    ''' <remarks></remarks>
    Private Function send_heartbeat() As Integer
        Dim url As String = "http://live.bilibili.com/mobile/freeSilverHeart"
        Dim req As New NetStream
        Dim param As New Parameters

        param.Add("appkey", APPKEY)
        param.Add("platform", "ios")
        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Debug.Print("Send Heartbeat succeeded, response returned value:")
        Debug.Print(str)

        Dim a As JObject = JsonConvert.DeserializeObject(str)
        Dim statuscode As HttpStatusCode = req.HTTP_Response.StatusCode
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In send_heartbeat() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)

        End If

        If statuscode <> Net.HttpStatusCode.OK Then
            RaiseEvent DebugOutput("错误：心跳发送失败")
            Return -1
        Else
            Return a.Value(Of Integer)("code")
        End If
    End Function

    ''' <summary>
    ''' 领取瓜子
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function get_award() As JObject
        Dim url As String = "http://live.bilibili.com/mobile/freeSilverAward"
        Dim req As New NetStream
        Dim param As New Parameters
        param.Add("appkey", APPKEY)
        param.Add("platform", "ios")

        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Debug.Print("Get award succeeded, response returned value:")
        Debug.Print(str)


        Dim a As JObject = JsonConvert.DeserializeObject(str)
        Dim statuscode As HttpStatusCode = req.HTTP_Response.StatusCode
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In get_award(captcha) : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)

        End If

        If statuscode <> Net.HttpStatusCode.OK Or a.Value(Of Integer)("code") <> 0 Then
            RaiseEvent DebugOutput(a.Value(Of String)("message"))
        End If
        Return a
    End Function

    ''' <summary>
    ''' 领取瓜子前的请求(前戏？)
    ''' </summary>
    ''' <returns>错误：-1，额外的分钟：>=0</returns>
    ''' <remarks></remarks>
    Private Function award_requests() As Integer
        Dim url As String = "http://live.bilibili.com/mobile/freeSilverSurplus"
        Dim req As New NetStream
        Dim param As New Parameters
        param.Add("appkey", APPKEY)
        param.Add("platform", "ios")
        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)

        Debug.Print("Get award request succeeded, response returned value:")
        Debug.Print(str)

        Dim a As JObject = JsonConvert.DeserializeObject(str)
        Dim statuscode As HttpStatusCode = req.HTTP_Response.StatusCode
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In award_requests() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)
        End If

        If statuscode <> Net.HttpStatusCode.OK Then
            Return -1
        Else
            Return a("data").Value(Of Integer)("surplus")
        End If
    End Function

    ''' <summary>
    ''' 每日签到函数
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function daily_sign() As JObject
        Dim url As String = "http://live.bilibili.com/sign/doSign"
        Dim status_url As String = "http://live.bilibili.com/sign/GetSignInfo"

        Dim http_req As New NetStream
        http_req.HttpGet(status_url)
        Dim rep As String = ReadToEnd(http_req.Stream)
        http_req.Close()

        Debug.Print("Daily sign succeeded, response returned value:")
        Debug.Print(rep)

        Dim ret As JObject = JsonConvert.DeserializeObject(rep)
        Dim sign_status As Integer = ret("data").Value(Of Integer)("status")

        If sign_status = 0 Then
            http_req.HttpGet(url)
            rep = ReadToEnd(http_req.Stream)
            http_req.Close()
            Return JsonConvert.DeserializeObject(rep)
        End If

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
        Dim http_req As New NetStream
        http_req.HttpGet(url)
        http_req.HttpGet(url2)
        Dim rep As String = ReadToEnd(http_req.Stream)
        http_req.Close()

        Debug.Print("Get send gift succeeded, response returned value:")
        Debug.Print(rep)

        Return JsonConvert.DeserializeObject(rep)
    End Function

    ''' <summary>
    ''' 获取用户道具列表，并确定是否自动送出
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function get_player_bag(Optional ByVal auto_send As Boolean = False) As JObject
        Dim url As String = "http://live.bilibili.com/gift/playerBag"
        Dim http_req As New NetStream
        http_req.HttpGet(url)
        Dim rep As String = ReadToEnd(http_req.Stream)

        Debug.Print("Get player bag succeeded, response returned value:")
        Debug.Print(rep)

        http_req.Close()

        Dim ret As JObject = JsonConvert.DeserializeObject(rep)

        If auto_send AndAlso _RoomId > 0 Then
            Dim arr As JArray = ret.Value(Of JArray)("data")
            '获取道具名称
            For Each item As JObject In arr
                Dim gift_id As Integer = item.Value(Of Integer)("gift_id")
                Dim gift_num As Integer = item.Value(Of Integer)("gift_num")
                Dim id As Integer = item.Value(Of Integer)("id")

                'HTTP请求
                Dim req_param As New Parameters
                Dim gift_send_url As String = "http://live.bilibili.com/giftBag/send"
                req_param.Add("giftId", gift_id)
                req_param.Add("roomid", _RoomId)
                req_param.Add("ruid", _RoomInfo("data").Value(Of Integer)("MASTERID"))
                req_param.Add("num", gift_num)
                req_param.Add("coinType", "silver")
                req_param.Add("Bag_id", id)
                req_param.Add("timestamp", CInt(VBUtil.Utils.Others.ToUnixTimestamp(Now)))
                req_param.Add("rnd", VBUtil.Utils.Others.rand.Next())
                req_param.Add("token", DefaultCookieContainer.GetCookies(New Uri(gift_send_url))("LIVE_LOGIN_DATA").Value)

                http_req.HttpPost(gift_send_url, req_param)

                Dim post_result As String = ReadToEnd(http_req.Stream)
                http_req.Close()
                Dim post_result_ds As JObject = JsonConvert.DeserializeObject(post_result)
                Dim post_result_code As Integer = post_result_ds.Value(Of Integer)("code")
                If post_result_code = 0 Then
                    RaiseEvent DebugOutput("自动送出道具成功(道具编号:" & gift_id & ",数量:" & gift_num & ")")
                Else
                    RaiseEvent DebugOutput("送出道具失败，返回数据:" & vbCrLf & post_result_ds.ToString)
                End If

                '不知道为什么送完后会加上一条这样的get
                Dim callback_url As String = "http://live.bilibili.com/giftBag/sendDaily"
                http_req.HttpGet(callback_url)
                Dim callback_str As String = ReadToEnd(http_req.Stream)
                http_req.Close()
                Dim callback_ds As JObject = JsonConvert.DeserializeObject(callback_str)
                Dim callback_code As Integer = callback_ds.Value(Of Integer)("code")
                If callback_code <> 0 Then
                    RaiseEvent DebugOutput("送出道具回调检测失败，返回数据:" & vbCrLf & post_result_ds.ToString)
                End If
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


    'public functions
    ''' <summary>
    ''' 构造函数，roomid之前是用来送投票券和道具的
    ''' </summary>
    ''' <param name="roomid"></param>
    ''' <remarks></remarks>
    Public Sub New(Optional ByVal roomid As Integer = 0)
        _workThd = New Thread(AddressOf GuaziCallBack)
        _workThd.Name = "Bili Live Auto Grabbing Silver Thread"

        _CommentThd = New Thread(AddressOf CommentThdCallback)
        _CommentThd.Name = "Bili Live Socket Thread"
        _CommentHeartBeat = New Thread(AddressOf CommentHeartBeatCallBack)
        _CommentHeartBeat.Name = "Bili Live Socket Heartbeat Thread"

        _RoomId = roomid
        _RoomInfo = Nothing
        _DownloadManager = New HTTP_Stream_Manager
        _expireTime = Date.MinValue
        Try
            get_room_info()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    '开始领取瓜子
    Public Sub AsyncStartGrabbingSilver()
        If _workThd.ThreadState = ThreadState.Stopped Or _workThd.ThreadState = ThreadState.Aborted Then
            _workThd = New Thread(AddressOf GuaziCallBack)
            _workThd.Name = "Bili Live Auto Grabbing Silver Thread"
        End If

        If _workThd.ThreadState = ThreadState.Unstarted Then
            _workThd.Start()
        End If
    End Sub
    '停止领取瓜子
    Public Sub AsyncEndGrabbingSilver()
        If _workThd.ThreadState = ThreadState.Running Then
            _workThd.Abort()
        End If
    End Sub
    '获得每日道具
    Public Sub AsyncGetDailyGift()
        Dim thd As New Thread( _
            Sub()
                Try
                    '领取道具
                    Dim gift_rep As JObject = get_send_gift()
                    If gift_rep.Value(Of Integer)("code") = 0 Then
                        RaiseEvent DebugOutput("领取每日道具成功，获得" & gift_rep.Value(Of JArray)("data").Count & "个道具")
                    Else
                        RaiseEvent DebugOutput("领取道具失败，返回数据:" & vbCrLf & gift_rep.ToString)
                    End If
                Catch ex As Exception
                    RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
                End Try
            End Sub)

        thd.Name = "Get Daily Gift Thread"
        thd.Start()
    End Sub
    '赠送每日道具
    Public Sub AsyncSendDailyGift()
        If _RoomId <= 0 Then Return
        Dim thd As New Thread( _
            Sub()
                Try
                    get_player_bag(True)

                Catch ex As Exception
                    RaiseEvent DebugOutput("[ERR] 抛出异常: " & ex.ToString)
                End Try
            End Sub)

        thd.Name = "Send Daily Gift Thread"
        thd.Start()
    End Sub
    '签到
    Public Sub AsyncDoSign()
        Dim thd As New Thread( _
            Sub()
                Try

                    '签到
                    Dim dosign As JObject = daily_sign()
                    Dim sign_state As Integer = dosign.Value(Of Integer)("code")

                    Select Case sign_state
                        Case 0
                            RaiseEvent DebugOutput("已完成签到")
                        Case Else
                            RaiseEvent DebugOutput("未知错误:[" & sign_state & "]" & dosign.Value(Of String)("msg"))
                    End Select

                Catch ex As Exception
                    RaiseEvent DebugOutput("ERR] 抛出异常: " & ex.ToString)
                End Try
            End Sub)

        thd.Name = "Daily Sign Thread"
        thd.Start()
    End Sub

    Public Property RoomID() As Integer
        Get
            Return _RoomId
        End Get
        Set(value As Integer)
            If _RoomId = value Then Return
            RaiseEvent DebugOutput("进入房间:" & value & "成功")
            _RoomId = value
            get_room_info()
            AsyncStopDownloadStream()
            Dim recv As Boolean = _isReceivingComment
            AsyncStopReceiveComment()
            Threading.ThreadPool.QueueUserWorkItem(
                Sub()
                    If recv Then
                        _CommentThd.Join()
                        AsyncStartReceiveComment()
                    End If
                End Sub)
        End Set
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
            Dim req As New NetStream
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
    Private Const DEFAULT_COMMENT_HOST As String = "livecmt-1.bilibili.com"
    Private Const DEFAULT_COMMENT_PORT As Integer = 788
    Private _CommentSocket As Socket
    Private _isReceivingComment As Boolean
    Private Sub CommentHeartBeatCallBack()
        Dim next_update_time As Date = Now.AddSeconds(10)

        Do
            Dim ts As TimeSpan = next_update_time - Now
            If ts.TotalMilliseconds > 0 Then
                Thread.Sleep(ts)
                next_update_time = next_update_time.AddSeconds(10)
            End If

            If _CommentSocket IsNot Nothing AndAlso _CommentSocket.Connected = True Then
                SendSocketHeartBeat()
            End If
        Loop
    End Sub
    Private Sub CommentThdCallback()
        If _RoomId <= 0 Then Return
        Dim ipaddr As IPAddress = Dns.GetHostAddresses(DEFAULT_COMMENT_HOST)(0)
        Dim ip_ed As IPEndPoint = New IPEndPoint(ipaddr, DEFAULT_COMMENT_PORT)

        _CommentSocket = New Sockets.Socket(Sockets.AddressFamily.InterNetwork, Sockets.SocketType.Stream, Sockets.ProtocolType.Tcp)
        Dim buffer(8191) As Byte
        Dim length As Integer = 0
        Try
            _CommentSocket.Connect(ip_ed)


            Debug.Print("Comment Socket: Sending User data")
            RaiseEvent DebugOutput("开始接收 " & _RoomId & " 房间的弹幕信息")

            Dim param As New JObject
            param.Add("roomid", _RoomId)
            param.Add("uid", 100000000000000 + CLng((200000000000000 * Utils.rand.NextDouble())))


            SendSocketData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(param)))

            Debug.Print("Comment Socket: Sending User data succeeded")

            SendSocketHeartBeat()
            Do
                length = _CommentSocket.Receive(buffer)
                Debug.Print("Comment Socket: Received a socket data: length = " & length & "Byte")
                If length <> 0 Then ParseSocketData(buffer, length)

            Loop

        Catch ex2 As ThreadAbortException

        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR]" & ex.ToString)
        Finally
            _CommentSocket.Disconnect(True)
            _CommentSocket = Nothing
        End Try
    End Sub
    Private Sub SendSocketData(ByVal data() As Byte)
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
        If _CommentSocket IsNot Nothing Then
            _CommentSocket.Send(post_data)
        End If
        buf.Close()
    End Sub
    Private Sub SendSocketHeartBeat()

        Debug.Print("Comment Socket: Sending Heartbeat data")

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
        If _CommentSocket IsNot Nothing Then
            _CommentSocket.Send(post_data)
        End If
        buf.Close()

        Debug.Print("Comment Socket: Sending Heartbeat data succeeded")
    End Sub
    Public Event ReceivedOnlinePeople(ByVal people As Integer)
    Public Event ReceivedComment(ByVal unixTimestamp As Long, ByVal username As String, ByVal msg As String)
    Public Event ReceivedGiftSent(ByVal unixTimestamp As Long, ByVal giftName As String, ByVal giftId As Integer, ByVal giftNum As Integer, ByVal user As String)
    Public Event ReceivedWelcome(ByVal admin As Boolean, ByVal vip As Boolean, ByVal name As String)
    Public Event ReceivedSystemMsg(ByVal msg As String, ByVal refer_url As String)
    Private Sub ParseSocketData(ByVal data() As Byte, ByVal length As Integer)

        '3: online people
        '5: msg
        Dim ms As New MemoryStream
        ms.Write(data, 0, length)
        ms.Position = 0

        While ms.Position < ms.Length
            Dim total_len As UInteger = ReadUI32(ms)
            Dim head_len As UShort = ReadUI16(ms)
            If head_len = 0 Then
                ms.Close()
                Return
            End If
            Dim version As UShort = ReadUI16(ms)
            Dim type As UInteger = ReadUI32(ms)
            Dim param5 As UInteger = ReadUI32(ms)

            Select Case type
                Case 3
                    RaiseEvent ReceivedOnlinePeople(ReadUI32(ms))

                Case 5
                    Dim buf(total_len - head_len - 1) As Byte
                    ms.Read(buf, 0, total_len - head_len)
                    Dim str As String = Encoding.UTF8.GetString(buf)

                    Debug.Print("Parsing socket data:" & str)
                    Dim str_obj As JObject = JsonConvert.DeserializeObject(str)

                    Select Case str_obj.Value(Of String)("cmd")

                        Case "DANMU_MSG"
                            '暂时先撸这么多参数吧

                            '弹幕颜色(RRGGBB)
                            Dim color As UInteger = str_obj("info").Value(Of JArray)(0)(3)
                            '弹幕发送UNIX时间戳
                            Dim post_time As UInteger = str_obj("info").Value(Of JArray)(0)(4)
                            '弹幕内容
                            Dim msg As String = str_obj("info").Value(Of String)(1)
                            '用户名称
                            Dim user_name As String = str_obj("info").Value(Of JArray)(2)(1)
                            '用户hash id
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
                            Dim gift_name As String = str_obj("data").Value(Of String)("giftName")
                            Dim gift_num As Integer = str_obj("data").Value(Of Integer)("num")
                            Dim user_name As String = str_obj("data").Value(Of String)("uname")
                            'Dim rcost As Integer = str_obj("data").Value(Of Integer)("rcost")
                            'Dim uid As Integer = str_obj("data").Value(Of Integer)("uid")
                            'Dim top_list As JArray = str_obj("data").Value(Of JArray)("top_list")
                            Dim timestamp As Long = str_obj("data").Value(Of Long)("timestamp")
                            Dim gift_id As Integer = str_obj("data").Value(Of Integer)("giftId")
                            Dim gift_type As Integer = str_obj("data").Value(Of Integer)("giftType")
                            Dim action As String = str_obj("data").Value(Of String)("action")
                            Dim super As Integer = str_obj("data").Value(Of Integer)("super")
                            Dim price As Integer = str_obj("data").Value(Of Integer)("price")
                            Dim rnd As String = str_obj("data").Value(Of String)("rnd")
                            Dim new_medal As Integer = str_obj("data").Value(Of Integer)("newMedal")
                            'Dim guardian_score As Integer = str_obj("data").Value(Of Integer)("guardianScore")
                            Dim room_id As Integer = str_obj.Value(Of Integer)("roomid")

                            RaiseEvent ReceivedGiftSent(timestamp, gift_name, gift_id, gift_num, user_name)
                        Case "WELCOME"
                            Dim is_admin As Integer = str_obj("data").Value(Of Integer)("isadmin")
                            Dim is_vip As Integer = str_obj("data").Value(Of Integer)("vip")
                            Dim uid As Integer = str_obj("data").Value(Of Integer)("uid")
                            Dim user_name As String = str_obj("data").Value(Of String)("uname")
                            Dim room_id As Integer = str_obj.Value(Of Integer)("roomid")

                            RaiseEvent ReceivedWelcome(is_admin, is_vip, user_name)
                        Case "SYS_MSG"
                            Dim msg As String = str_obj.Value(Of String)("msg")
                            Dim url As String = str_obj.Value(Of String)("url")
                            RaiseEvent ReceivedSystemMsg(msg, url)
                        Case Else

                    End Select
            End Select
        End While

        ms.Close()
    End Sub
    Public Sub AsyncStartReceiveComment()
        If _RoomId <= 0 Then Return
        If _CommentThd.ThreadState = ThreadState.Stopped Or _CommentThd.ThreadState = ThreadState.Aborted Then

            _CommentThd = New Thread(AddressOf CommentThdCallback)
            _CommentThd.Name = "Bili Live Socket Thread"
            _CommentHeartBeat = New Thread(AddressOf CommentHeartBeatCallBack)
            _CommentHeartBeat.Name = "Bili Live Socket Heartbeat Thread"
        End If

        If _CommentThd.ThreadState = ThreadState.Unstarted Then
            _isReceivingComment = True
            _CommentThd.Start()
            _CommentHeartBeat.Start()
        End If
    End Sub
    Public Sub AsyncStopReceiveComment()
        If _CommentThd.ThreadState = ThreadState.Running Then
            _isReceivingComment = False
            _CommentThd.Abort()
            _CommentHeartBeat.Abort()
        End If
    End Sub
End Class
''' <summary>
''' b站登录函数[附带RSA加密模块]
''' </summary>
''' <remarks></remarks>
Public Module Bilibili_Login
    Public Const LOGIN_URL As String = "https://passport.bilibili.com/login/dologin"
    Public Const LOGOUT_URL As String = "https://account.bilibili.com/login"
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
    ''' 使用精简登录模块，若密码输入不出错，则不需输入验证码
    ''' </summary>
    ''' <param name="userid">用户ID</param>
    ''' <param name="pwd">密码</param>
    ''' <param name="captcha">验证码</param>
    ''' <returns>登录是否成功</returns>
    ''' <remarks></remarks>
    Public Function LoginBackup(ByVal userid As String, ByVal pwd As String, Optional ByVal captcha As String = "", Optional ByRef login_result As String = Nothing) As Boolean
        Dim param As New Parameters
        param.Add("userid", userid)

        'Form1.DebugOutput("用户名: " & userid)

        'RSA加密
        Dim req As New NetStream

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

        pwd = hash & pwd
        Dim tempPwd() As Byte = System.Text.Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(pwd)

        Dim password() As Byte = rsa1.Encrypt(tempPwd, False)

        pwd = Convert.ToBase64String(password)

        param.Add("pwd", pwd)

        'Form1.DebugOutput("加密后的密码: " & pwd)

        param.Add("captcha", captcha)
        param.Add("keep", 1)


        'Form1.DebugOutput("发送登录信息...")


        req.HttpPost(BACKUP_LOGIN_URL, param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        If login_result IsNot Nothing Then
            login_result = str
        End If
        'Form1.DebugOutput("返回数据: " & str.Replace(vbCr, "").Replace(vbLf, ""))

        Return CheckLogin()
    End Function
    ''' <summary>
    ''' 获取验证码
    ''' </summary>
    ''' <returns>返回验证码图像</returns>
    ''' <remarks></remarks>
    Public Function GetCaptchaImage() As Image
        Dim req As New NetStream
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

        'Form1.DebugOutput("发送注销信息...")

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        'Form1.DebugOutput("返回数据:" & str)

        Return CheckLogin()
    End Function
End Module
