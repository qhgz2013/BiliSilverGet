using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Sockets;
using System.Net;
using guazi2.NetUtils;

namespace guazi2
{
    public class guazi : IDisposable
    {
        #region global member
        //源id
        private int _roomID;
        //短号id
        private int _roomURL;
        //房间信息
        private JObject _roomInfo;
        //轮播信息的获取时间（用来计算下一次更新轮播信息）
        private ulong _roomRoundInfoStartTime;
        //轮播信息
        private JObject _roomRoundInfo;
        //房间号映射
        private Dictionary<int, int> _roomMapping;

        //一个简单的多线程Trace
        private Tracer _tracer;
        //记录API调用结果
        private bool traceResponseString = true;
        //记录房间弹幕Socket数据
        private bool traceSocketData = false;
        //房间状态从直播到准备时的时间（也是用来更新轮播信息的）
        private DateTime _prepareStartTime;
        //轮播视频时长
        private int _roundVideoTime;
        #endregion


        public event NoArgumentDelegation RoomInfoUpdated;
        public guazi(int roomURL = 0)
        {
            _tracer = new Tracer("trace.log");
            _roomMapping = new Dictionary<int, int>();
            _commentThdLock = new ReaderWriterLock();

            _roomURL = roomURL;
            if (_roomURL > 0)
            {
                _roomID = get_roomid(_roomURL);
            }

            if (_roomID > 0)
            {
                _roomInfo = get_roomInfo(_roomID);
                RoomInfoUpdated?.Invoke();
            }
        }
        //完成房间ID变更后触发的事件
        public event NoArgumentDelegation ChangeRoomIDCompleted;
        /// <summary>
        /// 改变房间的ID
        /// </summary>
        /// <param name="newRoomURL">新的id（支持id和cid）</param>
        public void ChangeRoomURL(int newRoomURL)
        {
            _tracer.TraceInfo("ChangeRoomURL called: int newRoomURL=" + newRoomURL);
            //停止当前与房间有关的线程
            var continue_receive_comment = _commentParseThd != null;
            StopReceiveComment();
            if (_commentParseThd != null) _commentParseThd.Join();

            //var continue_streaming = _streamingThd != null;
            StopStreaming();

            //重新获取id及房间信息
            _roomURL = newRoomURL;
            if (_roomURL > 0)
                _roomID = get_roomid(_roomURL);

            if (_roomID > 0)
                _roomInfo = get_roomInfo(_roomID);

            //继续接收弹幕
            if (continue_receive_comment)
                StartReceiveComment();

            //更新轮播信息
            if (LiveStatus == "ROUND")
                GetRoundInfo();
            else
            {
                _roomRoundInfo = null;
                _roomRoundInfoStartTime = 0;
                _prepareStartTime = DateTime.MinValue;
                _roundVideoTime = 0;
            }

            ChangeRoomIDCompleted?.Invoke();
            RoomInfoUpdated?.Invoke();
        }

        #region Properties
        //直播间标题
        public string RoomTitle
        {
            get
            {
                if (_roomInfo == null) return "";
                return _roomInfo["data"].Value<string>("ROOMTITLE");
            }
        }
        //UP主名称
        public string AnchorName
        {
            get
            {
                if (_roomInfo == null) return "";
                return _roomInfo["data"].Value<string>("ANCHOR_NICK_NAME");
            }
        }
        //直播间状态
        public string LiveStatus
        {
            get
            {
                if (_roomInfo == null) return "";
                return _roomInfo["data"].Value<string>("LIVE_STATUS");
            }
        }
        //直播/轮播开始的Unix时间戳
        public ulong LiveTimelineUnixTS
        {
            get
            {
                if (_roomInfo == null) return 0;
                if (LiveStatus == "LIVE")
                    return _roomInfo["data"].Value<ulong>("LIVE_TIMELINE");
                else if (LiveStatus == "ROUND")
                    return _roomRoundInfoStartTime;
                return 0;
            }
        }
        //直播/轮播开始的DateTime
        public DateTime LiveTimeline
        {
            get
            {
                return util.FromUnixTimestamp(LiveTimelineUnixTS);
            }
        }
        //房间ID
        public int RoomID
        {
            get
            {
                return _roomURL;
            }
        }
        //房间CID
        public int RoomCID
        {
            get
            {
                return _roomID;
            }
        }
        //是否轮播
        public bool RoundLiving
        {
            get
            {
                if (_roomInfo == null) return false;
                //return _roomInfo["data"].Value<int>("ROUND_STATUS") == 1;
                return LiveStatus == "ROUND";
            }
        }
        //封面url
        public string CoverImageURL
        {
            get
            {
                if (_roomInfo == null) return "";
                return _roomInfo["data"].Value<string>("COVER");
            }
        }
        //粉丝数
        public int FansCount
        {
            get
            {
                if (_roomInfo == null) return 0;
                return _roomInfo["data"].Value<int>("FANS_COUNT");
            }
        }
        //轮播视频标题
        public string RoundTitle
        {
            get
            {
                if (_roomRoundInfo == null) return "";
                return _roomRoundInfo["data"].Value<string>("title");
            }
        }
        //下一次瓜子获取的数量
        public int NextGrabSilverCount
        {
            get
            {
                return _silver;
            }
        }
        //下一次瓜子获取的时间
        public DateTime NextGrabSilverTime
        {
            get
            {
                return _dGrabSilverTimeEnd;
            }
        }

        #endregion


        #region util functions
        //以默认的参数获取http请求（Timeout为15s，重试20次，间隔300ms）
        private NetStream get_request()
        {
            var req = new NetStream();
            req.TimeOut = 15000;
            req.RetryDelay = 300;
            req.RetryTimes = 20;
            return req;
        }
        public delegate void NoArgumentDelegation();
        //获取房间ID（异常时返回0）
        private int get_roomid(int roomURL)
        {
            _tracer.TraceInfo("get_roomid called: int roomURL=" + roomURL);
            //从内存中返回
            if (_roomMapping.ContainsKey(roomURL))
            {
                _tracer.TraceInfo("Returned RoomID using Memory Mapping: " + _roomURL + " => " + _roomMapping[_roomURL]);
                return _roomMapping[roomURL];
            }

            //从网页上获取
            if (roomURL > 0)
            {
                var url = "https://api.live.bilibili.com/room/v1/Room/room_init?id=" + roomURL;
                var request = get_request();
                try
                {
                    request.HttpGet(url);
                    var response = request.ReadResponseString();
                    request.Close();
                    var json = JsonConvert.DeserializeObject(response) as JObject;

                    if (json.Value<int>("code") == 0)
                    {
                        int id = json["data"].Value<int>("room_id");
                        _tracer.TraceInfo("Returned RoomID from HTML: " + _roomURL + " => " + id);
                        _roomMapping.Add(roomURL, id);
                        return id;
                    }
                    else
                    {
                        _tracer.TraceError("Could not get RoomID from Internet!");
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                    return 0;
                }
            }
            return 0;
        }
        //获取房间信息（异常时返回null）
        private JObject get_roomInfo(int roomid)
        {
            _tracer.TraceInfo("get_roomInfo called: int roomid=" + roomid);
            if (roomid > 0)
            {
                var request = new NetStream();
                var url = "https://api.live.bilibili.com/live/getInfo?roomid=" + roomid;
                //var url = "https://api.live.bilibili.com/live/getRoomInfoExt?roomid=" + roomid;
                try
                {
                    string str;
                    do
                    {
                        request.HttpGet(url);
                        str = request.ReadResponseString();
                        request.Close();
                    } while (do_refresh_check(str));
                    if (traceResponseString) _tracer.TraceInfo(str);

                    return (JObject)JsonConvert.DeserializeObject(str);
                }
                catch (Exception ex)
                {
                    _tracer.TraceError("params: roomid=" + roomid);
                    _tracer.TraceError(ex.ToString());
                    return null;
                }
            }
            return null;
        }
        //获取当前的unix时间戳
        private ulong get_timestamp()
        {
            return (ulong)util.ToUnixTimestamp(DateTime.Now);
        }
        //从ms转TimeSpan
        private TimeSpan get_timespan(int delta_ms)
        {
            var ct = DateTime.Now;
            return (ct.AddMilliseconds(delta_ms) - ct);
        }
        //网页重定向检查
        private bool do_refresh_check(string html_content)
        {
            var reg = Regex.Match(html_content, "<meta\\shttp-equiv=\"refresh\"\\s*content=\"\\d+;\\s*url='(?<url>[^']+)'\"\\s*>");
            if (reg.Success)
            {
                var url = reg.Result("${url}");
                _tracer.TraceInfo("Doing refresh fetch, url=" + url);
                var ns = get_request();
                ns.HttpGet(url);
                var response = ns.ReadResponseString();
                ns.Close();
                if (traceResponseString) _tracer.TraceInfo(response);
                return true;
            }
            return false;
        }
        #endregion


        #region Grab silver area
        //搜刮线程
        private Thread _grabSilverThread;
        //开始时间和结束时间（unix + DateTime）
        private ulong _grabsilverTimeStart, _grabSilverTimeEnd;
        private DateTime _dGrabSilverTimeStart, _dGrabSilverTimeEnd;
        //当前领取的瓜子数
        private int _silver;
        public delegate void NextSilverUpdateHandler(DateTime time, int silver);
        //开启新的瓜子宝箱的事件
        public event NextSilverUpdateHandler NextSilverUpdate;
        //搜刮线程开始及停止的事件
        public event NoArgumentDelegation SilverGrabStarted, SilverGrabStopped;
        public delegate void GrabSilverResultHandler(int get_silver, int total_silver);
        //领取瓜子成功时触发的事件
        public event GrabSilverResultHandler SilverGrabSucceeded;
        //领取瓜子失败时触发的事件
        public event NoArgumentDelegation SilverGrabFailed;

        //开始异步搜刮瓜子
        public void StartSilverGrabbing()
        {
            _tracer.TraceInfo("StartSilverGrabbing called: void");
            if (_roomID == 0) return;
            if (_grabSilverThread == null || (_grabSilverThread.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0)
            {
                _grabSilverThread = new Thread(_silverThreadCallback);
                _grabSilverThread.Name = "瓜子搜刮线程";
                _grabSilverThread.IsBackground = true;
                _grabSilverThread.Start();
            }
        }
        //停止异步搜刮瓜子
        public void StopSilverGrabbing()
        {
            _tracer.TraceInfo("StopSilverGrabbing called: void");
            if (_grabSilverThread != null)
            {
                _grabSilverThread.Abort();
                _grabSilverThread.Join();
            }
            _grabSilverThread = null;
        }
        //线程调用的函数
        private void _silverThreadCallback()
        {
            _tracer.TraceInfo("SilverThread started");
            SilverGrabStarted?.Invoke();

            var header = new Parameters();
            header.Add("Origin", "https://live.bilibili.com");
            header.Add("Referer", "https://live.bilibili.com/" + _roomURL);
            try
            {
                while (true)
                {
                    //获取新的瓜子宝箱
                    var get_new_tasks_url = "https://live.bilibili.com/FreeSilver/getCurrentTask";
                    var request = get_request();
                    _tracer.TraceInfo("Getting new silver task");
                    JObject json = null;

                    try
                    {
                        string response_str;
                        do
                        {
                            request.HttpGet(get_new_tasks_url, header);
                            response_str = request.ReadResponseString();
                            request.Close();
                        } while (do_refresh_check(response_str));
                        if (traceResponseString) _tracer.TraceInfo(response_str);

                        json = JsonConvert.DeserializeObject(response_str) as JObject;

                        //code judging
                        int code = json.Value<int>("code");
                        if (code == -10017)
                        {
                            //completed
                            _tracer.TraceInfo("silver grab completed, thread will exit");
                            break;
                        }
                        else if (code != 0)
                        {
                            throw new Exception("Unknown code: " + code);
                        }

                        //时间计算
                        int minutes = json["data"].Value<int>("minute");
                        _silver = json["data"].Value<int>("silver");
                        _grabsilverTimeStart = json["data"].Value<uint>("time_start");
                        _grabSilverTimeEnd = json["data"].Value<uint>("time_end");
                        _dGrabSilverTimeStart = DateTime.Now;
                        _dGrabSilverTimeEnd = _dGrabSilverTimeStart.AddMinutes(minutes);


                        NextSilverUpdate?.Invoke(_dGrabSilverTimeEnd, _silver);

                        //睡眠
                        int sleep_time = (int)(_dGrabSilverTimeEnd - DateTime.Now).TotalMilliseconds;
                        if (sleep_time > 0) Thread.Sleep(sleep_time);
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                        Thread.Sleep(5000);
                        continue;
                    }



                    //验证码
                    Image captcha = null;
                    int ocr_count = 0;
                    int captcha_result = -1;
                    var captcha_url = "https://live.bilibili.com/FreeSilver/getCaptcha?ts=" + get_timestamp();

                    do
                    {
                        try
                        {
                            _tracer.TraceInfo("Getting captcha image");
                            request.HttpGet(captcha_url, header);
                            captcha = Image.FromStream(request.Stream);
                        }
                        catch (ThreadAbortException) { throw; }
                        catch (Exception ex)
                        {
                            _tracer.TraceError(ex.ToString());
                            Thread.Sleep(3000);
                            continue;
                        }

                        //ocr
                        const string temp_filename = "tempCaptcha.jpg";
                        captcha.Save(temp_filename);
                        string result = Marshal.PtrToStringAnsi(ocr.OCR(temp_filename, -1));
                        _tracer.TraceInfo("OCR Result: " + result);
                        if (File.Exists(temp_filename)) File.Delete(temp_filename);
                        ocr_count++;

                        //computing
                        result = result.Replace(" ", "").Replace("Z", "2").Replace("l", "1"); //提升准确率的一些替换
                        var result_ptr = @"^\s*(?<first>\d+)\s*(?<operator>[+-])\s*(?<second>\d+)\s*$";
                        var match = Regex.Match(result, result_ptr);
                        if (match.Success)
                        {
                            int first = int.Parse(match.Result("${first}"));
                            int second = int.Parse(match.Result("${second}"));
                            string oper = match.Result("${operator}");
                            if (oper == "-") { captcha_result = first - second; }
                            else if (oper == "+") { captcha_result = first + second; }
                        }
                        else
                        {
                            captcha_result = -1;
                        }
                        _tracer.TraceInfo("Computed result: " + captcha_result);

                    } while (captcha_result == -1);


                    //领取宝箱
                    var award_url = "https://live.bilibili.com/FreeSilver/getAward";
                    var award_param = new Parameters();
                    award_param.Add("time_start", _grabsilverTimeStart);
                    award_param.Add("time_end", _grabSilverTimeEnd);
                    award_param.Add("captcha", captcha_result);

                    try
                    {
                        _tracer.TraceInfo("Award requesting");
                        string response;
                        do
                        {
                            request.HttpGet(award_url, header, award_param);
                            response = request.ReadResponseString();
                            request.Close();
                        } while (do_refresh_check(response));
                        if (traceResponseString) _tracer.TraceInfo(response);

                        json = JsonConvert.DeserializeObject(response) as JObject;
                        if (json.Value<int>("code") != 0)
                        {
                            SilverGrabFailed?.Invoke();
                        }
                        else
                        {
                            int silver = json["data"].Value<int>("awardSilver");
                            int total_silver = json["data"].Value<int>("silver");
                            SilverGrabSucceeded?.Invoke(silver, total_silver);
                        }
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                        Thread.Sleep(1000);
                        continue;
                    }


                    UserInfoUpdated?.Invoke();
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }

            _dGrabSilverTimeEnd = DateTime.MinValue;
            _dGrabSilverTimeStart = DateTime.MinValue;
            _silver = 0;

            SilverGrabStopped?.Invoke();
            _tracer.TraceInfo("SilverThread exited");
        }
        #endregion


        #region Sign
        public delegate void SignStatusHandler(int hadSignDays);
        //签到成功触发的事件
        public event SignStatusHandler SignSucceeded;
        //签到失败触发的事件
        public event NoArgumentDelegation SignFailed;
        //签到线程
        private Thread _signThread;
        //签到线程回调函数
        private void _signThreadCallback()
        {
            _tracer.TraceInfo("SignThread started");
            var url = "https://api.live.bilibili.com/sign/doSign";
            var info_url = "https://api.live.bilibili.com/sign/GetSignInfo";

            var request = get_request();
            try
            {
                _tracer.TraceInfo("Posting signing data");
                request.HttpGet(url);

                var response = request.ReadResponseString();
                if (traceResponseString) _tracer.TraceInfo(response);
                request.Close();

                var json = JsonConvert.DeserializeObject(response) as JObject;
                int code = json.Value<int>("code");
                if (code != 0 && code != -500)
                {
                    SignFailed?.Invoke();
                }
                else
                {
                    //checking sign status
                    _tracer.TraceInfo("Getting sign info");
                    do
                    {
                        request.HttpGet(info_url);
                        response = request.ReadResponseString();
                        request.Close();
                    } while (do_refresh_check(response));
                    if (traceResponseString) _tracer.TraceInfo(response);
                    json = JsonConvert.DeserializeObject(response) as JObject;

                    code = json.Value<int>("code");
                    if (code == 0)
                    {
                        int hadSignDays = json["data"].Value<int>("hadSignDays");
                        SignSucceeded?.Invoke(hadSignDays);
                    }
                    else
                    {
                        _tracer.TraceWarning("Failed to get sign status");
                        SignSucceeded?.Invoke(-1);
                    }
                    UserInfoUpdated?.Invoke();

                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }

            _tracer.TraceInfo("SignThread exited");
        }
        //签到 线程异步
        public void DoSign()
        {
            _tracer.TraceInfo("DoSign called");

            _signThread = new Thread(_signThreadCallback);
            _signThread.IsBackground = true;
            _signThread.Name = "签到线程";
            _signThread.Start();
        }
        #endregion


        #region Gift & Bag
        //成功获取每日礼物
        public event NoArgumentDelegation GetDailyGiftSucceeded;
        //获取每日礼物失败
        public event NoArgumentDelegation GetDailyGiftFailed;
        private Thread _getGiftThread;
        private void _getGiftThreadCallback()
        {
            _tracer.TraceInfo("getGiftThread started");
            var url1 = "https://api.live.bilibili.com/giftBag/sendDaily";
            var url2 = "https://api.live.bilibili.com/giftBag/getSendGift";
            var request = get_request();
            try
            {
                _tracer.TraceInfo("Getting daily gift");
                string response;
                do
                {
                    request.HttpGet(url1);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));
                if (traceResponseString) _tracer.TraceInfo(response);

                do
                {
                    request.HttpGet(url2);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));
                if (traceResponseString) _tracer.TraceInfo(response);

                GetDailyGiftSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
                GetDailyGiftFailed?.Invoke();
            }

            _tracer.TraceInfo("getGiftThread exited");
        }
        public void GetDailyGift()
        {
            _tracer.TraceInfo("GetDailyGift called");
            _getGiftThread = new Thread(_getGiftThreadCallback);
            _getGiftThread.IsBackground = true;
            _getGiftThread.Name = "每日道具领取线程";
            _getGiftThread.Start();
        }

        [Serializable]
        public struct BagItem
        {
            //礼物种类ID
            public int gift_id;
            //数量
            public int gift_num;
            //另外一个ID，类似于token
            public int bag_id;
            //有效期
            public string expire_at;
            //礼物类型
            public string gift_type;
            //名称
            public string gift_name;
            //价格
            public string gift_price;
            public BagItem(int _gift_id, int _gift_num, int _bag_id, string _expire_at, string _gift_type, string _gift_name, string _gift_price)
            {
                gift_id = _gift_id; gift_num = _gift_num; bag_id = _bag_id; expire_at = _expire_at; gift_type = _gift_type; gift_name = _gift_name; gift_price = _gift_price;
            }
        }
        //获取背包的所有道具
        public BagItem[] GetPlayerBag()
        {
            _tracer.TraceInfo("GetPlayerBag called");

            var ret = new List<BagItem>();
            var url = "https://api.live.bilibili.com/gift/playerBag";
            var request = get_request();
            try
            {
                _tracer.TraceInfo("Getting player bag");
                string response;
                do
                {
                    request.HttpGet(url);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));

                if (traceResponseString) _tracer.TraceInfo(response);

                var json = JsonConvert.DeserializeObject(response) as JObject;
                var array = json.Value<JArray>("data");

                foreach (JObject item in array)
                {
                    int gift_id = item.Value<int>("gift_id");
                    int num = item.Value<int>("gift_num");
                    int id = item.Value<int>("id");
                    string expireat = item.Value<string>("expireat");
                    string gift_type = item.Value<string>("gift_type");
                    string gift_name = item.Value<string>("gift_name");
                    string gift_price = item.Value<string>("gift_price");
                    ret.Add(new BagItem(gift_id, num, id, expireat, gift_type, gift_name, gift_price));
                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            return ret.ToArray();
        }

        public delegate void SendGiftSucceedHandler(string gift_name, int gift_id, int gift_num, int gift_price);
        //送出道具成功
        public event SendGiftSucceedHandler SendGiftSucceeded;
        //送出道具失败
        public event NoArgumentDelegation SendGiftFailed;
        //送出所有道具（异步）
        public void SendAllItem()
        {
            _tracer.TraceInfo("SendAllItem called: void");
            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                _tracer.TraceInfo("SendAllItemThread started");
                var items = GetPlayerBag();
                foreach (var item in items)
                {
                    //ThreadPool.QueueUserWorkItem(delegate
                    //{
                    SendItem(item);
                    //});
                }
                _tracer.TraceInfo("SendAllItemThread exited");
            });
        }
        //送出指定道具（只要设置gift_id, bag_id, gift_num三个属性就行）
        public void SendItem(BagItem item)
        {
            _tracer.TraceInfo("SendItem called: BagItem item");
            if (_roomInfo == null) return;
            try
            {
                int masterid = _roomInfo["data"].Value<int>("MASTERID");
                string token = NetStream.DefaultCookieContainer.GetCookies(new Uri("https://live.bilibili.com/"))["LIVE_LOGIN_DATA"].Value;
                SendItemRaw(item.gift_id, item.bag_id, item.gift_num, _roomID, masterid, token);
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
        }
        //送出道具（全手动指定参数）
        public void SendItemRaw(int gift_id, int bag_id, int gift_num, int room_id, int master_id, string token)
        {
            _tracer.TraceInfo("SendItemRaw called: int gift_id=" + gift_id + ", int bag_id=" + bag_id + ", int gift_num=" + gift_num + ", room_id=" + room_id + ", master_id=" + master_id + ", token=" + token);
            if (room_id <= 0 || gift_id <= 0 || gift_num <= 0 || master_id <= 0 || string.IsNullOrEmpty(token)) return;

            var request = get_request();
            var param = new Parameters();

            var url = "https://api.live.bilibili.com/giftBag/send";

            var header_param = new Parameters();
            header_param.Add("Origin", "https://live.bilibili.com");
            header_param.Add("Referer", "https://live.bilibili.com/" + _roomURL);

            param.Add("giftId", gift_id);
            param.Add("roomid", room_id);
            param.Add("ruid", master_id);
            param.Add("num", gift_num);
            param.Add("coinType", "silver");
            param.Add("Bag_id", bag_id);
            param.Add("timestamp", get_timestamp());
            param.Add("rnd", get_timestamp());
            param.Add("token", token);

            try
            {
                _tracer.TraceInfo("Posting sending info");
                string result;
                do
                {
                    request.HttpPost(url, param, headerParam: header_param);
                    result = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(result));

                if (traceResponseString) _tracer.TraceInfo(result);

                var json = JsonConvert.DeserializeObject(result) as JObject;
                int code = json.Value<int>("code");
                if (code == 0)
                {
                    string name = json["data"].Value<string>("giftName");
                    int num = json["data"].Value<int>("num");
                    int giftid = json["data"].Value<int>("giftId");
                    int price = json["data"].Value<int>("price");
                    SendGiftSucceeded?.Invoke(name, giftid, num, price);
                    UserInfoUpdated?.Invoke();
                }
                else
                {
                    SendGiftFailed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
        }
        #endregion


        #region Comment
        //主弹幕接收线程
        private Thread _commentParseThd;
        //弹幕心跳线程（由主弹幕线程控制）
        private Thread _commentHeartbeatThd;
        //接收弹幕的socket
        private Socket _commentSocket;

        //开始/结束接收弹幕的事件
        public event NoArgumentDelegation CommentReceiveStarted, CommentReceiveStopped;

        //private MemoryStream _commentPool;
        private void _commentHeartbeatThreadCallback()
        {
            _tracer.TraceInfo("commentHeartbeatThread started");
            DateTime next_time = DateTime.Now;
            try
            {
                do
                {
                    int sleep_time = (int)(next_time - DateTime.Now).TotalMilliseconds;
                    if (sleep_time > 0)
                    {
                        Thread.Sleep(sleep_time);
                    }
                    try
                    {
                        //sending heartbeat
                        //_tracer.TraceInfo("Comment socket: sending heartbeat");
                        uint total_len = 16;
                        ushort head_len = 16;
                        ushort version = 1;
                        uint type = 2;
                        uint param5 = 1;

                        var ms = new MemoryStream();
                        WriteUI32(ms, total_len);
                        WriteUI16(ms, head_len);
                        WriteUI16(ms, version);
                        WriteUI32(ms, type);
                        WriteUI32(ms, param5);
                        ms.Seek(0, SeekOrigin.Begin);

                        var buffer = new byte[ms.Length];
                        ms.Read(buffer, 0, buffer.Length);
                        ms.Close();

                        _commentSocket.Send(buffer);
                    }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                    }
                    next_time = DateTime.Now.AddSeconds(10);
                } while (_commentSocket != null && _commentSocket.Connected);
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            _tracer.TraceInfo("commentHeartbeatThread exited");
        }
        private void _commentParserThreadCallback()
        {
            _tracer.TraceInfo("commentParserThread started");
            try
            {
                //用于失败重连的循环
                while (_roomID > 0)
                {
                    //从api返回值中获取ip和port
                    _tracer.TraceInfo("Getting comment server domain from Internet");
                    var request = get_request();
                    var url = "https://live.bilibili.com/api/player?id=cid:" + _roomID;

                    IPAddress ipaddr = IPAddress.None;
                    int port = 0;
                    try
                    {
                        string response;
                        do
                        {
                            request.HttpGet(url);
                            response = request.ReadResponseString();
                            request.Close();
                        } while (do_refresh_check(response));
                        if (traceResponseString) _tracer.TraceInfo(response);

                        string server_address = "";

                        //server ns
                        var match = Regex.Match(response, "<server>(.+)</server>");
                        if (match.Success)
                        {
                            server_address = match.Result("$1");
                            if (string.IsNullOrEmpty(server_address)) throw new ArgumentNullException("server address");
                        }
                        else
                        {
                            throw new ArgumentException("Could not get server address");
                        }
                        //server port
                        match = Regex.Match(response, "<dm_port>(\\d+)</dm_port>");
                        if (match.Success)
                        {
                            port = int.Parse(match.Result("$1"));
                        }
                        else
                        {
                            throw new ArgumentNullException("Could not get server port");
                        }

                        //dns lookup
                        _tracer.TraceInfo("DNS Looking up: " + server_address);
                        ipaddr = Dns.GetHostAddresses(server_address)[0];
                    }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                        Thread.Sleep(1000);
                        continue;
                    }

                    //init connection
                    if (ipaddr == IPAddress.None || port == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    IPEndPoint ipEndPoint = new IPEndPoint(ipaddr, port);

                    //connect begin
                    _commentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    byte[] buffer = new byte[65536];
                    int length = 0;

                    try
                    {
                        //double stack network test
                        try
                        {
                            _tracer.TraceInfo("Comment socket: connecting (ipv4 mode)");
                            _commentSocket.Connect(ipEndPoint);
                        }
                        catch (Exception ex)
                        {
                            _tracer.TraceError(ex.ToString());
                            _tracer.TraceInfo("Failing to connect using ipv4 mode, falling to ipv6");
                            _tracer.TraceInfo("Comment socket: connecting (ipv6 mode)");
                            try
                            {
                                _commentSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                                _commentSocket.Connect(ipEndPoint);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }

                        //sending user info
                        _tracer.TraceInfo("Comment socket: sending user info");
                        var json = new JObject();
                        json.Add("roomid", _roomID);
                        json.Add("uid", _roomInfo["data"].Value<ulong>("UID"));

                        var send_bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));
                        var memory_stream = new MemoryStream();

                        uint total_len = 16 + (uint)send_bytes.Length;
                        ushort head_len = 16;
                        ushort version = 1;
                        uint type = 7;
                        uint param5 = 1;
                        WriteUI32(memory_stream, total_len);
                        WriteUI16(memory_stream, head_len);
                        WriteUI16(memory_stream, version);
                        WriteUI32(memory_stream, type);
                        WriteUI32(memory_stream, param5);
                        memory_stream.Write(send_bytes, 0, send_bytes.Length);
                        memory_stream.Seek(0, SeekOrigin.Begin);

                        send_bytes = new byte[memory_stream.Length];
                        memory_stream.Read(send_bytes, 0, send_bytes.Length);

                        _commentSocket.Send(send_bytes);
                        memory_stream.Close();


                        //recv start
                        _commentHeartbeatThd = new Thread(_commentHeartbeatThreadCallback);
                        _commentHeartbeatThd.IsBackground = true;
                        _commentHeartbeatThd.Name = "弹幕心跳发送线程";
                        _commentHeartbeatThd.Start();

                        _tracer.TraceInfo("Comment socket: ready to receive");
                        CommentReceiveStarted?.Invoke();
                        while (true)
                        {
                            length = _commentSocket.Receive(buffer);
                            if (length > 0)
                            {
                                _parseSocketData(buffer, length);
                            }
                        }
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                        Thread.Sleep(2000);
                        continue;
                    }
                    finally
                    {
                        CommentReceiveStopped?.Invoke();
                        try
                        {
                            if (_commentHeartbeatThd != null && (_commentHeartbeatThd.ThreadState & (System.Threading.ThreadState.Running | System.Threading.ThreadState.WaitSleepJoin)) != 0)
                            {
                                _commentHeartbeatThd.Abort();
                                _commentHeartbeatThd.Join();
                                _commentHeartbeatThd = null;
                            }
                        }
                        catch (Exception) { }
                    } //try

                } //while

            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            finally
            {
            } //try

            _tracer.TraceInfo("commentParserThread exited");
        }
        #region stream utils
        private static byte[] ReadBytes(Stream sin, int count)
        {
            int nRead = 0, r = 0;
            var buffer = new byte[count];
            do
            {
                r = sin.Read(buffer, nRead, count - nRead);
                nRead += r;
            } while (nRead != count && r != 0);
            return buffer;
        }
        private static uint ReadUI32(Stream sin)
        {
            var bytes = ReadBytes(sin, 4);
            uint ret = 0;
            for (int i = 0; i < 4; i++)
            {
                ret <<= 8;
                ret |= bytes[i];
            }
            return ret;
        }
        private static ushort ReadUI16(Stream sin)
        {
            var bytes = ReadBytes(sin, 2);
            ushort ret = 0;
            for (int i = 0; i < 2; i++)
            {
                ret <<= 8;
                ret |= bytes[i];
            }
            return ret;
        }
        private static void WriteUI32(Stream sout, uint data)
        {
            WriteUI16(sout, (ushort)(data >> 16));
            WriteUI16(sout, (ushort)(data & 0xffff));
        }
        private static void WriteUI16(Stream sout, ushort data)
        {
            var bytes = new byte[2];
            bytes[1] = (byte)(data & 0xff);
            bytes[0] = (byte)((data & 0xff00) >> 8);
            sout.Write(bytes, 0, 2);
        }
        #endregion
        //解析从socket接收到的数据
        private void _parseSocketData(byte[] data, int length)
        {
            var ms = new MemoryStream(length);
            ms.Write(data, 0, length);
            ms.Seek(0, SeekOrigin.Begin);

            while (ms.Position < ms.Length)
            {
                uint total_len = ReadUI32(ms);
                ushort head_len = ReadUI16(ms);
                ushort version = ReadUI16(ms);
                uint type = ReadUI32(ms);
                uint param5 = ReadUI32(ms);
                //_tracer.TraceInfo("Comment socket: parsing data: total_len: " + total_len + ", head_len: " + head_len + ", version: " + version + ", type: " + type + ", param5: " + param5);

                switch (type)
                {
                    case 3:
                        uint onlineUser = ReadUI32(ms);
                        OnlineUserUpdate?.Invoke(onlineUser);
                        break;
                    case 5:
                        var buf = new byte[total_len - head_len];
                        ms.Read(buf, 0, buf.Length);
                        var str_comment = Encoding.UTF8.GetString(buf);
                        _parseJsonComment(str_comment);
                        break;
                    default:
                        break;
                }
            }
            ms.Close();
        }
        public delegate void StringArgumentDelegation(string arg);
        public delegate void OnlineUserUpdateHandler(uint user_count);
        public delegate void MessageReceiveHandler(string user_name, string comment, int color, int user_level, string medal_name, int medal_level, string title_name, bool is_vip);
        public delegate void GiftReceiveHandler(string user_name, string gift_name, int gift_num, int gift_id, int price);
        public delegate void WelcomeReceiveHandler(string user_name, int uid, int is_admin, int is_vip);
        public delegate void WelcomeGuardReceiveHandler(string user_name, int uid, int guard_level);
        public delegate void SpecialGiftReceiveHandler(int id, int num, int time, string content);
        //解析到在线用户时的事件
        public event OnlineUserUpdateHandler OnlineUserUpdate;
        //接收到弹幕
        public event MessageReceiveHandler MessageRecv;
        //接收到礼物
        public event GiftReceiveHandler GiftRecv;
        //接收到屏蔽消息/系统消息/系统礼物消息
        public event StringArgumentDelegation RoomBlockMsgRecv, SysMsgRecv, SysGiftRecv;
        //接收到欢迎老爷的消息
        public event WelcomeReceiveHandler WelcomeRecv;
        //接收到舰长称号的消息
        public event WelcomeGuardReceiveHandler WelcomeGuardRecv;
        //直播间状态更新
        public event NoArgumentDelegation LiveRecv, PreparingRecv;
        //特别礼物消息
        public event SpecialGiftReceiveHandler SpecialGiftRecv;
        //接收到json格式的内容（非在线人数）
        public event StringArgumentDelegation JsonCommentRecv;

        private ReaderWriterLock _commentThdLock;
        private void _parseJsonComment(string str_json)
        {
            try
            {
                var json = JsonConvert.DeserializeObject(str_json) as JObject;
                string cmd = json.Value<string>("cmd");

                JsonCommentRecv?.Invoke(str_json);

                if (traceSocketData) _tracer.TraceInfo("Comment Socket: parsing string: " + str_json);

                switch (cmd)
                {
                    //弹幕消息
                    case "DANMU_MSG":
                        JArray comment_info = json["info"][0] as JArray;
                        string comment = json["info"].Value<string>(1);
                        JArray user_info = json["info"][2] as JArray;
                        JArray medal_info = json["info"][3] as JArray;
                        JArray user_level_info = json["info"][4] as JArray;
                        JArray title_info = json["info"][5] as JArray;

                        int fontsize = comment_info.Value<int>(2);
                        int fontcolor = comment_info.Value<int>(3);
                        string user_name = user_info.Value<string>(1);

                        string medal_name = "";
                        int medal_level = 0;
                        if (medal_info.HasValues)
                        {
                            medal_name = medal_info.Value<string>(1);
                            medal_level = medal_info.Value<int>(0);
                        }

                        string title_name = "";
                        if (title_info.HasValues)
                        {
                            title_name = title_info.Value<string>(0);
                        }
                        int user_level = user_level_info.Value<int>(0);

                        int is_vip = user_info.Value<int>(3);
                        MessageRecv?.Invoke(user_name, comment, fontcolor, user_level, medal_name, medal_level, title_name, is_vip == 1);
                        break;

                    case "SEND_GIFT":
                        string gift_name = json["data"].Value<string>("giftName");
                        int gift_num = json["data"].Value<int>("num");
                        user_name = json["data"].Value<string>("uname");
                        int gift_id = json["data"].Value<int>("giftId");
                        int gift_Type = json["data"].Value<int>("giftType");
                        int price = json["data"].Value<int>("price");
                        GiftRecv?.Invoke(user_name, gift_name, gift_num, gift_id, price);
                        break;

                    case "WELCOME":
                        user_name = json["data"].Value<string>("uname");
                        int isadmin = json["data"].Value<int>("isadmin");
                        is_vip = json["data"].Value<int>("vip");
                        int uid = json["data"].Value<int>("uid");
                        WelcomeRecv?.Invoke(user_name, uid, isadmin, is_vip);
                        break;

                    case "WELCOME_GUARD":
                        user_name = json["data"].Value<string>("username");
                        uid = json["data"].Value<int>("uid");
                        int guard_level = json["data"].Value<int>("guard_level");
                        WelcomeGuardRecv?.Invoke(user_name, uid, guard_level);
                        break;

                    case "SYS_MSG":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        var msg = json.Value<string>("msg");
                        //call for smallTV
                        int tv_id = json.Value<int>("tv_id");
                        int real_roomid = json.Value<int>("real_roomid");
                        if (tv_id > 0 && _autoJoinSmallTV)
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                JoinSmallTV(real_roomid, tv_id);
                            });
                        }
                        SysMsgRecv?.Invoke(msg);
                        break;

                    case "SYS_GIFT":
                        msg = json.Value<string>("msg");
                        SysGiftRecv?.Invoke(msg);
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;

                    case "PREPARING":
                        _prepareStartTime = DateTime.Now;
                        UpdateRoomInfo();
                        PreparingRecv?.Invoke();
                        RoomInfoUpdated?.Invoke();
                        break;

                    case "LIVE":
                        _prepareStartTime = DateTime.MinValue;
                        UpdateRoomInfo();
                        LiveRecv?.Invoke();
                        RoomInfoUpdated?.Invoke();
                        break;

                    case "BET_START":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "BET_BETTOR":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "BET_BANKER":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "BET_SEAL":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "BET_ENDING":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "BET_END":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "CHANGE_ROOM_INFO":
                        UpdateRoomInfo();
                        break;

                    case "ROOM_BLOCK_MSG":
                        user_name = json.Value<string>("uname");
                        RoomBlockMsgRecv?.Invoke(user_name);
                        break;

                    case "SEND_TOP":
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                    case "SPECIAL_GIFT":
                        //节奏风暴
                        var content = json["data"].Value<string>("content");
                        var id = json["data"].Value<int>("id");
                        var num = json["data"].Value<int>("num");
                        var time = json["data"].Value<int>("time");
                        if (!string.IsNullOrEmpty(content) && _autoJoinSmallTV)
                        {
                            SendComment(content, Color.White);
                        }
                        SpecialGiftRecv?.Invoke(id, num, time, content);
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;

                    default:
                        _tracer.TraceInfo("[" + cmd + "]:" + str_json);
                        break;
                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError("Error occured while parsing json:" + str_json);
                _tracer.TraceError(ex.ToString());
            }
        }
        public void StartReceiveComment()
        {
            _tracer.TraceInfo("StartReceiveComment called");
            _commentThdLock.AcquireWriterLock(Timeout.Infinite);
            if (_commentParseThd == null || (_commentParseThd.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0)
            {
                _commentParseThd = new Thread(_commentParserThreadCallback);
                _commentParseThd.Name = "弹幕解析线程";
                _commentParseThd.IsBackground = true;
                _commentParseThd.Start();
            }
            _commentThdLock.ReleaseWriterLock();
        }
        public void StopReceiveComment()
        {
            _tracer.TraceInfo("StopReceiveComment called");
            _commentThdLock.AcquireWriterLock(Timeout.Infinite);
            if (_commentSocket != null && _commentSocket.Connected) _commentSocket.Disconnect(false);
            if (_commentParseThd != null) _commentParseThd.Abort();
            _commentThdLock.ReleaseWriterLock();
        }
        public void SendComment(string msg, Color color)
        {
            _tracer.TraceInfo("SendComment called");
            if (_roomID == 0) return;
            ThreadPool.QueueUserWorkItem(delegate
            {
                var request = get_request();
                var post_param = new Parameters();
                post_param.Add("color", color.ToArgb());
                post_param.Add("fontsize", 25);
                post_param.Add("mode", 1);
                post_param.Add("msg", msg);
                post_param.Add("rnd", get_timestamp());
                post_param.Add("roomid", _roomID);

                _tracer.TraceInfo("posting comment: " + msg);
                try
                {
                    var url = "https://live.bilibili.com/msg/send";
                    string response;
                    do
                    {
                        request.HttpPost(url, post_param);
                        response = request.ReadResponseString();
                        request.Close();
                    } while (do_refresh_check(response));
                    if (traceResponseString) _tracer.TraceInfo(response);

                    var json = JsonConvert.DeserializeObject(response) as JObject;
                    var code = json.Value<int>("code");
                    if (code != 0)
                    {
                        _tracer.TraceWarning("Error returning code: " + code + " ,msg=" + json.Value<string>("msg"));
                    }
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                }

            });
        }
        #endregion


        #region AutoJoinSmallTV
        private bool _autoJoinSmallTV;
        public delegate void JoinSmallTVHandler(int roomid, int tv_id);
        public event JoinSmallTVHandler SmallTVJoined;
        private DateTime JoinSmallTV(int roomid, int tvid)
        {
            _tracer.TraceInfo("JoinSmallTV called");
            _tracer.TraceInfo("[SmallTV] roomid: " + roomid + ", tv_id: " + tvid);

            var url = "https://api.live.bilibili.com/SmallTV/join?roomid=" + roomid + "&id=" + tvid;
            var request = get_request();
            var header_param = new Parameters();
            header_param.Add("Origin", "https://live.bilibili.com");
            header_param.Add("Referer", "https://live.bilibili.com/" + _roomURL);
            try
            {
                string response;
                do
                {
                    var rnd = new Random();
                    Thread.Sleep(rnd.Next(3000, 9000));
                    request.HttpGet(url, header_param);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));
                if (traceResponseString) _tracer.TraceInfo(response);

                var json = JsonConvert.DeserializeObject(response) as JObject;
                int code = json.Value<int>("code");
                if (code == 0)
                {
                    int dt = json.Value<int>("dtime");
                    SmallTVJoined?.Invoke(roomid, tvid);
                    return DateTime.Now.AddSeconds(dt);
                }
                else
                {
                    _tracer.TraceError("Join SmallTV failed, code=" + code);
                }
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            return DateTime.MinValue;
        }
        public void StartAutoJoinSmallTV()
        {
            _autoJoinSmallTV = true;
        }
        public void StopAutoJoinSmallTV()
        {
            _autoJoinSmallTV = false;
        }
        #endregion


        #region Silver2Coin
        public void SilverToCoin()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                var ns = new NetStream();
                ns.RetryTimes = 3;
                var url = "https://api.live.bilibili.com/pay/v1/Exchange/silver2coin";
                var header = new Parameters();
                header.Add("Origin", "https://live.bilibili.com");
                header.Add("Referer", "https://live.bilibili.com/exchange");
                var body = new Parameters();
                body.Add("platform", "pc");
                try
                {
                    ns.HttpPost(url, body, headerParam: header);
                    var response = ns.ReadResponseString();

                    _tracer.TraceInfo(response);
                    ns.Close();
                }
                catch (Exception ex)
                {
                    _tracer.TraceError(ex.ToString());
                }
            });
        }
        #endregion

        public event NoArgumentDelegation UserInfoUpdated;
        [Serializable]
        public struct UserInfo
        {
            //public int uid;
            public string uname;
            public int gold;
            public int silver;
            public int level;
            public int currentExp;
            public int nextLevelExp;
            public int isvip;
        }
        public UserInfo GetUserInfo()
        {
            _tracer.TraceInfo("GetUserInfo called");
            var ret = new UserInfo();
            var request = get_request();
            var url = "https://api.live.bilibili.com/User/getUserInfo";
            try
            {
                string response;
                do
                {
                    request.HttpGet(url);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));
                if (traceResponseString) _tracer.TraceInfo(response);

                var json = JsonConvert.DeserializeObject(response) as JObject;
                ret.currentExp = json["data"].Value<int>("user_intimacy");
                ret.gold = json["data"].Value<int>("gold");
                ret.silver = json["data"].Value<int>("silver");
                ret.isvip = json["data"].Value<int>("vip");
                ret.level = json["data"].Value<int>("user_level");
                ret.nextLevelExp = json["data"].Value<int>("user_next_intimacy");
                //ret.uid
                ret.uname = json["data"].Value<string>("uname");
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            return ret;
        }
        public void UpdateRoomInfo()
        {
            _tracer.TraceInfo("UpdateRoomInfo called");
            ThreadPool.QueueUserWorkItem(delegate
            {
                _roomInfo = get_roomInfo(_roomID);

                if (LiveStatus == "ROUND")
                    GetRoundInfo();
                RoomInfoUpdated?.Invoke();
            });
        }


        #region Exp
        public event NoArgumentDelegation UserHeartbeated;
        private Thread _expThread;
        private void _expThreadCallback()
        {
            _tracer.TraceInfo("expThread started");

            try
            {
                var next_update_time = DateTime.Now;
                var request = get_request();
                var url = "https://api.live.bilibili.com/User/userOnlineHeart";

                var xhr_param = new Parameters();
                xhr_param.Add("Origin", "https://live.bilibili.com");
                xhr_param.Add("Referer", "https://live.bilibili.com/" + _roomURL);

                while (true)
                {
                    int sleep_time = (int)(next_update_time - DateTime.Now).TotalMilliseconds;
                    if (sleep_time > 0) Thread.Sleep(sleep_time);

                    _tracer.TraceInfo("posting online heartbeat");
                    string response = string.Empty;
                    do
                    {
                        try
                        {
                            request.HttpPost(url, new byte[] { }, "text/html", xhr_param);
                        }
                        catch (Exception ex)
                        {
                            _tracer.TraceError(ex.ToString());
                            continue;
                        }
                        response = request.ReadResponseString();
                        request.Close();
                    } while (do_refresh_check(response));
                    if (traceResponseString) _tracer.TraceInfo(response);

                    UserHeartbeated?.Invoke();
                    UserInfoUpdated?.Invoke();

                    next_update_time = DateTime.Now.AddMinutes(5);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }

            _tracer.TraceInfo("expThread exited");
        }
        public void StartGettingExp()
        {
            _tracer.TraceInfo("StartGettingExp called");

            if (_expThread == null || (_expThread.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0)
            {
                _expThread = new Thread(_expThreadCallback);
                _expThread.Name = "在线经验线程";
                _expThread.IsBackground = true;
                _expThread.Start();
            }
        }
        public void StopGettingExp()
        {
            _tracer.TraceInfo("StopGettingExp called");
            if (_expThread != null)
            {
                _expThread.Abort();
                _expThread.Join();
            }
            _expThread = null;
        }
        #endregion


        #region Event
        private Thread _eventThd;
        public delegate void NextEventHeartTimeHandler(DateTime time);
        public event NextEventHeartTimeHandler NextEventHeartTimeUpdated;
        private void _eventThdCallback()
        {
            _tracer.TraceInfo("eventThd started");

            var xhr_param = new Parameters();
            xhr_param.Add("Origin", "https://live.bilibili.com");
            xhr_param.Add("Referer", "https://live.bilibili.com/" + _roomURL);

            try
            {
                var request = get_request();
                if (_roomID == 0) throw new ArgumentNullException("Roomid");
                var url1 = "https://api.live.bilibili.com/eventRoom/index?ruid=" + _roomInfo["data"].Value<string>("MASTERID");//_roomID;
                var url2 = "https://api.live.bilibili.com/eventRoom/heart?roomid=" + _roomID;

                request.HttpGet(url1, xhr_param);
                var response = request.ReadResponseString();
                request.Close();

                if (traceResponseString) _tracer.TraceInfo(response);

                var json = JsonConvert.DeserializeObject(response) as JObject;
                var heartTime = json["data"].Value<int>("heartTime");


                heartTime *= 1000;
                bool can_continue = json["data"].Value<bool>("heart");
                while (can_continue)
                {
                    if (heartTime > 0)
                    {
                        NextEventHeartTimeUpdated?.Invoke(DateTime.Now.AddMilliseconds(heartTime));
                        Thread.Sleep(heartTime);
                    }
                    do
                    {
                        request.HttpGet(url2, headerParam: xhr_param);
                        response = request.ReadResponseString();
                        request.Close();
                    } while (do_refresh_check(response));

                    if (traceResponseString) _tracer.TraceInfo(response);
                    json = JsonConvert.DeserializeObject(response) as JObject;

                    can_continue = json["data"].Value<bool>("heart");

                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }

            _tracer.TraceInfo("eventThd exited");
        }
        public void StartEventHeartbeat()
        {
            _tracer.TraceInfo("StartEventHeartbeat called");
            if (_roomID == 0) return;
            if (_eventThd == null || (_eventThd.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0)
            {
                _eventThd = new Thread(_eventThdCallback);
                _eventThd.IsBackground = true;
                _eventThd.Name = "活动领取线程";
                _eventThd.Start();
            }
        }
        public void StopEventHeartbeat()
        {
            _tracer.TraceInfo("StopEventHeartbeat called");
            if (_eventThd != null) _eventThd.Abort();
            _eventThd = null;
        }
        #endregion



        #region Live streaming and status
        private Thread _roundThread;
        public void GetRoundInfo()
        {
            _tracer.TraceInfo("GetRoundInfo called");
            _roundVideoTime = 0;
            _roomRoundInfoStartTime = 0;
            _roomRoundInfo = null;
            if (_roomID == 0) return;
            var url = "https://live.bilibili.com/live/getRoundPlayVideo?room_id=" + _roomID;
            var request = get_request();
            try
            {
                _tracer.TraceInfo("Getting Round Info");
                string response;
                do
                {
                    request.HttpGet(url);
                    response = request.ReadResponseString();
                    request.Close();
                } while (do_refresh_check(response));
                if (traceResponseString) _tracer.TraceInfo(response);

                _roomRoundInfo = JsonConvert.DeserializeObject(response) as JObject;
                var cid = _roomRoundInfo["data"].Value<int>("cid");
                if (cid == -2) return;

                if (cid > 0)
                {
                    var ct = DateTime.Now;
                    _roomRoundInfoStartTime = (ulong)util.ToUnixTimestamp(ct - (ct.AddSeconds(_roomRoundInfo["data"].Value<int>("play_time")) - ct));

                    var video_url = _roomRoundInfo["data"].Value<string>("play_url");
                    _tracer.TraceInfo("Getting Round Info (Source Video)");
                    request.HttpGet(video_url);

                    response = request.ReadResponseString();
                    if (traceResponseString) _tracer.TraceInfo(response);
                    request.Close();

                    var json = JsonConvert.DeserializeObject(response) as JObject;
                    var timelength = json.Value<int>("timelength");
                    _roundVideoTime = timelength / 1000;
                }
                else if (cid == -1)
                {
                    var ct = DateTime.Now;
                    var play_time = _roomRoundInfo["data"].Value<int>("play_time");
                    play_time = 300 - play_time;
                    _roomRoundInfoStartTime = (ulong)util.ToUnixTimestamp(ct - (ct.AddSeconds(play_time) - ct));
                    _roundVideoTime = 300;
                }
                else if (cid == 0 || cid == -3) return; //unknown code
                //creating another thread to update info
                if (_roundThread != null)
                {
                    try { _roundThread.Abort(); }
                    catch (Exception) { }
                }
                _roundThread = new Thread(() =>
                {
                    _tracer.TraceInfo("updateRoundInfoThd started");
                    var sleep_time = (int)(util.FromUnixTimestamp(_roomRoundInfoStartTime).AddSeconds(_roundVideoTime) - DateTime.Now).TotalMilliseconds;
                    if (sleep_time > 0) Thread.Sleep(sleep_time);
                    GetRoundInfo();
                    RoomInfoUpdated?.Invoke();
                    _tracer.TraceInfo("updateRoundInfoThd exited");
                });
                _roundThread.IsBackground = true;
                _roundThread.Name = "轮播内容更新线程";
                _roundThread.Start();
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
        }
        private Thread _streamingThd;
        private Thread _streamingSpeedThd;
        private string _streamingPath;
        private ulong _streamingSpeed, _streamingLength;
        private DateTime _streamingStartTime;
        private bool _streamingRecvComment;
        private StreamWriter _streamingCommentWriter;
        public delegate void StreamingSpeedHandler(DateTime streamingStartTime, ulong streamingSpeed, ulong streamingSize);
        public event StreamingSpeedHandler StreamingSpeedUpdated;
        private void _StreamThdCallback()
        {
            _tracer.TraceInfo("StreamingThd started");
            var request = get_request();
            Stream inStream = null, outStream = null;
            _streamingCommentWriter = null;
            if (_roomID > 0)
            {
                do
                {
                    //非直播状态循环检查
                    if (LiveStatus != "LIVE")
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    try
                    {
                        //获取下载的url
                        _tracer.TraceInfo("Getting streaming url");
                        var url = "https://live.bilibili.com/api/playurl?player=1&cid=" + _roomID + "&quality=0";
                        string response;
                        do
                        {
                            request.HttpGet(url);
                            response = request.ReadResponseString();
                            request.Close();
                        } while (do_refresh_check(response));
                        if (traceResponseString) _tracer.TraceInfo(response);

                        var xml_doc = new System.Xml.XmlDocument();
                        xml_doc.LoadXml(response);

                        var streamingUrl = xml_doc["video"]["durl"]["url"].InnerText;

                        request.HttpGet(streamingUrl);
                        _tracer.TraceInfo("Streaming started");
                        StreamingStartted?.Invoke();
                        _streamingStartTime = DateTime.Now;
                        _streamingSpeed = 0; _streamingLength = 0;

                        //开启计算速度线程
                        _streamingSpeedThd = new Thread(_StreamingSpeedThdCallback);
                        _streamingSpeedThd.IsBackground = true;
                        _streamingSpeedThd.Name = "下载速度计算线程";
                        _streamingSpeedThd.Start();

                        //保存弹幕
                        if (_streamingRecvComment)
                        {
                            _writeStreamHead();
                            JsonCommentRecv += _writeStream;
                        }

                        //获取输入数据流
                        inStream = request.Stream;

                        var output_name = _streamingPath + "." + _streamingStartTime.ToString("yyyy-MM-dd HH-mm-ss") + ".flv";
                        outStream = new FileStream(output_name, FileMode.Create, FileAccess.Write, FileShare.Read);

                        int cur_read = 0;
                        const int buffer_size = 0x10000;
                        byte[] buffer = new byte[buffer_size];
                        do
                        {
                            cur_read = inStream.Read(buffer, 0, buffer_size);
                            _streamingSpeed += (ulong)cur_read;
                            _streamingLength += (ulong)cur_read;
                            outStream.Write(buffer, 0, cur_read);
                        } while (cur_read != 0);
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        _tracer.TraceError(ex.ToString());
                    }
                    finally
                    {
                        //终止线程
                        if (_streamingSpeedThd != null)
                        {
                            _streamingSpeedThd.Abort();
                            _streamingSpeedThd = null;
                        }
                        //终止弹幕写入
                        if (_streamingRecvComment)
                        {
                            JsonCommentRecv -= _writeStream;
                            _writeStreamFoot();
                        }

                        if (inStream != null) { inStream.Close(); inStream = null; }
                        if (outStream != null) { outStream.Close(); outStream = null; }

                        request.Close();
                        StreamingStopped?.Invoke();
                    }

                } while (true);
            }
            _tracer.TraceInfo("StreamingThd exited");
        }

        private void _StreamingSpeedThdCallback()
        {
            _tracer.TraceInfo("StreamingSpeedThd started");
            try
            {
                do
                {
                    Thread.Sleep(1000);
                    StreamingSpeedUpdated?.Invoke(_streamingStartTime, _streamingSpeed, _streamingLength);
                    _streamingSpeed = 0;
                } while (LiveStatus == "LIVE");
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }
            _tracer.TraceInfo("StreamingSpeedThd exited");
        }
        private void _writeStreamHead()
        {
            //if (_streamingCommentWriter == null) return;
            _streamingCommentWriter = new StreamWriter(_streamingPath + "." + _streamingStartTime.ToString("yyyy-MM-dd HH-mm-ss") + ".xml", false, Encoding.UTF8);
            _streamingCommentWriter.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?><i><chatserver>livecmt-2.bilibili.com</chatserver><chatid>1</chatid><mission>0</mission><maxlimit>99999</maxlimit><source>k-v</source>");
        }
        private void _writeStreamFoot()
        {
            if (_streamingCommentWriter == null) return;
            _streamingCommentWriter.Write("</i>");
            _streamingCommentWriter.Close();
            _streamingCommentWriter = null;
        }
        private void _writeStream(string str_json)
        {
            JObject json = null;
            try
            {
                json = JsonConvert.DeserializeObject(str_json) as JObject;
            }
            catch (Exception ex)
            {
                _tracer.TraceError(ex.ToString());
            }

            if (_streamingCommentWriter == null || json == null || json.Value<string>("cmd") != "DANMU_MSG") return;
            double start_time = (DateTime.Now - _streamingStartTime).TotalSeconds;
            var msg = json["info"].Value<string>(1);
            var msg_cfg = json["info"].Value<JArray>(0);
            msg_cfg[0] = start_time;

            var comment_str = new StringBuilder();
            comment_str.Append("<d p=\"");
            comment_str.Append(msg_cfg.Value<double>(0) + ","); //start time
            comment_str.Append(msg_cfg.Value<int>(1) + ","); //comment type
            comment_str.Append(msg_cfg.Value<int>(2) + ","); //font size
            comment_str.Append(msg_cfg.Value<int>(3) + ","); //font color
            comment_str.Append(msg_cfg.Value<long>(4) + ","); //unix timestamp
            comment_str.Append(msg_cfg.Value<int>(6) + ","); //comment pool type
            comment_str.Append(msg_cfg.Value<string>(7) + ","); //user
            comment_str.Append("0\">"); //comment index
            comment_str.Append(msg);
            comment_str.Append("</d>");

            _streamingCommentWriter.WriteLine(comment_str.ToString());
        }
        public event NoArgumentDelegation StreamingStartted, StreamingStopped;

        public void StartStreaming(string output_path, bool save_comment)
        {
            _tracer.TraceInfo("StartStreaming called");
            _streamingPath = output_path;
            _streamingRecvComment = save_comment;
            if (_streamingThd == null || (_streamingThd.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0)
            {
                _streamingThd = new Thread(_StreamThdCallback);
                _streamingThd.IsBackground = true;
                _streamingThd.Name = "直播录播线程";
                _streamingThd.Start();
            }
        }
        public void StopStreaming()
        {
            _tracer.TraceInfo("StopStreaming called");
            if (_streamingThd != null) _streamingThd.Abort();
            _streamingThd = null;
        }

        #endregion
        public void Dispose()
        {
            ((IDisposable)_tracer).Dispose();
        }
    }
}
