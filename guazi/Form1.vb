Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text
Imports VBUtil
Public Class Form1
#Region "调试输出控制部分[多线程调度安全]"
    'multithread safe debug output
    Friend Sub DebugOutput(ByVal s As String) Handles gz.DebugOutput
        Dim del As New _debugOutputSafe(AddressOf _debugOutput)
        Me.Invoke(del, s)
    End Sub
    Friend Delegate Sub _debugOutputSafe(ByVal s As String)
    Friend Sub _debugOutput(ByVal s As String)
        If ListView1.Items.Count > 200 Then
            ListView1.Items.RemoveAt(0)
        End If
        ListView1.Items.Add(Now.ToLongTimeString)
        ListView1.Items(ListView1.Items.Count - 1).SubItems.Add(s)
        ListView1.SelectedItems.Clear()
        ListView1.Items(ListView1.Items.Count - 1).Selected = True
    End Sub
    Private Sub ClearDebugOutput()
        TextBox1.Text = ""
    End Sub

    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged
        If ListView1.SelectedItems.Count <= 0 Then
            TextBox1.Text = ""
        Else
            TextBox1.Text = ListView1.SelectedItems(0).Text & " " & ListView1.SelectedItems(0).SubItems(1).Text
            ListView1.SelectedItems(0).EnsureVisible()
        End If
    End Sub
#End Region

    Private _initflag As Boolean
    'load config
    Private Sub LoadGuaziConfig()
        If Not IO.File.Exists(Application.StartupPath & "\setting.json") Then Return
        Dim sr As New IO.StreamReader(Application.StartupPath & "\setting.json")
        Dim str As String = sr.ReadToEnd
        sr.Close()
        Dim obj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.JsonConvert.DeserializeObject(str)
        bRoomId.Text = obj.Value(Of String)("roomid")
        AutoShutdown.Checked = obj.Value(Of Boolean)("auto_shutdown")
        AutoStart.Checked = obj.Value(Of Boolean)("auto_start")
        AutoSign.Checked = obj.Value(Of Boolean)("auto_sign")
        AutoGetItem.Checked = obj.Value(Of Boolean)("auto_get_item")
        AutoSendItem.Checked = obj.Value(Of Boolean)("auto_send_item")
        AutoGrab.Checked = obj.Value(Of Boolean)("auto_grab_silver")
        ReceiveSocket.Checked = obj.Value(Of Boolean)("receive_room_msg")
    End Sub
    'save config
    Private Sub SaveGuaziConfig()

        Dim obj As New Newtonsoft.Json.Linq.JObject
        obj.Add("roomid", bRoomId.Text)
        obj.Add("auto_shutdown", AutoShutdown.Checked)
        obj.Add("auto_start", AutoStart.Checked)
        obj.Add("auto_sign", AutoSign.Checked)
        obj.Add("auto_get_item", AutoGetItem.Checked)
        obj.Add("auto_send_item", AutoSendItem.Checked)
        obj.Add("auto_grab_silver", AutoGrab.Checked)
        obj.Add("receive_room_msg", ReceiveSocket.Checked)
        Dim str As String = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
        Dim sw As New IO.StreamWriter(Application.StartupPath & "\setting.json")
        sw.Write(str)
        sw.Close()


    End Sub
#Region "脚本1"
    Private WithEvents gz As guazi = Nothing
    Private _shutdown_timing As Boolean = False
    '完成搜刮，自动关机的模块
    Private Sub FinishGrabbing() Handles gz.FinishedGrabbing
        If AutoShutdown.Checked Then
            '每天第一次完成的时候关机，第二次点击的时候不会
            Dim str As String = ""
            If IO.File.Exists(Application.StartupPath & "\lastgrabbing") Then
                Dim sr As New IO.StreamReader(Application.StartupPath & "\lastgrabbing")
                str = sr.ReadToEnd
                sr.Close()
            End If
            Dim sw As New IO.StreamWriter(Application.StartupPath & "\lastgrabbing")
            sw.Write(Now.ToShortDateString)
            sw.Close()
            '执行关机
            If str <> Now.ToShortDateString Then
                _shutdown_timing = True
                System.Diagnostics.Process.Start("shutdown", " -s -t 60")
            End If
        End If
        '修改进度条
        Me.Invoke(New SafeSub(Sub()
                                  lblTimeOutput.Text = ""
                                  remainTime.Value = 0
                                  remainTime.Maximum = 0
                                  lblGuaziCount.Text = ""
                              End Sub))
    End Sub
    Private Sub AutoShutdown_CheckedChanged(sender As Object, e As EventArgs) Handles AutoShutdown.CheckedChanged
        If Not _initflag Then Return
        If _shutdown_timing AndAlso Not AutoShutdown.Checked Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
        SaveGuaziConfig()
    End Sub
    '取消关机
    Private Sub CancelShutdown(sender As Object, e As EventArgs) Handles Button2.Click
        If _shutdown_timing Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
    End Sub

    '窗体关闭/打开时的自动初始化/保存
    Friend Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Utils.NetUtils.SaveCookie(Application.StartupPath & "\cookie.dat")
        End
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _initflag = False
        'Auto load cookie
        Utils.NetUtils.LoadCookie(Application.StartupPath & "\cookie.dat")
        'login check
        If Not CheckLogin() Then
            'DebugOutput("状态：未登录")
            frmLogin.ShowDialog()
        Else
            'DebugOutput("状态：已经登录")
        End If

        _expireTime = Date.MinValue

        LoadGuaziConfig()
        gz = New guazi()
        If AutoStart.Checked Then
            goFuck.PerformClick()
        End If

        _initflag = True
        ReceiveSocket_CheckedChanged(sender, e) 'reperform checked change
    End Sub


    Private Sub testButton_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles testButton.LinkClicked
        Dim n As New VBUtil.CookieDebugger()
        n.Show()
    End Sub



    '倒计时模块
    Private _expireTime As Date
    Private _begTime As Date
    Private Delegate Sub SafeSub()
    Private Sub OnTimeUpdate(ByVal exptime As Date, ByVal silver As Integer) Handles gz.RefreshClock
        _expireTime = exptime
        _begTime = Now
        Me.Invoke(New SafeSub(Sub()
                                  Timer1.Enabled = True
                                  lblGuaziCount.Text = "下次可领取: " & silver & "瓜子"
                                  remainTime.Maximum = CInt((_expireTime - _begTime).TotalSeconds)
                                  remainTime.Value = 0
                              End Sub))
    End Sub
    Private Sub ChangeProgressTime(sender As Object, e As EventArgs) Handles Timer1.Tick
        If _expireTime < Now Then
            Timer1.Enabled = False
        Else
            Dim ts As TimeSpan = _expireTime - Now
            remainTime.Value = remainTime.Maximum - CInt(ts.TotalSeconds)
            lblTimeOutput.Text = ts.Minutes & ":" & Format(ts.Seconds, "00")
        End If

    End Sub

    '输入房间号的模块 (+ 自动领取瓜子)
    Private Sub bRoomId_KeyPress(sender As Object, e As KeyPressEventArgs) Handles bRoomId.KeyPress
        If e.KeyChar = vbCr Then
            AllocGuazi()
            If gz Is Nothing Then e.Handled = True
            goFuck.PerformClick()
            SaveGuaziConfig()
        End If
    End Sub

    'Parsing Room id and memory allocating new class
    Private Sub AllocGuazi()
        Dim roomid As Integer = 0
        If Not Integer.TryParse(bRoomId.Text, roomid) Then
            MessageBox.Show("这是数字你在逗我吗？", "WTF", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            bRoomId.Text = gz.RoomID
            Return
        End If
        If roomid <= 0 Then
            MessageBox.Show("你输入的都是什么啊(╯‵□′)╯︵┻━┻", "WTFx2", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            bRoomId.Text = gz.RoomID
            Return
        End If

        gz.RoomID = bRoomId.Text
    End Sub

    '开机启动模块
    Private Sub AutoStart_CheckedChanged(sender As Object, e As EventArgs) Handles AutoStart.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()

        '写入注册表
        Dim regname As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\microsoft\windows\currentversion\run", True)

        If AutoStart.Checked Then
            regname.SetValue("瓜子搜刮机", Application.ExecutablePath)
        Else
            regname.DeleteValue("瓜子搜刮机", False)
        End If
    End Sub


    Private Sub lblLogout_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lblLogout.LinkClicked
        If Not LogOut() Then
            'Dim a As New frmLogin
            'a.ShowDialog(Me)
            My.Forms.frmLogin.ShowDialog()
        End If
    End Sub

    Private Sub AutoSign_CheckedChanged(sender As Object, e As EventArgs) Handles AutoSign.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
    End Sub

    Private Sub AutoGetItem_CheckedChanged(sender As Object, e As EventArgs) Handles AutoGetItem.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
    End Sub

    Private Sub AutoSendItem_CheckedChanged(sender As Object, e As EventArgs) Handles AutoSendItem.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
    End Sub

    Private Sub AutoGrab_CheckedChanged(sender As Object, e As EventArgs) Handles AutoGrab.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
    End Sub

    Private Sub goFuck_Click(sender As Object, e As EventArgs) Handles goFuck.Click
        AllocGuazi()
        If gz Is Nothing Then Return

        If AutoSign.Checked Then gz.AsyncDoSign()
        If AutoGetItem.Checked Then gz.AsyncGetDailyGift()
        If AutoSendItem.Checked Then gz.AsyncSendDailyGift()
        If AutoGrab.Checked Then gz.AsyncStartGrabbingSilver()
    End Sub

#End Region
#Region "脚本2"

    Private _isRecording As Boolean
    Private _downloadSize As Long
    Private _downloadTime As Date
    Private Sub StartRecord_Click(sender As Object, e As EventArgs) Handles StartRecord.Click
        StartRecord.Enabled = False
        If _isRecording Then
            gz.AsyncStopDownloadStream()
        Else
            SaveFileDialog1.FileName = gz.RoomID & "_" & Now.ToString.Replace(":", "_").Replace("/", "_") & ".flv"
            If SaveFileDialog1.ShowDialog <> Windows.Forms.DialogResult.OK Then
                StartRecord.Enabled = True
                Return
            End If
            gz.AsyncStartDownloadStream(SaveFileDialog1.FileName)
        End If
    End Sub
    Private Sub OnStartRecord() Handles gz.DownloadStarted
        Me.Invoke(New SafeSub(Sub()
                                  StartRecord.Enabled = True
                                  StartRecord.Text = "点击停止录播"
                                  _downloadSize = 0
                                  _downloadTime = Now
                                  _isRecording = True
                              End Sub))
    End Sub

    Private Sub OnStopRecord() Handles gz.DownloadStopped
        Me.Invoke(New SafeSub(Sub()
                                  StartRecord.Enabled = True
                                  lblSpeed.Text = ""
                                  lblSize.Text = ""
                                  lblDownloadTime.Text = ""
                                  StartRecord.Text = "点击开始录播"
                                  _isRecording = False
                              End Sub))
    End Sub
    Private Sub OnSpeedChange(ByVal speed As Integer) Handles gz.DownloadSpeed
        Me.Invoke(New SafeSub(Sub()
                                  lblSpeed.Text = "当前速度:" & Format(speed / 1024, "0.00") & "KB/S"
                                  _downloadSize += speed
                                  lblSize.Text = "文件大小:" & Format(_downloadSize / 1024 / 1024, "0.00") & "MB"
                                  lblDownloadTime.Text = "录播时长:" & (Now - _downloadTime).ToString("h\:mm\:ss")
                              End Sub))
    End Sub

#End Region
#Region "脚本3"

    Private Sub ReceiveSocket_CheckedChanged(sender As Object, e As EventArgs) Handles ReceiveSocket.CheckedChanged
        If Not _initflag Then Return
        If gz Is Nothing Then Return
        If ReceiveSocket.Checked Then
            gz.AsyncStartReceiveComment()
        Else
            gz.AsyncStopReceiveComment()
        End If
        SaveGuaziConfig()
    End Sub
    Private Sub OnReceivingComment(ByVal unixTimestamp As Long, ByVal username As String, ByVal msg As String) Handles gz.ReceivedComment
        DebugOutput("收到弹幕:[" & username & "] " & msg)
    End Sub
    Private Sub OnReceivingGiftSent(ByVal unixTimestamp As Long, ByVal giftName As String, ByVal giftId As Integer, ByVal giftNum As Integer, ByVal user As String) Handles gz.ReceivedGiftSent
        DebugOutput("收到礼物:[" & user & "] " & giftName & "(id=" & giftId & ")" & " x" & giftNum)
    End Sub
    Private Sub OnReceivingWelcome(ByVal admin As Boolean, ByVal vip As Boolean, ByVal name As String) Handles gz.ReceivedWelcome
        DebugOutput("欢迎老爷:[" & name & "]")
    End Sub
    Private Sub OnReceivingOnlinePeople(ByVal people As Integer) Handles gz.ReceivedOnlinePeople
        Me.Invoke(New SafeSub(Sub()
                                  OnlinePeople.Text = "在线人数:" & people
                              End Sub))
    End Sub
    Private Sub OnReceivingSystemMsg(ByVal msg As String, ByVal refer_url As String) Handles gz.ReceivedSystemMsg
        DebugOutput("收到系统消息:" & msg)
    End Sub
#End Region

End Class
