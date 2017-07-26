﻿// net-util.cs
//
// 整合命令发送同步/异步HTTP请求
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Threading;

namespace guazi2
{
    namespace NetUtils
    {
        public partial class NetStream
        {
            #region Constants
            //请求方法
            public const string DEFAULT_GET_METHOD = "GET";
            public const string DEFAULT_POST_METHOD = "POST";
            public const string DEFAULT_HEAD_METHOD = "HEAD";

            //HTTP 1.1 请求头的内容
            public const string STR_SETCOOKIE = "Set-Cookie";
            public const string STR_ACCEPT_ENCODING = "Accept-Encoding";
            public const string STR_ACCEPT_LANGUAGE = "Accept-Language";
            public const string STR_HOST = "Host";
            public const string STR_CONNECTION = "Connection";
            public const string STR_ACCEPT = "Accept";
            public const string STR_USER_AGENT = "User-Agent";
            public const string STR_REFERER = "Referer";
            public const string STR_CONTENT_TYPE = "Content-Type";
            public const string STR_CONTENT_LENGTH = "Content-Length";
            public const string STR_ORIGIN = "Origin";
            public const string STR_COOKIE = "Cookie";
            public const string STR_EXPECT = "Expect";
            public const string STR_DATE = "Date";
            public const string STR_IF_MODIFIED_SINCE = "If-Modified-Since";
            public const string STR_RANGE = "Range";
            public const string STR_TRANSFER_ENCODING = "Transfer-Encoding";

            public const string STR_CONNECTION_KEEP_ALIVE = "keep-alive";
            public const string STR_CONNECTION_CLOSE = "close";
            public const string STR_ACCEPT_ENCODING_GZIP = "gzip";
            public const string STR_ACCEPT_ENCODING_DEFLATE = "deflate";

            //默认设置

            //默认接受的数据流类型
            public const string DEFAULT_ACCEPT_ENCODING = STR_ACCEPT_ENCODING_GZIP + ", " + STR_ACCEPT_ENCODING_DEFLATE;
            //默认接受的数据类型（文件类型）
            public const string DEFAULT_ACCEPT = "*/*";
            //默认超时时间（ms）
            public const int DEFAULT_TIMEOUT = int.MaxValue;
            public const int DEFAULT_READ_WRITE_TIMEOUT = 30000;
            //默认的代理url
            public const string DEFAULT_PROXY_URL = "";
            //默认的user agent（截取自chrome）
            public const string DEFAULT_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36";
            //默认发送的数据类型（MIME type）
            public const string DEFAULT_CONTENT_TYPE_PARAM = "application/x-www-form-urlencoded; charset=" + DEFAULT_ENCODING;
            public const string DEFAULT_CONTENT_TYPE_BINARY = "application/octet-stream";
            //默认接受的语言
            public const string DEFAULT_ACCEPT_LANGUAGE = "zh/cn,zh,en";
            //默认的文字编码类型
            public const string DEFAULT_ENCODING = "utf-8";

            //默认连接重试次数
            public const uint DEFAULT_RETRY_TIMES = 0;
            //默认重试的等待时间（ms）
            public const uint DEFAULT_RETRY_DELAY = 0;

            //默认保存cookie的文件名
            public const string DEFAULT_COOKIE_FILE_NAME = "cookie.dat";
            //默认的TCP连接数
            public const int DEFAULT_TCP_CONNECTION = 1000;
            #endregion

            #region Cookie Segment
            //默认保存cookie的容器
            public static CookieContainer DefaultCookieContainer = new CookieContainer();
            /// <summary>
            /// 从文件中读取cookie
            /// </summary>
            /// <param name="file">文件路径，若此处留空则使用默认文件名</param>
            public static void LoadCookie(string file = DEFAULT_COOKIE_FILE_NAME)
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.Exists || fi.Length > 0)
                    {
                        var stream = fi.OpenRead();
                        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        DefaultCookieContainer = (CookieContainer)formatter.Deserialize(stream);
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("在读取cookie文件时捕获到异常:\n" + ex.ToString());
                }
            }

            /// <summary>
            /// 从文件中写入cookie
            /// </summary>
            /// <param name="file">文件路径，若此处留空则使用默认文件名</param>
            public static void SaveCookie(string file = DEFAULT_COOKIE_FILE_NAME)
            {
                try
                {
                    var stream = File.Create(file);
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(stream, DefaultCookieContainer);
                    stream.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("在写入cookie文件时捕获到异常:\n" + ex.ToString());
                }
            }
            #endregion

        }
        #region Parameter Segment
        public class Parameters : ICollection<KeyValuePair<string, string>>
        {
            private List<KeyValuePair<string, string>> _list;
            public Parameters()
            {
                _list = new List<KeyValuePair<string, string>>();
            }
            /// <summary>
            /// 添加参数
            /// </summary>
            /// <typeparam name="T">参数类型</typeparam>
            /// <param name="key">参数名称</param>
            /// <param name="value">参数的值</param>
            public void Add<T>(string key, T value)
            {
                _list.Add(new KeyValuePair<string, string>(key, value.ToString()));
            }
            /// <summary>
            /// 对所有参数按名称进行排序
            /// </summary>
            /// <param name="desc">是否使用倒序排序（默认为正序）</param>
            public void SortParameters(bool desc = false)
            {
                var n = new List<KeyValuePair<string, string>>();
                IOrderedEnumerable<KeyValuePair<string, string>> sec = null;
                if (desc) sec = from KeyValuePair<string, string> item in _list orderby item.Key ascending select item;
                else sec = from KeyValuePair<string, string> item in _list orderby item.Key descending select item;
                foreach (var item in sec)
                {
                    n.Add(item);
                }
                _list = n;
            }
            /// <summary>
            /// 构造url的查询参数
            /// </summary>
            /// <param name="enableUrlEncode">是否使用url转义</param>
            /// <returns>与参数等价的query string</returns>
            public string BuildQueryString(bool enableUrlEncode = true)
            {
                var sb = new StringBuilder();
                foreach (var item in _list)
                {
                    sb.Append(item.Key);
                    if (!string.IsNullOrEmpty(item.Key)) sb.Append('=');
                    if (enableUrlEncode) sb.Append(Uri.EscapeDataString(item.Value));
                    else sb.Append(item.Value);
                    sb.Append('&');
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }
            /// <summary>
            /// 移除首个匹配项
            /// </summary>
            /// <param name="key">参数名称</param>
            /// <returns>是否移除成功</returns>
            public bool Remove(string key)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].Key == key)
                    {
                        _list.RemoveAt(i);
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// 移除指定下标的参数
            /// </summary>
            /// <param name="index">下标编号</param>
            /// <returns>是否移除成功</returns>
            public bool RemoveAt(int index)
            {
                if (index < _list.Count)
                {
                    _list.RemoveAt(index);
                    return true;
                }
                return false;
            }
            /// <summary>
            /// 移除所有匹配项
            /// </summary>
            /// <param name="key">参数名称</param>
            /// <returns>是否移除成功</returns>
            public bool RemoveAll(string key)
            {
                bool suc = false;
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].Key == key)
                    {
                        _list.RemoveAt(i);
                        suc = true;
                    }
                }
                return suc;
            }
            /// <summary>
            /// 列表中是否包含指定名称的参数
            /// </summary>
            /// <param name="key">参数名称</param>
            /// <returns>是否存在该参数</returns>
            public bool Contains(string key)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].Key == key)
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// 返回参数个数
            /// </summary>
            int ICollection<KeyValuePair<string, string>>.Count
            {
                get
                {
                    return _list.Count;
                }
            }
            /// <summary>
            /// 是否只读
            /// </summary>
            bool ICollection<KeyValuePair<string, string>>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            /// <summary>
            /// 添加参数
            /// </summary>
            /// <param name="item">要添加的参数</param>
            void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
            {
                _list.Add(item);
            }
            /// <summary>
            /// 清空参数列表
            /// </summary>
            void ICollection<KeyValuePair<string, string>>.Clear()
            {
                _list.Clear();
            }
            /// <summary>
            /// 是否包含某个参数（名称和数值全匹配）
            /// </summary>
            /// <param name="item">参数</param>
            /// <returns>是否存在该参数</returns>
            bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
            {
                return _list.Contains(item);
            }
            /// <summary>
            /// 将列表复制到数组
            /// </summary>
            /// <param name="array">输出的数组</param>
            /// <param name="arrayIndex">要复制的下标开始点</param>
            void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }
            /// <summary>
            /// 获取枚举器
            /// </summary>
            /// <returns>列表的枚举器</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            /// <summary>
            /// 获取枚举器
            /// </summary>
            /// <returns>列表的枚举器</returns>
            IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
            /// <summary>
            /// 移除匹配的参数
            /// </summary>
            /// <param name="item">参数</param>
            /// <returns></returns>
            bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
            {
                return _list.Remove(item);
            }

            private string GetItems(int index)
            {
                if (index < 0 || index >= _list.Count) return string.Empty;
                return _list[index].Value;
            }
            private string GetItems(string name)
            {
                foreach (var item in _list)
                {
                    if (item.Key == name)
                        return item.Value;
                }
                return string.Empty;
            }
            private void SetItem(int index, string value)
            {
                if (index < 0 || index >= _list.Count) return;
                SetItem(index, _list[index].Key, value);
            }
            private void SetItem(int index, string key, string value)
            {
                if (index < 0 || index >= _list.Count) return;
                _list[index] = new KeyValuePair<string, string>(key, value);
            }
            private void SetItem(int index, KeyValuePair<string, string> data)
            {
                SetItem(index, data.Key, data.Value);
            }
            private void SetItem(string key, string value)
            {
                int index = _list.FindIndex((x) => { if (x.Key == key) return true; else return false; });
                if (index == -1) throw new KeyNotFoundException(key);
                _list[index] = new KeyValuePair<string, string>(key, value);
            }
            private void SetItem(KeyValuePair<string, string> data)
            {
                SetItem(data.Key, data.Value);
            }
            public string this[int index]
            {
                get
                {
                    return GetItems(index);
                }
                set
                {
                    SetItem(index, value);
                }
            }
            public override string ToString()
            {
                return BuildQueryString();
            }
        }
        #endregion
        public partial class NetStream : IDisposable
        {
            //static initialize setting
            static NetStream()
            {
                LoadCookie(); //读取cookie
                ServicePointManager.DefaultConnectionLimit = DEFAULT_TCP_CONNECTION;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.MaxServicePointIdleTime = 2000;
                ServicePointManager.SetTcpKeepAlive(false, 0, 0);
            }
            private static bool _enableTracing = false;
            private HttpWebRequest _http_request;
            private HttpWebResponse _http_response;
            public HttpWebRequest HTTP_Request { get { return _http_request; } set { _http_request = value; } }
            public HttpWebResponse HTTP_Response { get { return _http_response; } set { _http_response = value; } }
            public Stream Stream { get; set; }
            public bool UseCookie { get; set; }
            public WebProxy Proxy { get; set; }
            public int TimeOut { get; set; }
            public string AcceptEncoding { get; set; }
            public string AcceptLanguage { get; set; }
            public string Accept { get; set; }
            public string UserAgent { get; set; }
            public string ContentType { get; set; }
            public int RetryTimes { get; set; }
            public int RetryDelay { get; set; }
            public int ReadWriteTimeOut { get; set; }
            public static CookieCollection ParseCookie(string header, string defaultDomain)
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("ParseCookie called: string header=" + header + ", string defaultDomain=" + defaultDomain);
                }
                if (string.IsNullOrEmpty(defaultDomain)) throw new ArgumentNullException(defaultDomain);

                var ret = new CookieCollection();
                int i = 0;
                var arg = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(header)) return ret;

                int len = header.Length;
                while (i < len)
                {
                    _skipChar(header, ref i);
                    string name = _parseCookieValue(header, ref i, ";;name");
                    i += 1;
                    _skipChar(header, ref i);
                    string value = _parseCookieValue(header, ref i, ";;value");
                    while (i < len && header[i] != ',')
                    {
                        i++;
                        _skipChar(header, ref i);
                        string pkey = _parseCookieValue(header, ref i, ";;name").ToLower();
                        if (i < len && header[i] != ',') i++;
                        _skipChar(header, ref i);
                        string pvalue = _parseCookieValue(header, ref i, pkey);
                        _skipChar(header, ref i);
                        arg.Add(pkey, pvalue);
                    }

                    if (i >= header.Length || header[i] == ',')
                    {
                        i++;
                        bool skipflg = false;
                        if (!arg.ContainsKey("path")) skipflg = true;
                        if (!skipflg)
                        {
                            var domain = arg.ContainsKey("domain") ? arg["domain"] : defaultDomain;
                            var c = new Cookie(name, value, arg["path"], domain);
                            c.HttpOnly = arg.ContainsKey("httponly");

                            if (arg.ContainsKey("max-age"))
                                c.Expires = DateTime.Now.AddSeconds(int.Parse(arg["max-age"]));
                            else if (arg.ContainsKey("maxage"))
                                c.Expires = DateTime.Now.AddSeconds(int.Parse(arg["maxage"]));
                            else if (arg.ContainsKey("expires"))
                                c.Expires = _parseCookieExpireTime(arg["expires"]);
                            else skipflg = true;

                            if (!skipflg) ret.Add(c);
                        }
                        arg.Clear();
                    }
                }
                return ret;
            }
            private static void _skipChar(string header, ref int index)
            {
                while (index < header.Length && header[index] == ' ')
                    index++;
            }
            private static string _parseCookieValue(string header, ref int index, string propertyName)
            {
                if (_enableTracing)
                {
                    //Tracer.GlobalTracer.TraceInfo("_parseCookieValue called");
                }
                string value = string.Empty;
                string limitstr = string.Empty;
                if (propertyName == ";;name") limitstr = ";,=";
                else if (propertyName == ";;value") limitstr = ";,";
                else if (propertyName == "expires") limitstr = ";=";
                else if (propertyName == "httponly") return string.Empty;
                else limitstr = ";,";
                while (index < header.Length && !limitstr.Contains(header[index]))
                {
                    value += header[index];
                    index++;
                }
                value = value.Trim('"');
                return value;
            }
            private static DateTime _parseCookieExpireTime(string str)
            {
                if (_enableTracing)
                {
                    //Tracer.GlobalTracer.TraceInfo("_parseCookieExpireTime called");
                }
                int i = 4;
                _skipChar(str, ref i);
                int day = int.Parse(str.Substring(i, 2));
                i += 3;
                string smonth = str.Substring(i, 3);
                const string csmonth = "JanFebMarAprMayJunJulAugSepOctNovDec";
                int month = csmonth.IndexOf(smonth) / 3 + 1;
                if (month < 1 || month > 12) throw new ArgumentOutOfRangeException("Could not parse month string: " + smonth);
                i += 4;
                int year;
                if (int.TryParse(str.Substring(i, 4), out year))
                {
                    i += 5;
                }
                else if (int.TryParse(str.Substring(i, 2), out year))
                {
                    i += 3;
                    year += DateTime.Now.Year / 100 * 100;
                }
                else
                    throw new FormatException("Year format incorrect");

                _skipChar(str, ref i);
                int hour = int.Parse(str.Substring(i, 2));
                i += 3;
                int minute = int.Parse(str.Substring(i, 2));
                i += 3;
                int second = int.Parse(str.Substring(i, 2));
                i += 3;
                _skipChar(str, ref i);

                bool gmt_marker = false;
                if (i < str.Length)
                {
                    var marker = str.Substring(i, 3);
                    if (string.Equals(marker, "GMT", StringComparison.CurrentCultureIgnoreCase)) gmt_marker = true;
                    i += 4;
                }
                var date = new DateTime(0, gmt_marker ? DateTimeKind.Utc : DateTimeKind.Unspecified);
                date = date.AddYears(year - 1).AddMonths(month - 1).AddDays(day - 1);
                date = date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                return date;
            }

            private static void _add_param_to_request_header(Parameters param, ref HttpWebRequest request)
            {
                if (param != null)
                {
                    foreach (var e in param)
                    {
                        switch (e.Key)
                        {
                            case STR_ACCEPT:
                                request.Accept = e.Value;
                                break;
                            case STR_CONNECTION:
                                switch (e.Value)
                                {
                                    case STR_CONNECTION_KEEP_ALIVE:
                                        request.Connection = "";
                                        request.KeepAlive = true;
                                        break;
                                    case STR_CONNECTION_CLOSE:
                                        request.KeepAlive = false;
                                        break;
                                    default:
                                        throw new ArgumentException("Invalid headerParam: " + STR_CONNECTION);
                                }
                                break;
                            case STR_CONTENT_LENGTH:
                                request.ContentLength = int.Parse(e.Value);
                                break;
                            case STR_CONTENT_TYPE:
                                request.ContentType = e.Value;
                                break;
                            case STR_EXPECT:
                                request.Expect = e.Value;
                                break;
                            case STR_DATE:
                                request.Date = DateTime.Parse(e.Value);
                                break;
                            case STR_HOST:
                                request.Host = e.Value;
                                break;
                            case STR_IF_MODIFIED_SINCE:
                                request.IfModifiedSince = DateTime.Parse(e.Value);
                                break;
                            case STR_RANGE:
                                string[] rangesplit = e.Value.Split('-');
                                if (rangesplit.Length != 2) throw new ArgumentException("Range format incorrect");
                                if (string.IsNullOrEmpty(rangesplit[0]))
                                    if (string.IsNullOrEmpty(rangesplit[1]))
                                        throw new ArgumentNullException("Range");
                                    else
                                        request.AddRange(-long.Parse(rangesplit[1]));
                                else
                                    if (string.IsNullOrEmpty(rangesplit[1]))
                                    request.AddRange(long.Parse(rangesplit[1]));
                                else
                                    request.AddRange(long.Parse(rangesplit[0]), long.Parse(rangesplit[1]));
                                break;
                            case STR_REFERER:
                                request.Referer = e.Value;
                                break;
                            case STR_TRANSFER_ENCODING:
                                request.TransferEncoding = e.Value;
                                break;
                            case STR_USER_AGENT:
                                request.UserAgent = e.Value;
                                break;
                            default:
                                request.Headers.Add(e.Key, e.Value);
                                break;
                        }
                    }
                }
            }
            public void HttpGet(string url, Parameters headerParam = null, Parameters urlParam = null, long range = -1)
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("HTTP GET " + url);
                }
                int cur_times = 0;
                do
                {
                    try
                    {
                        var post_url = url;
                        if (urlParam != null) post_url += "?" + urlParam.BuildQueryString();

                        HTTP_Request = (HttpWebRequest)WebRequest.Create(post_url);
                        HTTP_Request.KeepAlive = true;
                        HTTP_Request.ConnectionGroupName = "defaultConnectionGroup";
                        _add_param_to_request_header(headerParam, ref _http_request);

                        var keyList = HTTP_Request.Headers.AllKeys.ToList();
                        if (!keyList.Contains(STR_ACCEPT)) HTTP_Request.Accept = Accept;
                        if (!keyList.Contains(STR_ACCEPT_ENCODING)) HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding);
                        if (!keyList.Contains(STR_ACCEPT_LANGUAGE)) HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage);
                        if (!keyList.Contains(STR_USER_AGENT)) HTTP_Request.UserAgent = UserAgent;

                        if (Proxy != null) HTTP_Request.Proxy = Proxy;
                        if (UseCookie && !keyList.Contains(STR_COOKIE)) HTTP_Request.CookieContainer = DefaultCookieContainer;

                        HTTP_Request.Method = DEFAULT_GET_METHOD;
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeOut;
                        HTTP_Request.Timeout = TimeOut;

                        HTTP_Request.ContentLength = 0;

                        if (range >= 0)
                        {
                            if (keyList.Contains(STR_RANGE))
                            {
                                //throw new InvalidOperationException("HTTP header has contained Range parameter");
                                if (_enableTracing)
                                    Tracer.GlobalTracer.TraceWarning("HTTP头已经包含Range信息，range参数将会忽略");
                            }
                            else
                                HTTP_Request.AddRange(range);
                        }

                        HTTP_Response = (HttpWebResponse)HTTP_Request.GetResponse();

                        DefaultCookieContainer.Add(ParseCookie(HTTP_Response.Headers[STR_SETCOOKIE], HTTP_Response.ResponseUri.Host));

                        //if (HTTP_Response.StatusCode == HttpStatusCode.OK || HTTP_Response.StatusCode == HttpStatusCode.PartialContent)
                        //{
                        switch (HTTP_Response.ContentEncoding)
                        {
                            case STR_ACCEPT_ENCODING_GZIP:
                                Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                break;
                            case STR_ACCEPT_ENCODING_DEFLATE:
                                Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                break;
                            default:
                                Stream = HTTP_Response.GetResponseStream();
                                break;
                        }
                        //}
                        break;
                    }
                    catch (ThreadAbortException ex) { throw ex; }
                    catch (WebException ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;

                        if (ex.Response != null)
                        {
                            try
                            {
                                HTTP_Response = (HttpWebResponse)ex.Response;
                                switch (HTTP_Response.ContentEncoding)
                                {
                                    case STR_ACCEPT_ENCODING_GZIP:
                                        Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    case STR_ACCEPT_ENCODING_DEFLATE:
                                        Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    default:
                                        Stream = HTTP_Response.GetResponseStream();
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                } while (true);
            }
            public delegate void HttpFinishedResponseCallback(NetStream ns, object state);
            private struct _tmp_struct { public HttpFinishedResponseCallback cb; public object state; public _tmp_struct(HttpFinishedResponseCallback c, object s) { cb = c; state = s; } }
            public void HttpGetAsync(string url, HttpFinishedResponseCallback callback, object state = null, Parameters headerParam = null, Parameters urlParam = null, long range = -1)
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("HTTP GET " + url);
                }
                int cur_times = 0;
                do
                {
                    try
                    {
                        var post_url = url;
                        if (urlParam != null) post_url += "?" + urlParam.BuildQueryString();

                        HTTP_Request = (HttpWebRequest)WebRequest.Create(post_url);
                        HTTP_Request.KeepAlive = true;
                        HTTP_Request.ConnectionGroupName = "defaultConnectionGroup";
                        _add_param_to_request_header(headerParam, ref _http_request);

                        var keyList = HTTP_Request.Headers.AllKeys.ToList();
                        if (!keyList.Contains(STR_ACCEPT)) HTTP_Request.Accept = Accept;
                        if (!keyList.Contains(STR_ACCEPT_ENCODING)) HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding);
                        if (!keyList.Contains(STR_ACCEPT_LANGUAGE)) HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage);
                        if (!keyList.Contains(STR_USER_AGENT)) HTTP_Request.UserAgent = UserAgent;

                        HTTP_Request.Proxy = Proxy;
                        if (UseCookie && !keyList.Contains(STR_COOKIE)) HTTP_Request.CookieContainer = DefaultCookieContainer;

                        HTTP_Request.Method = DEFAULT_GET_METHOD;
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeOut;
                        HTTP_Request.Timeout = TimeOut;

                        //HTTP_Request.ContentLength = 0;

                        if (range >= 0)
                        {
                            if (keyList.Contains(STR_RANGE))
                            {
                                if (_enableTracing)
                                    Tracer.GlobalTracer.TraceWarning("HTTP头已经包含Range信息，range参数将会忽略");
                            }
                            else
                                HTTP_Request.AddRange(range);
                        }

                        HTTP_Response = null;
                        Stream = null;
                        HTTP_Request.BeginGetResponse(_httpGetAsyncResponse, new _tmp_struct (callback, state));
                        break;
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                } while (true);
            }
            private void _httpGetAsyncResponse(IAsyncResult iar)
            {
                try
                {

                    int cur_times = 0;
                    do
                    {
                        try
                        {
                            if (HTTP_Request == null) break;
                            HTTP_Response = (HttpWebResponse)HTTP_Request.EndGetResponse(iar);
                            if (HTTP_Response != null)
                            {
                                DefaultCookieContainer.Add(ParseCookie(HTTP_Response.Headers[STR_SETCOOKIE], HTTP_Response.ResponseUri.Host));

                                switch (HTTP_Response.ContentEncoding)
                                {
                                    case STR_ACCEPT_ENCODING_GZIP:
                                        Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    case STR_ACCEPT_ENCODING_DEFLATE:
                                        Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    default:
                                        Stream = HTTP_Response.GetResponseStream();
                                        break;
                                }
                                break;
                            }
                        }
                        catch (ThreadAbortException) { break; /* throw ex; */}
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.RequestCanceled) break; // throw ex;

                            if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                            cur_times++;
                            if (RetryTimes >= 0 && cur_times > RetryTimes) break;// throw ex;
                            if (RetryDelay > 0) Thread.Sleep(RetryDelay);

                            if (ex.Response != null)
                            {
                                try
                                {
                                    HTTP_Response = (HttpWebResponse)ex.Response;
                                    DefaultCookieContainer.Add(ParseCookie(HTTP_Response.Headers[STR_SETCOOKIE], HTTP_Response.ResponseUri.Host));
                                    switch (HTTP_Response.ContentEncoding)
                                    {
                                        case STR_ACCEPT_ENCODING_GZIP:
                                            Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                            break;
                                        case STR_ACCEPT_ENCODING_DEFLATE:
                                            Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                            break;
                                        default:
                                            Stream = HTTP_Response.GetResponseStream();
                                            break;
                                    }
                                }
                                catch (Exception) { }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                            cur_times++;
                            if (RetryTimes >= 0 && cur_times > RetryTimes) break;//throw ex;
                            if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                        }
                    } while (true);
                }
                finally
                {
                    try
                    {
                        var data = (_tmp_struct)iar.AsyncState;
                        data.cb.Invoke(this, data.state);
                    }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                    }
                }
            }
            public void HttpHead(string url, Parameters headerParam = null, Parameters urlParam = null)
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("HTTP HEAD " + url);
                }
                int cur_times = 0;
                do
                {
                    try
                    {
                        var post_url = url;
                        if (urlParam != null) post_url += "?" + urlParam.BuildQueryString();

                        HTTP_Request = (HttpWebRequest)WebRequest.Create(post_url);
                        HTTP_Request.KeepAlive = true;
                        HTTP_Request.ConnectionGroupName = "defaultConnectionGroup";
                        _add_param_to_request_header(headerParam, ref _http_request);

                        var keyList = HTTP_Request.Headers.AllKeys.ToList();
                        if (!keyList.Contains(STR_ACCEPT)) HTTP_Request.Accept = Accept;
                        if (!keyList.Contains(STR_ACCEPT_ENCODING)) HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding);
                        if (!keyList.Contains(STR_ACCEPT_LANGUAGE)) HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage);
                        if (!keyList.Contains(STR_USER_AGENT)) HTTP_Request.UserAgent = UserAgent;

                        if (Proxy != null) HTTP_Request.Proxy = Proxy;
                        if (UseCookie && !keyList.Contains(STR_COOKIE)) HTTP_Request.CookieContainer = DefaultCookieContainer;

                        HTTP_Request.Method = DEFAULT_HEAD_METHOD;
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeOut;
                        HTTP_Request.Timeout = TimeOut;

                        HTTP_Request.ContentLength = 0;

                        HTTP_Response = (HttpWebResponse)HTTP_Request.GetResponse();

                        DefaultCookieContainer.Add(ParseCookie(HTTP_Response.Headers[STR_SETCOOKIE], HTTP_Response.ResponseUri.Host));

                        Stream = null;
                        break;
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (WebException ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;

                        if (ex.Response != null)
                        {
                            try
                            {
                                HTTP_Response = (HttpWebResponse)ex.Response;
                                switch (HTTP_Response.ContentEncoding)
                                {
                                    case STR_ACCEPT_ENCODING_GZIP:
                                        Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    case STR_ACCEPT_ENCODING_DEFLATE:
                                        Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    default:
                                        Stream = HTTP_Response.GetResponseStream();
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                } while (true);
            }
            public void HttpPost(string url, byte[] postData, string postContentType = DEFAULT_CONTENT_TYPE_BINARY, Parameters headerParam = null, Parameters urlParam = null, long range = -1)
            {
                var stream = HttpPost(url, postData.Length, postContentType, headerParam, urlParam, range);
                stream.Write(postData, 0, postData.Length);
                stream.Close();
                HttpPostClose();
            }
            public void HttpPost(string url, Parameters postParam, string postContentType = DEFAULT_CONTENT_TYPE_PARAM, Parameters headerParam = null, Parameters urlParam = null, long range = -1)
            {
                HttpPost(url, Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(postParam.BuildQueryString()), postContentType, headerParam, urlParam, range);
            }
            public Stream HttpPost(string url, long postLength, string postContentType = DEFAULT_CONTENT_TYPE_BINARY, Parameters headerParam = null, Parameters urlParam = null, long range = -1)
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("HTTP POST " + url);
                }
                int cur_times = 0;
                do
                {
                    try
                    {
                        var post_url = url;
                        if (urlParam != null) post_url += "?" + urlParam.BuildQueryString();

                        HTTP_Request = (HttpWebRequest)WebRequest.Create(post_url);
                        HTTP_Request.KeepAlive = true;
                        HTTP_Request.ConnectionGroupName = "defaultConnectionGroup";

                        _add_param_to_request_header(headerParam, ref _http_request);
                        var keyList = HTTP_Request.Headers.AllKeys.ToList();
                        if (!keyList.Contains(STR_ACCEPT)) HTTP_Request.Accept = Accept;
                        if (!keyList.Contains(STR_ACCEPT_ENCODING)) HTTP_Request.Headers.Add(STR_ACCEPT_ENCODING, AcceptEncoding);
                        if (!keyList.Contains(STR_ACCEPT_LANGUAGE)) HTTP_Request.Headers.Add(STR_ACCEPT_LANGUAGE, AcceptLanguage);
                        if (!keyList.Contains(STR_USER_AGENT)) HTTP_Request.UserAgent = UserAgent;

                        if (Proxy != null) HTTP_Request.Proxy = Proxy;
                        if (UseCookie && !keyList.Contains(STR_COOKIE)) HTTP_Request.CookieContainer = DefaultCookieContainer;

                        HTTP_Request.Method = DEFAULT_POST_METHOD;
                        HTTP_Request.ReadWriteTimeout = ReadWriteTimeOut;
                        HTTP_Request.Timeout = TimeOut;

                        HTTP_Request.ContentLength = postLength;
                        HTTP_Request.ContentType = postContentType;

                        if (range >= 0)
                        {
                            if (keyList.Contains(STR_RANGE))
                            {
                                //throw new InvalidOperationException("HTTP header has contained Range parameter");
                                if (_enableTracing)
                                    Tracer.GlobalTracer.TraceWarning("HTTP头已经包含Range信息，range参数将会忽略");
                            }
                            else
                                HTTP_Request.AddRange(range);
                        }
                        return HTTP_Request.GetRequestStream();

                    }
                    catch (ThreadAbortException) { throw; }
                    catch (WebException ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;

                        if (ex.Response != null)
                        {
                            try
                            {
                                HTTP_Response = (HttpWebResponse)ex.Response;
                                switch (HTTP_Response.ContentEncoding)
                                {
                                    case STR_ACCEPT_ENCODING_GZIP:
                                        Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    case STR_ACCEPT_ENCODING_DEFLATE:
                                        Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    default:
                                        Stream = HTTP_Response.GetResponseStream();
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                } while (true);
            }
            public void HttpPostClose()
            {
                if (_enableTracing)
                {
                    Tracer.GlobalTracer.TraceInfo("HttpPostClose called");
                }
                if (HTTP_Request == null) return;
                try
                {
                    var stream = HTTP_Request.GetRequestStream();
                    if (stream.CanWrite)
                        stream.Close();
                }
                catch (Exception)
                {
                }

                int cur_times = 0;
                do
                {
                    try
                    {
                        HTTP_Response = (HttpWebResponse)HTTP_Request.GetResponse();

                        DefaultCookieContainer.Add(ParseCookie(HTTP_Response.Headers[STR_SETCOOKIE], HTTP_Response.ResponseUri.Host));

                        //if (HTTP_Response.StatusCode == HttpStatusCode.OK || HTTP_Response.StatusCode == HttpStatusCode.PartialContent)
                        //{
                        switch (HTTP_Response.ContentEncoding)
                        {
                            case STR_ACCEPT_ENCODING_GZIP:
                                Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                break;
                            case STR_ACCEPT_ENCODING_DEFLATE:
                                Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                break;
                            default:
                                Stream = HTTP_Response.GetResponseStream();
                                break;
                        }
                        //}
                        break;
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (WebException ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;

                        if (ex.Response != null)
                        {
                            try
                            {
                                HTTP_Response = (HttpWebResponse)ex.Response;
                                switch (HTTP_Response.ContentEncoding)
                                {
                                    case STR_ACCEPT_ENCODING_GZIP:
                                        Stream = new System.IO.Compression.GZipStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    case STR_ACCEPT_ENCODING_DEFLATE:
                                        Stream = new System.IO.Compression.DeflateStream(HTTP_Response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                                        break;
                                    default:
                                        Stream = HTTP_Response.GetResponseStream();
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                    catch (Exception ex)
                    {
                        if (_enableTracing) Tracer.GlobalTracer.TraceError(ex.ToString());
                        cur_times++;
                        if (RetryTimes >= 0 && cur_times > RetryTimes) throw ex;
                        if (RetryDelay > 0) Thread.Sleep(RetryDelay);
                    }
                } while (true);
            }
            public void Close()
            {
                if (HTTP_Request != null)
                {
                    try
                    {
                        HTTP_Request.Abort();
                    }
                    catch (Exception)
                    {
                    }
                    HTTP_Request = null;
                }
                if (Stream != null)
                {
                    try
                    {
                        Stream.Close();
                        Stream.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                    Stream = null;
                }
                if (HTTP_Response != null)
                {
                    try
                    {
                        HTTP_Response.Close();
                    }
                    catch (Exception)
                    {
                    }
                    HTTP_Response = null;
                }
            }
            public void Dispose()
            {
                Close();
            }
            public string ReadResponseString(Encoding encoding = null)
            {
                if (encoding == null)
                    encoding = Encoding.GetEncoding(DEFAULT_ENCODING);

                if (Stream == null || !Stream.CanRead) return string.Empty;
                var sr = new StreamReader(Stream);
                var str = sr.ReadToEnd();
                sr.Dispose();
                return str;
            }
            public NetStream()
            {
                UseCookie = true;
                Proxy = null;
                TimeOut = DEFAULT_TIMEOUT;
                ReadWriteTimeOut = DEFAULT_READ_WRITE_TIMEOUT;
                AcceptEncoding = DEFAULT_ACCEPT_ENCODING;
                AcceptLanguage = DEFAULT_ACCEPT_LANGUAGE;
                Accept = DEFAULT_ACCEPT;
                UserAgent = DEFAULT_USER_AGENT;
                ContentType = DEFAULT_CONTENT_TYPE_BINARY;
                RetryDelay = 0;
                RetryTimes = 0;

            }
        }


    }

}
