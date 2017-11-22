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
using System.Threading;
using System.Net;
using System.IO;

namespace guazi2
{
    public partial class api
    {
        protected api() { }

        #region urls
        public const string APPKEY = "1d8b6e7d45233436";
        public const string APPKEY_SECRET = "560c52ccd288fed045859ed18bffd973";

        public const string ANDROID_USER_AGENT = "Mozilla/5.0 BiliDroid/5.13.0 (bbcallen@gmail.com)";
        private const string _URL_AUTH_GET_KEY = "https://passport.bilibili.com/api/oauth2/getKey";
        private const string _URL_AUTH_LOGIN = "https://passport.bilibili.com/api/oauth2/login";
        private const string _URL_AUTH_INFO = "https://passport.bilibili.com/api/oauth2/info";
        private const string _URL_AUTH_SSO = "https://passport.bilibili.com/api/login/sso";

        //private const string _URL_QRLOGIN_URL = "https://passport.bilibili.com/qrcode/getLoginUrl";
        //private const string _URL_QRLOGIN_INFO = "https://passport.bilibili.com/qrcode/getLoginInfo";
        //private const string _URL_QRLOGIN_CONFIRM = "https://passport.bilibili.com/qrcode/login/confirm";
        private const string _TOKEN_FILE_NAME = "token.json";
        #endregion

        #region members
        //members for oauth api
        private static string _access_token; //access token
        private static string _refresh_token; //refresh token
        private static int _appid; //迷之app id
        private static DateTime _expire_time; //access token失效时间
        private static uint _mid; //用户id
        private static string _uname; //用户名称
        private static string _userid; //用户id
        public static bool IsLogined { get { return !string.IsNullOrEmpty(_access_token); /*_mid != 0;*/ } }
        public static string AccessToken { get { return _access_token; } }
        public static string RefreshToken { get { return _refresh_token; } }
        public static int AppID { get { return _appid; } }
        public static DateTime ExpireTime { get { return _expire_time; } }
        public static uint MID { get { return _mid; } }
        public static string UName { get { return _uname; } }
        public static string UserID { get { return _userid; } }
        //members for sso
        private static Thread _sso_refresh_thread;
        //error handling 
        private static int _last_error_code;
        private static string _last_error_message;
        public static int LastErrorCode { get { return _last_error_code; } }
        public static string LastErrorMessage { get { return _last_error_message; } }
        #endregion

        #region private functions
        //计算api的sign
        public static string CalculateSign(Parameters param)
        {
            param.SortParameters();
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var param_str = param.BuildQueryString() + APPKEY_SECRET;
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(param_str));
            return util.Hex(hash);
        }
        //获取验证信息
        private static void _fetch_auth_info()
        {
            if (string.IsNullOrEmpty(_access_token)) throw new ArgumentNullException("Access Token could not be null or empty");
            var ns = new NetStream();
            var param = new Parameters();
            param.Add("access_token", _access_token);
            param.Add("appkey", APPKEY);
            param.Add("build", "513000");
            param.Add("mobi_app", "android");
            param.Add("platform", "android");
            param.Add("ts", (long)util.ToUnixTimestamp(DateTime.Now));
            param.Add("sign", CalculateSign(param));

            ns.UserAgent = ANDROID_USER_AGENT;
            try
            {
                ns.HttpGet(_URL_AUTH_INFO, urlParam: param);
                var response = ns.ReadResponseString();
                ns.Close();

                var json = JsonConvert.DeserializeObject(response) as JObject;
                int code = json.Value<int>("code");

                if (code != 0)
                {
                    Tracer.GlobalTracer.TraceError("Error code: " + code);
                    Tracer.GlobalTracer.TraceError(json.ToString());
                }
                else
                {
                    _appid = json["data"].Value<int>("appid");
                    var ts = util.FromUnixTimestamp(json.Value<long>("ts"));
                    _expire_time = ts.AddSeconds(json["data"].Value<int>("expires_in"));
                    _mid = json["data"].Value<uint>("mid");
                    _uname = json["data"].Value<string>("uname");
                    _userid = json["data"].Value<string>("userid");
                }
            }
            catch (Exception ex)
            {
                Tracer.GlobalTracer.TraceError(ex.ToString());
                throw ex;
            }
            finally
            {
                ns.Close();
            }
        }
        //获取rsa的加密密钥
        private static void _fetch_encrypt_key(out string hash, out string public_key)
        {
            var ns = new NetStream();
            var param = new Parameters();
            param.Add("appkey", APPKEY);
            param.Add("build", "513000");
            param.Add("mobi_app", "android");
            param.Add("platform", "android");
            param.Add("ts", (long)util.ToUnixTimestamp(DateTime.Now));
            param.Add("sign", CalculateSign(param));

            ns.UserAgent = ANDROID_USER_AGENT;
            try
            {
                ns.HttpPost(_URL_AUTH_GET_KEY, param);
                ns.HttpPostClose();

                var response = ns.ReadResponseString();
                var json = JsonConvert.DeserializeObject(response) as JObject;

                int code = json.Value<int>("code");
                if (code != 0)
                {
                    hash = null;
                    public_key = null;
                    _last_error_code = code;
                    _last_error_message = json.Value<string>("message");
                }
                else
                {
                    hash = json["data"].Value<string>("hash");
                    public_key = json["data"].Value<string>("key");
                }
            }
            catch (Exception ex)
            {
                Tracer.GlobalTracer.TraceError(ex.ToString());
                throw ex;
            }
            finally
            {
                ns.Close();
            }
        }
        //将密码进行rsa加密
        private static string _encrypt_password(string hash, string public_key, string password)
        {
            var param = RSA.ConvertFromPemPublicKey(public_key);
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
            rsa.ImportParameters(param);
            var encrypted_bytes = rsa.Encrypt(Encoding.UTF8.GetBytes(hash + password), false);
            return Convert.ToBase64String(encrypted_bytes);
        }

        private static bool _check_sso_expired()
        {
            var cookies = NetStream.DefaultCookieContainer.GetCookies(new Uri("https://passport.bilibili.com/"));
            if (cookies.Count == 0) return true;

            DateTime dede_user_expire_time = DateTime.MinValue;
            foreach (Cookie item in cookies)
            {
                if (item.Name == "DedeUserID")
                    dede_user_expire_time = item.Expires;
            }
            if (dede_user_expire_time < DateTime.Now) return true;
            else return false;
        }
        private static bool _sso_auth()
        {
            try
            {
                if (!_check_sso_expired()) return true; //sso auth succeeded from local cache file

                if (_sso_refresh_thread != null)
                    _sso_refresh_thread.Abort();

                var ns = new NetStream();
                ns.UserAgent = ANDROID_USER_AGENT;
                var param = new Parameters();
                param.Add("access_key", _access_token);
                param.Add("appkey", APPKEY);
                param.Add("build", "513000");
                param.Add("gourl", "https://www.bilibili.com/");
                param.Add("mobi_app", "android");
                param.Add("platform", "android");
                param.Add("ts", (long)util.ToUnixTimestamp(DateTime.Now));
                param.Add("sign", CalculateSign(param));

                ns.HttpGet(_URL_AUTH_SSO, urlParam: param);
                //var response = ns.ReadResponseString();
                ns.Close();

                //sso success, check cookie status;
                var cookies = NetStream.DefaultCookieContainer.GetCookies(new Uri("https://passport.bilibili.com/"));
                if (cookies.Count == 0)
                {
                    //cookie check failed
                    throw new InvalidOperationException("Could not found cookies after sso request");
                }
                else
                {
                    DateTime dede_user_expire_time = DateTime.MinValue;
                    foreach (Cookie item in cookies)
                    {
                        if (item.Name == "DedeUserID")
                            dede_user_expire_time = item.Expires;
                    }

                    if (dede_user_expire_time == DateTime.MinValue)
                    {
                        //dede user check failed
                        throw new InvalidOperationException("Could not found DedeUserID cookie after sso request");
                    }
                    else
                    {
                        //auth success, creating next cd
                        _sso_refresh_thread = new Thread(new ThreadStart(delegate
                        {
                            while (dede_user_expire_time > DateTime.Now)
                            {
                                var ts = (dede_user_expire_time - DateTime.Now).TotalMilliseconds;
                                var sleep_time = (int)Math.Min(ts, 3600000);
                                if (sleep_time > 0)
                                    Thread.Sleep(sleep_time);
                            }
                            _sso_auth();
                        }));
                        _sso_refresh_thread.IsBackground = true;
                        _sso_refresh_thread.Name = "SSO Refresh Thread";
                        _sso_refresh_thread.Start();
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                Tracer.GlobalTracer.TraceError(ex.ToString());
                //error encountered while sso request
                if (_sso_refresh_thread != null)
                {
                    //handling async mode in sso thread
                    _sso_refresh_thread = new Thread(new ThreadStart(delegate
                    {
                        Thread.Sleep(15000);
                        _sso_auth();
                    }));
                    _sso_refresh_thread.IsBackground = true;
                    _sso_refresh_thread.Name = "SSO Refresh Thread (Failed State)";
                    _sso_refresh_thread.Start();
                }
            }
            return !_check_sso_expired();

        }
        private static void _load_token()
        {
            if (!File.Exists(_TOKEN_FILE_NAME)) return;
            try
            {
                var data = File.ReadAllText(_TOKEN_FILE_NAME);
                if (string.IsNullOrEmpty(data)) return;
                var json = JsonConvert.DeserializeObject(data) as JObject;

                _access_token = json.Value<string>("access_token");
                _refresh_token = json.Value<string>("refresh_token");
                _appid = json.Value<int>("appid");
                _expire_time = util.FromUnixTimestamp(json.Value<long>("expire_time"));
                _mid = json.Value<uint>("mid");
                _uname = json.Value<string>("uname");
                _userid = json.Value<string>("userid");
            }
            catch (Exception ex)
            {
                Tracer.GlobalTracer.TraceError(ex.ToString());
            }
        }
        private static void _save_token()
        {
            var json = new JObject();
            json.Add("access_token", _access_token == null ? string.Empty : _access_token);
            json.Add("refresh_token", _refresh_token == null ? string.Empty : _refresh_token);
            json.Add("appid", _appid);
            json.Add("expire_time", (long)util.ToUnixTimestamp(_expire_time));
            json.Add("mid", _mid);
            json.Add("uname", _uname == null ? string.Empty : _uname);
            json.Add("userid", _userid == null ? string.Empty : _userid);

            File.WriteAllText(_TOKEN_FILE_NAME, JsonConvert.SerializeObject(json));
        }
        #endregion


        #region public functions
        public static bool Login(string username, string password)
        {
            string hash, public_key;
            _fetch_encrypt_key(out hash, out public_key);

            var ns = new NetStream();
            var param = new Parameters();
            param.Add("appkey", APPKEY);
            param.Add("build", "513000");
            param.Add("mobi_app", "android");
            param.Add("password", _encrypt_password(hash, public_key, password));
            param.Add("platform", "android");
            param.Add("ts", (long)util.ToUnixTimestamp(DateTime.Now));
            param.Add("username", username);
            param.Add("sign", CalculateSign(param));

            ns.UserAgent = ANDROID_USER_AGENT;
            try
            {
                ns.HttpPost(_URL_AUTH_LOGIN, param);
                ns.HttpPostClose();

                var response = ns.ReadResponseString();
                var json = JsonConvert.DeserializeObject(response) as JObject;

                int code = json.Value<int>("code");
                if (code != 0)
                {
                    _last_error_code = code;
                    _last_error_message = json.Value<string>("message");
                    return false;
                }
                else
                {
                    _access_token = json["data"].Value<string>("access_token");
                    var ts = util.FromUnixTimestamp(json.Value<long>("ts"));
                    _expire_time = ts.AddSeconds(json["data"].Value<long>("expires_in"));
                    _mid = json["data"].Value<uint>("mid");
                    _refresh_token = json["data"].Value<string>("refresh_token");

                    //initialize other param
                    try
                    {
                        _fetch_auth_info();
                    }
                    catch { }
                    while (!_sso_auth())
                        Thread.Sleep(3000);
                    //NetStream.SaveCookie();
                    _save_token();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.GlobalTracer.TraceError(ex.ToString());
                throw ex;
            }
            finally
            {
                ns.Close();
            }
        }
        static api()
        {
            //NetStream.LoadCookie();
            _load_token();
            if (!string.IsNullOrEmpty(_access_token))
                _sso_auth();
        }
        #endregion
    }
    public class ocr
    {
        protected ocr() { }
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCR", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCR(string file, int type);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRBarCodes(string file, int type);
        [DllImport("ocr\\AspriseOCR", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);
    }
}
