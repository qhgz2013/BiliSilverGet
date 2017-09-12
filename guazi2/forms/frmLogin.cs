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

        public static bool CheckLoginStatus()
        {

            var cc = NetUtils.NetStream.DefaultCookieContainer.GetCookies(new Uri("https://www.bilibili.com"));
            var login_cookie = cc["DedeUserID"];
            if (login_cookie == null || login_cookie.Expired)
                return false;
            return true;
        }

        private bool _is_login_success;
        private bool _cancelled;
        public bool Canceled { get { return _cancelled; } }
        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_is_login_success)
                _cancelled = true;
            _frm_init = false;
        }

        private bool _frm_init;
        private void frmLogin_Load(object sender, EventArgs e)
        {
            _frm_init = true;
            _load_qrcode();
        }
        private void _load_qrcode()
        {

            ThreadPool.QueueUserWorkItem(delegate
            {
                string url;
                do
                {
                    url = api.GetLoginQRUrl();
                    if (string.IsNullOrEmpty(url))
                        Thread.Sleep(1000);
                    else break;
                } while (true);
                var encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.M);
                var code = encoder.Encode(url);

                var renderer = new Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer(new Gma.QrCodeNet.Encoding.Windows.Render.FixedModuleSize(5, Gma.QrCodeNet.Encoding.Windows.Render.QuietZoneModules.Two), Brushes.Black, Brushes.White);
                var ms = new MemoryStream();
                renderer.WriteToStream(code.Matrix, System.Drawing.Imaging.ImageFormat.Bmp, ms);

                ms.Seek(0, SeekOrigin.Begin);
                var img = Image.FromStream(ms);
                if (_frm_init)
                {
                    Invoke(new ThreadStart(delegate
                    {
                        pictureBox1.Image = img;
                    }));
                }

                _qr_login();
            });
        }
        private void _qr_login()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (!(_is_login_success = api.QRLogin()))
                {
                    Thread.Sleep(1000);
                    if (util.FromUnixTimestamp(api.OAuthQRTimestamp + api.OAuthQrValidateTime) < DateTime.Now)
                    {
                        _load_qrcode();
                        break;
                    }
                }

                if (_is_login_success)
                {
                    if (_frm_init)
                    {
                        Invoke(new ThreadStart(delegate { Close(); }));
                    }
                }
            });
        }
    }
}
