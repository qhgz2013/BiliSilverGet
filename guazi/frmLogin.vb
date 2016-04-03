Public Class frmLogin
    '保存是否登录
    Private _islogin As Boolean = False

    '是否使用主站登录，如果使用，则需要输入验证码
    '如果不需要，则使用迷你登录模块，在密码确保不会连续输入错误下，不需输入验证码
    Private Const USING_MAIN_LOGIN_MODULE As Boolean = False
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim result As Boolean
        Dim result_str As String = ""
        If USING_MAIN_LOGIN_MODULE Then
            result = Login(TextBox2.Text, TextBox3.Text, TextBox4.Text, result_str)
        Else
            result = LoginBackup(TextBox2.Text, TextBox3.Text, , result_str)
        End If

        If result Then
            'MessageBox.Show("登录成功")
            Me.Hide()
            _islogin = True
        Else
            MessageBox.Show("登录失败，返回数据: " & vbCrLf & result_str, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
            TextBox4.Text = ""
        End If
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        PictureBox1.Image = GetCaptchaImage()
    End Sub

    Private Sub TextBox4_GotFocus(sender As Object, e As EventArgs) Handles TextBox4.GotFocus
        If PictureBox1.Image Is Nothing Then
            PictureBox1.Image = GetCaptchaImage()
        End If
    End Sub

    Private Sub frmLogin_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not _islogin Then
            Form1.Form1_FormClosing(sender, e)
        End If
    End Sub

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not USING_MAIN_LOGIN_MODULE Then
            TextBox4.Visible = False
            LinkLabel1.Visible = False
        End If
    End Sub

    Private Sub TextBox2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox2.KeyPress
        If e.KeyChar = vbCr Then
            TextBox3.Focus()
            e.Handled = True
        End If

    End Sub

    Private Sub TextBox3_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox3.KeyPress
        If e.KeyChar = vbCr Then
            Button1.PerformClick()
            e.Handled = True
        End If
    End Sub
End Class