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
Imports guazi.Utils.NetUtils
Imports guazi.Utils.StreamUtils
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
                'While Not award_requests()
                'Thread.Sleep(3000)
                'End While
                send_heartbeat()
                'Dim answer As String
                Dim awardsilver, getVote, svote As Integer
                RaiseEvent DebugOutput("开始领取！")
                For i As Integer = 1 To 11
                    'answer = captcha_wrapper()
                    'Dim count As Integer = 1
                    'While answer = ""
                    'RaiseEvent DebugOutput("验证码识别错误，重试第" & count & "次")
                    'answer = captcha_wrapper()
                    'count += 1
                    'End While
                    'Thread.Sleep(1000)
                    je = get_award()
                    If je.Value(Of Integer)("code") = 0 Then

                        awardsilver = je("data").Value(Of Integer)("awardSilver")
                        silver = je("data").Value(Of Integer)("silver")
                        'getVote = je("data").Value(Of Integer)("getVote")
                        'svote = je("data").Value(Of Integer)("svote")
                        'vote = je("data").Value(Of Integer)("vote")

                        If awardsilver > 0 Then
                            Exit For
                        End If
                    Else
                        RaiseEvent DebugOutput("错误,重试第" & i & "次")
                        Thread.Sleep(60000)
                    End If
                Next
                RaiseEvent DebugOutput("成功！得到" & awardsilver & "个银瓜子(总" & silver & "个)，" & getVote & "张投票券(总" & vote & "张,当天可用" & svote & "张)")
                'If vote > svote Then vote = svote
                'send_vote(vote)
                'RaiseEvent DebugOutput("自动送出全部(" & vote & "张)投票券")
                Thread.Sleep(1000)
            Loop
        Catch ex As Exception
            RaiseEvent DebugOutput("[ERR] exception : " & vbCrLf & ex.ToString)
        End Try
        RaiseEvent FinishedExecuting()
        'RaiseEvent DebugOutput("[Debug] Thread Callback Ended")
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
    ''' math.rand()
    ''' </summary>
    ''' <returns>0~1的随机数</returns>
    ''' <remarks></remarks>
    Private Function randomR() As Double

        Return Utils.Others.rand.NextDouble
    End Function

    ''' <summary>
    ''' 验证数学表达式是否可求值
    ''' </summary>
    ''' <param name="string_this">待验证的数学表达式</param>
    ''' <returns>是否可以用于求值</returns>
    ''' <remarks></remarks>
    Private Function safe_to_eval(ByVal string_this As String) As Boolean
        Dim match As Match = Regex.Match(string_this, "^[\d\+\-\s]+$")

        'RaiseEvent DebugOutput("[Debug] In safe_to_eval(string_this) return: " & match.Success.ToString)

        If match.Success Then
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' 表达式求值(这里只做了简单的两位数加减，如果要做全的话又得搬代码了)
    ''' </summary>
    ''' <param name="string_this">表达式</param>
    ''' <returns>表达式的值</returns>
    ''' <remarks></remarks>
    Private Function eval(ByVal string_this As String) As String
        '只做一个简单的加减法功能，如果要完全的表达式算值的话..
        Dim a1 As String = "0"
        Dim a2 As String = "0"
        Dim oper As String = ""
        Dim l As Boolean = False
        For i As Integer = 0 To string_this.Length - 1
            If IsNumeric(string_this(i)) Then
                If l Then
                    a2 &= string_this(i)
                Else
                    a1 &= string_this(i)
                End If
            ElseIf string_this(i) = "+" Then
                oper = string_this(i)
                l = True
            ElseIf string_this(i) = "-" Then
                oper = string_this(i)
                l = True
            ElseIf string_this(i) = " " Then
            Else
                Throw New Exception("Invalid Char")
            End If
        Next

        Dim ret As String

        Select Case oper
            Case "+"
                ret = Integer.Parse(a1) + Integer.Parse(a2)
            Case "-"
                ret = Integer.Parse(a1) - Integer.Parse(a2)
            Case Else
                ret = ""
                Throw New Exception("Invalid Operator")
        End Select
        'RaiseEvent DebugOutput("[Debug] eval(string_this) return: " & ret)

        Return ret
    End Function

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
    ''' <returns>此时是否能领取瓜子</returns>
    ''' <remarks></remarks>
    Private Function award_requests() As Boolean
        Dim url As String = "http://live.bilibili.com/freeSilver/getSurplus"
        Dim req As New NetStream
        Dim param As New Parameters
        param.Add("r", randomR)
        req.HttpGet(url, , param)
        Dim str As String = ReadToEnd(req.Stream)
        Dim a As JObject = JsonConvert.DeserializeObject(str)
        req.Close()

        If DEBUG_RETURN_INFO Then
            RaiseEvent DebugOutput("[Debug] In award_requests() : ")
            RaiseEvent DebugOutput("[Debug] [HTTP request]" & url)
            RaiseEvent DebugOutput("[Debug] [HTTP string]" & a.ToString)
        End If

        If req.HTTP_Response.StatusCode <> Net.HttpStatusCode.OK Or a.Value(Of Integer)("code") <> 0 Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Sub New(Optional ByVal roomid As Integer = 0)
        _workThd = New Thread(AddressOf CallBack)
        _workThd.Name = "Auto Grabbing Silvers"

        _RoomId = roomid
        _RoomInfo = Nothing

        _workThd.Start()
    End Sub
End Class
''' <summary>
''' 自动登录b站封装类
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
