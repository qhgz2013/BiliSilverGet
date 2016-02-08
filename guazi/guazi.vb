'b站挂瓜子的脚本而已

'Author: Beining --<i@cnbeining.com>
'Co-op: SuperFashi
'Purpose: Auto grab silver of Bilibili
'Created: 10/22/2015
'Last modified: 12/8/2015
' https://www.cnbeining.com/
' https://github.com/cnbeining

'source code : python ->  vb .net
'translator: pandasxd (12/13/2015)

Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports VBUtil.Utils.NetUtils
Imports VBUtil.Utils.StreamUtils
Imports VBUtil
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' 哼，我就要瓜子，你来咬我啊
''' </summary>
''' <remarks></remarks>

Public Module Variables
End Module
Public Class guazi
    'Debug v1.2 added mobile
    Public Event DebugOutput(ByVal msg As String)
    Public Event FinishedGrabbing()
    Public Event FinishedExecuting()
    Public Event StartedExecuting()
    Private _workThd As Thread
    Private _startTime As Integer
    Private _RoomId As Integer
    Private _RoomInfo As JObject

    Private Const APPKEY As String = "85eb6835b0a1034e" '"c1b107428d337928" 
    Private Const SECRETKEY As String = "2ad42749773c441109bdc0191257a664" '"49c0356b37c67b5524c3646b694e8a57" 
    Private Const VER As String = "0.98.86"

    Private Const DEBUG_RETURN_INFO As Boolean = False
    Private Function calc_sign(ByVal str As String) As String
        Dim md5 As New System.Security.Cryptography.MD5CryptoServiceProvider

        Return Utils.Others.Hex(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str))).ToLower
    End Function

    ''' <summary>
    ''' 线程回调函数
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CallBack()
        'RaiseEvent DebugOutput("[Debug] Thread Callback Started")
        RaiseEvent StartedExecuting()
        'get room info
        get_room_info()
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

            '领取并送出道具
            Dim gift_rep As JObject = get_send_gift()
            If gift_rep.Value(Of Integer)("code") = 0 Then
                RaiseEvent DebugOutput("领取每日道具成功，获得" & gift_rep.Value(Of JArray)("data").Count & "个道具")
            Else
                RaiseEvent DebugOutput("领取道具失败，返回数据:" & vbCrLf & gift_rep.ToString)
            End If

            RaiseEvent DebugOutput("自动送出道具(如果道具背包拥有)")
            get_player_bag(True)

            '循环领取瓜子
            Do
                Dim je As JObject = get_new_task_time_and_award()

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
                Dim silver As Integer = je("data").Value(Of Integer)("silver")
                Dim vote As Integer = je("data").Value(Of Integer)("vote")

                RaiseEvent DebugOutput("预计下次领取需要" & minutes & "分钟，可以领取" & silver & "个银瓜子和" & vote & "张投票券")

                Thread.Sleep(3000)

                Dim picktime As Date = Now + New TimeSpan(0, minutes, 0)
                While (picktime - Now).TotalSeconds > 0
                    Thread.Sleep(New TimeSpan(0, 1, 0))
                    send_heartbeat()
                    RaiseEvent DebugOutput("还剩下" & Math.Round((picktime - Now).TotalMinutes) & "分钟")

                End While

                Dim times As Integer = 0
                While award_requests() <> 0 AndAlso times <= 10
                    times += 1
                    RaiseEvent DebugOutput("领取请求发送错误，重试第" & times & "次")
                    Thread.Sleep(10000)
                End While

                Dim awardsilver, getVote, svote As Integer
                RaiseEvent DebugOutput("开始领取！")
                For i As Integer = 1 To 11

                    je = get_award()
                    If je.Value(Of Integer)("code") = 0 Then

                        awardsilver = je("data").Value(Of Integer)("awardSilver")
                        silver = je("data").Value(Of Integer)("silver")
                        getVote = 0
                        svote = 0
                        vote = 0

                        If awardsilver > 0 Then
                            Exit For
                        End If
                    Else
                        RaiseEvent DebugOutput("错误,重试第" & i & "次")
                        Thread.Sleep(60000)
                    End If
                Next
                RaiseEvent DebugOutput("成功！得到" & awardsilver & "个银瓜子(总" & silver & "个)，" & getVote & "张投票券(总" & vote & "张,当天可用" & svote & "张)")

                Thread.Sleep(1000)
            Loop
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] exception : " & vbCrLf & ex.ToString)
        End Try
        RaiseEvent FinishedExecuting()
    End Sub

    ''' <summary>
    ''' 获取房间的信息，用于投票
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub get_room_info()
        If _RoomId = 0 Then Return
        Dim url As String = "http://live.bilibili.com/live/getInfo"
        Dim param As New Parameters
        param.Add("roomid", _RoomId)
        Dim req As New NetStream
        req.HttpGet(url, , param)

        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        _RoomInfo = JsonConvert.DeserializeObject(str)

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("In get_room_info : " & _RoomInfo.ToString)
        End If

        _startTime = Int(Utils.Others.ToUnixTimestamp(Now))

    End Sub

    ''' <summary>
    ''' 发送投票请求(因为投票券是当天有效的，get到后立马扔了不要留)
    ''' </summary>
    ''' <param name="num">要撒花的数量(注意不要超过最大数值)</param>
    ''' <remarks>为了安全，明天添加个数值检测 url:/activity/getStarVote?timestamp=x</remarks>
    Private Sub send_vote(ByVal num As Integer)
        Dim url As String = "http://live.bilibili.com/gift/send"
        If _RoomId = 0 Then Return
        If num <= 0 Then Return
        Dim param As New Parameters
        param.Add("giftId", 11)
        param.Add("roomid", _RoomId)
        param.Add("ruid", _RoomInfo("data").Value(Of Integer)("MASTERID"))
        param.Add("num", num)
        param.Add("coinType", "gold")
        param.Add("timestamp", Int(Utils.Others.ToUnixTimestamp(Now)))
        param.Add("rnd", _startTime)
        Dim token As String = DefaultCookieContainer.GetCookies(New Uri(url))("LIVE_LOGIN_DATA").Value
        param.Add("token", token)

        '就是少了这个东东结果一直卡参数错误上了_(:з」∠)_
        '哔——站码农都是傻逼么
        Dim headerparam As New Parameters
        headerparam.Add("X-Requested-With", "XMLHttpRequest")

        Dim req As New NetStream
        req.HttpPost(url, param, headerparam)

        Dim str As String = ReadToEnd(req.Stream)
        req.Close()
        Dim n As JObject = JsonConvert.DeserializeObject(str)
        str = n.ToString

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] in send_vote: " & str)
        End If
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
        'If _RoomId Then param.Add("roomid", _RoomId)

        'sign calc
        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))

        'param.Add("r", CStr(randomR()))
        Dim req As New NetStream
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
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
        'param.Add("r", randomR)

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        Dim a As JObject = JsonConvert.DeserializeObject(str)
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In send_heartbeat() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)

        End If

        If req.HTTP_Response.StatusCode <> Net.HttpStatusCode.OK Then
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
        'param.Add("r", randomR)
        param.Add("appkey", APPKEY)
        param.Add("platform", "ios")

        'If _RoomId Then param.Add("roomid", _RoomId)

        param.Add("sign", calc_sign(param.BuildURLQuery & SECRETKEY))

        'param.Add("captcha", captcha)
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        Dim a As JObject = JsonConvert.DeserializeObject(str)
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In get_award(captcha) : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)

        End If

        If req.HTTP_Response.StatusCode <> Net.HttpStatusCode.OK Or a.Value(Of Integer)("code") <> 0 Then
            RaiseEvent DebugOutput(a.Value(Of String)("message"))
        End If
        Return a
    End Function

    ''' <summary>
    ''' 领取瓜子前的请求(前戏？)
    ''' </summary>
    ''' <returns>状态码</returns>
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
        Dim a As JObject = JsonConvert.DeserializeObject(str)
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In award_requests() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)
        End If

        If req.HTTP_Response.StatusCode <> Net.HttpStatusCode.OK Then
            Return -1
        Else
            Return a.Value(Of Integer)("code")
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
                'req_param.Add("ruid", _RoomInfo.Value(Of Integer)("MASTERID"))
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

    ''' <summary>
    ''' 构造函数，roomid之前是用来送投票券和道具的
    ''' </summary>
    ''' <param name="roomid"></param>
    ''' <remarks></remarks>
    Public Sub New(Optional ByVal roomid As Integer = 0)
        _workThd = New Thread(AddressOf CallBack)
        _workThd.Name = "Bili Live Auto Grabbing Silver Thread"

        _RoomId = roomid
        _RoomInfo = Nothing

        _workThd.Start()
    End Sub
End Class
''' <summary>
''' 自动挂机累加在线时长的东东
''' </summary>
''' <remarks></remarks>
Public Class keep_on_line
    Private _thd As Thread
    Private _next_update_time As DateTime
    Private _beg_time As DateTime
    Private _thread_abort_flag As Boolean
    Public Sub New()
        _thd = New Thread(AddressOf CallBack)
        _thd.Name = "Bili Live Heart Beat Thread"
        _next_update_time = Now
        _thread_abort_flag = False
        _beg_time = Now
        _thd.Start()
    End Sub
    Public Sub Abort()
        _thread_abort_flag = True
        _thd.Join()
    End Sub
    Private Sub CallBack()
        Dim req_url As String = "http://live.bilibili.com/feed/heartBeat/heartBeat"
        Dim req As New NetStream
        While Not _thread_abort_flag
            Try
                Thread.Sleep(1000)
                If _next_update_time < Now Then
                    _next_update_time = _next_update_time.AddMinutes(1)

                    Dim req_param As New Parameters
                    Dim hb As String = "0"
                    req_param.Add("hb", hb)
                    req.HttpPost(req_url, req_param)

                    Dim ret_str As String = ReadToEnd(req.Stream)
                    req.Close()
                    'replace
                    ret_str = Regex.Replace(ret_str, "\((.*?)\);", "$1")
                    Dim ret_obj As JObject = JsonConvert.DeserializeObject(ret_str)
                    Dim code As Integer = ret_obj.Value(Of Integer)("code")
                    'hb = ret_obj("data").Value(Of String)("hb")
                    If code = 0 Then
                        'RaiseEvent DebugOutput("发送在线心跳包成功,已在线" & Int((Now - _beg_time).TotalMinutes) & "分钟")
                    Else
                        RaiseEvent DebugOutput("发送在线心跳包失败:" & code)
                    End If

                End If
            Catch ex As Exception
                RaiseEvent DebugOutput("[ERROR] " & ex.ToString)
            End Try
        End While
    End Sub
    Public Event DebugOutput(ByVal msg As String)
End Class
''' <summary>
''' b站登录函数[附带RSA加密模块]
''' </summary>
''' <remarks></remarks>
Public Module Bilibili_Login
    Public Const LOGIN_URL As String = "https://account.bilibili.com/login/dologin"
    Public Const LOGOUT_URL As String = "https://account.bilibili.com/login"
    Public Const BACKUP_LOGIN_URL As String = "https://account.bilibili.com/ajax/miniLogin/login"
    Public Const LOGIN_PUBLIC_KEY As String = "https://account.bilibili.com/login?act=getkey"
    Public Const BILIBILI_MAIN_PAGE As String = "http://www.bilibili.com"
    Public Const CAPTCHA_URL As String = "https://account.bilibili.com/captcha"
    Public Const API_MY_INFO As String = "http://api.bilibili.com/myinfo"
    '2015/12/31  RSA login succeeded
    Public Function Login(ByVal userid As String, ByVal pwd As String, ByVal captcha As String) As Boolean
        Dim param As New Parameters
        param.Add("act", "login")
        param.Add("userid", ToURLCharacter(userid))
        param.Add("keeptime", 604800)

        Form1.DebugOutput("用户名: " & userid)

        'RSA加密
        Dim req As New NetStream

        Form1.DebugOutput("获取RSA公钥URL: " & LOGIN_PUBLIC_KEY)
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

        param.Add("pwd", ToURLCharacter(pwd))

        'Form1.DebugOutput("加密后的密码: " & pwd)

        param.Add("vdcode", captcha)


        Form1.DebugOutput("发送登录信息...")

        req.HttpPost(LOGIN_URL, param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        Form1.DebugOutput("返回数据: " & str)

        Return CheckLogin()
    End Function
    Public Function LoginBackup(ByVal userid As String, ByVal pwd As String, Optional ByVal captcha As String = "") As Boolean
        Dim param As New Parameters
        param.Add("userid", ToURLCharacter(userid))

        Form1.DebugOutput("用户名: " & userid)

        'RSA加密
        Dim req As New NetStream

        Form1.DebugOutput("获取RSA公钥URL: " & LOGIN_PUBLIC_KEY)
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

        param.Add("pwd", ToURLCharacter(pwd))

        'Form1.DebugOutput("加密后的密码: " & pwd)

        param.Add("captcha", captcha)
        param.Add("keep", 1)


        Form1.DebugOutput("发送登录信息...")


        req.HttpPost(BACKUP_LOGIN_URL, param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        Form1.DebugOutput("返回数据: " & str.Replace(vbCr, "").Replace(vbLf, ""))

        Return CheckLogin()
    End Function
    Public Function GetCaptchaImage() As Image
        Dim req As New NetStream
        req.HttpGet(CAPTCHA_URL)
        Dim img As Image = Image.FromStream(req.Stream)
        req.Close()
        Return img
    End Function
    Public Function CheckLogin() As Boolean
        Dim req As New NetStream
        req.HttpGet(API_MY_INFO)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()
        str = str.Replace(vbCr, "").Replace(vbLf, "")
        Form1.DebugOutput("API请求: [登录信息]: " & API_MY_INFO & vbCrLf & JsonConvert.DeserializeObject(str).ToString)
        Return If(InStr(str, "-101"), False, True)
    End Function
    Public Function LogOut() As Boolean
        Dim url As String = LOGOUT_URL
        Dim param As New Parameters
        param.Add("act", "exit")
        Dim req As New NetStream

        Form1.DebugOutput("发送注销信息...")

        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        req.Close()

        Form1.DebugOutput("返回数据:" & str)

        Return CheckLogin()
    End Function
End Module
