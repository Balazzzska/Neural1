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
        Perceptron brain;
        public class Point
        {
            public double X;
            public double Y;
            public double Bias;
            public double label {
                get {
                    return Y > f(X) ? 1 : -1;
                }
            }

            public Point(double x, double y)
            {
                X = x;
                Y = y;
                Bias = 1;
            }

            public static double f(double x)
            {
                return -0.12 * x - 0.6;
            }

            public int PixelX {
                get {
                    return (int)Map(X, -1.2, 1.2, 0, 500);
                }
            }

            public int PixelY {
                get {
                    return (int)Map(Y, -1.2, 1.2, 500, 0);
                }
            }

            private static double Map(double x, double x1, double x2, double y1, double y2)
            {
                var m = (y2 - y1) / (x2 - x1);
                var c = y1 - m * x1; // point of interest: c is also equal to y2 - m * x2, though double math might lead to slightly different results.
                return m * x + c;
            }

            public void Draw(Graphics g, bool isbad)
            {
                int _X = PixelX;
                int _Y = PixelY;

                g.FillEllipse(label > 0 ? Brushes.Black : Brushes.White, _X - 10, _Y - 10, 20, 20);
                g.FillEllipse(isbad ? Brushes.ForestGreen : Brushes.Red, _X - 5, _Y - 5, 10, 10);
            }
        }

        public Form1()
        {
            InitializeComponent();
            random = new Random();
        }

        List<Point> points = new List<Point>();
        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World!");


            /*            brain = new Perceptron(3);
            pictureBox1.Size = new Size(500, 500);
            for (int i = 0; i < 100; i++)
            {
                var point = new Point(2 * random.NextDouble() - 1, 2 * random.NextDouble() - 1);
                points.Add(point);
            }
            Magic();*/


            var nn = new NeuralNetwork(2, 2, 1);

            var answers = new Dictionary<List<double>, List<double>>
            {
                { new List<double>() { 1, 0 }, new List<double>() { 1 } },
                { new List<double>() { 0, 1 }, new List<double>() { 1 } },
                { new List<double>() { 1, 1 }, new List<double>() { 0 } },
                { new List<double>() { 0, 0 }, new List<double>() { 0 } }
            };

            for (int i = 0; i < 50000; i++)
            {
                var input = answers.Keys.OrderBy(p => random.NextDouble()).First();
                nn.Train(input, answers[input]);
            }

            foreach(var v in answers)
            {
                var q = nn.FeedForward(v.Key);
                q.ForEach(p => Console.Write(p.ToString("0.000") + " "));
                Console.WriteLine();
            }
        }

        private static double doubleit(double d)
        {
            return d * 2;
        }

        void Magic()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Wheat);

            Point l1 = new Point(-1, Point.f(-1));
            Point l2 = new Point(1, Point.f(1));
            g.DrawLine(Pens.Black, l1.PixelX, l1.PixelY, l2.PixelX, l2.PixelY);

            foreach (var point in points)
            {
                List<double> pt = new List<double>() { point.X, point.Y, point.Bias };
                double target = point.label;
                double guess = brain.Guess(pt);
                point.Draw(g, guess == target);
            }

            l1 = new Point(-1, brain.GuessY(-1));
            l2 = new Point(1, brain.GuessY(1));
            g.DrawLine(new Pen(Color.Blue, 3), l1.PixelX, l1.PixelY, l2.PixelX, l2.PixelY);

            pictureBox1.Image = bmp;

            foreach (var point in points)
            {
                List<double> pt = new List<double>() { point.X, point.Y, point.Bias };
                double target = point.label;
                brain.Train(pt, target);
            }
        }



        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                Magic();
            }

            if (keyData == Keys.Escape)
                Application.Exit();

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
