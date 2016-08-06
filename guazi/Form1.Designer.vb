<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意:  以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.时间 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.信息 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.lblRemainHeartbeatTimeOutput = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.RefreshPlayerBag = New System.Windows.Forms.Button()
        Me.SendGift = New System.Windows.Forms.Button()
        Me.listPlayerBag = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Label5 = New System.Windows.Forms.Label()
        Me.remainHeartbeatTime = New System.Windows.Forms.ProgressBar()
        Me.lblSize = New System.Windows.Forms.Label()
        Me.OnlinePeople = New System.Windows.Forms.Label()
        Me.lblDownloadTime = New System.Windows.Forms.Label()
        Me.ReceiveSocket = New System.Windows.Forms.CheckBox()
        Me.lblSpeed = New System.Windows.Forms.Label()
        Me.StartRecord = New System.Windows.Forms.Button()
        Me.lblGuaziCount = New System.Windows.Forms.Label()
        Me.lblTimeOutput = New System.Windows.Forms.Label()
        Me.AutoGetItem = New System.Windows.Forms.CheckBox()
        Me.AutoJoiningAct = New System.Windows.Forms.CheckBox()
        Me.AutoSendItem = New System.Windows.Forms.CheckBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.remainTime = New System.Windows.Forms.ProgressBar()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.AutoGrab = New System.Windows.Forms.CheckBox()
        Me.AutoSign = New System.Windows.Forms.CheckBox()
        Me.AutoLiveOn = New System.Windows.Forms.CheckBox()
        Me.AutoStart = New System.Windows.Forms.CheckBox()
        Me.AutoShutdown = New System.Windows.Forms.CheckBox()
        Me.testButton = New System.Windows.Forms.LinkLabel()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.bRoomId = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblLogout = New System.Windows.Forms.LinkLabel()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.AutoStartSpecialEvent = New System.Windows.Forms.CheckBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.remainSpEvent = New System.Windows.Forms.ProgressBar()
        Me.lblSpEventTimeOutput = New System.Windows.Forms.Label()
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer3 = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.RefreshUserInfo = New System.Windows.Forms.Button()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.lblLv = New System.Windows.Forms.Label()
        Me.lblVip = New System.Windows.Forms.Label()
        Me.lblUserExp = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.lblSilver = New System.Windows.Forms.Label()
        Me.lblGold = New System.Windows.Forms.Label()
        Me.pbarUserExp = New System.Windows.Forms.ProgressBar()
        Me.lblUname = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.commentMsg = New System.Windows.Forms.TextBox()
        Me.commentColor = New System.Windows.Forms.ColorDialog()
        Me.commentColorOutput = New System.Windows.Forms.Panel()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(507, 431)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBox1.Size = New System.Drawing.Size(500, 77)
        Me.TextBox1.TabIndex = 0
        Me.TextBox1.WordWrap = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(507, 11)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "调试输出"
        '
        'ListView1
        '
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.时间, Me.信息})
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Location = New System.Drawing.Point(507, 26)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(498, 399)
        Me.ListView1.TabIndex = 10
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        '时间
        '
        Me.时间.Text = "时间"
        Me.时间.Width = 77
        '
        '信息
        '
        Me.信息.Text = "信息"
        Me.信息.Width = 395
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.lblRemainHeartbeatTimeOutput)
        Me.GroupBox1.Controls.Add(Me.GroupBox2)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.remainHeartbeatTime)
        Me.GroupBox1.Controls.Add(Me.lblSize)
        Me.GroupBox1.Controls.Add(Me.OnlinePeople)
        Me.GroupBox1.Controls.Add(Me.lblDownloadTime)
        Me.GroupBox1.Controls.Add(Me.ReceiveSocket)
        Me.GroupBox1.Controls.Add(Me.lblSpeed)
        Me.GroupBox1.Controls.Add(Me.StartRecord)
        Me.GroupBox1.Controls.Add(Me.lblGuaziCount)
        Me.GroupBox1.Controls.Add(Me.lblTimeOutput)
        Me.GroupBox1.Controls.Add(Me.AutoGetItem)
        Me.GroupBox1.Controls.Add(Me.AutoJoiningAct)
        Me.GroupBox1.Controls.Add(Me.AutoSendItem)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.remainTime)
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Controls.Add(Me.AutoGrab)
        Me.GroupBox1.Controls.Add(Me.AutoSign)
        Me.GroupBox1.Controls.Add(Me.AutoLiveOn)
        Me.GroupBox1.Controls.Add(Me.AutoStart)
        Me.GroupBox1.Controls.Add(Me.AutoShutdown)
        Me.GroupBox1.Location = New System.Drawing.Point(4, 127)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(495, 356)
        Me.GroupBox1.TabIndex = 11
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "坑爹脚本_(:3」∠)_"
        '
        'lblRemainHeartbeatTimeOutput
        '
        Me.lblRemainHeartbeatTimeOutput.AutoSize = True
        Me.lblRemainHeartbeatTimeOutput.Location = New System.Drawing.Point(361, 314)
        Me.lblRemainHeartbeatTimeOutput.Name = "lblRemainHeartbeatTimeOutput"
        Me.lblRemainHeartbeatTimeOutput.Size = New System.Drawing.Size(0, 12)
        Me.lblRemainHeartbeatTimeOutput.TabIndex = 17
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.RefreshPlayerBag)
        Me.GroupBox2.Controls.Add(Me.SendGift)
        Me.GroupBox2.Controls.Add(Me.listPlayerBag)
        Me.GroupBox2.Location = New System.Drawing.Point(139, 162)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(350, 141)
        Me.GroupBox2.TabIndex = 16
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "道具清单:("
        '
        'RefreshPlayerBag
        '
        Me.RefreshPlayerBag.Location = New System.Drawing.Point(6, 111)
        Me.RefreshPlayerBag.Name = "RefreshPlayerBag"
        Me.RefreshPlayerBag.Size = New System.Drawing.Size(90, 25)
        Me.RefreshPlayerBag.TabIndex = 0
        Me.RefreshPlayerBag.Text = "刷新道具清单"
        Me.RefreshPlayerBag.UseVisualStyleBackColor = True
        '
        'SendGift
        '
        Me.SendGift.Location = New System.Drawing.Point(102, 111)
        Me.SendGift.Name = "SendGift"
        Me.SendGift.Size = New System.Drawing.Size(242, 25)
        Me.SendGift.TabIndex = 1
        Me.SendGift.Text = "赠送选定道具（或者双击道具清单也可以）"
        Me.SendGift.UseVisualStyleBackColor = True
        '
        'listPlayerBag
        '
        Me.listPlayerBag.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.listPlayerBag.FullRowSelect = True
        Me.listPlayerBag.Location = New System.Drawing.Point(6, 20)
        Me.listPlayerBag.Name = "listPlayerBag"
        Me.listPlayerBag.Size = New System.Drawing.Size(338, 85)
        Me.listPlayerBag.TabIndex = 0
        Me.listPlayerBag.UseCompatibleStateImageBehavior = False
        Me.listPlayerBag.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "道具名称"
        Me.ColumnHeader1.Width = 89
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "道具数量"
        Me.ColumnHeader2.Width = 70
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "道具有效期"
        Me.ColumnHeader3.Width = 95
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(106, 314)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(65, 12)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "领取进度条"
        '
        'remainHeartbeatTime
        '
        Me.remainHeartbeatTime.Location = New System.Drawing.Point(177, 316)
        Me.remainHeartbeatTime.Name = "remainHeartbeatTime"
        Me.remainHeartbeatTime.Size = New System.Drawing.Size(178, 10)
        Me.remainHeartbeatTime.TabIndex = 8
        '
        'lblSize
        '
        Me.lblSize.AutoSize = True
        Me.lblSize.Location = New System.Drawing.Point(370, 111)
        Me.lblSize.Name = "lblSize"
        Me.lblSize.Size = New System.Drawing.Size(0, 12)
        Me.lblSize.TabIndex = 15
        '
        'OnlinePeople
        '
        Me.OnlinePeople.AutoSize = True
        Me.OnlinePeople.Location = New System.Drawing.Point(186, 141)
        Me.OnlinePeople.Name = "OnlinePeople"
        Me.OnlinePeople.Size = New System.Drawing.Size(59, 12)
        Me.OnlinePeople.TabIndex = 1
        Me.OnlinePeople.Text = "在线人数:"
        '
        'lblDownloadTime
        '
        Me.lblDownloadTime.AutoSize = True
        Me.lblDownloadTime.Location = New System.Drawing.Point(245, 111)
        Me.lblDownloadTime.Name = "lblDownloadTime"
        Me.lblDownloadTime.Size = New System.Drawing.Size(0, 12)
        Me.lblDownloadTime.TabIndex = 14
        '
        'ReceiveSocket
        '
        Me.ReceiveSocket.AutoSize = True
        Me.ReceiveSocket.Location = New System.Drawing.Point(13, 140)
        Me.ReceiveSocket.Name = "ReceiveSocket"
        Me.ReceiveSocket.Size = New System.Drawing.Size(120, 16)
        Me.ReceiveSocket.TabIndex = 0
        Me.ReceiveSocket.Text = "接收直播弹幕信息"
        Me.ReceiveSocket.UseVisualStyleBackColor = True
        '
        'lblSpeed
        '
        Me.lblSpeed.AutoSize = True
        Me.lblSpeed.Location = New System.Drawing.Point(106, 111)
        Me.lblSpeed.Name = "lblSpeed"
        Me.lblSpeed.Size = New System.Drawing.Size(0, 12)
        Me.lblSpeed.TabIndex = 13
        '
        'StartRecord
        '
        Me.StartRecord.Location = New System.Drawing.Point(13, 100)
        Me.StartRecord.Name = "StartRecord"
        Me.StartRecord.Size = New System.Drawing.Size(87, 34)
        Me.StartRecord.TabIndex = 12
        Me.StartRecord.Text = "点击开始录播"
        Me.StartRecord.UseVisualStyleBackColor = True
        '
        'lblGuaziCount
        '
        Me.lblGuaziCount.AutoSize = True
        Me.lblGuaziCount.Location = New System.Drawing.Point(128, 86)
        Me.lblGuaziCount.Name = "lblGuaziCount"
        Me.lblGuaziCount.Size = New System.Drawing.Size(107, 12)
        Me.lblGuaziCount.TabIndex = 11
        Me.lblGuaziCount.Text = "下次可领取:? 瓜子"
        '
        'lblTimeOutput
        '
        Me.lblTimeOutput.AutoSize = True
        Me.lblTimeOutput.Location = New System.Drawing.Point(377, 65)
        Me.lblTimeOutput.Name = "lblTimeOutput"
        Me.lblTimeOutput.Size = New System.Drawing.Size(0, 12)
        Me.lblTimeOutput.TabIndex = 10
        '
        'AutoGetItem
        '
        Me.AutoGetItem.AutoSize = True
        Me.AutoGetItem.Location = New System.Drawing.Point(13, 162)
        Me.AutoGetItem.Name = "AutoGetItem"
        Me.AutoGetItem.Size = New System.Drawing.Size(120, 16)
        Me.AutoGetItem.TabIndex = 5
        Me.AutoGetItem.Text = "自动领取每日道具"
        Me.AutoGetItem.UseVisualStyleBackColor = True
        '
        'AutoJoiningAct
        '
        Me.AutoJoiningAct.AutoSize = True
        Me.AutoJoiningAct.Location = New System.Drawing.Point(13, 208)
        Me.AutoJoiningAct.Name = "AutoJoiningAct"
        Me.AutoJoiningAct.Size = New System.Drawing.Size(120, 16)
        Me.AutoJoiningAct.TabIndex = 5
        Me.AutoJoiningAct.Text = "自动参加抽奖活动"
        Me.AutoJoiningAct.UseVisualStyleBackColor = True
        '
        'AutoSendItem
        '
        Me.AutoSendItem.AutoSize = True
        Me.AutoSendItem.Location = New System.Drawing.Point(13, 186)
        Me.AutoSendItem.Name = "AutoSendItem"
        Me.AutoSendItem.Size = New System.Drawing.Size(120, 16)
        Me.AutoSendItem.TabIndex = 5
        Me.AutoSendItem.Text = "自动送出全部道具"
        Me.AutoSendItem.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(115, 65)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 12)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "领取进度条"
        '
        'remainTime
        '
        Me.remainTime.Location = New System.Drawing.Point(186, 67)
        Me.remainTime.Name = "remainTime"
        Me.remainTime.Size = New System.Drawing.Size(178, 10)
        Me.remainTime.TabIndex = 8
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(379, 80)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(98, 25)
        Me.Button2.TabIndex = 6
        Me.Button2.Text = "取消关机"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'AutoGrab
        '
        Me.AutoGrab.AutoSize = True
        Me.AutoGrab.Location = New System.Drawing.Point(13, 64)
        Me.AutoGrab.Name = "AutoGrab"
        Me.AutoGrab.Size = New System.Drawing.Size(96, 16)
        Me.AutoGrab.TabIndex = 5
        Me.AutoGrab.Text = "自动领取瓜子"
        Me.AutoGrab.UseVisualStyleBackColor = True
        '
        'AutoSign
        '
        Me.AutoSign.AutoSize = True
        Me.AutoSign.Location = New System.Drawing.Point(13, 42)
        Me.AutoSign.Name = "AutoSign"
        Me.AutoSign.Size = New System.Drawing.Size(72, 16)
        Me.AutoSign.TabIndex = 5
        Me.AutoSign.Text = "自动签到"
        Me.AutoSign.UseVisualStyleBackColor = True
        '
        'AutoLiveOn
        '
        Me.AutoLiveOn.AutoSize = True
        Me.AutoLiveOn.Location = New System.Drawing.Point(13, 313)
        Me.AutoLiveOn.Name = "AutoLiveOn"
        Me.AutoLiveOn.Size = New System.Drawing.Size(78, 16)
        Me.AutoLiveOn.TabIndex = 5
        Me.AutoLiveOn.Text = "挂机领EXP"
        Me.AutoLiveOn.UseVisualStyleBackColor = True
        '
        'AutoStart
        '
        Me.AutoStart.AutoSize = True
        Me.AutoStart.Location = New System.Drawing.Point(13, 20)
        Me.AutoStart.Name = "AutoStart"
        Me.AutoStart.Size = New System.Drawing.Size(84, 16)
        Me.AutoStart.TabIndex = 5
        Me.AutoStart.Text = "开机自启动"
        Me.AutoStart.UseVisualStyleBackColor = True
        '
        'AutoShutdown
        '
        Me.AutoShutdown.AutoSize = True
        Me.AutoShutdown.Location = New System.Drawing.Point(253, 85)
        Me.AutoShutdown.Name = "AutoShutdown"
        Me.AutoShutdown.Size = New System.Drawing.Size(120, 16)
        Me.AutoShutdown.TabIndex = 3
        Me.AutoShutdown.Text = "领取完后自动关机"
        Me.AutoShutdown.UseVisualStyleBackColor = True
        '
        'testButton
        '
        Me.testButton.AutoSize = True
        Me.testButton.Location = New System.Drawing.Point(736, 11)
        Me.testButton.Name = "testButton"
        Me.testButton.Size = New System.Drawing.Size(53, 12)
        Me.testButton.TabIndex = 12
        Me.testButton.TabStop = True
        Me.testButton.Text = "测试按钮"
        '
        'Timer1
        '
        '
        'bRoomId
        '
        Me.bRoomId.Location = New System.Drawing.Point(78, 2)
        Me.bRoomId.Name = "bRoomId"
        Me.bRoomId.Size = New System.Drawing.Size(68, 21)
        Me.bRoomId.TabIndex = 15
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(7, 5)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(65, 12)
        Me.Label2.TabIndex = 14
        Me.Label2.Text = "输入房间ID"
        '
        'lblLogout
        '
        Me.lblLogout.AutoSize = True
        Me.lblLogout.Location = New System.Drawing.Point(171, 5)
        Me.lblLogout.Name = "lblLogout"
        Me.lblLogout.Size = New System.Drawing.Size(77, 12)
        Me.lblLogout.TabIndex = 17
        Me.lblLogout.TabStop = True
        Me.lblLogout.Text = "点击退出登录"
        '
        'SaveFileDialog1
        '
        Me.SaveFileDialog1.DefaultExt = "flv"
        Me.SaveFileDialog1.Filter = "FLV视频文件|*.flv|所有文件|*.*"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.AutoStartSpecialEvent)
        Me.GroupBox3.Controls.Add(Me.Label4)
        Me.GroupBox3.Controls.Add(Me.remainSpEvent)
        Me.GroupBox3.Controls.Add(Me.lblSpEventTimeOutput)
        Me.GroupBox3.Location = New System.Drawing.Point(4, 489)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(495, 58)
        Me.GroupBox3.TabIndex = 18
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "限时活动"
        '
        'AutoStartSpecialEvent
        '
        Me.AutoStartSpecialEvent.AutoSize = True
        Me.AutoStartSpecialEvent.Location = New System.Drawing.Point(13, 20)
        Me.AutoStartSpecialEvent.Name = "AutoStartSpecialEvent"
        Me.AutoStartSpecialEvent.Size = New System.Drawing.Size(132, 16)
        Me.AutoStartSpecialEvent.TabIndex = 0
        Me.AutoStartSpecialEvent.Text = "屠龙宝刀，点击就送"
        Me.AutoStartSpecialEvent.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(151, 21)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(65, 12)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "领取进度条"
        '
        'remainSpEvent
        '
        Me.remainSpEvent.Location = New System.Drawing.Point(222, 23)
        Me.remainSpEvent.Name = "remainSpEvent"
        Me.remainSpEvent.Size = New System.Drawing.Size(178, 10)
        Me.remainSpEvent.TabIndex = 8
        '
        'lblSpEventTimeOutput
        '
        Me.lblSpEventTimeOutput.AutoSize = True
        Me.lblSpEventTimeOutput.Location = New System.Drawing.Point(406, 21)
        Me.lblSpEventTimeOutput.Name = "lblSpEventTimeOutput"
        Me.lblSpEventTimeOutput.Size = New System.Drawing.Size(0, 12)
        Me.lblSpEventTimeOutput.TabIndex = 10
        '
        'Timer2
        '
        '
        'Timer3
        '
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.RefreshUserInfo)
        Me.GroupBox4.Controls.Add(Me.Label13)
        Me.GroupBox4.Controls.Add(Me.lblLv)
        Me.GroupBox4.Controls.Add(Me.lblVip)
        Me.GroupBox4.Controls.Add(Me.lblUserExp)
        Me.GroupBox4.Controls.Add(Me.Label9)
        Me.GroupBox4.Controls.Add(Me.lblSilver)
        Me.GroupBox4.Controls.Add(Me.lblGold)
        Me.GroupBox4.Controls.Add(Me.pbarUserExp)
        Me.GroupBox4.Controls.Add(Me.lblUname)
        Me.GroupBox4.Controls.Add(Me.Label8)
        Me.GroupBox4.Controls.Add(Me.Label7)
        Me.GroupBox4.Controls.Add(Me.Label6)
        Me.GroupBox4.Location = New System.Drawing.Point(4, 26)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(497, 95)
        Me.GroupBox4.TabIndex = 19
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "用户信息"
        '
        'RefreshUserInfo
        '
        Me.RefreshUserInfo.Location = New System.Drawing.Point(399, 64)
        Me.RefreshUserInfo.Name = "RefreshUserInfo"
        Me.RefreshUserInfo.Size = New System.Drawing.Size(90, 25)
        Me.RefreshUserInfo.TabIndex = 0
        Me.RefreshUserInfo.Text = "刷新用户信息"
        Me.RefreshUserInfo.UseVisualStyleBackColor = True
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(395, 39)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(41, 12)
        Me.Label13.TabIndex = 19
        Me.Label13.Text = "等级："
        '
        'lblLv
        '
        Me.lblLv.AutoSize = True
        Me.lblLv.Location = New System.Drawing.Point(442, 39)
        Me.lblLv.Name = "lblLv"
        Me.lblLv.Size = New System.Drawing.Size(0, 12)
        Me.lblLv.TabIndex = 19
        '
        'lblVip
        '
        Me.lblVip.AutoSize = True
        Me.lblVip.Location = New System.Drawing.Point(8, 61)
        Me.lblVip.Name = "lblVip"
        Me.lblVip.Size = New System.Drawing.Size(0, 12)
        Me.lblVip.TabIndex = 18
        '
        'lblUserExp
        '
        Me.lblUserExp.AutoSize = True
        Me.lblUserExp.Location = New System.Drawing.Point(284, 39)
        Me.lblUserExp.Name = "lblUserExp"
        Me.lblUserExp.Size = New System.Drawing.Size(0, 12)
        Me.lblUserExp.TabIndex = 17
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(8, 39)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(65, 12)
        Me.Label9.TabIndex = 2
        Me.Label9.Text = "用户经验："
        '
        'lblSilver
        '
        Me.lblSilver.AutoSize = True
        Me.lblSilver.Location = New System.Drawing.Point(377, 17)
        Me.lblSilver.Name = "lblSilver"
        Me.lblSilver.Size = New System.Drawing.Size(0, 12)
        Me.lblSilver.TabIndex = 1
        '
        'lblGold
        '
        Me.lblGold.AutoSize = True
        Me.lblGold.Location = New System.Drawing.Point(234, 17)
        Me.lblGold.Name = "lblGold"
        Me.lblGold.Size = New System.Drawing.Size(0, 12)
        Me.lblGold.TabIndex = 1
        '
        'pbarUserExp
        '
        Me.pbarUserExp.Location = New System.Drawing.Point(82, 41)
        Me.pbarUserExp.Name = "pbarUserExp"
        Me.pbarUserExp.Size = New System.Drawing.Size(196, 10)
        Me.pbarUserExp.TabIndex = 8
        '
        'lblUname
        '
        Me.lblUname.AutoSize = True
        Me.lblUname.Location = New System.Drawing.Point(80, 17)
        Me.lblUname.Name = "lblUname"
        Me.lblUname.Size = New System.Drawing.Size(0, 12)
        Me.lblUname.TabIndex = 0
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(324, 17)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(53, 12)
        Me.Label8.TabIndex = 0
        Me.Label8.Text = "银瓜子："
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(175, 17)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(53, 12)
        Me.Label7.TabIndex = 0
        Me.Label7.Text = "金瓜子："
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(8, 17)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(65, 12)
        Me.Label6.TabIndex = 0
        Me.Label6.Text = "用户名称："
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(505, 522)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(59, 12)
        Me.Label11.TabIndex = 20
        Me.Label11.Text = "发送弹幕:"
        '
        'commentMsg
        '
        Me.commentMsg.Location = New System.Drawing.Point(570, 519)
        Me.commentMsg.Name = "commentMsg"
        Me.commentMsg.Size = New System.Drawing.Size(352, 21)
        Me.commentMsg.TabIndex = 21
        '
        'commentColor
        '
        Me.commentColor.AnyColor = True
        Me.commentColor.Color = System.Drawing.Color.White
        '
        'commentColorOutput
        '
        Me.commentColorOutput.Location = New System.Drawing.Point(962, 521)
        Me.commentColorOutput.Name = "commentColorOutput"
        Me.commentColorOutput.Size = New System.Drawing.Size(17, 16)
        Me.commentColorOutput.TabIndex = 22
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(921, 522)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(35, 12)
        Me.Label12.TabIndex = 20
        Me.Label12.Text = "颜色:"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1015, 559)
        Me.Controls.Add(Me.commentColorOutput)
        Me.Controls.Add(Me.commentMsg)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.lblLogout)
        Me.Controls.Add(Me.bRoomId)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.testButton)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "瓜子搜刮机 ~ 终极绝杀版 ~"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents 时间 As System.Windows.Forms.ColumnHeader
    Friend WithEvents 信息 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents testButton As System.Windows.Forms.LinkLabel
    Friend WithEvents AutoShutdown As System.Windows.Forms.CheckBox
    Friend WithEvents AutoStart As System.Windows.Forms.CheckBox
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents lblTimeOutput As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents remainTime As System.Windows.Forms.ProgressBar
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents lblGuaziCount As System.Windows.Forms.Label
    Friend WithEvents AutoGrab As System.Windows.Forms.CheckBox
    Friend WithEvents AutoSendItem As System.Windows.Forms.CheckBox
    Friend WithEvents AutoGetItem As System.Windows.Forms.CheckBox
    Friend WithEvents AutoSign As System.Windows.Forms.CheckBox
    Friend WithEvents bRoomId As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblLogout As System.Windows.Forms.LinkLabel
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents ReceiveSocket As System.Windows.Forms.CheckBox
    Friend WithEvents OnlinePeople As System.Windows.Forms.Label
    Friend WithEvents lblDownloadTime As Label
    Friend WithEvents lblSpeed As Label
    Friend WithEvents StartRecord As Button
    Friend WithEvents lblSize As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents RefreshPlayerBag As Button
    Friend WithEvents SendGift As Button
    Friend WithEvents listPlayerBag As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents AutoLiveOn As CheckBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents AutoStartSpecialEvent As CheckBox
    Friend WithEvents lblRemainHeartbeatTimeOutput As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents remainHeartbeatTime As ProgressBar
    Friend WithEvents Label4 As Label
    Friend WithEvents remainSpEvent As ProgressBar
    Friend WithEvents lblSpEventTimeOutput As Label
    Friend WithEvents Timer2 As Timer
    Friend WithEvents Timer3 As Timer
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents lblUname As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents RefreshUserInfo As Button
    Friend WithEvents lblLv As Label
    Friend WithEvents lblVip As Label
    Friend WithEvents lblUserExp As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents lblSilver As Label
    Friend WithEvents lblGold As Label
    Friend WithEvents pbarUserExp As ProgressBar
    Friend WithEvents Label8 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents commentMsg As TextBox
    Friend WithEvents commentColor As ColorDialog
    Friend WithEvents commentColorOutput As Panel
    Friend WithEvents Label12 As Label
    Friend WithEvents Label13 As Label
    Friend WithEvents AutoJoiningAct As CheckBox
End Class
