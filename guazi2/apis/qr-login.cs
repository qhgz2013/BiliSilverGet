using guazi2.NetUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace guazi2
{
    public partial class api
    {
        private const string _qrcode_url = "https://passport.bilibili.com/qrcode/getLoginUrl";
        private const string _qrcode_info = "https://passport.bilibili.com/qrcode/getLoginInfo";

        /// <summary>
        /// 获取二维码登录的授权key和url
        /// </summary>
        /// <returns></returns>
        private static string __oauth_key = null;
        public static string OAuthKey { get { return __oauth_key; } }
        private static long __oauth_qr_timestamp = 0;
        public static long OAuthQRTimestamp { get { return __oauth_qr_timestamp; } }
        private static string __oauth_url = null;
        public static string OAuthUrl { get { return __oauth_url; } }
        private const int _oauth_qr_validate_time = 180; //3 minutes
        public const int OAuthQrValidateTime = _oauth_qr_validate_time;
        private const int _oauth_qr_check_time = 3000; //3 seconds
        public static string GetLoginQRUrl()
        {
            Tracer.GlobalTracer.TraceInfo("api.GetLoginQRCode called: out string oauthKey");
            __oauth_qr_timestamp = 0;
            __oauth_key = null;
            __oauth_url = null;

            var ns = new NetStream();
            var xhr_param = new Parameters();
            xhr_param.Add("Origin", "https://passport.bilibili.com");
            xhr_param.Add("Referer", "https://passport.bilibili.com/login");
            xhr_param.Add("X-Requested-With", "XMLHttpRequest");

            ns.RetryTimes = 3;
            ns.RetryDelay = 1000;
            try
            {
                ns.HttpGet(_qrcode_url, xhr_param);
                var response = ns.ReadResponseString();
                ns.Close();
                Tracer.GlobalTracer.TraceInfo(response);

                var json = JsonConvert.DeserializeObject(response) as JObject;
                __oauth_key = json["data"].Value<string>("oauthKey");
                __oauth_url = json["data"].Value<string>("url");
                __oauth_qr_timestamp = (long)util.ToUnixTimestamp(DateTime.Now);
                return __oauth_url;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ns.Close();
            }
        }
        /// <summary>
        /// 二维码登陆（阻塞式，直到登陆成功）
        /// </summary>
        public static bool QRLogin()
        {
            Tracer.GlobalTracer.TraceInfo("api.QRLogin called: void");
            if (string.IsNullOrEmpty(__oauth_key) || __oauth_qr_timestamp == 0 || string.IsNullOrEmpty(__oauth_url)) return false;

            var ns = new NetStream();
            var xhr_param = new Parameters();
            xhr_param.Add("Origin", "https://passport.bilibili.com");
            xhr_param.Add("Referer", "https://passport.bilibili.com/login");
            xhr_param.Add("X-Requested-With", "XMLHttpRequest");

            var post_param = new Parameters();
            post_param.Add("oauthKey", __oauth_key);
            post_param.Add("gourl", "");

            do
            {
                try
                {
                    Tracer.GlobalTracer.TraceInfo("Posting QRLogin HeartBeat ts=" + DateTime.Now.ToString());
                    ns.HttpPost(_qrcode_info, post_param, headerParam: xhr_param);
                    var response = ns.ReadResponseString();

                    var json = JsonConvert.DeserializeObject(response) as JObject;
                    var status = json.Value<bool>("status");
                    if (status) return true;
                }
                catch (WebException) { }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    ns.Close();
                }
                Thread.Sleep(_oauth_qr_check_time);
            } while (util.FromUnixTimestamp(__oauth_qr_timestamp + _oauth_qr_validate_time) > DateTime.Now);
            return false;
        }
    }
}
