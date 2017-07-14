using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VBUtil.Utils;
using VBUtil.Utils.NetUtils;

namespace GeeTestLib
{
    public partial class GeeTestForBili
    {

        //constants
        private static Point[] _slice_pos = new Point[] { new Point(-157, -58), new Point(-145, -58), new Point(-265, -58), new Point(-277, -58), new Point(-181, -58), new Point(-169, -58), new Point(-241, -58), new Point(-253, -58), new Point(-109, -58), new Point(-97, -58), new Point(-289, -58), new Point(-301, -58), new Point(-85, -58), new Point(-73, -58), new Point(-25, -58), new Point(-37, -58), new Point(-13, -58), new Point(-1, -58), new Point(-121, -58), new Point(-133, -58), new Point(-61, -58), new Point(-49, -58), new Point(-217, -58), new Point(-229, -58), new Point(-205, -58), new Point(-193, -58), new Point(-145, 0), new Point(-157, 0), new Point(-277, 0), new Point(-265, 0), new Point(-169, 0), new Point(-181, 0), new Point(-253, 0), new Point(-241, 0), new Point(-97, 0), new Point(-109, 0), new Point(-301, 0), new Point(-289, 0), new Point(-73, 0), new Point(-85, 0), new Point(-37, 0), new Point(-25, 0), new Point(-1, 0), new Point(-13, 0), new Point(-133, 0), new Point(-121, 0), new Point(-49, 0), new Point(-61, 0), new Point(-229, 0), new Point(-217, 0), new Point(-193, 0), new Point(-205, 0) };

        //{0} timestamp (ms)
        private static string _challenge_url = "https://passport.bilibili.com/captcha/gc?cType=2&vcType=2&_={0}";
        //{0} gt
        private static string _gettype_url = "https://api.geetest.com/gettype.php?gt={0}&callback=cb";
        //{0} gt {1} challenge {2} path
        private static string _get_url = "https://api.geetest.com/get.php?gt={0}&challenge={1}&width=100%&product=float&offline=false&protocol=&path={2}&type=slide&callback=cb";
        private static string _ajax_url = "https://api.geetest.com/ajax.php";
        public static string DoChallenge()
        {
            //前戏: 获取有关geetest的参数

            //string gt & string challenge
            var challenge_url = string.Format(_challenge_url, Convert.ToInt64(Others.ToUnixTimestamp(DateTime.Now) * 1000));
            var req = new NetStream();
            var xhr_param = new Parameters();
            xhr_param.Add("X-Requested-With", "XMLHttpRequest");
            xhr_param.Add("Referer", "https://passport.bilibili.com/login");
            req.HttpGet(challenge_url, xhr_param);
            var rep = req.ReadResponseString();
            var json_rep = JsonConvert.DeserializeObject(rep) as JObject;
            var challenge = json_rep["data"].Value<string>("challenge");
            var gt = json_rep["data"].Value<string>("gt");
            req.Close();

            //string path & string static_server
            var ref_param = new Parameters();
            ref_param.Add("Referer", "https://passport.bilibili.com/login");
            var gettype_url = string.Format(_gettype_url, gt);
            req.HttpGet(gettype_url, ref_param);
            rep = req.ReadResponseString();
            json_rep = JsonConvert.DeserializeObject(rep.Substring(3, rep.Length - 4)) as JObject;
            var path = json_rep["data"].Value<string>("path");
            var static_server = json_rep["data"]["static_servers"].Value<string>(0);
            req.Close();

            //Image fullbg & Image slice & Image bg
            var get_url = string.Format(_get_url, gt, challenge, path);
            req.HttpGet(get_url, ref_param);
            rep = req.ReadResponseString();
            json_rep = JsonConvert.DeserializeObject(rep.Substring(3, rep.Length - 4)) as JObject;
            var bg = json_rep.Value<string>("bg"); //with source
            var fullbg = json_rep.Value<string>("fullbg");
            var slice = json_rep.Value<string>("slice");
            challenge = json_rep.Value<string>("challenge");
            req.Close();

            //image downloading
            var fullbg_img = _load_image_from_url("https://" + static_server + "/" + fullbg);
            var bg_img = _load_image_from_url("https://" + static_server + "/" + bg);
            var slice_img = _load_image_from_url("https://" + static_server + "/" + slice);

            //sorting the images
            fullbg_img = _sort_image(fullbg_img);
            bg_img = _sort_image(bg_img);

            //find difference
            var target_pos = _difference(fullbg_img, bg_img);

            //simulating mouse track
            var target_x = target_pos.Left - 6;
            var sim_path = _load_random_path(target_x);
            sim_path.Insert(0, new int[] { 22, 22, 0 });
            //sim_path.Insert(1, new int[] { 0, 0, 0 });
            var sim_path_enc = _encrypt(sim_path);

            //building request data
            var passtime = 0;
            foreach (var item in sim_path) { passtime += item[2]; }
            var dst_target_x = -sim_path[0][0];
            foreach (var item in sim_path) { dst_target_x += item[0]; }
            var validate_param = new Parameters();
            validate_param.Add("gt", gt);
            validate_param.Add("challenge", challenge);
            validate_param.Add("userresponse", _get_user_response(dst_target_x, challenge));
            validate_param.Add("passtime", passtime);
            var imgload = rnd.Next(0, 200) + 50;
            Thread.Sleep(passtime);
            validate_param.Add("imgload", imgload);
            validate_param.Add("a", sim_path_enc);
            validate_param.Add("callback", "cb");
            var ajax_url = _ajax_url;
            req.HttpGet(ajax_url, urlParam: validate_param, headerParam: ref_param);
            rep = req.ReadResponseString();
            json_rep = JsonConvert.DeserializeObject(rep.Substring(3, rep.Length - 4)) as JObject;

            int success = json_rep.Value<int>("success");
            string ret = string.Empty;
            if (success == 1)
            {
                ret = json_rep.Value<string>("validate");
            }
            req.Close();
            return ret;
        }
        /// <summary>
        /// 从url处获取图像（经过memory缓存）
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static Image _load_image_from_url(string url)
        {
            var req = new NetStream();
            req.RetryTimes = 3;
            var header = new Parameters();
            header.Add("Origin", "https://passport.bilibili.com");
            header.Add("Referer", "https://passport.bilibili.com/login");
            req.HttpGet(url, header);

            var stream = req.Stream;
            var out_stream = new MemoryStream();
            int read = 0;
            var buffer = new byte[16384];
            do
            {
                read = stream.Read(buffer, 0, 16384);
                out_stream.Write(buffer, 0, read);
            } while (read > 0);

            out_stream.Seek(0, SeekOrigin.Begin);
            req.Close();
            return Image.FromStream(out_stream);
        }
        /// <summary>
        /// 重排图像
        /// </summary>
        /// <param name="img_in"></param>
        /// <returns></returns>
        private static Image _sort_image(Image img_in)
        {
            var bmp = new Bitmap(img_in.Width, img_in.Height);
            var gr = Graphics.FromImage(bmp);
            int width = 10;
            int height = img_in.Height >> 1;

            for (int i = 0; i < _slice_pos.Length; i++)
            {
                var tmp_pos = _slice_pos[i];
                var img_pos = new Point(-tmp_pos.X, -tmp_pos.Y);
                var cur_pos = new Point(width * (i % (_slice_pos.Length >> 1)), height * (i / (_slice_pos.Length >> 1)));

                gr.DrawImage(img_in, new Rectangle(cur_pos, new Size(width, height)), new Rectangle(img_pos, new Size(width, height)), GraphicsUnit.Pixel);
            }
            gr.Dispose();

            return bmp;
        }
        /// <summary>
        /// 图像差异对比
        /// </summary>
        /// <param name="bg1"></param>
        /// <param name="bg2"></param>
        /// <param name="color_threshold"></param>
        /// <returns></returns>
        private static Rectangle _difference(Image bg1, Image bg2, int color_threshold = 4000)
        {
            if (bg1.Width != bg2.Width || bg1.Height != bg2.Height) throw new ArgumentException("Image size is not the same!");

            Bitmap bmp1 = new Bitmap(bg1), bmp2 = new Bitmap(bg2);
            var bit1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var bit2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var data1 = new byte[bit1.Stride * bmp1.Height];
            Marshal.Copy(bit1.Scan0, data1, 0, data1.Length);
            var data2 = new byte[data1.Length];
            Marshal.Copy(bit2.Scan0, data2, 0, data2.Length);

            var sample_list = new List<Point>();

            for (int x = 0; x < bmp1.Width; x++)
            {
                for (int y = 0; y < bmp1.Height; y++)
                {
                    int offset = y * bit1.Stride + x * 3;

                    byte b1 = data1[offset], b2 = data2[offset];
                    byte g1 = data1[offset + 1], g2 = data2[offset + 1];
                    byte r1 = data1[offset + 2], r2 = data2[offset + 2];

                    int rms_value = (int)(Math.Pow(r1 - r2, 2) + Math.Pow(g1 - g2, 2) + Math.Pow(b1 - b2, 2));
                    if (rms_value > color_threshold)
                        sample_list.Add(new Point(x, y));
                }
            }

            bmp1.UnlockBits(bit1);
            bmp2.UnlockBits(bit2);

            if (sample_list.Count == 0) return new Rectangle();
            int min_x = sample_list[0].X, max_x = sample_list[0].X;
            int min_y = sample_list[0].Y, max_y = sample_list[0].Y;

            for (int i = 1; i < sample_list.Count; i++)
            {
                if (sample_list[i].X > max_x) max_x = sample_list[i].X;
                else if (sample_list[i].X < min_x) min_x = sample_list[i].X;
                if (sample_list[i].Y > max_y) max_y = sample_list[i].Y;
                else if (sample_list[i].Y < min_y) min_y = sample_list[i].Y;
            }

            return new Rectangle(min_x, min_y, max_x - min_x, max_y - min_y);
        }

        private static Random rnd = new Random();

        public class t : EventArgs { public List<int[]> data; public int xpos; }
        public static event EventHandler<t> XPOS;
        //generate a random path (including position and time) from 0 to xpos
        private static List<int[]> _load_random_path(int xpos)
        {
            var t = new t();
            t.xpos = xpos;
            XPOS?.Invoke(null, t);
            return t.data;
        }
        //functions extracted from gt.js
        private static char _replace(int[] a2)
        {
            //from: https://github.com/wsguest/geetest/blob/master/Geek.cs#L378
            var b = new int[][] { new int[] { 1, 0 }, new int[] { 2, 0 }, new int[] { 1, -1 },
                new int[] { 1, 1 }, new int[] { 0, 1 }, new int[] { 0, -1 },
                new int[] { 3, 0 }, new int[] { 2, -1 }, new int[] { 2, 1 } };
            var c = "stuvwxyz~";
            for (var d = 0; d < b.Length; d++)
                if (a2[0] == b[d][0] && a2[1] == b[d][1])
                    return c[d];
            return '\0';
        }
        private static string _encode(int n)
        {
            //from https://github.com/wsguest/geetest/blob/master/Geek.cs#L356
            const string b = "()*,-./0123456789:?@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqr";
            var c = b.Length;
            char d = (char)0;
            var e = Math.Abs(n);
            var f = e / c;
            if (f >= c)
                f = c - 1;
            if (f != 0)
            {
                d = b[f];
                e %= c;
            }
            var g = "";
            if (n < 0)
                g += "!";
            if (d != 0)
                g += "$";

            return g + (d == 0 ? "" : d.ToString()) + b[e];
        }
        private static string _encrypt(List<int[]> data)
        {
            var d = data;// diff(action);
            string dx = "", dy = "", dt = "";
            for (var j = 0; j < d.Count; j++)
            {
                var b = _replace(d[j]);
                if (b != 0)
                {
                    dy += b.ToString();
                }
                else
                {
                    dx += (_encode(d[j][0]));
                    dy += (_encode(d[j][1]));
                }
                dt += (_encode(d[j][2]));
            }
            return dx + "!!" + dy + "!!" + dt;
        }
        private static string _get_user_response(int xpos, string challenge)
        {
            var ct = challenge.Substring(32);
            if (ct.Length < 2)
                return "";
            int[] d = new int[ct.Length];
            for (var e = 0; e < ct.Length; e++)
            {
                var f = ct[e];
                if (f > 57)
                    d[e] = f - 87;
                else
                    d[e] = f - 48;
            }
            var c = 36 * d[0] + d[1];
            var g = xpos + c;
            ct = challenge.Substring(0, 32);
            var i = new List<List<char>>(5);
            for (var ii = 0; ii < 5; ii++)
            {
                i.Add(new List<char>());
            }
            Dictionary<char, int> j = new Dictionary<char, int>();
            int k = 0;
            foreach (var h in ct)
            {
                if (!j.Keys.Contains(h) || j[h] != 1)
                {
                    j[h] = 1;
                    i[k].Add(h);
                    k++;
                    k %= 5;
                }

            }
            int n = g, o = 4;
            var p = "";
            var q = new int[] { 1, 2, 5, 10, 50 }.ToList(); ;
            Random rnd = new Random();
            while (n > 0)
            {
                if (n - q[o] >= 0)
                {
                    int m = rnd.Next(0, i[o].Count);
                    p += i[o][m];
                    n -= q[o];
                }
                else
                {
                    i.RemoveAt(o);
                    q.RemoveAt(o);
                    o--;
                }
            }
            return p;
        }
    }
}
