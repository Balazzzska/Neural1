using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Car
{
    public partial class Graph : Form
    {
        public Graph(int crvcnt)
        {
            InitializeComponent();

            var p = zedGraphControl1.GraphPane;

            for (int i = 0; i < crvcnt; i++)
                p.AddCurve("ASD", null, Color.Black);

            p.Title.IsVisible = false;
            p.Legend.IsVisible = false;
        }


        private void Graph_Load(object sender, EventArgs e)
        {

        }

        int time = 0;
        public void Add(List<double> d)
        {
            var p = zedGraphControl1.GraphPane;
            for (int i = 0; i < p.CurveList.Count; i++)
                p.CurveList[i].AddPoint(time, d[i]);

            time++;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
        }
    }
}
