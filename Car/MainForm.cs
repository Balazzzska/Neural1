using Neural;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Car.Helper;

namespace Car
{
    public partial class MainForm : Form
    {
        List<Car> cars;
        Image backGround;
        List<Line> obstacles;

        const int POPULATIONSIZE = 80;
        const int LAPSTOCOMPLETE = 3;
        readonly Vector2 STARTPOSITION = new Vector2(300, 900);
        Line FinishLine = new Line(400, 800, 400, 1200);
        public MainForm()
        {
            InitializeComponent();

            iskeydown = new Dictionary<Keys, bool>
            {
                { Keys.W, false },
                { Keys.S, false },
                { Keys.A, false },
                { Keys.D, false }
            };
        }

        Graph graph1, graph2;
        private void MainForm_Load(object sender, EventArgs e)
        {
            cars = new List<Car>();
            for (int i = 0; i < POPULATIONSIZE; i++)
            {
                cars.Add(new Car(STARTPOSITION));
                Thread.Sleep(2);
            }
            LoadMap();

            graph1 = new Graph(1)
            {
                Left = 1000,
                Top = 0
            };
            graph1.BringToFront();
            graph1.Show();

            graph2 = new Graph(1)
            {
                Left = 1000,
                Top = 500
            };
            graph2.BringToFront();
            graph2.Show();
        }

        private void LoadMap()
        {
            var file = File.ReadAllLines("data3.csv");
            var points = new List<Vector2>();
            foreach (var b in file)
            {
                var s = b.Split(',');
                var v = new Vector2(float.Parse(s[0]), float.Parse(s[1]));

                //v *= 1.4f; //scale up

                points.Add(v);
            }
            obstacles = new List<Line>();

            List<Vector2> points2 = new List<Vector2>();
            List<Vector2> points3 = new List<Vector2>();
            for (int i = 1; i < points.Count; i++)
            {
                var a = points[i - 1];
                var b = points[i];

                var alpha = atan2(a.Y - b.Y, a.X - b.X);
                var delta = new Vector2(55, 0);
                delta = Vector2.Transform(delta, Matrix3x2.CreateRotation(alpha + PI / 2));
                var midpoint = a / 2 + b / 2;
                points2.Add(midpoint + delta);
                points3.Add(midpoint - delta);
            }

            CreatePolygon(points2, out List<Line> poly2);
            CreatePolygon(points3, out List<Line> poly3);

            obstacles.AddRange(poly2);
            obstacles.AddRange(poly3);

            backGround = DrawCircuit(Color.AntiqueWhite);
        }
        Image DrawCircuit(Color backcolor)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(backcolor);
            foreach (var l in obstacles)
                g.DrawLine(new Pen(Color.DarkGray, 2f), l.a.X, l.a.Y, l.b.X, l.b.Y);

            return bmp;
        }

        int frame = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Bitmap bmp = (Bitmap)backGround.Clone();
            Graphics g = Graphics.FromImage(bmp);

            g.DrawString((frame++).ToString(),
                         Font, Brushes.Black, 10, 100);

            bool allcrashed = true;
            bool raceEnded = false;

            foreach (var c in cars)
            {
                //GetKeyboardInput(out float steer_angle, out float throttle);

                if (!c.Crashed)
                {
                    var raycast = c.RayCast(obstacles, out List<Vector2> pts);
                    var vision = raycast.Select(p => p / 100f).ToList();

                    c.MakeDesision(vision, out float wheel);
                    var throttle = 1f;
                    float steer_angle = Helper.Map(wheel, -1, 1, -PI / 4f, PI / 4f);

                    c.Move(steer_angle, throttle);
                    c.CheckFinishLine(FinishLine);
                    c.CheckCollision(obstacles);

                    allcrashed = false;
                }
            }

            if (frame == 1000)
                allcrashed = true;

            if (cars.Any(p => p.FinishedLaps == LAPSTOCOMPLETE))
                raceEnded = true;

            foreach (var c in cars.Where(p => p.Crashed))
                c.Draw(g, c.Crashed ? Color.Red : Color.ForestGreen);
            foreach (var c in cars.Where(p => !p.Crashed))
                c.Draw(g, c.Crashed ? Color.Red : Color.ForestGreen);

            g.DrawLine(new Pen(Color.Green, 3f),
                       FinishLine.a.X,
                       FinishLine.a.Y,
                       FinishLine.b.X,
                       FinishLine.b.Y);

            if (allcrashed || raceEnded)
            {
                var rank = cars.OrderByDescending(p => p.DistanceTravelled);
                var worstcar = rank.Last();
                var bestcar = rank.First();

                if (raceEnded)
                {
                    bestcar = cars.Where(p => p.FinishedLaps == LAPSTOCOMPLETE)
                                  .OrderBy(p => p.DistanceTravelled)
                                  .First();
                    graph1.Add(new List<double>() { frame });
                }
                else
                    graph1.Add(new List<double>() { double.NaN });

                graph2.Add(new List<double>() { bestcar.DistanceTravelled });

                g.DrawString(bestcar.DistanceTravelled.ToString(),
                             Font, Brushes.Black, 10, 150);

                var hist = cars.GroupBy(p => p.FinishedLaps).Select(p => p.Key.ToString() + " " + p.Count().ToString()).ToList();

                int y = 230;
                for (int i = 0; i < hist.Count; i++)
                    g.DrawString(hist[i],
                                 Font, Brushes.Black, 10, y += 30);

                cars.Clear();
                cars.Add(bestcar.Clone());
                cars[0].Position = STARTPOSITION;

                for (int i = 0; i < POPULATIONSIZE - 1; i++)
                {
                    var car = new Car(STARTPOSITION);
                    car.Brain = Neural.Matrix.FromList(bestcar.Brain.ToList());
                    car.Mutate();
                    cars.Add(car);
                }

                bestcar.Draw(g, Color.Purple);

                frame = 0;
                WaitAfterRace();
            }

            pictureBox1.Image = bmp;
        }

        void WaitAfterRace()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            timer1.Enabled = false;
            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Enabled = true;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

        void GetKeyboardInput(out float steer_angle, out float throttle)
        {
            steer_angle = 0f;
            if (iskeydown[Keys.A])
                steer_angle -= 1;
            if (iskeydown[Keys.D])
                steer_angle += 1;

            throttle = 0;
            if (iskeydown[Keys.W])
                throttle += 1f;
            if (iskeydown[Keys.S])
                throttle -= 1f;
        }

        Dictionary<Keys, bool> iskeydown;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (iskeydown.ContainsKey(e.KeyCode))
                iskeydown[e.KeyCode] = true;
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (iskeydown.ContainsKey(e.KeyCode))
                iskeydown[e.KeyCode] = false;
            base.OnKeyUp(e);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
                Application.Exit();

            if (keyData == Keys.Space)
            {
                timer1.Enabled = !timer1.Enabled;
            }

            if (keyData == Keys.R)
            {
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

}
