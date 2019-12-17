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

namespace Car
{
    public partial class Form1 : Form
    {
        //http://kidscancode.org/godot_recipes/2d/car_steering/


        Vector2 carPosition;
        Vector2 carVelocity;
        Vector2 carAcceleration;
        float carThrottle;  //-1: fék, 1: gáz
        float carWheel;     //-1: bal, 0: közép, 1: jobb

        float engine_power = 1; //
        float carRotation; //Kell a kirajzolashoz
        float wheel_base = 40;  // Distance from front to rear wheel
        float carLength = 50; //Total length of the car

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
            carPosition = new Vector2(50, 300);
            carVelocity = Vector2.Zero;
            carAcceleration = Vector2.Zero;
            carWheel = 0;
        }

        float cos(double d) => (float)Math.Cos(d);
        float sin(double d) => (float)Math.Sin(d);
        float atan2(double x, double y) => (float)Math.Atan2(x, y);
        private void Timer1_Tick(object sender, EventArgs e)
        {
            Loop();
            Draw();
        }

        void getinput(out float steer_angle)
        {
            const float PI = (float)Math.PI;
            const float steering_angle = PI / 8;   // Amount that front wheel turns, in radians
            steer_angle = 0f;
            if (iskeydown[Keys.A]) steer_angle -= 1;
            if (iskeydown[Keys.D]) steer_angle += 1;
            steer_angle *= steering_angle;

            carAcceleration = Vector2.Zero;
            if (iskeydown[Keys.W])
                carAcceleration = new Vector2(engine_power, 0);
        }

        void calculate_steering(float steer_angle)
        {
            /*var rear_wheel = carPosition;
            rear_wheel.X -= wheel_base / 2f;

            var front_wheel = carPosition;
            front_wheel.X += wheel_base / 2f;

            rear_wheel += carVelocity;
            front_wheel += Vector2.Transform(carVelocity, Matrix3x2.CreateRotation(steer_angle));
            */

            Vector2 front_wheel = carPosition;
            front_wheel.X += wheel_base / 2;
            front_wheel = Vector2.Transform(front_wheel, Matrix3x2.CreateRotation(carRotation, carPosition));

            Vector2 rear_wheel = carPosition;
            rear_wheel.X -= new Vector2(cos(carRotation), sin(carRotation)) * (wheel_base / 2);

            rear_wheel += Vector2.Transform(carVelocity, Matrix3x2.CreateRotation(carRotation));
            front_wheel += Vector2.Transform(carVelocity, Matrix3x2.CreateRotation(carRotation + steer_angle));

            Text = front_wheel.ToString() + "  |   " + front_wheel.ToString();

            carPosition = (rear_wheel + front_wheel) / 2f;
            carRotation = atan2(front_wheel.Y - rear_wheel.Y, front_wheel.X - rear_wheel.X);

            /* var new_heading = Vector2.Normalize(front_wheel - rear_wheel);
              carVelocity = new_heading * carVelocity.Length();
              Text = new_heading.ToString();

              carRotation = atan2();*/
        }

        void Loop()
        {
            getinput(out float steer_angle);

            carVelocity += carAcceleration;
            calculate_steering(steer_angle);

            //carPosition += carVelocity;
        }

        void Draw()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Wheat);

            g.RotateTransform(carRotation * 180);
            g.TranslateTransform(carPosition.X, carPosition.Y, MatrixOrder.Append);

            g.FillRectangle(Brushes.Black, -wheel_base / 2 - 5, -13, 10, 3);
            g.FillRectangle(Brushes.Black, wheel_base / 2 - 5, -13, 10, 3);
            g.FillRectangle(Brushes.Black, -wheel_base / 2 - 5, 10, 10, 3);
            g.FillRectangle(Brushes.Black, wheel_base / 2 - 5, 10, 10, 3);

            g.FillRectangle(Brushes.DarkOrange, -carLength / 2, -10, carLength, 20);
            g.FillRectangle(Brushes.DarkBlue, carLength / 2 - 5, -10, 5, 20);
            g.ResetTransform();

            pictureBox1.Image = bmp;
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

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
