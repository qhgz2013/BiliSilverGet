using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int _currentIndex; // -1=auto, others are in the list
        private float _currentOffset;
        private double _totalHeight;
        private double _currentHeight;

        private const int _max_item_count = 1000;
        private float _singleLineHeight;
        //private SizeF _size;
        public commentGraphic(int width, int height)
        {
            if (width < 1 || height < 1) throw new ArgumentException("Invalid width/height");
            _width = width; _height = height;
            //_size = new SizeF(_width, _height);
            _currentIndex = -1;
            _currentHeight = -_height;
            _stringList = new List<ColorStringCollection>();
            _heightList = new List<float>();
            var bm = new Bitmap(1, 1);
            var gr = Graphics.FromImage(bm);
            _singleLineHeight = gr.MeasureString("_|", _defaultFont).Height;
            gr.Dispose();
            bm.Dispose();
        }
        private List<float> _heightList;
        private List<ColorStringCollection> _stringList;
        private Font _defaultFont = new Font("微软雅黑", 10);
        private int _ScrollerWidth = 5;
        public void Clear()
        {
            _heightList.Clear();
            _stringList.Clear();
            _totalHeight = 0;
            _currentIndex = -1;
            _currentHeight = -_height;
        }
        public Image GetImage()
        {
            //timing starts
            var sw = new Stopwatch();
            sw.Start();

            var bmp = new Bitmap(_width, _height);
            var size = new SizeF(_width - _ScrollerWidth, _height);
            if (_stringList.Count > 0)
            {
                var gr = Graphics.FromImage(bmp);
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                int index = _currentIndex;
                if (index == -1)
                {
                    _currentOffset = _height - 1;
                    for (int i = _heightList.Count - 1; i >= 0 && _currentOffset > 0; i--)
                    {
                        _currentOffset -= _heightList[i];
                        index = i;
                    }
                }

                var begin_point = new PointF(0, _currentOffset);
                for (int i = index; i < _stringList.Count && begin_point.Y < _height; i++)
                {
                    var item = _stringList[i];
                    foreach (var str in item)
                    {
                        DrawString(gr, str.Value, _defaultFont, new SolidBrush(str.Key), ref begin_point, size);
                    }
                    if (begin_point.X != 0)
                    {
                        begin_point.X = 0;
                        begin_point.Y += _singleLineHeight;
                    }
                }

                //draw scroll bar
                if (_totalHeight > _height)
                {
                    float bar_height = (float)(_height / _totalHeight * _height);
                    var ofs = (float)(_currentHeight / _totalHeight * _height);
                    gr.FillRectangle(Brushes.DarkGray, _width - _ScrollerWidth - 1, ofs, _ScrollerWidth, bar_height);
                }
                gr.Dispose();
            }

            //timing stops
            sw.Stop();
            var ellapsed_ms = sw.ElapsedMilliseconds;
            Debug.Print("GetImage called, function ellapsed " + ellapsed_ms + "ms");
            
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
            var bmp = new Bitmap(1, 1);
            var gr = Graphics.FromImage(bmp);
            
            PointF pt = new PointF(0, 0);
            SizeF size = new SizeF(_width - _ScrollerWidth, _height);

            foreach (var item in csc)
            {
                var brush = new SolidBrush(item.Key);
                DrawString(gr, item.Value, _defaultFont, brush, ref pt, size);
            }

            gr.Dispose();

            if (pt.X > 0)
            {
                pt.X = 0;
                pt.Y += _singleLineHeight;
            }

            _stringList.Add(csc);
            _heightList.Add(pt.Y);
            _totalHeight += pt.Y;
            if (_currentIndex == -1) _currentHeight += pt.Y;
            if (_stringList.Count > _max_item_count)
            {
                _totalHeight -= _heightList[0];
                //if (_currentIndex == 0) _currentHeight -= _heightList[0];
                _currentHeight -= _heightList[0];
                if (_currentHeight < 0) _currentHeight = 0;
                if (_currentIndex > 0) _currentIndex--;
                _heightList.RemoveAt(0);
                _stringList.RemoveAt(0);
            }
        }
        public void ScrollDown(int dHeight = 50)
        {
            if (_currentIndex == -1 || dHeight <= 0) return;
            _currentHeight += dHeight;
            if (_currentHeight + _height >= _totalHeight)
            {
                _currentIndex = -1;
                _currentHeight = _totalHeight - _height;
            }
            else
            {
                _currentOffset -= dHeight;
                for (int i = _currentIndex; i < _stringList.Count && _currentOffset < 0; i++)
                {
                    _currentIndex = i;
                    _currentOffset += _heightList[_currentIndex];
                }
                _currentOffset -= _heightList[_currentIndex--];
            }
        }
        public void ScrollUp(int dHeight = 50)
        {
            if (dHeight <= 0 || _totalHeight <= _height) return;
            _currentHeight -= dHeight;
            if (_currentHeight < 0)
            {
                _currentHeight = 0;
                _currentIndex = 0;
                //_currentOffset = 0;
                if (_totalHeight > _height) _currentOffset = 0;
                else _currentOffset = (float)(_height - _totalHeight - 1);
            }
            else
            {
                if (_currentIndex == -1)
                {
                    _currentOffset = _height - 1;
                    _currentIndex = _heightList.Count - 1;
                }
                _currentOffset += dHeight;
                //for (int i = _currentIndex; i >= 0 && _currentOffset > 0; i--)
                for (int i = _currentIndex; i >= 0 && _currentOffset >= 0; i--)
                    {
                    _currentIndex = i;
                    _currentOffset -= _heightList[i];
                }
            }

        }
        public void Resize(int width, int height)
        {
            //timer starts
            var sw = new Stopwatch();
            sw.Start();

            if (width == 0 || height == 0 || (_width == width && _height == height)) return;
            _width = width;
            _height = height;
            //clear
            _heightList.Clear();
            _totalHeight = 0;
            _currentIndex = -1;
            _currentHeight = -_height;
            //addline
            var bmp = new Bitmap(1, 1);
            var gr = Graphics.FromImage(bmp);
            SizeF size = new SizeF(_width - _ScrollerWidth, _height);
            foreach (var csc in _stringList)
            {

                PointF pt = new PointF(0, 0);
                foreach (var item in csc)
                {
                    var brush = new SolidBrush(item.Key);
                    DrawString(gr, item.Value, _defaultFont, brush, ref pt, size);
                }

                if (pt.X > 0)
                {
                    pt.X = 0;
                    pt.Y += _singleLineHeight;
                }
                
                _heightList.Add(pt.Y);
                _totalHeight += pt.Y;
            }
            _currentHeight = _totalHeight - _height;
            for (int i = _currentIndex; i >= 0; i--)
            {
                _currentHeight -= _heightList[i];
            }

            gr.Dispose();

            //timing stops
            sw.Stop();
            var ellapsed_ms = sw.ElapsedMilliseconds;
            Debug.Print("Resize called, function ellapsed " + ellapsed_ms + "ms");
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
