namespace guazi2
{
    partial class frmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tUserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tCaptcha = new System.Windows.Forms.TextBox();
            this.pCaptcha = new System.Windows.Forms.PictureBox();
            this.bConfirm = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.lRefreshCaptcha = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pCaptcha)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "账号";
            // 
            // tUserName
            // 
            this.tUserName.Location = new System.Drawing.Point(59, 6);
            this.tUserName.Name = "tUserName";
            this.tUserName.Size = new System.Drawing.Size(198, 21);
            this.tUserName.TabIndex = 1;
            this.tUserName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tUserName_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "密码";
            // 
            // tPassword
            // 
            this.tPassword.Location = new System.Drawing.Point(59, 32);
            this.tPassword.Name = "tPassword";
            this.tPassword.PasswordChar = '●';
            this.tPassword.ShortcutsEnabled = false;
            this.tPassword.Size = new System.Drawing.Size(198, 21);
            this.tPassword.TabIndex = 2;
            this.tPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tPassword_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "验证码";
            // 
            // tCaptcha
            // 
            this.tCaptcha.Location = new System.Drawing.Point(176, 59);
            this.tCaptcha.Name = "tCaptcha";
            this.tCaptcha.Size = new System.Drawing.Size(81, 21);
            this.tCaptcha.TabIndex = 3;
            this.tCaptcha.Enter += new System.EventHandler(this.tCaptcha_Enter);
            this.tCaptcha.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tCaptcha_KeyPress);
            // 
            // pCaptcha
            // 
            this.pCaptcha.Location = new System.Drawing.Point(59, 56);
            this.pCaptcha.Name = "pCaptcha";
            this.pCaptcha.Size = new System.Drawing.Size(104, 56);
            this.pCaptcha.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pCaptcha.TabIndex = 2;
            this.pCaptcha.TabStop = false;
            // 
            // bConfirm
            // 
            this.bConfirm.Location = new System.Drawing.Point(75, 118);
            this.bConfirm.Name = "bConfirm";
            this.bConfirm.Size = new System.Drawing.Size(88, 45);
            this.bConfirm.TabIndex = 4;
            this.bConfirm.Text = "确定";
            this.bConfirm.UseVisualStyleBackColor = true;
            this.bConfirm.Click += new System.EventHandler(this.bConfirm_Click);
            // 
            // bCancel
            // 
            this.bCancel.Location = new System.Drawing.Point(176, 119);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(81, 44);
            this.bCancel.TabIndex = 5;
            this.bCancel.Text = "取消";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // lRefreshCaptcha
            // 
            this.lRefreshCaptcha.AutoSize = true;
            this.lRefreshCaptcha.Location = new System.Drawing.Point(176, 89);
            this.lRefreshCaptcha.Name = "lRefreshCaptcha";
            this.lRefreshCaptcha.Size = new System.Drawing.Size(65, 12);
            this.lRefreshCaptcha.TabIndex = 6;
            this.lRefreshCaptcha.TabStop = true;
            this.lRefreshCaptcha.Text = "刷新验证码";
            this.lRefreshCaptcha.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lRefreshCaptcha_LinkClicked);
            // 
            // frmLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 175);
            this.Controls.Add(this.lRefreshCaptcha);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bConfirm);
            this.Controls.Add(this.pCaptcha);
            this.Controls.Add(this.tCaptcha);
            this.Controls.Add(this.tPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tUserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "frmLogin";
            this.Text = "frmLogin";
            ((System.ComponentModel.ISupportInitialize)(this.pCaptcha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tCaptcha;
        private System.Windows.Forms.PictureBox pCaptcha;
        private System.Windows.Forms.Button bConfirm;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.LinkLabel lRefreshCaptcha;
    }
}