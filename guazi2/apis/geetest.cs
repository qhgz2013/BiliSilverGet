using guazi2.NetUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace guazi2
{
    //BILIBILI LOGIN API ---- GEETEST SLIDE VALIDATION CODE PART
    public partial class api
    {

        //constants
        #region constants
        private static Point[] _slice_pos = new Point[] { new Point(-157, -58), new Point(-145, -58), new Point(-265, -58), new Point(-277, -58), new Point(-181, -58), new Point(-169, -58), new Point(-241, -58), new Point(-253, -58), new Point(-109, -58), new Point(-97, -58), new Point(-289, -58), new Point(-301, -58), new Point(-85, -58), new Point(-73, -58), new Point(-25, -58), new Point(-37, -58), new Point(-13, -58), new Point(-1, -58), new Point(-121, -58), new Point(-133, -58), new Point(-61, -58), new Point(-49, -58), new Point(-217, -58), new Point(-229, -58), new Point(-205, -58), new Point(-193, -58), new Point(-145, 0), new Point(-157, 0), new Point(-277, 0), new Point(-265, 0), new Point(-169, 0), new Point(-181, 0), new Point(-253, 0), new Point(-241, 0), new Point(-97, 0), new Point(-109, 0), new Point(-301, 0), new Point(-289, 0), new Point(-73, 0), new Point(-85, 0), new Point(-37, 0), new Point(-25, 0), new Point(-1, 0), new Point(-13, 0), new Point(-133, 0), new Point(-121, 0), new Point(-49, 0), new Point(-61, 0), new Point(-229, 0), new Point(-217, 0), new Point(-193, 0), new Point(-205, 0) };

        //{0} timestamp (ms)
        private static string _challenge_url = "https://passport.bilibili.com/captcha/gc?cType=2&vcType=2&_={0}";
        //{0} gt
        private static string _gettype_url = "https://api.geetest.com/gettype.php?gt={0}&callback=cb";
        //{0} gt {1} challenge {2} path
        private static string _get_url = "https://api.geetest.com/get.php?gt={0}&challenge={1}&width=100%&product=float&offline=false&protocol=&path={2}&type=slide&callback=cb";
        private static string _ajax_url = "https://api.geetest.com/ajax.php";
        #endregion

        #region variables
        private static Random _rnd = new Random();
        private static string _challenge;
        private static string _validate;
        #endregion

        #region public
        public static string Challenge { get { return _challenge; } }
        public static string Validate { get { return _validate; } }
        public static string DoChallenge()
        {
            _challenge = string.Empty;
            _validate = string.Empty;
            try
            {
                //前戏: 获取有关geetest的参数

                //string gt & string challenge
                var challenge_url = string.Format(_challenge_url, Convert.ToInt64(util.ToUnixTimestamp(DateTime.Now) * 1000));
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
                var imgload = _rnd.Next(0, 200) + 50;
                Thread.Sleep(passtime);
                validate_param.Add("imgload", imgload);
                validate_param.Add("a", sim_path_enc);
                validate_param.Add("callback", "cb");
                var ajax_url = _ajax_url;
                req.HttpGet(ajax_url, urlParam: validate_param, headerParam: ref_param);
                rep = req.ReadResponseString();
                json_rep = JsonConvert.DeserializeObject(rep.Substring(3, rep.Length - 4)) as JObject;

                Debug.Print(json_rep.Value<string>("message"));
                int success = json_rep.Value<int>("success");
                if (success == 1)
                {
                    _validate = json_rep.Value<string>("validate");
                    _challenge = challenge;
                    //writing data
                    //var str_json = JsonConvert.SerializeObject(sim_path);
                    //var json = new JObject();
                    //json.Add("length", target_x);
                    //json.Add("data", str_json);
                    //var fs = new StreamWriter("apis/traineddata.txt", true);
                    //fs.WriteLine(JsonConvert.SerializeObject(json));
                    //fs.Close();
                }
                req.Close();
            }
            catch (Exception)
            {
            }
            return _validate;
        }
        public class MouseTrackInputEventArgs : EventArgs { public List<int[]> data; public int xpos; }
        public static event EventHandler<MouseTrackInputEventArgs> XPOS;
        #endregion

        #region private
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
        private static Rectangle _difference(Image bg1, Image bg2, int color_threshold = 10000)
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

        private static List<int[]> _load_random_path(int xpos)
        {
            return _load_random_path_method_auto(xpos);
        }
        //[已被抛弃：妈蛋还是forbidden]
        private static List<int[]> _load_random_path_method_0(int xpos)
        {
            var x0 = _rnd.Next(18, 26);
            var y0 = _rnd.Next(18, 26);
            var ret = new List<int[]>();
            ret.Add(new int[] { x0, y0, 0 });

            var pathCount = _rnd.Next(80, 150) - 1;
            var xpos_map = new List<int>(xpos + 1);
            for (int i = 0; i <= xpos; i++) { xpos_map.Add(i); }
            var selected_xpos = new List<int>(pathCount);
            for (int i = 0; i < pathCount; i++)
            {
                if (xpos_map.Count == 0)
                    selected_xpos.Add(_rnd.Next(xpos + 1));
                else
                {
                    var index = _rnd.Next(xpos_map.Count);
                    selected_xpos.Add(xpos_map[index]);
                    xpos_map.RemoveAt(index);
                }
            }
            selected_xpos.Sort();
            var x = new List<int>(pathCount);
            x.Add(0);
            for (int i = 1; i < pathCount; i++)
            {
                x.Add(selected_xpos[i] - selected_xpos[i - 1]);
            }

            var y_origin = new List<int>(pathCount);
            var phi = _rnd.NextDouble() * 2 * Math.PI;
            var a = _rnd.NextDouble() * 5 + 12;
            var omega = _rnd.NextDouble() * 0.02 + 0.01;
            for (int i = 0; i < pathCount; i++)
            {
                y_origin.Add((int)(Math.Sin(omega * i + phi) * a));
            }
            var y = new List<int>(pathCount);
            y.Add(0);
            for (int i = 1; i < pathCount; i++)
            {
                y.Add(y_origin[i] - y_origin[i - 1]);
            }

            double begin_t = _rnd.NextDouble() * 2 + 6;
            var max_stop = 3;
            var cur_stop = 0;
            var t = new List<int>(pathCount);
            t.Add(0);
            for (int i = 1; i < pathCount; i++)
            {
                var delta_t = _rnd.NextDouble() * 0.5 - 0.2;
                var possibility = _rnd.NextDouble();

                if (possibility > 0.96 && cur_stop++ < max_stop)
                {
                    t.Add((int)begin_t + _rnd.Next(400, 800));
                }
                else
                {
                    //t.Add((int)(begin_t * (x[i] == 0 ? 1.7 : x[i])));
                    t.Add((int)begin_t);
                }
                begin_t += delta_t;
            }
            t[t.Count - 1] = t[t.Count - 1] + _rnd.Next(400, 800);


            //merging
            for (int i = 0; i < pathCount; i++)
            {
                ret.Add(new int[] { x[i], y[i], t[i] });
            }
            return ret;
        }
        private static List<int[]> _load_random_path_method_auto(int xpos)
        {
            var file = new StreamReader("apis/traineddata.txt");
            var list = new List<KeyValuePair<int, List<int[]>>>();
            string line = "";
            do
            {
                line = file.ReadLine();
                if (line == null) break;
                var json = JsonConvert.DeserializeObject(line) as JObject;
                var pos = json.Value<int>("length");
                var data = new List<int[]>();
                data = JsonConvert.DeserializeObject<List<int[]>>(json.Value<string>("data"));
                list.Add(new KeyValuePair<int, List<int[]>>(pos, data));
            } while (true);
            file.Close();

            if (list.Count == 0) return new List<int[]>();

            list.Sort((data1, data2) =>
            {
                return data1.Key.CompareTo(data2.Key);
            });

            //containing the same xpos path
            var same_pos_paths = new List<List<int[]>>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Key == xpos)
                    same_pos_paths.Add(list[i].Value);
            }
            if (same_pos_paths.Count > 0)
            {
                var index = _rnd.Next(same_pos_paths.Count);
                return same_pos_paths[index];
            }

            //not containing the same xpos path, finding the nearest path for simulation
            var dst = Math.Abs(xpos - list[0].Key);
            var linear_pos_paths = new List<List<int[]>>();
            var linear_index = 0;
            linear_pos_paths.Add(list[0].Value);

            for (int i = 1; i < list.Count; i++)
            {
                var new_dst = Math.Abs(xpos - list[i].Key);
                if (new_dst < dst)
                {
                    linear_pos_paths.Clear();
                    dst = new_dst;
                    linear_index = i;
                }
                if (new_dst == dst)
                {
                    linear_pos_paths.Add(list[i].Value);
                }
            }
            if (linear_pos_paths.Count > 0)
            {
                var index = _rnd.Next(same_pos_paths.Count);
                var path = linear_pos_paths[index];
                
                var length = 0;
                for (int i = 2; i < path.Count; i++) length += path[i][0];

                var amplified_size = xpos * 1.0 / length;
                //amplifying path
                int x = 0, y = 0, t = 0;
                List<double> xs = new List<double>(), ys = new List<double>(), ts = new List<double>();
                for (int i = 1; i < path.Count; i++)
                {
                    xs.Add((path[i][0] + x) * amplified_size);
                    ys.Add((path[i][1] + y) * amplified_size);
                    ts.Add((path[i][2] + t) * amplified_size);
                    x += path[i][0];
                    y += path[i][1];
                    t += path[i][2];
                }
                var new_path = new List<int[]>();
                new_path.Add(path[0]);
                new_path.Add(path[1]);
                for (int i = 0; i < xs.Count - 1; i++)
                {
                    new_path.Add(new int[] { (int)(xs[i + 1] - xs[i]), (int)(ys[i + 1] - ys[i]), (int)(ts[i + 1] - ts[i]) });
                }
                return new_path;
            }
            return new List<int[]>();
        }
        private static List<int[]> _load_random_path_method_mamual(int xpos)
        {
            var t = new MouseTrackInputEventArgs();
            t.xpos = xpos;
            XPOS?.Invoke(null, t);
            t.data.Insert(0, new int[] { _rnd.Next(18, 25), _rnd.Next(18, 25), 0 });
            
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
        #endregion
    }
}
