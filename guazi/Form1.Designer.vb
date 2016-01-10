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
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.时间 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.信息 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.lblLogout = New System.Windows.Forms.LinkLabel()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.CheckBox2 = New System.Windows.Forms.CheckBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.getGuazi = New System.Windows.Forms.Button()
        Me.bRoomId = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.testButton = New System.Windows.Forms.LinkLabel()
        Me.GroupBox1.SuspendLayout()
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
        Me.Label1.Location = New System.Drawing.Point(12, 173)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "调试输出"
        '
        'ListView1
        '
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.时间, Me.信息})
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Location = New System.Drawing.Point(6, 188)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(498, 189)
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
        Me.GroupBox1.Controls.Add(Me.lblLogout)
        Me.GroupBox1.Controls.Add(Me.Button2)
        Me.GroupBox1.Controls.Add(Me.CheckBox2)
        Me.GroupBox1.Controls.Add(Me.Button1)
        Me.GroupBox1.Controls.Add(Me.CheckBox1)
        Me.GroupBox1.Controls.Add(Me.getGuazi)
        Me.GroupBox1.Controls.Add(Me.bRoomId)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 6)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(495, 117)
        Me.GroupBox1.TabIndex = 11
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "坑爹脚本1:b站直播自动领瓜子"
        '
        'lblLogout
        '
        Me.lblLogout.AutoSize = True
        Me.lblLogout.Location = New System.Drawing.Point(7, 90)
        Me.lblLogout.Name = "lblLogout"
        Me.lblLogout.Size = New System.Drawing.Size(77, 12)
        Me.lblLogout.TabIndex = 7
        Me.lblLogout.TabStop = True
        Me.lblLogout.Text = "点击退出登录"
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(135, 44)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(98, 25)
        Me.Button2.TabIndex = 6
        Me.Button2.Text = "取消关机"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'CheckBox2
        '
        Me.CheckBox2.AutoSize = True
        Me.CheckBox2.Location = New System.Drawing.Point(9, 71)
        Me.CheckBox2.Name = "CheckBox2"
        Me.CheckBox2.Size = New System.Drawing.Size(84, 16)
        Me.CheckBox2.TabIndex = 5
        Me.CheckBox2.Text = "开机自启动"
        Me.CheckBox2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(266, 18)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(89, 26)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "保存设置"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(9, 49)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(120, 16)
        Me.CheckBox1.TabIndex = 3
        Me.CheckBox1.Text = "领取完后自动关机"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'getGuazi
        '
        Me.getGuazi.Location = New System.Drawing.Point(172, 18)
        Me.getGuazi.Name = "getGuazi"
        Me.getGuazi.Size = New System.Drawing.Size(88, 27)
        Me.getGuazi.TabIndex = 2
        Me.getGuazi.Text = "GO"
        Me.getGuazi.UseVisualStyleBackColor = True
        '
        'bRoomId
        '
        Me.bRoomId.Location = New System.Drawing.Point(82, 22)
        Me.bRoomId.Name = "bRoomId"
        Me.bRoomId.Size = New System.Drawing.Size(68, 21)
        Me.bRoomId.TabIndex = 1
        Me.bRoomId.Visible = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(65, 12)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "输入房间ID"
        Me.Label2.Visible = False
        '
        'testButton
        '
        Me.testButton.AutoSize = True
        Me.testButton.Location = New System.Drawing.Point(13, 150)
        Me.testButton.Name = "testButton"
        Me.testButton.Size = New System.Drawing.Size(53, 12)
        Me.testButton.TabIndex = 12
        Me.testButton.TabStop = True
        Me.testButton.Text = "测试按钮"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(517, 472)
        Me.Controls.Add(Me.testButton)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBox1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents 时间 As System.Windows.Forms.ColumnHeader
    Friend WithEvents 信息 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents getGuazi As System.Windows.Forms.Button
    Friend WithEvents bRoomId As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents testButton As System.Windows.Forms.LinkLabel
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents CheckBox2 As System.Windows.Forms.CheckBox
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents lblLogout As System.Windows.Forms.LinkLabel

End Class
