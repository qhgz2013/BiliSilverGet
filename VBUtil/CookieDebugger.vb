Imports System.Net

Public Class CookieDebugger
    Private Const _RESET_TIME As Integer = 60
    Private _time As Integer = 0
    Private WithEvents _editor As New CookieEditor

    Private Sub CookieDebugger_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Timer1.Stop()
    End Sub
    Private Sub CookieDebugger_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Timer1.Start()
        ProgressBar1.Maximum = _RESET_TIME
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim ts As New TimeSpan(0, 0, _time)
        lblTime.Text = ts.ToString
        ProgressBar1.Value = _RESET_TIME - _time
        If _time = 0 Then Refresh_Item()

        _time -= 1
    End Sub

    Private Sub Refresh_Item()
        _time = _RESET_TIME
        ListView1.Items.Clear()
        Dim table As Hashtable = Utils.NetUtils.DefaultCookieContainer.GetType().InvokeMember("m_domainTable", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance, Nothing, Utils.NetUtils.DefaultCookieContainer, New Object() {})

        For Each pathList As Object In table.Values
            Dim lstCookieCol As SortedList = pathList.GetType().InvokeMember("m_list", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance, Nothing, pathList, New Object() {})
            For Each cc As CookieCollection In lstCookieCol.Values
                For Each c As Cookie In cc
                    Dim lvi As New ListViewItem(c.Domain)
                    lvi.SubItems.Add(c.Name)
                    lvi.SubItems.Add(c.Value)
                    lvi.SubItems.Add(c.Expires)
                    lvi.SubItems.Add(c.Expired)
                    lvi.SubItems.Add(c.HttpOnly)
                    lvi.SubItems.Add(c.Discard)

                    ListView1.Items.Add(lvi)
                Next
            Next
        Next
    End Sub

    Private Sub OnCookieUpdate(ByVal c1 As Cookie, ByVal c2 As Cookie) Handles _editor.UpdateCookie
        Try
            If c1 IsNot Nothing Then
                Dim new_cc As New CookieContainer

                Dim table As Hashtable = Utils.NetUtils.DefaultCookieContainer.GetType().InvokeMember("m_domainTable", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance, Nothing, Utils.NetUtils.DefaultCookieContainer, New Object() {})
                For Each pathList As Object In table.Values
                    Dim lstCookieCol As SortedList = pathList.GetType().InvokeMember("m_list", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance, Nothing, pathList, New Object() {})
                    For Each cc As CookieCollection In lstCookieCol.Values
                        For Each c As Cookie In cc
                            If (c.Domain <> c1.Domain AndAlso c.Value <> c1.Value AndAlso c.Name <> c1.Name AndAlso c.Expires <> c1.Expires) Then new_cc.Add(c)
                        Next
                    Next
                Next
                Utils.NetUtils.DefaultCookieContainer = new_cc
            End If

            Utils.NetUtils.DefaultCookieContainer.Add(c2)
            Refresh_Item()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub 新增项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 新增项ToolStripMenuItem.Click
        _editor.DefaultCookie = Nothing
        _editor.ShowDialog()
        Refresh_Item()
    End Sub

    Private Sub 修改选定项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 修改选定项ToolStripMenuItem.Click
        If ListView1.SelectedItems.Count = 0 Then Return
        Dim lvi As ListViewItem = ListView1.SelectedItems(0)
        Dim c As New Cookie(lvi.SubItems(1).Text, lvi.SubItems(2).Text)
        c.Domain = lvi.SubItems(0).Text
        c.Expires = lvi.SubItems(3).Text
        c.HttpOnly = If(lvi.SubItems(5).Text = "True", True, False)
        c.Discard = If(lvi.SubItems(6).Text = "True", True, False)
        _editor.DefaultCookie = c
        _editor.ShowDialog()
        Refresh_Item()
    End Sub

    Private Sub 删除选定项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 删除选定项ToolStripMenuItem.Click
        '算了直接开新的把
        Utils.NetUtils.DefaultCookieContainer = New CookieContainer()
        Dim result = From lvi As ListViewItem In ListView1.Items Where lvi.Checked = False Select lvi
        For Each lvi As ListViewItem In result
            Dim c As New Cookie(lvi.SubItems(1).Text, lvi.SubItems(2).Text)
            c.Domain = lvi.SubItems(0).Text
            c.Expires = lvi.SubItems(3).Text
            c.HttpOnly = If(lvi.SubItems(5).Text = "True", True, False)
            c.Discard = If(lvi.SubItems(6).Text = "True", True, False)
            Utils.NetUtils.DefaultCookieContainer.Add(c)
        Next
        Refresh_Item()
    End Sub

    Private Sub 删除过期Expired项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 删除过期Expired项ToolStripMenuItem.Click
        Utils.NetUtils.DefaultCookieContainer = New CookieContainer()
        Dim result = From lvi As ListViewItem In ListView1.Items Where lvi.SubItems(4).Text = "False" Select lvi
        For Each lvi As ListViewItem In result
            Dim c As New Cookie(lvi.SubItems(1).Text, lvi.SubItems(2).Text)
            c.Domain = lvi.SubItems(0).Text
            c.Expires = lvi.SubItems(3).Text
            c.HttpOnly = If(lvi.SubItems(5).Text = "True", True, False)
            c.Discard = If(lvi.SubItems(6).Text = "True", True, False)
            Utils.NetUtils.DefaultCookieContainer.Add(c)
        Next
        Refresh_Item()
    End Sub

    Private Sub 删除所有项ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 删除所有项ToolStripMenuItem.Click
        Utils.NetUtils.DefaultCookieContainer = New CookieContainer()
        Refresh_Item()
    End Sub
End Class