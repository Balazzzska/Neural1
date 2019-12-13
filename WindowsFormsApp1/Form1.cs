using System.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Random random;

        public Form1()
        {
            InitializeComponent();
            random = new Random();
        }

        NeuralNetwork nn = new NeuralNetwork(2, 10, 1);
        Dictionary<List<double>, List<double>> answers = new Dictionary<List<double>, List<double>>
            {
                { new List<double>() { -1, 1 }, new List<double>() { 1 } },
                { new List<double>() { 1, -1 }, new List<double>() { 1 } },
                { new List<double>() { 1, 1 }, new List<double>() { 0 } },
                { new List<double>() { -1, -1 }, new List<double>() { 0 } },
                { new List<double>() { 0, 0 }, new List<double>() { 0.5 } },
            };

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World!");
            Draw(nn);
        }

        void Draw(NeuralNetwork nn)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Wheat);

            var res = 50;
            float s = pictureBox1.Width / (res+1);
            for (double x = 0; x <= res; x++)
                for (double y = 0; y <= res; y++)
                {
                    double i1 = x * 2 / res - 1;
                    double i2 = y * 2 / res - 1;
                    var q = nn.FeedForward(new List<double>() { i1, i2 }).First();
                    q *= 255;
                    var w = (int)q;
                    Color c = Color.FromArgb(w, w, w);
                    g.FillRectangle(new SolidBrush(c), (float)(x * s), (float)(y * s), s, s);
                }

            pictureBox1.Image = bmp;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
                Draw(nn);

            if (keyData == Keys.R)
            {
                nn.Reset();
                Draw(nn);
            }

            if (keyData == Keys.T)
            {
                for (int i = 0; i < 1000; i++)
                {
                    var input = answers.Keys.OrderBy(p => random.NextDouble()).First();
                    nn.Train(input, answers[input]);
                }
                Draw(nn);
            }

            if (keyData == Keys.Escape)
                Application.Exit();

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
