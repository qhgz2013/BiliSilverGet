namespace guazi2
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.tRoomID = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblShowCookie = new System.Windows.Forms.LinkLabel();
            this.lOnlineUser2 = new System.Windows.Forms.Label();
            this.lRoomName = new System.Windows.Forms.Label();
            this.lLiveTime = new System.Windows.Forms.Label();
            this.lRoomStatus = new System.Windows.Forms.Label();
            this.pExp = new System.Windows.Forms.ProgressBar();
            this.lSilver = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lExp = new System.Windows.Forms.Label();
            this.lUserLevel = new System.Windows.Forms.Label();
            this.lUserName = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cRecordSaveComment = new System.Windows.Forms.CheckBox();
            this.lStreamingStat = new System.Windows.Forms.Label();
            this.cRecord = new System.Windows.Forms.CheckBox();
            this.lEventTime = new System.Windows.Forms.Label();
            this.lGrabTime = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bSendAllGift = new System.Windows.Forms.Button();
            this.bRefreshBag = new System.Windows.Forms.Button();
            this.pBag = new System.Windows.Forms.FlowLayoutPanel();
            this.cJoinActivity = new System.Windows.Forms.CheckBox();
            this.cAutoOnlineHeart = new System.Windows.Forms.CheckBox();
            this.cAutoJoinSmallTV = new System.Windows.Forms.CheckBox();
            this.cAutoGetGift = new System.Windows.Forms.CheckBox();
            this.cAutoSign = new System.Windows.Forms.CheckBox();
            this.cAutoGrabSilver = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tSendComment = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.pCommentOutput = new System.Windows.Forms.PictureBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pCommentFreq = new System.Windows.Forms.PictureBox();
            this.pGiftPrice = new System.Windows.Forms.PictureBox();
            this.pOnlineUser = new System.Windows.Forms.PictureBox();
            this.lGGiftPrice = new System.Windows.Forms.Label();
            this.lGCommentCount = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lGRunTime = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lRunTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lCommentCount = new System.Windows.Forms.Label();
            this.lTotalGiftPrice = new System.Windows.Forms.Label();
            this.lCommentFreq = new System.Windows.Forms.Label();
            this.lGiftPrice = new System.Windows.Forms.Label();
            this.lMaxOnlineUser = new System.Windows.Forms.Label();
            this.lOnlineUser = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pCommentOutput)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pCommentFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pGiftPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pOnlineUser)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "房间ID";
            // 
            // tRoomID
            // 
            this.tRoomID.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tRoomID.Location = new System.Drawing.Point(58, 26);
            this.tRoomID.Name = "tRoomID";
            this.tRoomID.Size = new System.Drawing.Size(96, 23);
            this.tRoomID.TabIndex = 1;
            this.tRoomID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tRoomID_KeyPress);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblShowCookie);
            this.groupBox1.Controls.Add(this.lOnlineUser2);
            this.groupBox1.Controls.Add(this.lRoomName);
            this.groupBox1.Controls.Add(this.lLiveTime);
            this.groupBox1.Controls.Add(this.lRoomStatus);
            this.groupBox1.Controls.Add(this.pExp);
            this.groupBox1.Controls.Add(this.lSilver);
            this.groupBox1.Controls.Add(this.tRoomID);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lExp);
            this.groupBox1.Controls.Add(this.lUserLevel);
            this.groupBox1.Controls.Add(this.lUserName);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(7, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(667, 93);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "个人/房间信息";
            // 
            // lblShowCookie
            // 
            this.lblShowCookie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblShowCookie.AutoSize = true;
            this.lblShowCookie.Location = new System.Drawing.Point(566, 16);
            this.lblShowCookie.Name = "lblShowCookie";
            this.lblShowCookie.Size = new System.Drawing.Size(95, 17);
            this.lblShowCookie.TabIndex = 6;
            this.lblShowCookie.TabStop = true;
            this.lblShowCookie.Text = "查看cookie信息";
            this.lblShowCookie.Visible = false;
            this.lblShowCookie.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblShowCookie_LinkClicked);
            // 
            // lOnlineUser2
            // 
            this.lOnlineUser2.AutoSize = true;
            this.lOnlineUser2.Location = new System.Drawing.Point(7, 70);
            this.lOnlineUser2.Name = "lOnlineUser2";
            this.lOnlineUser2.Size = new System.Drawing.Size(59, 17);
            this.lOnlineUser2.TabIndex = 5;
            this.lOnlineUser2.Text = "直播人数:";
            // 
            // lRoomName
            // 
            this.lRoomName.AutoSize = true;
            this.lRoomName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lRoomName.Location = new System.Drawing.Point(6, 49);
            this.lRoomName.Name = "lRoomName";
            this.lRoomName.Size = new System.Drawing.Size(74, 21);
            this.lRoomName.TabIndex = 4;
            this.lRoomName.Text = "房间标题";
            // 
            // lLiveTime
            // 
            this.lLiveTime.AutoSize = true;
            this.lLiveTime.Location = new System.Drawing.Point(221, 70);
            this.lLiveTime.Name = "lLiveTime";
            this.lLiveTime.Size = new System.Drawing.Size(56, 17);
            this.lLiveTime.TabIndex = 2;
            this.lLiveTime.Text = "直播时长";
            // 
            // lRoomStatus
            // 
            this.lRoomStatus.AutoSize = true;
            this.lRoomStatus.Location = new System.Drawing.Point(147, 70);
            this.lRoomStatus.Name = "lRoomStatus";
            this.lRoomStatus.Size = new System.Drawing.Size(68, 17);
            this.lRoomStatus.TabIndex = 2;
            this.lRoomStatus.Text = "直播间状态";
            // 
            // pExp
            // 
            this.pExp.Location = new System.Drawing.Point(172, 36);
            this.pExp.Name = "pExp";
            this.pExp.Size = new System.Drawing.Size(231, 10);
            this.pExp.TabIndex = 1;
            // 
            // lSilver
            // 
            this.lSilver.AutoSize = true;
            this.lSilver.Location = new System.Drawing.Point(346, 16);
            this.lSilver.Name = "lSilver";
            this.lSilver.Size = new System.Drawing.Size(15, 17);
            this.lSilver.TabIndex = 0;
            this.lSilver.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(293, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "银瓜子:";
            // 
            // lExp
            // 
            this.lExp.AutoSize = true;
            this.lExp.Location = new System.Drawing.Point(409, 33);
            this.lExp.Name = "lExp";
            this.lExp.Size = new System.Drawing.Size(27, 17);
            this.lExp.TabIndex = 0;
            this.lExp.Text = "0/0";
            // 
            // lUserLevel
            // 
            this.lUserLevel.AutoSize = true;
            this.lUserLevel.Location = new System.Drawing.Point(409, 16);
            this.lUserLevel.Name = "lUserLevel";
            this.lUserLevel.Size = new System.Drawing.Size(30, 17);
            this.lUserLevel.TabIndex = 0;
            this.lUserLevel.Text = "UL0";
            // 
            // lUserName
            // 
            this.lUserName.AutoSize = true;
            this.lUserName.Location = new System.Drawing.Point(170, 16);
            this.lUserName.Name = "lUserName";
            this.lUserName.Size = new System.Drawing.Size(56, 17);
            this.lUserName.TabIndex = 0;
            this.lUserName.Text = "用户名称";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(7, 103);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(667, 440);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cRecordSaveComment);
            this.tabPage1.Controls.Add(this.lStreamingStat);
            this.tabPage1.Controls.Add(this.cRecord);
            this.tabPage1.Controls.Add(this.lEventTime);
            this.tabPage1.Controls.Add(this.lGrabTime);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.cJoinActivity);
            this.tabPage1.Controls.Add(this.cAutoOnlineHeart);
            this.tabPage1.Controls.Add(this.cAutoJoinSmallTV);
            this.tabPage1.Controls.Add(this.cAutoGetGift);
            this.tabPage1.Controls.Add(this.cAutoSign);
            this.tabPage1.Controls.Add(this.cAutoGrabSilver);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(659, 410);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "设置";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cRecordSaveComment
            // 
            this.cRecordSaveComment.AutoSize = true;
            this.cRecordSaveComment.Location = new System.Drawing.Point(6, 209);
            this.cRecordSaveComment.Name = "cRecordSaveComment";
            this.cRecordSaveComment.Size = new System.Drawing.Size(111, 21);
            this.cRecordSaveComment.TabIndex = 6;
            this.cRecordSaveComment.Text = "录播时保存弹幕";
            this.cRecordSaveComment.UseVisualStyleBackColor = true;
            // 
            // lStreamingStat
            // 
            this.lStreamingStat.AutoSize = true;
            this.lStreamingStat.Location = new System.Drawing.Point(16, 233);
            this.lStreamingStat.Name = "lStreamingStat";
            this.lStreamingStat.Size = new System.Drawing.Size(0, 17);
            this.lStreamingStat.TabIndex = 5;
            // 
            // cRecord
            // 
            this.cRecord.AutoSize = true;
            this.cRecord.Location = new System.Drawing.Point(6, 182);
            this.cRecord.Name = "cRecord";
            this.cRecord.Size = new System.Drawing.Size(51, 21);
            this.cRecord.TabIndex = 4;
            this.cRecord.Text = "录播";
            this.cRecord.UseVisualStyleBackColor = true;
            this.cRecord.CheckedChanged += new System.EventHandler(this.cRecord_CheckedChanged);
            // 
            // lEventTime
            // 
            this.lEventTime.AutoSize = true;
            this.lEventTime.Location = new System.Drawing.Point(133, 142);
            this.lEventTime.Name = "lEventTime";
            this.lEventTime.Size = new System.Drawing.Size(0, 17);
            this.lEventTime.TabIndex = 3;
            // 
            // lGrabTime
            // 
            this.lGrabTime.AutoSize = true;
            this.lGrabTime.Location = new System.Drawing.Point(111, 7);
            this.lGrabTime.Name = "lGrabTime";
            this.lGrabTime.Size = new System.Drawing.Size(0, 17);
            this.lGrabTime.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bSendAllGift);
            this.groupBox2.Controls.Add(this.bRefreshBag);
            this.groupBox2.Controls.Add(this.pBag);
            this.groupBox2.Location = new System.Drawing.Point(217, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(331, 306);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "道具背包";
            // 
            // bSendAllGift
            // 
            this.bSendAllGift.Location = new System.Drawing.Point(237, 60);
            this.bSendAllGift.Name = "bSendAllGift";
            this.bSendAllGift.Size = new System.Drawing.Size(88, 32);
            this.bSendAllGift.TabIndex = 3;
            this.bSendAllGift.Text = "送出全部道具";
            this.bSendAllGift.UseVisualStyleBackColor = true;
            this.bSendAllGift.Click += new System.EventHandler(this.bSendAllGift_Click);
            // 
            // bRefreshBag
            // 
            this.bRefreshBag.Location = new System.Drawing.Point(237, 22);
            this.bRefreshBag.Name = "bRefreshBag";
            this.bRefreshBag.Size = new System.Drawing.Size(88, 32);
            this.bRefreshBag.TabIndex = 3;
            this.bRefreshBag.Text = "刷新道具背包";
            this.bRefreshBag.UseVisualStyleBackColor = true;
            this.bRefreshBag.Click += new System.EventHandler(this.bRefreshBag_Click);
            // 
            // pBag
            // 
            this.pBag.AutoSize = true;
            this.pBag.Location = new System.Drawing.Point(6, 22);
            this.pBag.Name = "pBag";
            this.pBag.Size = new System.Drawing.Size(230, 278);
            this.pBag.TabIndex = 2;
            // 
            // cJoinActivity
            // 
            this.cJoinActivity.AutoSize = true;
            this.cJoinActivity.Location = new System.Drawing.Point(5, 141);
            this.cJoinActivity.Name = "cJoinActivity";
            this.cJoinActivity.Size = new System.Drawing.Size(123, 21);
            this.cJoinActivity.TabIndex = 0;
            this.cJoinActivity.Text = "自动参加在线活动";
            this.cJoinActivity.UseVisualStyleBackColor = true;
            this.cJoinActivity.CheckedChanged += new System.EventHandler(this.cJoinActivity_CheckedChanged);
            // 
            // cAutoOnlineHeart
            // 
            this.cAutoOnlineHeart.AutoSize = true;
            this.cAutoOnlineHeart.Location = new System.Drawing.Point(5, 114);
            this.cAutoOnlineHeart.Name = "cAutoOnlineHeart";
            this.cAutoOnlineHeart.Size = new System.Drawing.Size(73, 21);
            this.cAutoOnlineHeart.TabIndex = 0;
            this.cAutoOnlineHeart.Text = "挂机EXP";
            this.cAutoOnlineHeart.UseVisualStyleBackColor = true;
            this.cAutoOnlineHeart.CheckedChanged += new System.EventHandler(this.cAutoOnlineHeart_CheckedChanged);
            // 
            // cAutoJoinSmallTV
            // 
            this.cAutoJoinSmallTV.AutoSize = true;
            this.cAutoJoinSmallTV.Location = new System.Drawing.Point(6, 87);
            this.cAutoJoinSmallTV.Name = "cAutoJoinSmallTV";
            this.cAutoJoinSmallTV.Size = new System.Drawing.Size(123, 21);
            this.cAutoJoinSmallTV.TabIndex = 0;
            this.cAutoJoinSmallTV.Text = "自动参加电视抽奖";
            this.cAutoJoinSmallTV.UseVisualStyleBackColor = true;
            this.cAutoJoinSmallTV.CheckedChanged += new System.EventHandler(this.cAutoJoinSmallTV_CheckedChanged);
            // 
            // cAutoGetGift
            // 
            this.cAutoGetGift.AutoSize = true;
            this.cAutoGetGift.Location = new System.Drawing.Point(6, 60);
            this.cAutoGetGift.Name = "cAutoGetGift";
            this.cAutoGetGift.Size = new System.Drawing.Size(99, 21);
            this.cAutoGetGift.TabIndex = 0;
            this.cAutoGetGift.Text = "自动领取道具";
            this.cAutoGetGift.UseVisualStyleBackColor = true;
            this.cAutoGetGift.CheckedChanged += new System.EventHandler(this.cAutoGetGift_CheckedChanged);
            // 
            // cAutoSign
            // 
            this.cAutoSign.AutoSize = true;
            this.cAutoSign.Location = new System.Drawing.Point(6, 33);
            this.cAutoSign.Name = "cAutoSign";
            this.cAutoSign.Size = new System.Drawing.Size(75, 21);
            this.cAutoSign.TabIndex = 0;
            this.cAutoSign.Text = "自动签到";
            this.cAutoSign.UseVisualStyleBackColor = true;
            this.cAutoSign.CheckedChanged += new System.EventHandler(this.cAutoSign_CheckedChanged);
            // 
            // cAutoGrabSilver
            // 
            this.cAutoGrabSilver.AutoSize = true;
            this.cAutoGrabSilver.Location = new System.Drawing.Point(6, 6);
            this.cAutoGrabSilver.Name = "cAutoGrabSilver";
            this.cAutoGrabSilver.Size = new System.Drawing.Size(99, 21);
            this.cAutoGrabSilver.TabIndex = 0;
            this.cAutoGrabSilver.Text = "自动搜刮瓜子";
            this.cAutoGrabSilver.UseVisualStyleBackColor = true;
            this.cAutoGrabSilver.CheckedChanged += new System.EventHandler(this.cAutoGrabSilver_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tSendComment);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.pCommentOutput);
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(659, 410);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "弹幕信息";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tSendComment
            // 
            this.tSendComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tSendComment.Location = new System.Drawing.Point(64, 381);
            this.tSendComment.Name = "tSendComment";
            this.tSendComment.Size = new System.Drawing.Size(589, 23);
            this.tSendComment.TabIndex = 5;
            this.tSendComment.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tSendComment_KeyPress);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 384);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 17);
            this.label7.TabIndex = 4;
            this.label7.Text = "发送弹幕:";
            // 
            // pCommentOutput
            // 
            this.pCommentOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pCommentOutput.Location = new System.Drawing.Point(6, 6);
            this.pCommentOutput.Name = "pCommentOutput";
            this.pCommentOutput.Size = new System.Drawing.Size(647, 372);
            this.pCommentOutput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pCommentOutput.TabIndex = 3;
            this.pCommentOutput.TabStop = false;
            this.pCommentOutput.SizeChanged += new System.EventHandler(this.pCommentOutput_SizeChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pCommentFreq);
            this.tabPage3.Controls.Add(this.pGiftPrice);
            this.tabPage3.Controls.Add(this.pOnlineUser);
            this.tabPage3.Controls.Add(this.lGGiftPrice);
            this.tabPage3.Controls.Add(this.lGCommentCount);
            this.tabPage3.Controls.Add(this.label11);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.label16);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.label10);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.lGRunTime);
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.lRunTime);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.lCommentCount);
            this.tabPage3.Controls.Add(this.lTotalGiftPrice);
            this.tabPage3.Controls.Add(this.lCommentFreq);
            this.tabPage3.Controls.Add(this.lGiftPrice);
            this.tabPage3.Controls.Add(this.lMaxOnlineUser);
            this.tabPage3.Controls.Add(this.lOnlineUser);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(659, 410);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "统计信息";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pCommentFreq
            // 
            this.pCommentFreq.Location = new System.Drawing.Point(6, 77);
            this.pCommentFreq.Name = "pCommentFreq";
            this.pCommentFreq.Size = new System.Drawing.Size(177, 55);
            this.pCommentFreq.TabIndex = 15;
            this.pCommentFreq.TabStop = false;
            // 
            // pGiftPrice
            // 
            this.pGiftPrice.Location = new System.Drawing.Point(316, 6);
            this.pGiftPrice.Name = "pGiftPrice";
            this.pGiftPrice.Size = new System.Drawing.Size(177, 55);
            this.pGiftPrice.TabIndex = 16;
            this.pGiftPrice.TabStop = false;
            // 
            // pOnlineUser
            // 
            this.pOnlineUser.Location = new System.Drawing.Point(6, 6);
            this.pOnlineUser.Name = "pOnlineUser";
            this.pOnlineUser.Size = new System.Drawing.Size(177, 55);
            this.pOnlineUser.TabIndex = 17;
            this.pOnlineUser.TabStop = false;
            // 
            // lGGiftPrice
            // 
            this.lGGiftPrice.AutoSize = true;
            this.lGGiftPrice.Location = new System.Drawing.Point(554, 40);
            this.lGGiftPrice.Name = "lGGiftPrice";
            this.lGGiftPrice.Size = new System.Drawing.Size(39, 17);
            this.lGGiftPrice.TabIndex = 3;
            this.lGGiftPrice.Text = "0瓜子";
            // 
            // lGCommentCount
            // 
            this.lGCommentCount.AutoSize = true;
            this.lGCommentCount.Location = new System.Drawing.Point(245, 111);
            this.lGCommentCount.Name = "lGCommentCount";
            this.lGCommentCount.Size = new System.Drawing.Size(27, 17);
            this.lGCommentCount.TabIndex = 3;
            this.lGCommentCount.Text = "0条";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(499, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 17);
            this.label11.TabIndex = 3;
            this.label11.Text = "历史总计:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(189, 111);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 17);
            this.label9.TabIndex = 3;
            this.label9.Text = "历史总计:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 171);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(95, 17);
            this.label16.TabIndex = 3;
            this.label16.Text = "程序总运行时间:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 154);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(107, 17);
            this.label14.TabIndex = 3;
            this.label14.Text = "本次程序运行时间:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(189, 94);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 17);
            this.label10.TabIndex = 3;
            this.label10.Text = "本次总计:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(189, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 17);
            this.label5.TabIndex = 4;
            this.label5.Text = "弹幕密度:";
            // 
            // lGRunTime
            // 
            this.lGRunTime.AutoSize = true;
            this.lGRunTime.Location = new System.Drawing.Point(119, 171);
            this.lGRunTime.Name = "lGRunTime";
            this.lGRunTime.Size = new System.Drawing.Size(49, 17);
            this.lGRunTime.TabIndex = 7;
            this.lGRunTime.Text = "0:00:00";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(499, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 17);
            this.label8.TabIndex = 5;
            this.label8.Text = "本次总计:";
            // 
            // lRunTime
            // 
            this.lRunTime.AutoSize = true;
            this.lRunTime.Location = new System.Drawing.Point(119, 154);
            this.lRunTime.Name = "lRunTime";
            this.lRunTime.Size = new System.Drawing.Size(49, 17);
            this.lRunTime.TabIndex = 7;
            this.lRunTime.Text = "0:00:00";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(499, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "礼物价值:";
            // 
            // lCommentCount
            // 
            this.lCommentCount.AutoSize = true;
            this.lCommentCount.Location = new System.Drawing.Point(245, 94);
            this.lCommentCount.Name = "lCommentCount";
            this.lCommentCount.Size = new System.Drawing.Size(27, 17);
            this.lCommentCount.TabIndex = 7;
            this.lCommentCount.Text = "0条";
            // 
            // lTotalGiftPrice
            // 
            this.lTotalGiftPrice.AutoSize = true;
            this.lTotalGiftPrice.Location = new System.Drawing.Point(554, 23);
            this.lTotalGiftPrice.Name = "lTotalGiftPrice";
            this.lTotalGiftPrice.Size = new System.Drawing.Size(39, 17);
            this.lTotalGiftPrice.TabIndex = 8;
            this.lTotalGiftPrice.Text = "0瓜子";
            // 
            // lCommentFreq
            // 
            this.lCommentFreq.AutoSize = true;
            this.lCommentFreq.Location = new System.Drawing.Point(245, 77);
            this.lCommentFreq.Name = "lCommentFreq";
            this.lCommentFreq.Size = new System.Drawing.Size(52, 17);
            this.lCommentFreq.TabIndex = 9;
            this.lCommentFreq.Text = "0条/10s";
            // 
            // lGiftPrice
            // 
            this.lGiftPrice.AutoSize = true;
            this.lGiftPrice.Location = new System.Drawing.Point(554, 6);
            this.lGiftPrice.Name = "lGiftPrice";
            this.lGiftPrice.Size = new System.Drawing.Size(39, 17);
            this.lGiftPrice.TabIndex = 10;
            this.lGiftPrice.Text = "0瓜子";
            // 
            // lMaxOnlineUser
            // 
            this.lMaxOnlineUser.AutoSize = true;
            this.lMaxOnlineUser.Location = new System.Drawing.Point(222, 23);
            this.lMaxOnlineUser.Name = "lMaxOnlineUser";
            this.lMaxOnlineUser.Size = new System.Drawing.Size(15, 17);
            this.lMaxOnlineUser.TabIndex = 11;
            this.lMaxOnlineUser.Text = "0";
            // 
            // lOnlineUser
            // 
            this.lOnlineUser.AutoSize = true;
            this.lOnlineUser.Location = new System.Drawing.Point(243, 6);
            this.lOnlineUser.Name = "lOnlineUser";
            this.lOnlineUser.Size = new System.Drawing.Size(15, 17);
            this.lOnlineUser.TabIndex = 12;
            this.lOnlineUser.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(189, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 17);
            this.label6.TabIndex = 13;
            this.label6.Text = "Max:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(189, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "在线人数:";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 567);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "瓜子搜挂机v2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pCommentOutput)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pCommentFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pGiftPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pOnlineUser)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tRoomID;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label lSilver;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lUserLevel;
        private System.Windows.Forms.Label lUserName;
        private System.Windows.Forms.Label lRoomName;
        private System.Windows.Forms.CheckBox cJoinActivity;
        private System.Windows.Forms.CheckBox cAutoOnlineHeart;
        private System.Windows.Forms.CheckBox cAutoJoinSmallTV;
        private System.Windows.Forms.CheckBox cAutoGetGift;
        private System.Windows.Forms.CheckBox cAutoSign;
        private System.Windows.Forms.CheckBox cAutoGrabSilver;
        private System.Windows.Forms.ProgressBar pExp;
        private System.Windows.Forms.Label lExp;
        private System.Windows.Forms.PictureBox pCommentOutput;
        private System.Windows.Forms.TextBox tSendComment;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FlowLayoutPanel pBag;
        private System.Windows.Forms.Label lGrabTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button bSendAllGift;
        private System.Windows.Forms.Button bRefreshBag;
        private System.Windows.Forms.Label lRoomStatus;
        private System.Windows.Forms.Label lLiveTime;
        private System.Windows.Forms.Label lEventTime;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.PictureBox pCommentFreq;
        private System.Windows.Forms.PictureBox pGiftPrice;
        private System.Windows.Forms.PictureBox pOnlineUser;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lCommentCount;
        private System.Windows.Forms.Label lTotalGiftPrice;
        private System.Windows.Forms.Label lCommentFreq;
        private System.Windows.Forms.Label lGiftPrice;
        private System.Windows.Forms.Label lMaxOnlineUser;
        private System.Windows.Forms.Label lOnlineUser;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cRecord;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label lStreamingStat;
        private System.Windows.Forms.Label lOnlineUser2;
        private System.Windows.Forms.Label lGGiftPrice;
        private System.Windows.Forms.Label lGCommentCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lGRunTime;
        private System.Windows.Forms.Label lRunTime;
        private System.Windows.Forms.CheckBox cRecordSaveComment;
        private System.Windows.Forms.LinkLabel lblShowCookie;
    }
}

