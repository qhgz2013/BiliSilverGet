Imports System.Threading
Imports System.Security.Cryptography
Imports System.Text
Imports VBUtil.Utils
Imports VBUtil.Utils.NetUtils
Imports Newtonsoft.Json.Linq

Public Class Form1
#Region "调试输出控制部分[多线程调度安全]"
    'multithread safe debug output
    Friend Sub DebugOutput(ByVal s As String) Handles gz.DebugOutput
        Dim del As New _debugOutputSafe(AddressOf _debugOutput)
        Try
            Me.Invoke(del, s)
        Catch ex As Exception
        End Try
    End Sub
    Friend Delegate Sub _debugOutputSafe(ByVal s As String)
    Friend Sub _debugOutput(ByVal s As String)
        If ListView1.Items.Count > 500 Then
            ListView1.Items.RemoveAt(0)
        End If
        ListView1.Items.Add(Now.ToLongTimeString)
        ListView1.Items(ListView1.Items.Count - 1).SubItems.Add(s)

        '修改滚动的条件，不然慌得眼瞎
        If ListView1.SelectedItems.Count > 0 Then
            Dim sel_item As ListViewItem = ListView1.SelectedItems(0)
            Dim index As Integer = ListView1.Items.IndexOf(sel_item)

            If index + 2 = ListView1.Items.Count Then
                '在选择最后一个时开启滚动
                ListView1.SelectedItems.Clear()
                ListView1.Items(ListView1.Items.Count - 1).Selected = True
            End If
        Else
            '或者在无选择时开启
            ListView1.SelectedItems.Clear()
            ListView1.Items(ListView1.Items.Count - 1).Selected = True
        End If

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
    Private Function RandomText(ByVal textList As String()) As String
        Return textList(rand.Next(textList.Length))
    End Function

    Private _initflag As Boolean '窗体初始化标志，false代表未完成初始化，true代表窗体已初始化（Form.Load事件回调函数完成）
    Private _last_sign_date As Date '最后成功签到的日期
    Private _last_finished_grabbing_date As Date '最后完成搜刮瓜子的日期
    Private _last_get_gift_date As Date '最后成功领取每日礼包的日期
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
        AutoStartSpecialEvent.Checked = obj.Value(Of Boolean)("auto_start_sp_event")
        AutoLiveOn.Checked = obj.Value(Of Boolean)("auto_get_exp")
        AutoJoiningAct.Checked = obj.Value(Of Boolean)("auto_join_activity")
        ReceiveSocket.Checked = obj.Value(Of Boolean)("receive_room_msg")
        _last_sign_date = FromUnixTimeStamp(obj.Value(Of Double)("last_sign_date"))
        _last_finished_grabbing_date = FromUnixTimeStamp(obj.Value(Of Double)("last_finish_grabbing_date"))
        _last_get_gift_date = FromUnixTimeStamp(obj.Value(Of Double)("last_get_gift_date"))
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
        obj.Add("auto_start_sp_event", AutoStartSpecialEvent.Checked)
        obj.Add("auto_get_exp", AutoLiveOn.Checked)
        obj.Add("auto_join_activity", AutoJoiningAct.Checked)
        obj.Add("receive_room_msg", ReceiveSocket.Checked)
        obj.Add("last_sign_date", ToUnixTimestamp(_last_sign_date))
        obj.Add("last_finish_grabbing_date", ToUnixTimestamp(_last_finished_grabbing_date))
        obj.Add("last_get_gift_date", ToUnixTimestamp(_last_get_gift_date))
        Dim str As String = Newtonsoft.Json.JsonConvert.SerializeObject(obj)
        Dim sw As New IO.StreamWriter(Application.StartupPath & "\setting.json")
        sw.Write(str)
        sw.Close()
    End Sub
#Region "脚本1 - 自动领取瓜子&签到&……"
    Private WithEvents gz As guazi = Nothing
    Private _shutdown_timing As Boolean = False
    '完成搜刮，自动关机的模块
    Private Sub FinishGrabbing() Handles gz.FinishedGrabbing
        If AutoShutdown.Checked Then
            '执行关机 每天第一次完成的时候关机，第二次点击的时候不会
            If _last_finished_grabbing_date.Date <> Now.Date Then
                _shutdown_timing = True
                SaveGuaziConfig()
                System.Diagnostics.Process.Start("shutdown", " -s -t 180")
            End If
        End If
        _last_finished_grabbing_date = Now
        '修改进度条
        Me.Invoke(New SafeSub(Sub()
                                  lblTimeOutput.Text = ""
                                  remainTime.Value = 0
                                  remainTime.Maximum = 0
                                  lblGuaziCount.Text = ""
                              End Sub))
    End Sub
    '自动关机的选项改变，若没有勾选但是已经倒计关机时执行取消关机的指令
    Private Sub AutoShutdown_CheckedChanged(sender As Object, e As EventArgs) Handles AutoShutdown.CheckedChanged
        If Not _initflag Then Return
        If _shutdown_timing AndAlso Not AutoShutdown.Checked Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
        SaveGuaziConfig()
    End Sub
    '取消关机的按钮
    Private Sub CancelShutdown(sender As Object, e As EventArgs) Handles Button2.Click
        If _shutdown_timing Then
            System.Diagnostics.Process.Start("shutdown", " -a")
            _shutdown_timing = False
        End If
    End Sub

    '窗体关闭/打开时的自动初始化/保存
    Friend Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Utils.NetUtils.SaveCookie(Application.StartupPath & "\cookie.dat")
        SaveGuaziConfig()
        gz.AsyncEndGrabbingSilver()
        gz.AsyncStopDownloadStream()
        gz.AsyncStopLiveOn()
        gz.AsyncStopReceiveComment()
        gz.AsyncStopTimeLimitedEvent()
        gz.AsyncStopListeningAct()
        Me.Hide()
        'End
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _initflag = False
        '读取cookie
        Utils.NetUtils.LoadCookie(Application.StartupPath & "\cookie.dat")
        'Trace配置
        If IO.File.Exists("trace.log") Then IO.File.Delete("trace.log")
        Trace.Listeners.Add(New TextWriterTraceListener("trace.log"))
        Trace.AutoFlush = True
        '登陆检测
        If Not CheckLogin() Then
            frmLogin.ShowDialog()
        End If

        _expireTime = Date.MinValue
        '读取设置
        LoadGuaziConfig()
        gz = New guazi()
        _initflag = True

        gz.RoomURL = bRoomId.Text
        goFuck(sender, e)

        '弹幕颜色
        commentColorOutput.BackColor = commentColor.Color

        commentColor.CustomColors = New Integer() {&HFFFFFF, &H6868FF, &HFFCC66, &HFCFF00, &HFF7E, &H4FEDFF, &H98FF, &HFFFFFF,
        &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF, &HFFFFFF}

    End Sub

    'cookie debugger
    Private Sub testButton_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles testButton.LinkClicked
        Dim n As New VBUtil.CookieDebugger()
        n.Show(Me)
    End Sub


    '倒计时模块
    Private _expireTime As Date
    Private Delegate Sub SafeSub()
    Private Sub OnTimeUpdate(ByVal exptime As Date, ByVal silver As Integer) Handles gz.RefreshClock
        _expireTime = exptime
        Me.Invoke(New SafeSub(Sub()
                                  Timer1.Enabled = True
                                  lblGuaziCount.Text = "下次可领取: " & silver & "瓜子"
                                  remainTime.Maximum = CInt((_expireTime - Now).TotalSeconds)
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
            If String.IsNullOrEmpty(bRoomId.Text) Then
                MessageBox.Show(RandomText(New String() {
                    "你输入的房间号为空", "阿卡林！我看不到房间号", "没有房间号我要死了", "你确定你按回车键的时候没有进行过人生的思考？", "你倒是给我先输点房间号进去再按下回车啊喂（╯－＿－）╯╧╧"
                                           }), "_(:3」∠)_", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                Dim roomid As Integer
                If Integer.TryParse(bRoomId.Text, roomid) Then
                    If roomid > 0 Then
                        Try
                            gz.RoomURL = roomid
                            goFuck(sender, e)
                            SaveGuaziConfig()
                        Catch ex As Exception
                            MessageBox.Show("出错啦，重试一下吧:-)")
                        End Try
                    Else
                        MessageBox.Show(RandomText(New String() {
                            "你坑我！这根本不能当房间号……", "你随便编个数字好歹也要合理合法有依据吧", "房间号错误：小于等于0", "这……这房间号有毒！根本不可能进的啊"
                                                   }), "_(:3」∠)_", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                Else
                    MessageBox.Show(RandomText(New String() {
                        "你输入的房间号有问题啊亲", "原谅我的智商，我没看懂你的房间号输入的是什么", "房间号好歹也是个数字啊额...", "难道是我码程序的方式不对？这房间号不科学啊"
                                               }), "_(:3」∠)_", MessageBoxButtons.OK, MessageBoxIcon.Error)

                End If
            End If
            e.Handled = True
        End If
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

    '退出登录
    Private Sub lblLogout_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lblLogout.LinkClicked
        If Not LogOut() Then
            My.Forms.frmLogin.ShowDialog()
        End If
    End Sub

    '自动签到
    Private Sub AutoSign_CheckedChanged(sender As Object, e As EventArgs) Handles AutoSign.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoSign.Checked AndAlso _last_sign_date.Date <> Now.Date Then
            gz.AsyncDoSign()
        End If
    End Sub

    Private Sub AutoGetItem_CheckedChanged(sender As Object, e As EventArgs) Handles AutoGetItem.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoGetItem.Checked AndAlso _last_get_gift_date.Date <> Now.Date Then
            gz.AsyncGetDailyGift()
        End If
    End Sub

    Private Sub AutoSendItem_CheckedChanged(sender As Object, e As EventArgs) Handles AutoSendItem.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoSendItem.Checked Then gz.AsyncSendDailyGift()
    End Sub

    Private Sub AutoGrab_CheckedChanged(sender As Object, e As EventArgs) Handles AutoGrab.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoGrab.Checked Then gz.AsyncStartGrabbingSilver()
    End Sub

    Private Sub goFuck(sender As Object, e As EventArgs)
        RefreshPlayerBag.PerformClick()
        RefreshUserInfo.PerformClick()

        AutoSign_CheckedChanged(sender, e)
        AutoGetItem_CheckedChanged(sender, e)
        AutoSendItem_CheckedChanged(sender, e)
        AutoGrab_CheckedChanged(sender, e)
        ReceiveSocket_CheckedChanged(sender, e)

        AutoStartSpecialEvent_CheckedChanged(sender, e)
        AutoLiveOn_CheckedChanged(sender, e)
        AutoJoiningAct_CheckedChanged(sender, e)
    End Sub

#End Region
#Region "脚本2 - 录播"

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
#Region "脚本3 - 接收弹幕"

    Private Sub ReceiveSocket_CheckedChanged(sender As Object, e As EventArgs) Handles ReceiveSocket.CheckedChanged
        If Not _initflag Then Return
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
    Private Sub OnReceivingSystemMsg(ByVal msg As String, ByVal refer_url As String, ByVal roomid As UInteger) Handles gz.ReceivedSystemMsg
        DebugOutput("收到系统消息:" & msg)
    End Sub
    Private Sub OnReceivingLiveStart() Handles gz.StatusLive
        DebugOutput("<直播间消息:直播已开始>")
    End Sub
    Private Sub OnReceivingLiveStop() Handles gz.StatusPreparing
        DebugOutput("<直播间消息:直播已结束>")
    End Sub
    Private Sub OnReceivingBlockMsg(ByVal uid As Integer, ByVal uname As String) Handles gz.RoomBlockMsg
        DebugOutput("<直播间消息:已屏蔽用户 " & uname & " 的弹幕>")
    End Sub
#End Region
#Region "脚本1 - 道具栏模块"
    Private _item_list As JArray = Nothing
    '刷新背包
    Private Sub RefreshPlayerBag_Click(sender As Object, e As EventArgs) Handles RefreshPlayerBag.Click
        If Not _initflag Then Return
        ThreadPool.QueueUserWorkItem(
            Sub()
                Try
                    Dim data As JObject = gz.SyncGetPlayerBag
                    _item_list = data.Value(Of JArray)("data")
                    If _item_list Is Nothing Then Return
                    Me.Invoke(Sub()
                                  listPlayerBag.Items.Clear()
                              End Sub)
                    For Each item As JObject In _item_list
                        Dim lvi As New ListViewItem(item.Value(Of String)("gift_name"))
                        lvi.SubItems.Add(item.Value(Of Integer)("gift_num"))
                        lvi.SubItems.Add(item.Value(Of String)("expireat"))
                        Me.Invoke(Sub()
                                      listPlayerBag.Items.Add(lvi)
                                  End Sub)
                    Next
                Catch ex As Exception
                    DebugOutput("[ERR]" & ex.ToString)
                    Debug.Print(ex.ToString)
                End Try
            End Sub)
    End Sub
    '送出道具
    Private Sub SendGift_Click(sender As Object, e As EventArgs) Handles SendGift.Click
        If Not _initflag Then Return
        If listPlayerBag.SelectedItems.Count <= 0 Then Return

        Dim index As Integer = listPlayerBag.Items.IndexOf(listPlayerBag.SelectedItems(0))
        If index < 0 Then Return

        If _item_list Is Nothing Then Return

        Dim item_data As JObject = _item_list(index)
        Dim gift_name As String = item_data.Value(Of String)("gift_name")
        Dim gift_id As Integer = item_data.Value(Of Integer)("gift_id")
        Dim bag_id As Integer = item_data.Value(Of Integer)("id")
        Dim gift_num As Integer = item_data.Value(Of Integer)("gift_num")

        '输入要赠送的数量
        Dim input_str As String = InputBox("输入要赠送的" & gift_name & " 的数量", "_(:3」∠)_", gift_num)
        Dim input_num As Integer = 0
        If Integer.TryParse(input_str, input_num) Then
            If input_num > 0 AndAlso input_num <= gift_num Then
                ThreadPool.QueueUserWorkItem(
                    Sub()
                        gz.SyncSendGift(gift_id, bag_id, input_num)
                    End Sub)
            Else
                MessageBox.Show(RandomText(New String() {
                    "你的输入的数字有点过分啦(～￣▽￣)～*", "你要送这种数量大丈夫？", "数字输入范围出错", "其实，我也妄想着，送出-1个道具是不是等于获得一个，也妄想着自己能一直送，直到显示-999999个道具……"
                                           }), "_(:3」∠)_", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        ElseIf Not String.IsNullOrEmpty(input_str) Then
            MessageBox.Show(RandomText(New String() {
                "你输入的数量有问题啊亲", "原谅我的智商，我没看懂你输入的是什么", "数量好歹也是个数字啊额...", "难道是我码程序的方式不对？这数量不数学啊"
                                       }), "_(:3」∠)_", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

    End Sub
    Private Sub listPlayerBag_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles listPlayerBag.MouseDoubleClick
        SendGift.PerformClick()
    End Sub
#End Region
#Region "脚本1 - 新增的回调函数"
    '一堆不痛不痒的回调函数
    Private Sub onSucceededSign() Handles gz.DoSignSucceeded
        _last_sign_date = Now
    End Sub
    Private Sub onSucceededGettingDailyGift() Handles gz.GetDailyGiftFinished
        _last_get_gift_date = Now
    End Sub
    Private Sub onSucceededSendingGift() Handles gz.SendDailyGiftFinished

    End Sub

    Private Sub onUserInfoChanged() Handles gz.UserInfoChanged
        Me.Invoke(Sub()
                      RefreshUserInfo.PerformClick()
                  End Sub)
    End Sub
    Private Sub onUserBaggageChanged() Handles gz.UserBaggageChanged
        Me.Invoke(Sub()
                      RefreshPlayerBag.PerformClick()
                  End Sub)
    End Sub
#End Region
#Region "脚本4 - 自动挂经验"
    '自动挂经验的控件函数
    Private Sub AutoLiveOn_CheckedChanged(sender As Object, e As EventArgs) Handles AutoLiveOn.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoLiveOn.Checked Then
            gz.AsyncBeginLiveOn()
        Else
            gz.AsyncStopLiveOn()
            Timer2.Enabled = False
            remainHeartbeatTime.Value = 0
            remainHeartbeatTime.Maximum = 0
            lblRemainHeartbeatTimeOutput.Text = ""
        End If
    End Sub
    Private _exp_expire_time As Date

    Private Sub onLiveOnTimeChange(ByVal expire As Date) Handles gz.NextOnlineHeartBeatTime
        Me.Invoke(
            Sub()
                _exp_expire_time = expire
                remainHeartbeatTime.Maximum = CInt((_exp_expire_time - Now).TotalSeconds)
                remainHeartbeatTime.Value = 0
                Timer2.Enabled = True
            End Sub)
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If _exp_expire_time < Now Then
            Timer2.Enabled = False
        Else
            Dim ts As TimeSpan = _exp_expire_time - Now
            remainHeartbeatTime.Value = remainHeartbeatTime.Maximum - CInt(ts.TotalSeconds)
            lblRemainHeartbeatTimeOutput.Text = ts.Minutes & ":" & Format(ts.Seconds, "00")
        End If

    End Sub
#End Region
#Region "脚本SP - 活动领取模块"
    '特殊活动的控件函数
    Private Sub AutoStartSpecialEvent_CheckedChanged(sender As Object, e As EventArgs) Handles AutoStartSpecialEvent.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()

        If AutoStartSpecialEvent.Checked Then
            gz.AsyncBeginTimeLimitedEvent()
        Else
            gz.AsyncStopTimeLimitedEvent()
            Timer3.Enabled = False
            remainSpEvent.Value = 0
            remainSpEvent.Maximum = 0
            lblSpEventTimeOutput.Text = ""
        End If
    End Sub
    Private _sp_event_expire_time As Date
    Private Sub onSpEventTimeChange(ByVal expire As Date) Handles gz.NextEventGrabTime
        Me.Invoke(
            Sub()
                _sp_event_expire_time = expire
                remainSpEvent.Maximum = CInt((_sp_event_expire_time - Now).TotalSeconds)
                remainSpEvent.Value = 0
                Timer3.Enabled = True
            End Sub)
    End Sub

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        If _sp_event_expire_time < Now Then
            Timer3.Enabled = False
        Else
            Dim ts As TimeSpan = _sp_event_expire_time - Now
            remainSpEvent.Value = remainSpEvent.Maximum - CInt(ts.TotalSeconds)
            lblSpEventTimeOutput.Text = ts.Minutes & ":" & Format(ts.Seconds, "00")
        End If
    End Sub
#End Region

    Private Sub RefreshUserInfo_Click(sender As Object, e As EventArgs) Handles RefreshUserInfo.Click

        ThreadPool.QueueUserWorkItem(
        Sub()
            Try
                Dim json As JObject = gz.SyncGetUserInfo
                '{"code":"REPONSE_OK","msg":"ok","data":{"uname":"\u80a5\u80a5\u7684\u718a\u732b","face":"http:\/\/i0.hdslb.com\/bfs\/face\/c12bbf876292dd52ea9a691d78e86abd2b5dc493.jpg","silver":85224,"gold":0,"achieve":115,"vip":0,"svip":0,"user_level":17,"user_next_level":18,"user_intimacy":685704,"user_next_intimacy":1000000,"user_level_rank":345626,"billCoin":650}}
                json = json.Value(Of JObject)("data")
                Dim username As String = json.Value(Of String)("uname")
                Dim curlv As Integer = json.Value(Of Integer)("user_level")
                Dim nextlv As Integer = json.Value(Of Integer)("user_next_level")
                Dim curexp As ULong = json.Value(Of ULong)("user_intimacy")
                Dim nextexp As ULong = json.Value(Of ULong)("user_next_intimacy")
                'Dim rank As UInteger = json.Value(Of UInteger)("user_level_rank") '已移除，理由很简单：不充钱玩你【哔——】
                Dim silver As UInteger = json.Value(Of UInteger)("silver")
                Dim gold As UInteger = json.Value(Of UInteger)("gold")
                Dim vip As Integer = ((json.Value(Of Integer)("svip") And 1) << 1) Or (json.Value(Of Integer)("vip") And 1)

                'output
                Me.Invoke(
                    Sub()
                        lblUserExp.Text = curexp & "/" & nextexp
                        'lblRank.Text = rank
                        lblSilver.Text = silver
                        lblGold.Text = gold
                        lblVip.Text = If(vip And 2, "你是土豪老爷", If(vip And 1, "你是老爷", ""))
                        lblUname.Text = username
                        pbarUserExp.Maximum = nextexp
                        pbarUserExp.Value = curexp
                        lblLv.Text = curlv
                    End Sub)
            Catch ex As Exception

            End Try
        End Sub)

    End Sub

    Private Sub commentColorOutput_Click(sender As Object, e As EventArgs) Handles commentColorOutput.Click
        If commentColor.ShowDialog <> DialogResult.OK Then Return
        commentColorOutput.BackColor = commentColor.Color
    End Sub

    Private Sub commentMsg_KeyPress(sender As Object, e As KeyPressEventArgs) Handles commentMsg.KeyPress
        If e.KeyChar = vbCr Then
            e.Handled = True
            If String.IsNullOrEmpty(commentMsg.Text) Then Return
            Dim msg As String = commentMsg.Text
            commentMsg.Text = ""
            ThreadPool.QueueUserWorkItem(
                Sub()
                    gz.SyncSendComment(msg, commentColor.Color)
                End Sub)

        End If
    End Sub

    '参加活动
    Private Sub AutoJoiningAct_CheckedChanged(sender As Object, e As EventArgs) Handles AutoJoiningAct.CheckedChanged
        If Not _initflag Then Return
        SaveGuaziConfig()
        If AutoJoiningAct.Checked Then
            gz.AsyncListenAct()
        Else
            gz.AsyncStopListeningAct()
        End If
    End Sub

End Class
