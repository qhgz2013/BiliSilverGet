Public Class frmLogin
    Private _islogin As Boolean = False
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim result As Boolean = LoginBackup(TextBox2.Text, TextBox3.Text)
        If result Then
            MessageBox.Show("登录成功")
            Me.Hide()
            _islogin = True
        Else
            MessageBox.Show("登录失败")
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
        TextBox4.Visible = False
        LinkLabel1.Visible = False
    End Sub
End Class