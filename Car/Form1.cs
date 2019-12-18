using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using static Car.Helper;

namespace Car
{
    public class Line
    {
        public Vector2 a;
        public Vector2 b;
        public Line(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }

        public Line(float x1, float y1, float x2, float y2)
        {
            this.a = new Vector2(x1, y1);
            this.b = new Vector2(x2, y2);
        }
    }

    public partial class Form1 : Form
    {
        const float PI = (float)Math.PI;
        Car car;
        List<Line> obstacles;
        public Form1()
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

        private void Form1_Load(object sender, EventArgs e)
        {
            car = new Car(150, 300);

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
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);

            if (!car.Crashed)
            {
                getinput(out float steer_angle, out float throttle);
                car.Move(steer_angle, throttle);
                car.CheckCollision(obstacles);

                var r = car.RayCast(obstacles, out List<Vector2> pts);
                Text = car.Speed.ToString();
                g.Clear(Color.AntiqueWhite);

                car.Draw(g);

                foreach (var l in obstacles)
                    g.DrawLine(new Pen(Color.DarkGray, 2f), l.a.X, l.a.Y, l.b.X, l.b.Y);

                foreach (var p in pts)
                {
                    var d = (car.Position - p).Length();
                    g.DrawLine(
                        d < 100 ? Pens.Red : Pens.Black,
                        car.Position.X,
                        car.Position.Y,
                        p.X,
                        p.Y);
                }
            }
            else
            {
                g.Clear(Color.Red);

                car.Draw(g);

                foreach (var l in obstacles)
                    g.DrawLine(new Pen(Color.DarkGray, 2f), l.a.X, l.a.Y, l.b.X, l.b.Y);
            }
            pictureBox1.Image = bmp;
        }
        void getinput(out float steer_angle, out float throttle)
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

            if(keyData == Keys.Space)
            {
                car = new Car(300, 300);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
