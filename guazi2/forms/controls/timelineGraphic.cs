using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace guazi2
{
    public class timelineGraphic
    {
        private int _width, _height, _timelineLength, _markerDuration;
        private const int _defaultTimelineLength = 15 * 60;
        private const int _defaultMarkerDuration = 3 * 60;

        private SortedList<DateTime, double>[] _innerList;
        private DateTime _minTime, _maxTime;

        //private Tracer _tracer;

        private Pen _border_pen;
        private Pen _marker_pen;
        private Pen[] _line_pen;
        private int _series_count;

        public timelineGraphic(int width, int height, int timelineLength = _defaultTimelineLength, int markerDuration = _defaultMarkerDuration, int series_count = 1)
        {
            _width = width; _height = height; _timelineLength = timelineLength; _markerDuration = markerDuration;
            _series_count = series_count;
            _innerList = new SortedList<DateTime, double>[_series_count];

            //_tracer = new Tracer();

            _border_pen = Pens.Black;
            _marker_pen = Pens.Gray;
            _line_pen = new Pen[_series_count];
            for (int i = 0; i < _series_count; i++)
            {
                _innerList[i] = new SortedList<DateTime, double>();
                _line_pen[i] = Pens.Blue;
            }
        }
        public void AddValue(double value, DateTime specific_time, int series = 0)
        {
            if (series >= _series_count) return;
            if (specific_time.AddSeconds(_timelineLength) < _maxTime) return;

            if (_innerList[series].Count == 0)
            {
                _minTime = specific_time;_maxTime = specific_time;
            }
            else
            {
                if (_minTime > specific_time) { _minTime = specific_time; }
                if (_maxTime < specific_time) { _maxTime = specific_time; }
            }


            for (int i = 0; i < _innerList[series].Count; i++)
            {
                var item = _innerList[series].ElementAt(i);
                if (item.Key.AddSeconds(_timelineLength) < _maxTime)
                {
                    _innerList[series].RemoveAt(i);
                    i--;
                }
            }

            _innerList[series].Add(specific_time, value);
        }
        public void AddValue(double value, int series = 0)
        {
            AddValue(value, DateTime.Now, series);
        }
        private double get_time_pos(DateTime dt)
        {
            var ts = (_maxTime - dt);
            return (_width - 1 - _width * ts.TotalSeconds / _timelineLength);
        }
        public Image PlotImage()
        {
            var out_bmp = new Bitmap(_width, _height);

            var gr = Graphics.FromImage(out_bmp);
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            gr.DrawRectangle(_border_pen, 0, 0, _width - 1, _height - 1);

            double marker_distance = (double)_width * _markerDuration / _timelineLength;

            var tt = DateTime.Now;
            var ts = (tt.AddSeconds(_timelineLength) - tt);
            tt = _minTime - ts;
            DateTime axis_minimum = new DateTime(tt.Year, tt.Month, tt.Day, tt.Hour, 0, 0);
            //begin axis paint
            //todo: 优化
            while (axis_minimum < _maxTime)
            {
                var pos = (int)get_time_pos(axis_minimum);
                if (pos > 0 && pos < _width)
                {
                    gr.DrawLine(_marker_pen, pos, 0, pos, _height - 1);
                }
                axis_minimum = axis_minimum.AddSeconds(_markerDuration);
            }

            for (int cur_series = 0; cur_series < _series_count; cur_series++)
            {

                if (_innerList[cur_series].Count == 0)
                {
                    gr.Dispose();
                    return out_bmp;
                }
                //getting min&max value

                double min_val = _innerList[cur_series].ElementAt(0).Value; double max_val = min_val;
                for (int i = 1; i < _innerList[cur_series].Count; i++)
                {
                    var val = _innerList[cur_series].ElementAt(i).Value;
                    if (val < min_val) { min_val = val; }
                    if (val > max_val) { max_val = val; }
                }

                double dval = max_val - min_val;

                var last_element = _innerList[cur_series].ElementAt(0);

                for (int i = 1; i < _innerList[cur_series].Count; i++)
                {
                    var cur_element = _innerList[cur_series].ElementAt(i);
                    float x1 = (float)get_time_pos(last_element.Key);
                    float x2 = (float)get_time_pos(cur_element.Key);

                    float y1 = dval != 0 ? (float)(_height - 1 - (_height * (last_element.Value - min_val) / dval)) : _height - 1;
                    float y2 = dval != 0 ? (float)(_height - 1 - (_height * (cur_element.Value - min_val) / dval)) : _height - 1;

                    gr.DrawLine(_line_pen[cur_series], x1, y1, x2, y2);
                    last_element = cur_element;
                }
            }

            gr.Dispose();
            return out_bmp;
        }
        public void Reset()
        {
            for (int i = 0; i < _series_count; i++)
            {
                _innerList[i].Clear();
            }
            _minTime = DateTime.MinValue;
            _maxTime = DateTime.MinValue;
        }
        public void ChangePenColor(Color new_color, int series)
        {
            if (series >= _series_count) return;
            _line_pen[series] = new Pen(new_color);
        }
    }
}
