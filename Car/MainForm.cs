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
using System.Threading.Tasks;
using System.Windows.Forms;
using static Car.Helper;

namespace Car
{
    public partial class MainForm : Form
    {
        Car car;
        NeuralNetwork neuralNetwork;
        Image backGround;
        List<Line> obstacles;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            car = new Car(250, 320);
            LoadMap();

            neuralNetwork = new NeuralNetwork(9, 6, 1);
        }

        private void LoadMap()
        {
            var file = File.ReadAllLines("data2.csv");
            var points = new List<Vector2>();
            foreach (var b in file)
            {
                var s = b.Split(',');
                var v = new Vector2(float.Parse(s[0]), float.Parse(s[1]));

                v *= 1.4f; //scale up

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
                var delta = new Vector2(60, 0);
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

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Bitmap bmp = (Bitmap)backGround.Clone();
            Graphics g = Graphics.FromImage(bmp);

            if (!car.Crashed)
            {
                car.Draw(g);

                var raycast = car.RayCast(obstacles, out List<Vector2> pts);
                foreach (var p in pts)
                {
                    var d = (car.Position - p).Length();

                    Color c = Color.Orange;
                    if (d < 50) c = Color.OrangeRed;
                    if (d > 80) c = Color.ForestGreen;

                    g.DrawLine(
                        new Pen(c, 1),
                        car.Position.X,
                        car.Position.Y,
                        p.X,
                        p.Y);
                }

                var vision = raycast.Select(p => p / 100f).ToList();
                var a = neuralNetwork.FeedForward(vision.Select(p => (double)p).ToList());

                float steer_angle = Helper.Map((float)a[0], 0, 1, -PI / 4f, PI / 4f);

                var throttle = 1f;
                //GetInput(out float steer_angle, out float throttle);

                car.Move(steer_angle, throttle);
                car.CheckCollision(obstacles);
            }
            else
            {
                bmp = (Bitmap)DrawCircuit(Color.Gray);
                g = Graphics.FromImage(bmp);
                g.DrawString(
                    "Distance traveled: " + car.DistanceTravelled.ToString("0.0"),
                    this.Font,
                    Brushes.Black,
                    10,
                    50);
                car.Draw(g);
                timer1.Enabled = false;
            }

            pictureBox1.Image = bmp;
        }
        void GetInput(out float steer_angle, out float throttle)
        {
            const float steering_angle = PI / 4;   // Amount that front wheel turns, in radians
            steer_angle = 0f;
            if (iskeydown[Keys.A]) steer_angle -= 1;
            if (iskeydown[Keys.D]) steer_angle += 1;
            steer_angle *= steering_angle;

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
                car = new Car(250, 320);
                timer1.Enabled = true;
            }

            if (keyData == Keys.R)
            {
                neuralNetwork.Reset();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

}
