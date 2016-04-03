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
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.时间 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.信息 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.goFuck = New System.Windows.Forms.Button()
        Me.lblGuaziCount = New System.Windows.Forms.Label()
        Me.lblTimeOutput = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.remainTime = New System.Windows.Forms.ProgressBar()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.AutoGrab = New System.Windows.Forms.CheckBox()
        Me.AutoSendItem = New System.Windows.Forms.CheckBox()
        Me.AutoGetItem = New System.Windows.Forms.CheckBox()
        Me.AutoSign = New System.Windows.Forms.CheckBox()
        Me.AutoStart = New System.Windows.Forms.CheckBox()
        Me.AutoShutdown = New System.Windows.Forms.CheckBox()
        Me.testButton = New System.Windows.Forms.LinkLabel()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.lblSpeed = New System.Windows.Forms.Label()
        Me.StartRecord = New System.Windows.Forms.Button()
        Me.bRoomId = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblLogout = New System.Windows.Forms.LinkLabel()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.lblDownloadTime = New System.Windows.Forms.Label()
        Me.lblSize = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(6, 383)
        Me.TextBox1.Multiline = True
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBox1.Size = New System.Drawing.Size(498, 77)
        Me.TextBox1.TabIndex = 0
        Me.TextBox1.WordWrap = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 236)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "调试输出"
        '
        'ListView1
        '
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.时间, Me.信息})
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Location = New System.Drawing.Point(6, 251)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(498, 126)
        Me.ListView1.TabIndex = 10
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        '时间
        '
        Me.时间.Text = "时间"
        Me.时间.Width = 91
        '
        '信息
        '
        Me.信息.Text = "信息"
        Me.信息.Width = 395
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.goFuck)
        Me.GroupBox1.Controls.Add(Me.lblGuaziCount)
        Me.GroupBox1.Controls.Add(Me.lblTimeOutput)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.remainTime)
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Controls.Add(Me.AutoGrab)
        Me.GroupBox1.Controls.Add(Me.AutoSendItem)
        Me.GroupBox1.Controls.Add(Me.AutoGetItem)
        Me.GroupBox1.Controls.Add(Me.AutoSign)
        Me.GroupBox1.Controls.Add(Me.AutoStart)
        Me.GroupBox1.Controls.Add(Me.AutoShutdown)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 26)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(495, 113)
        Me.GroupBox1.TabIndex = 11
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "坑爹脚本1:b站直播自动领瓜子"
        '
        'goFuck
        '
        Me.goFuck.Location = New System.Drawing.Point(9, 18)
        Me.goFuck.Name = "goFuck"
        Me.goFuck.Size = New System.Drawing.Size(480, 27)
        Me.goFuck.TabIndex = 17
        Me.goFuck.Text = "↓开启神秘之旅↓"
        Me.goFuck.UseVisualStyleBackColor = True
        '
        'lblGuaziCount
        '
        Me.lblGuaziCount.AutoSize = True
        Me.lblGuaziCount.Location = New System.Drawing.Point(353, 92)
        Me.lblGuaziCount.Name = "lblGuaziCount"
        Me.lblGuaziCount.Size = New System.Drawing.Size(107, 12)
        Me.lblGuaziCount.TabIndex = 11
        Me.lblGuaziCount.Text = "下次可领取:? 瓜子"
        '
        'lblTimeOutput
        '
        Me.lblTimeOutput.AutoSize = True
        Me.lblTimeOutput.Location = New System.Drawing.Point(282, 92)
        Me.lblTimeOutput.Name = "lblTimeOutput"
        Me.lblTimeOutput.Size = New System.Drawing.Size(0, 12)
        Me.lblTimeOutput.TabIndex = 10
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(11, 92)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(65, 12)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "领取进度条"
        '
        'remainTime
        '
        Me.remainTime.Location = New System.Drawing.Point(98, 92)
        Me.remainTime.Name = "remainTime"
        Me.remainTime.Size = New System.Drawing.Size(178, 12)
        Me.remainTime.TabIndex = 8
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(266, 46)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(98, 25)
        Me.Button2.TabIndex = 6
        Me.Button2.Text = "取消关机"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'AutoGrab
        '
        Me.AutoGrab.AutoSize = True
        Me.AutoGrab.Location = New System.Drawing.Point(319, 73)
        Me.AutoGrab.Name = "AutoGrab"
        Me.AutoGrab.Size = New System.Drawing.Size(96, 16)
        Me.AutoGrab.TabIndex = 5
        Me.AutoGrab.Text = "自动领取瓜子"
        Me.AutoGrab.UseVisualStyleBackColor = True
        '
        'AutoSendItem
        '
        Me.AutoSendItem.AutoSize = True
        Me.AutoSendItem.Location = New System.Drawing.Point(217, 73)
        Me.AutoSendItem.Name = "AutoSendItem"
        Me.AutoSendItem.Size = New System.Drawing.Size(96, 16)
        Me.AutoSendItem.TabIndex = 5
        Me.AutoSendItem.Text = "自动送出道具"
        Me.AutoSendItem.UseVisualStyleBackColor = True
        '
        'AutoGetItem
        '
        Me.AutoGetItem.AutoSize = True
        Me.AutoGetItem.Location = New System.Drawing.Point(91, 73)
        Me.AutoGetItem.Name = "AutoGetItem"
        Me.AutoGetItem.Size = New System.Drawing.Size(120, 16)
        Me.AutoGetItem.TabIndex = 5
        Me.AutoGetItem.Text = "自动领取每日道具"
        Me.AutoGetItem.UseVisualStyleBackColor = True
        '
        'AutoSign
        '
        Me.AutoSign.AutoSize = True
        Me.AutoSign.Location = New System.Drawing.Point(13, 73)
        Me.AutoSign.Name = "AutoSign"
        Me.AutoSign.Size = New System.Drawing.Size(72, 16)
        Me.AutoSign.TabIndex = 5
        Me.AutoSign.Text = "自动签到"
        Me.AutoSign.UseVisualStyleBackColor = True
        '
        'AutoStart
        '
        Me.AutoStart.AutoSize = True
        Me.AutoStart.Location = New System.Drawing.Point(13, 51)
        Me.AutoStart.Name = "AutoStart"
        Me.AutoStart.Size = New System.Drawing.Size(84, 16)
        Me.AutoStart.TabIndex = 5
        Me.AutoStart.Text = "开机自启动"
        Me.AutoStart.UseVisualStyleBackColor = True
        '
        'AutoShutdown
        '
        Me.AutoShutdown.AutoSize = True
        Me.AutoShutdown.Location = New System.Drawing.Point(140, 51)
        Me.AutoShutdown.Name = "AutoShutdown"
        Me.AutoShutdown.Size = New System.Drawing.Size(120, 16)
        Me.AutoShutdown.TabIndex = 3
        Me.AutoShutdown.Text = "领取完后自动关机"
        Me.AutoShutdown.UseVisualStyleBackColor = True
        '
        'testButton
        '
        Me.testButton.AutoSize = True
        Me.testButton.Location = New System.Drawing.Point(235, 236)
        Me.testButton.Name = "testButton"
        Me.testButton.Size = New System.Drawing.Size(53, 12)
        Me.testButton.TabIndex = 12
        Me.testButton.TabStop = True
        Me.testButton.Text = "测试按钮"
        '
        'Timer1
        '
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.lblSize)
        Me.GroupBox2.Controls.Add(Me.lblDownloadTime)
        Me.GroupBox2.Controls.Add(Me.lblSpeed)
        Me.GroupBox2.Controls.Add(Me.StartRecord)
        Me.GroupBox2.Location = New System.Drawing.Point(6, 145)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(494, 64)
        Me.GroupBox2.TabIndex = 13
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "坑爹脚本2:录播"
        '
        'lblSpeed
        '
        Me.lblSpeed.AutoSize = True
        Me.lblSpeed.Location = New System.Drawing.Point(103, 31)
        Me.lblSpeed.Name = "lblSpeed"
        Me.lblSpeed.Size = New System.Drawing.Size(0, 12)
        Me.lblSpeed.TabIndex = 1
        '
        'StartRecord
        '
        Me.StartRecord.Location = New System.Drawing.Point(10, 20)
        Me.StartRecord.Name = "StartRecord"
        Me.StartRecord.Size = New System.Drawing.Size(87, 34)
        Me.StartRecord.TabIndex = 0
        Me.StartRecord.Text = "点击开始录播"
        Me.StartRecord.UseVisualStyleBackColor = True
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
        'lblDownloadTime
        '
        Me.lblDownloadTime.AutoSize = True
        Me.lblDownloadTime.Location = New System.Drawing.Point(242, 31)
        Me.lblDownloadTime.Name = "lblDownloadTime"
        Me.lblDownloadTime.Size = New System.Drawing.Size(0, 12)
        Me.lblDownloadTime.TabIndex = 1
        '
        'lblSize
        '
        Me.lblSize.AutoSize = True
        Me.lblSize.Location = New System.Drawing.Point(353, 31)
        Me.lblSize.Name = "lblSize"
        Me.lblSize.Size = New System.Drawing.Size(0, 12)
        Me.lblSize.TabIndex = 2
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(517, 472)
        Me.Controls.Add(Me.lblLogout)
        Me.Controls.Add(Me.bRoomId)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.testButton)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBox1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
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
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents goFuck As System.Windows.Forms.Button
    Friend WithEvents bRoomId As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblLogout As System.Windows.Forms.LinkLabel
    Friend WithEvents StartRecord As System.Windows.Forms.Button
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents lblSpeed As System.Windows.Forms.Label
    Friend WithEvents lblSize As System.Windows.Forms.Label
    Friend WithEvents lblDownloadTime As System.Windows.Forms.Label

End Class
