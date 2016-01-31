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
        ListView1.Items.Insert(0, Now.ToLongTimeString)
        ListView1.Items(0).SubItems.Add(s)
    End Sub
    Private Sub ClearDebugOutput()
        TextBox1.Text = ""
    End Sub
#End Region
    Private WithEvents gz As guazi = Nothing
    Private _shutdown_timing As Boolean = False
    Private Sub FinishGrabbing() Handles gz.FinishedGrabbing
        If CheckBox1.Checked Then
            Dim str As String = ""
            If IO.File.Exists(Application.StartupPath & "\lastgrabbing") Then
                Dim sr As New IO.StreamReader(Application.StartupPath & "\lastgrabbing")
                str = sr.ReadToEnd
                sr.Close()
            End If
            Dim sw As New IO.StreamWriter(Application.StartupPath & "\lastgrabbing")
            sw.Write(Now.ToShortDateString)
            sw.Close()

            If str <> Now.ToShortDateString Then
                _shutdown_timing = True
                System.Diagnostics.Process.Start("shutdown", " -s -t 60")
            End If
        End If
    End Sub
    Private Delegate Sub SafeSub()
    Private Sub FinishExecuting() Handles gz.FinishedExecuting
        Dim del As New SafeSub(Sub()
                                   getGuazi.Enabled = True
                               End Sub)
        Me.Invoke(del)
    End Sub
    Friend Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Utils.NetUtils.SaveCookie(Application.StartupPath & "\cookie.dat")
        End
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Auto load cookie
        Utils.NetUtils.LoadCookie(Application.StartupPath & "\cookie.dat")
        LoadGuaziConfig()
        'login check
        If Not CheckLogin() Then
            DebugOutput("状态：未登录")
            frmLogin.ShowDialog()
        Else
            DebugOutput("状态：已经登录")
            If CheckBox2.Checked Then
                getGuazi.PerformClick()
            End If
        End If
    End Sub


    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged
        If ListView1.SelectedItems.Count <= 0 Then
            TextBox1.Text = ""
        Else
            TextBox1.Text = ListView1.SelectedItems(0).Text & " " & ListView1.SelectedItems(0).SubItems(1).Text

        End If
    End Sub

    Private Sub getGuazi_Click(sender As Object, e As EventArgs) Handles getGuazi.Click
        'If Not IsNumeric(bRoomId.Text) Then
        'MessageBox.Show("房间号是数字你在逗我呢_(:з」∠)_")
        'Return
        'End If
        gz = New guazi '(bRoomId.Text)
        getGuazi.Enabled = False
    End Sub

    Private Sub testButton_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles testButton.LinkClicked
        Dim n As New VBUtil.CookieDebugger()
        n.Show()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If _shutdown_timing AndAlso Not CheckBox1.Checked Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
    End Sub

    'load config
    Private Sub LoadGuaziConfig()
        If Not IO.File.Exists(Application.StartupPath & "\setting.json") Then Return
        Dim sr As New IO.StreamReader(Application.StartupPath & "\setting.json")
        Dim str As String = sr.ReadToEnd
        sr.Close()
        Dim obj As Newtonsoft.Json.Linq.JObject = Newtonsoft.Json.JsonConvert.DeserializeObject(str)
        bRoomId.Text = obj.Value(Of String)("roomid")
        CheckBox1.Checked = obj.Value(Of Boolean)("auto_shutdown")
        CheckBox2.Checked = obj.Value(Of Boolean)("auto_start")
    End Sub
    'save config
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim roomid As String = bRoomId.Text
        Dim auto_shutdown As Boolean = CheckBox1.Checked
        Dim auto_start As Boolean = CheckBox2.Checked
        Dim obj As New Newtonsoft.Json.Linq.JObject
        obj.Add("roomid", roomid)
        obj.Add("auto_shutdown", auto_shutdown)
        obj.Add("auto_start", auto_start)
        Dim str As String = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
        Dim sw As New IO.StreamWriter(Application.StartupPath & "\setting.json")
        sw.Write(str)
        sw.Close()

        '写入注册表
        Dim regname As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\microsoft\windows\currentversion\run", True)

        If auto_start Then
            regname.SetValue("瓜子搜刮机", Application.ExecutablePath)
        Else
            regname.DeleteValue("瓜子搜刮机", False)
        End If

    End Sub
    '取消关机
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If _shutdown_timing Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
    End Sub

    'log out
    Private Sub Logout_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lblLogout.LinkClicked
        If Not LogOut() Then
            DebugOutput("退出登录成功")
            frmLogin.ShowDialog()
        Else
            DebugOutput("退出登录失败")
            MessageBox.Show("退出登录失败，请重试")
        End If
    End Sub
End Class
