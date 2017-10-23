using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        }

        private bool _cancelled;
        public bool Canceled { get { return _cancelled; } }
        private bool _form_init;
        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            _form_init = false;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            _form_init = true;
        }
        private bool _check_input_valid()
        {
            if (string.IsNullOrEmpty(tUserName.Text))
            {
                tUserName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(tPassword.Text))
            {
                tPassword.Focus();
                return false;
            }

            return true;
        }
        private void bConfirm_Click(object sender, EventArgs e)
        {
            var username = tUserName.Text;
            var password = tPassword.Text;
            if (!_check_input_valid()) return;
            bConfirm.Enabled = false;
            ThreadPool.QueueUserWorkItem(delegate
            {
                var result = api.Login(username, password);
                if (result)
                {
                    //login success
                    if (_form_init)
                        Invoke(new ThreadStart(delegate
                        {
                            Close();
                        }));
                }
                else
                {
                    //login failed
                    if (_form_init)
                        Invoke(new ThreadStart(delegate
                        {
                            MessageBox.Show("登陆失败: [错误代号 " + api.LastErrorCode + "] " + (api.LastErrorMessage != null ? api.LastErrorMessage : ""));
                            bConfirm.Enabled = true;
                        }));
                }
            });
        }

        private void tUserName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (_check_input_valid())
                    bConfirm.PerformClick();
            }
        }

        private void tPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (_check_input_valid())
                    bConfirm.PerformClick();
            }
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            _cancelled = true;
            Close();
        }
    }
}
