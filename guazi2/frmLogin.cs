using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace guazi2
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        public static bool CheckLoginStatus()
        {

            var cc = VBUtil.Utils.NetUtils.Global.DefaultCookieContainer.GetCookies(new Uri("http://www.bilibili.com"));
            var login_cookie = cc["DedeUserID"];
            if (login_cookie == null || login_cookie.Expired)
                return false;
            return true;
        }
        private bool _canceled;
        public bool Canceled { get { return _canceled; } }

        public string User_Name { get { return tUserName.Text; } set { tUserName.Text = value; } }
        public string PassWord { get { return tPassword.Text; } set { tPassword.Text = value; } }
        public string Captcha { get { return tCaptcha.Text; } set { tCaptcha.Text = value; } }
        private void bConfirm_Click(object sender, EventArgs e)
        {
            _canceled = false;
            tryLogin();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            _canceled = true;
            tUserName.Text = "";
            tPassword.Text = "";
            tCaptcha.Text = "";
            Close();
        }
        private void tryLogin()
        {
            //login check
            if (string.IsNullOrEmpty(tUserName.Text))
                tUserName.Focus();
            else if (string.IsNullOrEmpty(tPassword.Text))
                tPassword.Focus();
            else if (string.IsNullOrEmpty(tCaptcha.Text))
            {
                if (pCaptcha.Image == null)
                {
                    pCaptcha.Image = api.GetCaptchaImage();
                    tCaptcha.Text = "";
                }
                tCaptcha.Focus();
            }
            else
            {
                //login
                string login_result;
                try
                {
                    if (api.Login(User_Name, PassWord, Captcha, 2592000, out login_result))
                    {
                        Close();
                    }
                    if (login_result.Contains("验证码错误"))
                    {
                        pCaptcha.Image = api.GetCaptchaImage();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "登录出现错误，请稍后再重试：\r\n" + ex.ToString(), "出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pCaptcha.Image = api.GetCaptchaImage();
                    tCaptcha.Text = "";
                }
            }
        }
        private void tUserName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                tryLogin();
            }
        }

        private void tPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                tryLogin();
            }
        }

        private void tCaptcha_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                tryLogin();
            }
        }

        private void tCaptcha_Enter(object sender, EventArgs e)
        {
            if (pCaptcha.Image == null)
            {
                pCaptcha.Image = api.GetCaptchaImage();
            }
        }

        private void lRefreshCaptcha_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pCaptcha.Image = api.GetCaptchaImage();
        }
    }
}
