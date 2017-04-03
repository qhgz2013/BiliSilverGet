using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace guazi2
{
    public class commentGraphic
    {
        private int _width, _height;
        private bool _autoScroll;
        private long _currentHeight;
        private long _totalHeight;
        private const int _max_item_count = 1000;
        public commentGraphic(int width, int height)
        {
            _width = width; _height = height;
            _totalHeight = 0;
            _autoScroll = true;
            _imgCacheList = new List<Image>();
            _stringList = new List<ColorStringCollection>();
        }
        private List<Image> _imgCacheList;
        private List<ColorStringCollection> _stringList;
        private Color _commentTag = Color.Orange;
        private Font _defaultFont = new Font("微软雅黑", 10);
        private int _ScrollerWidth = 5;
        public void Clear()
        {
            _imgCacheList.Clear();
            _stringList.Clear();
            _totalHeight = 0;
            _autoScroll = true;
        }
        public Image GetImage()
        {
            var bmp = new Bitmap(_width, _height);
            var gr = Graphics.FromImage(bmp);

            long height = _totalHeight - 1;
            if (_autoScroll)
                _currentHeight = _totalHeight - _height;

            for (int i = _imgCacheList.Count - 1; i >= 0; i--)
            {
                var cur_img = _imgCacheList[i];
                height -= cur_img.Height;

                if (height > _currentHeight + _height) continue;

                gr.DrawImage(cur_img, 0, height - _currentHeight);

                if (height < _currentHeight) break;
            }

            //draw scroll bar
            if (_totalHeight > _height)
            {
                float bar_height = (float)_height / _totalHeight * _height;
                var ofs = (float)_currentHeight / _totalHeight * _height;
                gr.FillRectangle(Brushes.DarkGray, _width - _ScrollerWidth - 1, ofs, _ScrollerWidth, bar_height);
            }
            gr.Dispose();
            return bmp;
        }
        private float DrawString(Graphics gr, string str, Font font, Brush brush, ref PointF begin_point, SizeF layout_size)
        {
            var total_str = "";

            float new_line_height = gr.MeasureString(" ", font).Height;

            for (int i = 0; i < str.Length; i++)
            {
                var new_str = total_str + str[i];
                var new_size = gr.MeasureString(new_str, font);
                if (begin_point.X + new_size.Width > layout_size.Width)
                {
                    //超过当前行，选择换行
                    gr.DrawString(total_str, font, brush, begin_point);
                    total_str = str[i].ToString();
                    begin_point.X = 0;
                    begin_point.Y += new_line_height;
                }
                else
                {
                    total_str = new_str;
                }
            }

            if (!string.IsNullOrEmpty(total_str))
            {
                var size = gr.MeasureString(total_str, font);
                gr.DrawString(total_str, font, brush, begin_point);
                begin_point.X += size.Width;
            }

            return begin_point.X == 0 ? begin_point.Y : begin_point.Y + new_line_height;
        }
        public Image test()
        {
            var bmp = new Bitmap(_width, _height);
            var gr = Graphics.FromImage(bmp);

            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            var pointF = new PointF(0, 0);
            DrawString(gr, "测试一下", _defaultFont, Brushes.Black, ref pointF, new SizeF(_width, _height));
            DrawString(gr, "测试一下", _defaultFont, Brushes.Red, ref pointF, new SizeF(_width, _height));
            DrawString(gr, "测试一下ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890啊啊", _defaultFont, Brushes.Green, ref pointF, new SizeF(_width, _height));

            DrawString(gr, "测试一下", _defaultFont, Brushes.Blue, ref pointF, new SizeF(_width, _height));

            gr.Dispose();
            return bmp;
        }
        public void AddLine(ColorStringCollection csc)
        {
            var bmp = new Bitmap(_width, _height);
            var gr = Graphics.FromImage(bmp);

            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            PointF pt = new PointF(0, 0);
            SizeF size = new SizeF(_width - _ScrollerWidth, _height);

            foreach (var item in csc)
            {
                var brush = new SolidBrush(item.Key);
                DrawString(gr, item.Value, _defaultFont, brush, ref pt, size);
            }

            float height = gr.MeasureString(" |", _defaultFont).Height;
            height = pt.Y + (pt.X > 0 ? height : 0);
            gr.Dispose();

            int int_height = (int)Math.Ceiling(height);
            var bmp2 = new Bitmap(_width, int_height);
            gr = Graphics.FromImage(bmp2);
            gr.DrawImage(bmp, 0, 0);
            gr.Dispose();

            _imgCacheList.Add(bmp2);
            _stringList.Add(csc);
            _totalHeight += bmp2.Height;
            if (_imgCacheList.Count > _max_item_count)
            {
                _totalHeight -= _imgCacheList[0].Height;
                if (!_autoScroll) _currentHeight -= _imgCacheList[0].Height;
                _imgCacheList.RemoveAt(0);
                _stringList.RemoveAt(0);
            }
        }
        public void ScrollDown(int dHeight = 50)
        {
            if (_autoScroll || dHeight <= 0) return;
            _currentHeight += dHeight;
            if (_currentHeight + _height > _totalHeight) _autoScroll = true;
        }
        public void ScrollUp(int dHeight = 50)
        {
            if (dHeight < 0 || _totalHeight <= _height) return;
            _currentHeight -= dHeight;
            if (_currentHeight < 0) _currentHeight = 0;
            _autoScroll = false;
        }
        public void Resize(int width, int height)
        {
            if (width == 0 || height == 0 || (_width == width && _height == height)) return;
            _imgCacheList.Clear();
            _totalHeight = 0;
            _width = width;
            _height = height;
            //call redraw
            for (int i = 0; i < _stringList.Count; i++)
            {
                var bmp = new Bitmap(_width, _height);
                var gr = Graphics.FromImage(bmp);

                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                PointF pt = new PointF(0, 0);
                SizeF size = new SizeF(_width - _ScrollerWidth, _height);

                foreach (var item in _stringList[i])
                {
                    var brush = new SolidBrush(item.Key);
                    DrawString(gr, item.Value, _defaultFont, brush, ref pt, size);
                }

                float fHeight = gr.MeasureString(" |", _defaultFont).Height;
                fHeight = pt.Y + (pt.X > 0 ? fHeight : 0);
                gr.Dispose();

                int int_height = (int)Math.Ceiling(fHeight);
                var bmp2 = new Bitmap(_width, int_height);
                gr = Graphics.FromImage(bmp2);
                gr.DrawImage(bmp, 0, 0);
                gr.Dispose();

                _imgCacheList.Add(bmp2);
                _totalHeight += bmp2.Height;
            }
        }
    }

    public class ColorStringCollection : IList<KeyValuePair<Color, string>>
    {
        private List<KeyValuePair<Color, string>> _innerList;

        public ColorStringCollection()
        {
            _innerList = new List<KeyValuePair<Color, string>>();
        }

        public void Add(Color color, string str)
        {
            _innerList.Add(new KeyValuePair<Color, string>(color, str));
        }
        public KeyValuePair<Color, string> this[int index]
        {
            get
            {
                return ((IList<KeyValuePair<Color, string>>)_innerList)[index];
            }

            set
            {
                ((IList<KeyValuePair<Color, string>>)_innerList)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<KeyValuePair<Color, string>>)_innerList).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<KeyValuePair<Color, string>>)_innerList).IsReadOnly;
            }
        }

        public void Add(KeyValuePair<Color, string> item)
        {
            ((IList<KeyValuePair<Color, string>>)_innerList).Add(item);
        }

        public void Clear()
        {
            ((IList<KeyValuePair<Color, string>>)_innerList).Clear();
        }

        public bool Contains(KeyValuePair<Color, string> item)
        {
            return ((IList<KeyValuePair<Color, string>>)_innerList).Contains(item);
        }

        public void CopyTo(KeyValuePair<Color, string>[] array, int arrayIndex)
        {
            ((IList<KeyValuePair<Color, string>>)_innerList).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Color, string>> GetEnumerator()
        {
            return ((IList<KeyValuePair<Color, string>>)_innerList).GetEnumerator();
        }

        public int IndexOf(KeyValuePair<Color, string> item)
        {
            return ((IList<KeyValuePair<Color, string>>)_innerList).IndexOf(item);
        }

        public void Insert(int index, KeyValuePair<Color, string> item)
        {
            ((IList<KeyValuePair<Color, string>>)_innerList).Insert(index, item);
        }

        public bool Remove(KeyValuePair<Color, string> item)
        {
            return ((IList<KeyValuePair<Color, string>>)_innerList).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<KeyValuePair<Color, string>>)_innerList).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<KeyValuePair<Color, string>>)_innerList).GetEnumerator();
        }
    }

}
