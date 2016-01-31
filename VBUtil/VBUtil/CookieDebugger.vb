Imports System.Net

Public Class CookieDebugger
    Private Const _RESET_TIME As Integer = 60
    Private _time As Integer = 0

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
End Class