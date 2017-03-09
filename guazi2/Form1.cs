using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace guazi2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        guazi _guazi;
        commentGraphic _commentGr;
        private void Form1_Load(object sender, EventArgs e)
        {
            VBUtil.Utils.NetUtils.Global.LoadCookie();
            _guazi = new guazi();
            _onlineUserGraphic = new timelineGraphic(pOnlineUser.Width, pOnlineUser.Height);
            _giftPriceGraphic = new timelineGraphic(pGiftPrice.Width, pGiftPrice.Height);
            _commentFreqGraphic = new timelineGraphic(pCommentFreq.Width, pCommentFreq.Height);

            _guazi.OnlineUserUpdate += _guazi_OnlineUserUpdate;
            _guazi.GiftRecv += _guazi_GiftRecv;
            _guazi.MessageRecv += _guazi_MessageRecv;
            _guazi.UserInfoUpdated += _guazi_UserInfoUpdated;
            _guazi.NextSilverUpdate += _guazi_NextSilverUpdate;
            _guazi.WelcomeRecv += _guazi_WelcomeRecv;
            _guazi.NextEventHeartTimeUpdated += _guazi_NextEventHeartTimeUpdated;
            _guazi.SysMsgRecv += _guazi_SysMsgRecv;
            _guazi.SysGiftRecv += _guazi_SysMsgRecv;
            _guazi.SmallTVJoined += _guazi_SmallTVJoined;
            _guazi.LiveRecv += _guazi_LiveRecv;
            _guazi.PreparingRecv += _guazi_PreparingRecv;
            _guazi.RoomBlockMsgRecv += _guazi_RoomBlockMsgRecv;
            _guazi.ChangeRoomIDCompleted += _guazi_ChangeRoomIDCompleted;
            _guazi.StreamingSpeedUpdated += _guazi_StreamingSpeedUpdated;
            _guazi.RoomInfoUpdated += _guazi_RoomInfoUpdated;
            _guazi.StreamingStopped += _guazi_StreamingStopped;

            pCommentOutput.MouseWheel += PCommentOutput_MouseWheel;

            //test area
            _update_gift_bag();

            _commentGr = new commentGraphic(pCommentOutput.Width, pCommentOutput.Height);

            //check cookie
            if (!frmLogin.CheckLoginStatus())
            {
                var login_form = new frmLogin();
                login_form.ShowDialog();
                if (login_form.Canceled)
                    Close();
            }

            load_config();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            VBUtil.Utils.NetUtils.Global.SaveCookie();
            save_config();
        }

        private void load_config()
        {
            if (!System.IO.File.Exists("config.json")) return;
            var sr = new System.IO.StreamReader("config.json", Encoding.UTF8);
            var str_json = sr.ReadToEnd();
            sr.Close();

            var json = JsonConvert.DeserializeObject(str_json) as JObject;
            tRoomID.Text = json.Value<string>("RoomNum");
            cAutoGrabSilver.Checked = json.Value<bool>("AutoGrabSilver");
            cAutoGetGift.Checked = json.Value<bool>("AutoGetGift");
            cAutoJoinSmallTV.Checked = json.Value<bool>("AutoJoinSmallTV");
            cAutoOnlineHeart.Checked = json.Value<bool>("AutoOnlineHeart");
            cAutoSign.Checked = json.Value<bool>("AutoSign");
            cJoinActivity.Checked = json.Value<bool>("JoinActivity");
            cRecord.Checked = json.Value<bool>("Record");
            cRecordSaveComment.Checked = json.Value<bool>("RecordSaveComment");

            var stat = json.Value<JObject>("Statistics");
            _global_total_comment_count = stat.Value<ulong>("TotalCommentRecv");
            _global_total_gift_price = stat.Value<ulong>("TotalGiftPrice");
            _global_run_time = new TimeSpan(stat.Value<long>("TotalRunTime"));

            ChangeRoomID();
        }
        private void save_config()
        {
            var new_startTime = DateTime.Now;
            var cur_ts = (new_startTime - _startTime);
            _global_run_time = _global_run_time.Add(cur_ts);
            _startTime = new_startTime;

            var json = new JObject();
            json.Add("RoomNum", tRoomID.Text);
            json.Add("AutoGrabSilver", cAutoGrabSilver.Checked);
            json.Add("AutoGetGift", cAutoGetGift.Checked);
            json.Add("AutoJoinSmallTV", cAutoJoinSmallTV.Checked);
            json.Add("AutoOnlineHeart", cAutoOnlineHeart.Checked);
            json.Add("AutoSign", cAutoSign.Checked);
            json.Add("JoinActivity", cJoinActivity.Checked);
            json.Add("Record", cRecord.Checked);
            json.Add("RecordSaveComment", cRecordSaveComment.Checked);

            var stat = new JObject();
            stat.Add("TotalCommentRecv", _global_total_comment_count);
            stat.Add("TotalGiftPrice", _global_total_gift_price);
            stat.Add("TotalRunTime", _global_run_time.Ticks);

            json.Add("Statistics", stat);

            var str_json = JsonConvert.SerializeObject(json);
            var sw = new System.IO.StreamWriter("config.json", false, Encoding.UTF8);
            sw.WriteLine(str_json);
            sw.Close();
        }

        #region Room Info & ID Management

        private void init_room_info()
        {
            _max_user_count = 0;
            _gift_price = 0;
            _total_gift_price = 0;
            _comment_count = 0;
            _total_comment_count = 0;
            _onlineUserGraphic.Reset();
            _giftPriceGraphic.Reset();
            _commentFreqGraphic.Reset();
            _commentGr.Clear();
            pCommentOutput.Image = null;

            if (_guazi.LiveStatus == "LIVE")
                lRoomStatus.Text = "直播中";
            else if (_guazi.LiveStatus == "ROUND")
                lRoomStatus.Text = "轮播中: " + _guazi.RoundTitle;
            else if (_guazi.LiveStatus == "PREPARING")
                lRoomStatus.Text = "准备中";

            lLiveTime.Left = lRoomStatus.Left + lRoomStatus.Width + 10;

            var ts = (DateTime.Now - _guazi.LiveTimeline);
            lLiveTime.Text = (Math.Floor(ts.TotalHours) > 0 ? (Math.Floor(ts.TotalHours).ToString() + ":") : "") + ts.Minutes.ToString("0#") + ":" + ts.Seconds.ToString("0#");

            lRoomName.Text = _guazi.RoomTitle;
            _guazi_UserInfoUpdated();
        }
        private void _guazi_RoomInfoUpdated()
        {
            Invoke(new NoArgSTA(delegate
            {
                if (_guazi.LiveStatus == "LIVE")
                    lRoomStatus.Text = "直播中";
                else if (_guazi.LiveStatus == "ROUND")
                    lRoomStatus.Text = "轮播中: " + _guazi.RoundTitle;
                else if (_guazi.LiveStatus == "PREPARING")
                    lRoomStatus.Text = "准备中";
                lLiveTime.Left = lRoomStatus.Left + lRoomStatus.Width + 10;
            }));
        }
        private void _guazi_ChangeRoomIDCompleted()
        {
            Invoke(new NoArgSTA(delegate
            {
                tRoomID.Enabled = true;
                cAutoGrabSilver_CheckedChanged(null, new EventArgs());
                cAutoGetGift_CheckedChanged(null, new EventArgs());
                cAutoJoinSmallTV_CheckedChanged(null, new EventArgs());
                cAutoOnlineHeart_CheckedChanged(null, new EventArgs());
                cAutoSign_CheckedChanged(null, new EventArgs());
                cRecord_CheckedChanged(null, new EventArgs());
                cJoinActivity_CheckedChanged(null, new EventArgs());
            }));
        }
        private void tRoomID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ChangeRoomID();
            }
        }
        private void ChangeRoomID()
        {
            int roomid;
            if (int.TryParse(tRoomID.Text, out roomid) == false)
            {
                return;
            }
            tRoomID.Enabled = false;
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    _guazi.ChangeRoomURL(roomid);
                    Invoke(new NoArgSTA(delegate
                    {
                        init_room_info();
                        _guazi.StartReceiveComment();
                    }));
                });
        }

        #endregion


        #region Streaming
        private void _guazi_StreamingStopped()
        {
            Invoke(new NoArgSTA(delegate { lStreamingStat.Text = ""; }));
        }

        private void _guazi_StreamingSpeedUpdated(DateTime streamingStartTime, ulong streamingSpeed, ulong streamingSize)
        {
            var ts = DateTime.Now - streamingStartTime;
            var str = (ts.Hours > 0) ? (Math.Floor(ts.TotalHours).ToString() + ":" + ts.Minutes.ToString("0#") + ":" + ts.Seconds.ToString("0#")) : (ts.Minutes + ":" + ts.Seconds.ToString("0#"));
            Invoke(new NoArgSTA(delegate
            {
                lStreamingStat.Text = str + " " + _int_to_string((long)streamingSpeed).ToUpper() + "B/s" + " 总:" + _int_to_string((long)streamingSize).ToUpper() + "B";
            }));
        }
        #endregion

        #region Event Callback
        private void _guazi_RoomBlockMsgRecv(string arg)
        {
            var csc = new ColorStringCollection();
            csc.Add(Color.Blue, "<已屏蔽");
            csc.Add(Color.Black, arg);
            csc.Add(Color.Blue, "的消息>");
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private void _guazi_PreparingRecv()
        {
            var csc = new ColorStringCollection();
            csc.Add(Color.Blue, "<直播准备中>");
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private void _guazi_LiveRecv()
        {
            var csc = new ColorStringCollection();
            csc.Add(Color.Blue, "<直播已开始>");
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private void _guazi_SmallTVJoined(int roomid, int tv_id)
        {
            var csc = new ColorStringCollection();
            csc.Add(Color.Blue, "已参加");
            csc.Add(Color.Black, roomid.ToString());
            csc.Add(Color.Blue, "房间的小电视抽奖");
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private void _guazi_SysMsgRecv(string arg)
        {
            var csc = new ColorStringCollection();
            csc.Add(Color.Blue, "收到系统消息: ");
            csc.Add(Color.Black, arg.Replace(":?", ""));
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private DateTime _nextEventTime = DateTime.MinValue;
        private void _guazi_NextEventHeartTimeUpdated(DateTime time)
        {
            _nextEventTime = time;
        }
        #endregion
        private void _guazi_WelcomeRecv(string user_name, int uid, int is_admin, int is_vip)
        {
            return;
            var csc = new ColorStringCollection();
            csc.Add(Color.Red, user_name);
            csc.Add(Color.Gray, "进入直播间");
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private DateTime _nextSilverTime = DateTime.MinValue;
        private int _nextSilver = 0;
        private void _guazi_NextSilverUpdate(DateTime time, int silver)
        {
            _nextSilverTime = time;
            _nextSilver = silver;
        }

        private void _guazi_UserInfoUpdated()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                var user = _guazi.GetUserInfo();
                Invoke(new NoArgSTA(delegate
                {
                    lUserName.Text = user.uname;
                    lUserLevel.Text = "UL" + user.level;
                    lSilver.Text = user.silver.ToString();
                    pExp.Maximum = user.nextLevelExp;
                    pExp.Value = user.currentExp;
                    lExp.Text = _int_to_string(user.currentExp) + "/" + _int_to_string(user.nextLevelExp);
                }));
            });
        }

        #region Statistics
        private int _comment_count;
        private long _total_comment_count;
        private int _gift_price;
        private long _total_gift_price;
        private int _max_user_count;
        private ulong _global_total_comment_count, _global_total_gift_price;
        private DateTime _startTime = DateTime.Now;
        private TimeSpan _global_run_time;
        private void _guazi_MessageRecv(string user_name, string comment, int color, int user_level, string medal_name, int medal_level, string title_name, bool is_vip)
        {
            _comment_count++;
            _total_comment_count++;
            _global_total_comment_count++;
            var csc = new ColorStringCollection();
            if (user_level > 0) csc.Add(Color.Purple, "[UL" + user_level + "]");
            if (!string.IsNullOrEmpty(medal_name) && medal_level > 0) csc.Add(Color.Green, "[" + medal_name + medal_level + "]");
            if (is_vip) csc.Add(Color.Red, user_name + " :"); else csc.Add(Color.Gray, user_name + " :");
            csc.Add(Color.Black, comment);
            Invoke(new NoArgSTA(delegate
            {
                _commentGr.AddLine(csc);
                pCommentOutput.Image = _commentGr.GetImage();
            }));
        }

        private void _guazi_GiftRecv(string user_name, string gift_name, int gift_num, int gift_id, int price)
        {
            int worth = gift_num * price;
            _gift_price += worth;
            _total_gift_price += worth;
            _global_total_gift_price += (uint)worth;
            return;

            if (worth >= 1000)
            {
                var csc = new ColorStringCollection();
                csc.Add(Color.Orange, user_name);
                csc.Add(Color.Gray, "赠送");
                csc.Add(Color.Chocolate, gift_name + "×" + gift_num);
                Invoke(new NoArgSTA(delegate
                {
                    _commentGr.AddLine(csc);
                    pCommentOutput.Image = _commentGr.GetImage();
                }));
            }
        }

        private delegate void NoArgSTA();

        private timelineGraphic _onlineUserGraphic, _giftPriceGraphic, _commentFreqGraphic;

        //Event callback
        private void _guazi_OnlineUserUpdate(uint user_count)
        {
            if (user_count > _max_user_count) { _max_user_count = (int)user_count; }
            Invoke(new NoArgSTA(() =>
           {

               string type = "#,##0";
               lOnlineUser.Text = user_count.ToString(type);
               lOnlineUser2.Text = "在线人数: " + lOnlineUser.Text;
               lMaxOnlineUser.Text = _max_user_count.ToString(type);
               lGiftPrice.Text = _gift_price.ToString(type) + "瓜子";
               lGGiftPrice.Text = _global_total_gift_price.ToString(type) + "瓜子";
               lTotalGiftPrice.Text = _total_gift_price.ToString(type) + "瓜子";
               lCommentFreq.Text = _comment_count.ToString(type) + "条/10s";
               lCommentCount.Text = _total_comment_count.ToString(type) + "条";
               lGCommentCount.Text = _global_total_comment_count.ToString(type) + "条";

               DateTime cur_time = DateTime.Now;
               _onlineUserGraphic.AddValue(user_count, cur_time);
               _giftPriceGraphic.AddValue(_gift_price, cur_time);
               _commentFreqGraphic.AddValue(_comment_count, cur_time);

               _comment_count = 0;
               _gift_price = 0;

               pOnlineUser.Image = _onlineUserGraphic.PlotImage();
               pGiftPrice.Image = _giftPriceGraphic.PlotImage();
               pCommentFreq.Image = _commentFreqGraphic.PlotImage();
           }));
        }

        #endregion
        private string _int_to_string(long num)
        {
            if (num < 1000) return num.ToString();
            if (num < 1000000) return Math.Round(num / 1000.0, 2).ToString() + "k";
            if (num < 1000000000) return Math.Round(num / 1000000.0, 2).ToString() + "m";
            return Math.Round(num / 1000000000.0, 2).ToString() + "g";
        }

        private guazi.BagItem[] _gift_bag;
        private void _update_gift_bag()
        {
            Invoke(new NoArgSTA(delegate
            {
                pBag.Controls.Clear();
            }));
            ThreadPool.QueueUserWorkItem(delegate
            {
                _gift_bag = _guazi.GetPlayerBag();
                for (int i = 0; i < _gift_bag.Length; i++)
                {
                    var item = _gift_bag[i];

                    Invoke(new NoArgSTA(delegate
                    {
                        var lblName = new Label();
                        lblName.AutoSize = true;
                        lblName.Font = pBag.Font;
                        lblName.Text = item.gift_name + " x " + item.gift_num;

                        var lblExpires = new Label();
                        lblExpires.AutoSize = true;
                        lblExpires.Font = pBag.Font;
                        lblExpires.Text = item.expire_at;

                        var btnSend = new Button();
                        btnSend.Font = pBag.Font;
                        btnSend.Width = 50;
                        btnSend.Height = 25;
                        btnSend.Text = "发送";
                        btnSend.Tag = i;
                        btnSend.Click += delegate
                        {
                            _guazi.SendItem(_gift_bag[(int)btnSend.Tag]);
                            _update_gift_bag();
                        };

                        pBag.Controls.Add(lblName);
                        pBag.Controls.Add(lblExpires);
                        pBag.Controls.Add(btnSend);
                        pBag.SetFlowBreak(btnSend, true);
                    }));
                }
            });
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            //silver time updating
            if (_nextSilverTime > DateTime.Now)
            {
                var ts = (_nextSilverTime - DateTime.Now);
                lGrabTime.Text = ts.Minutes + ":" + ts.Seconds.ToString("0#") + " " + _nextSilver + "瓜子";
            }
            else
                lGrabTime.Text = "";

            if (_nextEventTime > DateTime.Now)
            {
                var ts = (_nextEventTime - DateTime.Now);
                lEventTime.Text = ts.Minutes + ":" + ts.Seconds.ToString("0#");
            }
            else
                lEventTime.Text = "";

            //live time
            if (_guazi.LiveTimelineUnixTS != 0)
            {
                var ts = (DateTime.Now - _guazi.LiveTimeline);
                lLiveTime.Text = (Math.Floor(ts.TotalHours) > 0 ? (Math.Floor(ts.TotalHours).ToString() + ":") : "") + ts.Minutes.ToString("0#") + ":" + ts.Seconds.ToString("0#");
            }
            else
                lLiveTime.Text = "";
            lLiveTime.Left = lRoomStatus.Left + lRoomStatus.Width + 10;

            var runtime_ts = DateTime.Now - _startTime;
            lRunTime.Text = (Math.Floor(runtime_ts.TotalHours).ToString()) + ":" + runtime_ts.Minutes.ToString("0#") + ":" + runtime_ts.Seconds.ToString("0#");

            runtime_ts = runtime_ts.Add(_global_run_time);
            if (runtime_ts.Days > 0)
                lGRunTime.Text = Math.Floor(runtime_ts.TotalDays).ToString() + "d " + runtime_ts.Hours.ToString() + ":" + runtime_ts.Minutes.ToString("0#") + ":" + runtime_ts.Seconds.ToString("0#");
            else
                lGRunTime.Text = runtime_ts.Hours.ToString() + ":" + runtime_ts.Minutes.ToString("0#") + ":" + runtime_ts.Seconds.ToString("0#");
        }
        #region UI Callback
        private void PCommentOutput_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _commentGr.ScrollUp(e.Delta);
                pCommentOutput.Image = _commentGr.GetImage();
            }
            else
            {
                _commentGr.ScrollDown(-e.Delta);
                pCommentOutput.Image = _commentGr.GetImage();
            }
        }

        private void cAutoGrabSilver_CheckedChanged(object sender, EventArgs e)
        {
            if (cAutoGrabSilver.Checked)
            {
                _guazi.StartSilverGrabbing();
            }
            else
            {
                _guazi.StopSilverGrabbing();
            }
        }

        private void cAutoSign_CheckedChanged(object sender, EventArgs e)
        {
            if (cAutoSign.Checked)
            {
                _guazi.DoSign();
            }
        }

        private void cAutoGetGift_CheckedChanged(object sender, EventArgs e)
        {
            if (cAutoGetGift.Checked)
            {
                _guazi.GetDailyGift();
            }
        }

        private void cAutoJoinSmallTV_CheckedChanged(object sender, EventArgs e)
        {
            if (cAutoJoinSmallTV.Checked)
            {
                _guazi.StartAutoJoinSmallTV();
            }
            else
            {
                _guazi.StopAutoJoinSmallTV();
            }
        }

        private void tSendComment_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                _guazi.SendComment(tSendComment.Text, Color.White);
                tSendComment.Text = "";
            }
        }


        private void bRefreshBag_Click(object sender, EventArgs e)
        {
            _update_gift_bag();
        }

        private void bSendAllGift_Click(object sender, EventArgs e)
        {
            _guazi.SendAllItem();
            _gift_bag = null;
            pBag.Controls.Clear();
        }

        private void pCommentOutput_SizeChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("Comment Output Resized to: " + pCommentOutput.Width + " x " + pCommentOutput.Height);
            _commentGr.Resize(pCommentOutput.Width, pCommentOutput.Height);
            pCommentOutput.Image = _commentGr.GetImage();
        }

        private void cRecord_CheckedChanged(object sender, EventArgs e)
        {
            if (cRecord.Checked)
            {
                saveFileDialog1.FileName = _guazi.RoomTitle;
                if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    cRecord.Checked = false;
                    return;
                }
                var fileName = saveFileDialog1.FileName;
                _guazi.StartStreaming(fileName, cRecordSaveComment.Checked);
            }
            else
            {
                _guazi.StopStreaming();
            }
        }

        private void cAutoOnlineHeart_CheckedChanged(object sender, EventArgs e)
        {
            if (cAutoOnlineHeart.Checked)
            {
                _guazi.StartGettingExp();
            }
            else
            {
                _guazi.StopGettingExp();
            }
        }

        private void cJoinActivity_CheckedChanged(object sender, EventArgs e)
        {
            if (cJoinActivity.Checked)
            {
                _guazi.StartEventHeartbeat();
            }
            else
            {
                _guazi.StopEventHeartbeat();
            }
        }
        #endregion
    }
}
