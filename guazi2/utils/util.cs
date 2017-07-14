// util.cs
//
// 放一些常用的函数
//
using System;
using System.Text;

namespace guazi2
{
    public partial class util
    {
        /// <summary>
        /// 将指定DateTime转为Unix时间戳
        /// </summary>
        /// <param name="time">输入的时间</param>
        /// <returns>Unix时间戳</returns>
        public static double ToUnixTimestamp(DateTime time)
        {
            if (time.Ticks == 0) return 0;
            return double.Parse((time.ToUniversalTime().Ticks / 10000000 - 62135596800).ToString());
        }
        /// <summary>
        /// 将指定Unix时间戳转为DateTime
        /// </summary>
        /// <param name="time">Unix时间戳</param>
        /// <returns>等价的时间</returns>
        public static DateTime FromUnixTimestamp(double time)
        {
            return (new DateTime(Convert.ToInt64((time + 62135596800) * 10000000))).ToLocalTime();
        }
        /// <summary>
        /// 将byte数组以16进制形式转为string
        /// </summary>
        /// <param name="bytes">输入的数组元素</param>
        /// <param name="upper_case">输出是否大写</param>
        /// <returns></returns>
        public static string Hex(byte[] bytes, bool upper_case = false)
        {
            var sb = new StringBuilder();
            foreach (var item in bytes)
            {
                sb.Append(item.ToString("X2"));
            }
            if (upper_case) return sb.ToString();
            else return sb.ToString().ToLower();
        }
        /// <summary>
        /// 将16进制字符串转为byte数组
        /// </summary>
        /// <param name="bytes">16进制的字符串</param>
        /// <returns></returns>
        public static byte[] Hex(string bytes)
        {
            if (bytes.Length % 1 != 0) bytes = "0" + bytes;

            int len = bytes.Length >> 1;
            var ret = new byte[len];

            for (int i = 0; i < len; i++)
            {
                ret[i] = byte.Parse(bytes.Substring(i << 1, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return ret;
        }
        /// <summary>
        /// 生成multipart/form-data表单的随机边界值
        /// </summary>
        /// <param name="min_len">最小的字符串长度</param>
        /// <param name="max_len">最大的字符串长度（该值包含在内）</param>
        /// <returns></returns>
        public static string GenerateFormDataBoundary(int min_len = 30, int max_len = 40)
        {
            if (min_len <= 0 || max_len <= 0 || min_len > max_len) return string.Empty;
            const string char_list = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
            var rnd = new Random();
            int len = rnd.Next(min_len, max_len + 1);

            var ret = string.Empty;
            for (int i = 0; i < len; i++)
            {
                ret += char_list[rnd.Next(char_list.Length)];
            }
            return ret;
        }
        /// <summary>
        /// 以参数形式生成表单数据的头
        /// </summary>
        /// <param name="boundary">边界值</param>
        /// <param name="headers">头部实参</param>
        /// <returns></returns>
        public static string GenerateFormDataObject(string boundary, NetUtils.Parameters headers)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(boundary)) return string.Empty;
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    sb.Append(item.Key);
                    sb.Append(": ");
                    sb.Append(item.Value);
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            return sb.ToString();
        }
        /// <summary>
        /// 生成表单数据的结尾
        /// </summary>
        /// <param name="boundary">边界值</param>
        /// <returns></returns>
        public static string GenerateFormDataEnding(string boundary)
        {
            if (string.IsNullOrEmpty(boundary)) return string.Empty;
            return "\r\n--" + boundary + "--\r\n";
        }
    }
}
