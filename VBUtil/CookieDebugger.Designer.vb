<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CookieDebugger
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
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.新增项ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.修改选定项ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.删除ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.删除选定项ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.删除过期Expired项ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.删除所有项ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblTime = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ListView1
        '
        Me.ListView1.CheckBoxes = True
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5, Me.ColumnHeader6, Me.ColumnHeader7})
        Me.ListView1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Location = New System.Drawing.Point(12, 12)
        Me.ListView1.MultiSelect = False
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(740, 430)
        Me.ListView1.TabIndex = 0
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Domain"
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Name"
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Value"
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Expires"
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Expired"
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "HttpOnly"
        '
        'ColumnHeader7
        '
        Me.ColumnHeader7.Text = "Discard"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.新增项ToolStripMenuItem, Me.修改选定项ToolStripMenuItem, Me.删除ToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(153, 92)
        '
        '新增项ToolStripMenuItem
        '
        Me.新增项ToolStripMenuItem.Name = "新增项ToolStripMenuItem"
        Me.新增项ToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.新增项ToolStripMenuItem.Text = "新增项"
        '
        '修改选定项ToolStripMenuItem
        '
        Me.修改选定项ToolStripMenuItem.Name = "修改选定项ToolStripMenuItem"
        Me.修改选定项ToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.修改选定项ToolStripMenuItem.Text = "修改选定项"
        '
        '删除ToolStripMenuItem
        '
        Me.删除ToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.删除选定项ToolStripMenuItem, Me.删除过期Expired项ToolStripMenuItem, Me.删除所有项ToolStripMenuItem})
        Me.删除ToolStripMenuItem.Name = "删除ToolStripMenuItem"
        Me.删除ToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.删除ToolStripMenuItem.Text = "删除"
        '
        '删除选定项ToolStripMenuItem
        '
        Me.删除选定项ToolStripMenuItem.Name = "删除选定项ToolStripMenuItem"
        Me.删除选定项ToolStripMenuItem.Size = New System.Drawing.Size(188, 22)
        Me.删除选定项ToolStripMenuItem.Text = "删除已勾项"
        '
        '删除过期Expired项ToolStripMenuItem
        '
        Me.删除过期Expired项ToolStripMenuItem.Name = "删除过期Expired项ToolStripMenuItem"
        Me.删除过期Expired项ToolStripMenuItem.Size = New System.Drawing.Size(188, 22)
        Me.删除过期Expired项ToolStripMenuItem.Text = "删除过期(Expired)项"
        '
        '删除所有项ToolStripMenuItem
        '
        Me.删除所有项ToolStripMenuItem.Name = "删除所有项ToolStripMenuItem"
        Me.删除所有项ToolStripMenuItem.Size = New System.Drawing.Size(188, 22)
        Me.删除所有项ToolStripMenuItem.Text = "删除所有项"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(105, 448)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(162, 23)
        Me.ProgressBar1.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(10, 455)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(89, 12)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "下次自动更新："
        '
        'lblTime
        '
        Me.lblTime.AutoSize = True
        Me.lblTime.Location = New System.Drawing.Point(278, 455)
        Me.lblTime.Name = "lblTime"
        Me.lblTime.Size = New System.Drawing.Size(0, 12)
        Me.lblTime.TabIndex = 3
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'CookieDebugger
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(764, 476)
        Me.Controls.Add(Me.lblTime)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.ListView1)
        Me.Name = "CookieDebugger"
        Me.Text = "CookieDebugger"
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblTime As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader7 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents 新增项ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents 修改选定项ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents 删除ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents 删除选定项ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents 删除过期Expired项ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents 删除所有项ToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
