Imports System
Imports System.Net
Public Class CookieEditor
    Friend DefaultCookie As Cookie = Nothing
    Public Event UpdateCookie(ByVal origin As Cookie, ByVal current As Cookie)
    Private Sub CookieEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If DefaultCookie IsNot Nothing Then
            TextBox1.Text = DefaultCookie.Domain
            TextBox2.Text = DefaultCookie.Name
            TextBox3.Text = DefaultCookie.Value
            DateTimePicker1.Value = DefaultCookie.Expires
            CheckBox1.Checked = DefaultCookie.HttpOnly
            CheckBox2.Checked = DefaultCookie.Discard
        Else
            TextBox1.Text = ""
            TextBox2.Text = ""
            TextBox3.Text = ""
            DateTimePicker1.Value = Now
            CheckBox1.Checked = False
            CheckBox2.Checked = False
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If String.IsNullOrEmpty(TextBox2.Text) Or String.IsNullOrEmpty(TextBox3.Text) Then Return
        Dim c As Cookie = New Cookie(TextBox2.Text, TextBox3.Text)
        c.HttpOnly = CheckBox1.Checked
        c.Discard = CheckBox2.Checked
        c.Domain = TextBox1.Text
        c.Expires = DateTimePicker1.Value
        RaiseEvent UpdateCookie(DefaultCookie, c)
        DefaultCookie = Nothing
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        DefaultCookie = Nothing
        Me.Close()
    End Sub
End Class