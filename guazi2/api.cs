using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBUtil.Utils.NetUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Runtime.InteropServices;

namespace guazi2
{
    public class api
    {
        protected api() { }

        #region constants
        private const string LOGIN_URL= "https://passport.bilibili.com/login/dologin";
        private const string LOGOUT_URL = "https://passport.bilibili.com/login";
        private const string BACKUP_LOGIN_URL = "https://passport.bilibili.com/ajax/miniLogin/login";
        private const string MAIN_PAGE = "http://www.bilibili.com/";
        private const string CAPTCHA_URL = "https://passport.bilibili.com/captcha";
        private const string API_MYINFO = "http://api.bilibili.com/myinfo";
        private const string LOGIN_PUBLIC_KEY = "https://passport.bilibili.com/login?act=getkey";
        #endregion

        #region Public Static Functions
        /// <summary>
        /// 检查登录是否成功 [throwable]
        /// </summary>
        /// <returns></returns>
        public static bool CheckLogin()
        {
            var request = new NetStream(Timeout: 15000, RetryTimes: 20);
            request.HttpGet(API_MYINFO);
            var str_response = request.ReadResponseString();
            request.Close();
            str_response = str_response.Replace("\r", "").Replace("\n", "");
            return !str_response.Contains("-101");
        }
        /// <summary>
        /// 登录到b站
        /// </summary>
        /// <param name="userID">邮箱/手机号</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <param name="login_time">登录保持时间（s）</param>
        /// <param name="login_result">[输出]登录返回的结果</param>
        /// <returns></returns>
        public static bool Login(string userID, string password, string captcha, int login_time, out string login_result)
        {
            var param = new Parameters();
            param.Add("act", "login");
            param.Add("userid", userID);
            param.Add("gourl", "http://www.bilibili.com/");
            param.Add("keeptime", login_time);
            
            var request = new NetStream(Timeout: 15000, RetryTimes: 20);

            //fetching public key
            request.HttpGet(LOGIN_PUBLIC_KEY);
            var str_rsa_key = request.ReadResponseString();
            request.Close();

            JObject rsa_key = (JObject)JsonConvert.DeserializeObject(str_rsa_key);

            var public_key = rsa_key.Value<string>("key");
            var hash = rsa_key.Value<string>("hash");

            //encrypting password
            var rsa_crypt_server = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsa_crypt_server.ImportParameters(VBUtil.RSA.RSA_Converter.ConvertFromPemPublicKey(public_key));

            var mixed_password = hash + password;
            byte[] hex_password = Encoding.UTF8.GetBytes(mixed_password);
            byte[] hex_encrypted_password = rsa_crypt_server.Encrypt(hex_password, false);
            var encrypted_password = Convert.ToBase64String(hex_encrypted_password);

            param.Add("pwd", encrypted_password);
            param.Add("vdcode", captcha);

            //posting login data
            request.HttpPost(LOGIN_URL, param);
            var str_response = request.ReadResponseString();
            request.Close();

            login_result = str_response;

            return CheckLogin();
        }
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <returns></returns>
        public static Image GetCaptchaImage()
        {
            var request = new NetStream(Timeout: 15000, RetryTimes: 20);
            request.HttpGet(CAPTCHA_URL);
            var img = Image.FromStream(request.Stream);
            request.Close();
            return img;
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public static bool Logout()
        {
            var param = new Parameters();
            param.Add("act", "exit");
            var request = new NetStream(Timeout: 15000, RetryTimes: 20);
            request.HttpGet(LOGOUT_URL, urlParam: param);
            var str_response = request.ReadResponseString();
            request.Close();
            return CheckLogin();
        }
        #endregion
    }
    public class ocr
    {
        protected ocr() { }
        [DllImport("AspriseOCR", EntryPoint ="OCR", CallingConvention =CallingConvention.Cdecl)]
        public static extern IntPtr OCR(string file, int type);
        [DllImport("AspriseOCR", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
        [DllImport("AspriseOCR", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRBarCodes(string file, int type);
        [DllImport("AspriseOCR", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);
    }
}
