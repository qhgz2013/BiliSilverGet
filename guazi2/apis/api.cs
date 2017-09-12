using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using guazi2.NetUtils;

namespace guazi2
{
    public partial class api
    {
        protected api() { }

        #region constants
        private const string LOGIN_URL= "https://passport.bilibili.com/web/login";
        private const string LOGOUT_URL = "https://passport.bilibili.com/login";
        private const string MAIN_PAGE = "http://www.bilibili.com/";
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
            var request = new NetStream();
            request.TimeOut = 15000;
            request.RetryTimes = 20;
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
        /// <param name="challenge">GeeTest返回的Challenge</param>
        /// <param name="validate">GeeTest返回的Validate</param>
        /// <param name="seccode">不知用途的代码，默认为validate+"|jordan"</param>
        /// <returns></returns>
        public static bool Login(string userID, string password, string challenge = null, string validate = null, string seccode = null)
        {
            //最新的测试是forbidden，破解随缘吧，有空就折腾轨迹拟合，没空就算了
            throw new NotImplementedException();

            var request = new NetStream();
            request.TimeOut = 15000;
            request.RetryTimes = 20;

            //fetching public key
            request.HttpGet(LOGIN_PUBLIC_KEY);
            var str_rsa_key = request.ReadResponseString();
            request.Close();

            JObject rsa_key = (JObject)JsonConvert.DeserializeObject(str_rsa_key);

            var public_key = rsa_key.Value<string>("key");
            var hash = rsa_key.Value<string>("hash");

            //encrypting password
            var rsa_crypt_server = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsa_crypt_server.ImportParameters(RSA.ConvertFromPemPublicKey(public_key));

            var mixed_password = hash + password;
            byte[] hex_password = Encoding.UTF8.GetBytes(mixed_password);
            byte[] hex_encrypted_password = rsa_crypt_server.Encrypt(hex_password, false);
            var encrypted_password = Convert.ToBase64String(hex_encrypted_password);

            //xhr header
            var xhr_param = new Parameters();
            xhr_param.Add("X-Requested-With", "XMLHttpRequest");
            xhr_param.Add("Origin", "https://passport.bilibili.com");
            xhr_param.Add("Referer", "https://passport.bilibili.com/login");
            //post body
            var param = new Parameters();
            param.Add("cType", 2);
            param.Add("vcType", 2);
            param.Add("captcha", "gc");
            param.Add("user", userID);

            param.Add("pwd", encrypted_password);
            param.Add("keep", true);

            param.Add("gourl", "http://www.bilibili.com/");
            //if (challenge == null) challenge = _challenge;
            param.Add("challenge",  challenge);
            //if (validate == null) validate = _validate;
            param.Add("validate", validate);
            if (seccode == null) seccode = validate + "|jordan";
            param.Add("seccode", seccode);

            //posting login data
            try
            {
                request.HttpPost(LOGIN_URL, param, headerParam: xhr_param);
                var str_response = request.ReadResponseString();
                request.Close();

                var response = JsonConvert.DeserializeObject(str_response) as JObject;
                if (response.Value<int>("code") == 0) return true;
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public static bool Logout()
        {
            var param = new Parameters();
            param.Add("act", "exit");
            var request = new NetStream();
            request.TimeOut = 15000;
            request.RetryTimes = 20;
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
        [DllImport("ocr\\AspriseOCR", EntryPoint ="OCR", CallingConvention =CallingConvention.Cdecl)]
        public static extern IntPtr OCR(string file, int type);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRBarCodes(string file, int type);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);
    }
}
