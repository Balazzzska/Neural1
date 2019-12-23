using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Car.Helper;


namespace Car
{
    public class Car
    {
        //http://kidscancode.org/godot_recipes/2d/car_steering/

        public Vector2 Position;
        private Vector2 LastPosition;
        public List<Vector2> Trail;

        const float Wheelbase = 30;  // Distance from front to rear wheel
        const float Length = 40; //Total length of the car
        const float Width = Length / 1.618f;

        //Forward acceleration force.
        const float EnginePower = 0.4f; //

        //Friction is the force applied by the ground. 
        //It’s high if driving on sand, but low if driving on ice. 
        //Friction is proportional to velocity - the faster you’re going the stronger the force.
        const float Friction = -0.04f;

        //Drag is the force resulting from wind resistance. 
        //It’s based on the car’s cross-section - a large truck or van experiences more drag than a sleek race car. 
        //Drag is proportional to the velocity squared.
        const float Drag = -0.00015f;

        float Heading;
        public float Speed;
        public bool Crashed;
        public float DistanceTravelled;
        const int VISIONCOUNT = 9;

        public int FinishedLaps;

        Random random;
        public Neural.Matrix Brain;

        public Car(Vector2 position)
        {
            Position = position;
            Speed = 0;
            Trail = new List<Vector2>();
            Crashed = false;
            DistanceTravelled = 0;
            Heading = 0;
            random = new Random();
            FinishedLaps = 0;

            Brain = new Neural.Matrix(VISIONCOUNT, 1);
            Brain.Randomize();
        }

        public void Move(float steerAngle, float throttle)
        {
            LastPosition = Position;
            Trail.Add(Position);

            var acceleration = 0f;

            //Friction and drag
            var frictionForce = Speed * Friction;
            var dragForce = Speed * Speed * Drag;

            acceleration += frictionForce;
            acceleration += dragForce;

            //This will ensure that the car doesn’t keep creeping forward at very low speeds as friction never quite reaches zero.
            if (Speed < 1e-10)
                Speed = 0;

            //Steering
            Vector2 frontWheel = Position + Wheelbase / 2 * new Vector2(cos(Heading), sin(Heading));
            Vector2 backWheel = Position - Wheelbase / 2 * new Vector2(cos(Heading), sin(Heading));

            var dt = 1;
            backWheel += Speed * dt * new Vector2(cos(Heading), sin(Heading));
            frontWheel += Speed * dt * new Vector2(cos(Heading + steerAngle), sin(Heading + steerAngle));

            Position = (frontWheel + backWheel) / 2;
            Heading = atan2(frontWheel.Y - backWheel.Y, frontWheel.X - backWheel.X);

            DistanceTravelled += Speed;

            //Acceleration
            acceleration += throttle * EnginePower;
            Speed += acceleration;
        }

        public void Draw(Graphics g, Color c)
        {
            g.RotateTransform(Heading * 180f / (float)Math.PI);
            g.TranslateTransform(Position.X, Position.Y, MatrixOrder.Append);

            if (Crashed) {
                g.DrawRectangle(Pens.Black, -Length / 2, -Width / 2, Length, Width);
            }
            else
            {
                g.FillRectangle(new SolidBrush(c), -Length / 2, -Width / 2, Length, Width);
                g.FillRectangle(Brushes.White, Length / 2 - 5, -Width / 2, 5, Width);
            }
            g.ResetTransform();

            if (!Crashed)
                foreach (var t in Trail)
                    g.FillEllipse(new SolidBrush(c),
                        t.X - 1,
                        t.Y - 1,
                        2,
                        2);
        }
        public void CheckCollision(List<Line> obstacles)
        {
            var bounding = GetBoundingRectangle();

            foreach (var b in bounding)
            {
                foreach (var o in obstacles)
                {
                    FindIntersection(b, o, out bool l, out bool s, out Vector2 i, out Vector2 c1, out Vector2 c2);

                    if (s)
                        Crashed = true;
                }
            }
        }
        public void CheckFinishLine(Line line)
        {
            FindIntersection(line,
                             new Line(LastPosition, Position),
                             out bool lines_intersect,
                             out bool segments_intersect,
                             out Vector2 intersection,
                             out Vector2 close_p1,
                             out Vector2 close_p2);

            if (segments_intersect)
                FinishedLaps++;
        }
        private static double Sigmoid(double d) => 1 / (1 + Math.Exp(-d));
        public void MakeDesision(List<float> vision, out float wheel)
        {
            var p = Neural.Matrix.DotProduct(Neural.Matrix.FromList(vision), Brain);
            var sum = 2 * Sigmoid(p.SUM()) - 1;
            wheel = (float)sum;
        }
        public void Mutate()
        {
            int r = random.Next(1, 5);
            for (int i = 0; i < r; i++)
            {
                var k = random.Next(0, Brain.RowCount - 1);
                Brain.Data[k, 0] = 2 * random.NextDouble() - 1;
                Thread.Sleep(1);
            }
        }
        public List<float> RayCast(List<Line> lines, out List<Vector2> intersectionpoints)
        {
            const float PI = (float)Math.PI;
            var result = new List<float>();

            intersectionpoints = new List<Vector2>();
            float deg = -PI / 4f;
            float degstep = PI / 16f;
            for (int i = 0; i < VISIONCOUNT; i++)
            {
                var v = new Vector2(100, 0);
                v = Vector2.Transform(v, Matrix3x2.CreateRotation(deg + Heading));
                v += Position;

                List<Vector2> tmp = new List<Vector2>();

                foreach (var l in lines)
                {
                    FindIntersection(Position, v, l.a, l.b, out bool intersect, out bool segmentsintersect, out Vector2 intersection, out Vector2 p1, out Vector2 p2);

                    if (segmentsintersect)
                        tmp.Add(intersection);
                }

                var a = tmp.Count > 0 ? tmp.OrderBy(p => p.Length()).FirstOrDefault() : v;
                intersectionpoints.Add(a);
                result.Add((a - Position).Length());

                deg += degstep;
            }

            return result;
        }
        public List<Line> GetBoundingRectangle()
        {
            var w = Length / 2;
            var l = Width / 2;

            List<Vector2> pts = new List<Vector2>
            {
                new Vector2(-w, l),
                new Vector2(-w, -l),
                new Vector2(w, -l),
                new Vector2(w, l),
            };

            pts = pts.Select(q => Vector2.Transform(q, Matrix3x2.CreateRotation(Heading))).ToList();
            pts = pts.Select(q => q + Position).ToList();

            CreatePolygon(pts, out List<Line> p);
            return p;
        }
    }
}
