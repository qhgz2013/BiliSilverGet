using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace GeeTestLib
{
    public partial class PathTraining : Form
    {
        public PathTraining()
        {
            InitializeComponent();
        }

        private void PathTraining_Load(object sender, EventArgs e)
        {
        }
        private static Random rnd = new Random();
        private double cur_val;
        private List<int[]> _move_list;
        private DateTime beg_time;
        private void _refresh_new_position()
        {
            //cur_val = rnd.NextDouble();
            _move_list = new List<int[]>();
            var gr = CreateGraphics();
            gr.FillRectangle(Brushes.White, new Rectangle(20, 20, 180, 50));
            gr.DrawRectangle(Pens.Red, new Rectangle(40, 30, (int)(cur_val * 100), 30));
        }
        private bool _is_down;
        private void PathTraining_MouseDown(object sender, MouseEventArgs e)
        {
            _is_down = true;
            beg_time = DateTime.Now;
        }

        private void PathTraining_MouseUp(object sender, MouseEventArgs e)
        {
            _is_down = false;
            var deltas = new List<int[]>();
            deltas.Add(new int[] { 0, 0, 0 });
            for (int i = 0; i < _move_list.Count - 1; i++)
            {
                deltas.Add(new int[] { _move_list[i + 1][0] - _move_list[i][0], _move_list[i + 1][1] - _move_list[i][1], _move_list[i + 1][2] - _move_list[i][2] });
            }
            //merge half data
            //for (int i = 0; i < deltas.Count - 1; i++)
            //{
            //    deltas[i] = new int[] { deltas[i][0] + deltas[i + 1][0], deltas[i][1] + deltas[i + 1][1], deltas[i][2] + deltas[i + 1][2] };
            //    deltas.RemoveAt(i + 1);
            //}
            //v-t plot
            double t = 0;
            var vt_list = new List<KeyValuePair<double, double>>();
            for (int i = 0; i < deltas.Count; i++)
            {
                if (deltas[i][2] != 0)
                {
                    var data = (Math.Sqrt(Math.Pow(deltas[i][0], 2) + Math.Pow(deltas[i][1], 2))) / deltas[i][2] * 1000;
                    vt_list.Add(new KeyValuePair<double, double>(t, data));
                    t += deltas[i][2];
                }
            }
            //to plot
            chart1.Series[0].Points.Clear();
            for (int i = 0; i < vt_list.Count; i++)
            {
                chart1.Series[0].Points.AddXY(vt_list[i].Key, vt_list[i].Value);
            }
            //_refresh_new_position();
            dataOut = deltas;
            Close();
        }
        public List<int[]> dataOut;
        public void setw(int x)
        {
            cur_val = x / 100.0;
            _refresh_new_position();
        }
        private void PathTraining_MouseMove(object sender, MouseEventArgs e)
        {
            if (_is_down)
            {
                _move_list.Add(new int[] { e.X, e.Y, (int)(DateTime.Now - beg_time).TotalMilliseconds });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            _refresh_new_position();
        }
    }
}
