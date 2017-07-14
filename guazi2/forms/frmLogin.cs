using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace guazi2
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            start_geetest();
        }
        private void start_geetest()
        {
            lGeeTest.Text = "发送滑动验证请求...";
            _thd_for_challenge = new Thread(challenge_callback);
            _thd_for_challenge.IsBackground = true;
            _thd_for_challenge.Name = "GeeTest后台处理线程";
            _thd_for_challenge.Start();
        }

        public static bool CheckLoginStatus()
        {

            var cc = NetUtils.NetStream.DefaultCookieContainer.GetCookies(new Uri("http://www.bilibili.com"));
            var login_cookie = cc["DedeUserID"];
            if (login_cookie == null || login_cookie.Expired)
                return false;
            return true;
        }
        private bool _canceled;
        private bool _frm_created;
        public bool Canceled { get { return _canceled; } }
        public string User_Name { get { return tUserName.Text; } set { tUserName.Text = value; } }
        public string PassWord { get { return tPassword.Text; } set { tPassword.Text = value; } }
        private Thread _thd_for_challenge;
        private void challenge_callback()
        {
            string validate_string = string.Empty;
            do
            {
                validate_string = api.DoChallenge();
            } while (string.IsNullOrEmpty(validate_string));

            //Invoking
            if (_frm_created)
            {
                Invoke(new ThreadStart(delegate
                {
                    lGeeTest.Text = "滑动验证已通过";
                }));
            }
        }
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
            Close();
        }
        private void tryLogin()
        {
            //login check
            if (string.IsNullOrEmpty(tUserName.Text))
                tUserName.Focus();
            else if (string.IsNullOrEmpty(tPassword.Text))
                tPassword.Focus();
            else if (string.IsNullOrEmpty(api.Challenge) || string.IsNullOrEmpty(api.Validate))
            {
                MessageBox.Show(this, "请等待滑动验证通过", "Emmm，手速好快！", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                //login
                try
                {
                    if (api.Login(User_Name, PassWord))
                    {
                        Close();
                    }
                    else
                    {
                        start_geetest();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "登录出现错误，请稍后再重试：\r\n" + ex.ToString(), "出错啦", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    start_geetest();
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

        private void frmLogin_Load(object sender, EventArgs e)
        {
            _frm_created = true;
        }
    }
}
