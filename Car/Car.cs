using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Car.Helper;


namespace Car
{
    public class Car
    {
        //http://kidscancode.org/godot_recipes/2d/car_steering/

        public Vector2 Position;
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

        public Car(float posx, float posy)
        {
            Position = new Vector2(posx, posy);
            Speed = 0;
            Trail = new List<Vector2>();
            Crashed = false;
            DistanceTravelled = 0;
            Heading = 0;
        }

        public void Move(float steerAngle, float throttle)
        {
            Trail.Add(Position);

            var acceleration = 0f;

            //Friction and drag
            var frictionForce = Speed * Friction;
            var dragForce = Speed * Speed * Drag;

            acceleration += frictionForce;
            acceleration += dragForce;

            //This will ensure that the car doesn’t keep creeping forward at very low speeds as friction never quite reaches zero.
            if (Speed < 0.2)
                frictionForce *= 3;

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

            if (Speed < 1e-10)
                Speed = 0;
        }
        public void Draw(Graphics g)
        {
            g.RotateTransform(Heading * 180f / (float)Math.PI);
            g.TranslateTransform(Position.X, Position.Y, MatrixOrder.Append);

            g.FillRectangle(Brushes.DarkOrange, -Length / 2, -Width / 2, Length, Width);
            g.FillRectangle(Brushes.DarkBlue, Length / 2 - 5, -Width / 2, 5, Width);
            g.ResetTransform();

            foreach (var t in Trail)
                g.FillEllipse(Brushes.DarkMagenta,
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

        public List<float> RayCast(List<Line> lines, out List<Vector2> intersectionpoints)
        {
            const float PI = (float)Math.PI;
            var result = new List<float>();

            intersectionpoints = new List<Vector2>();
            float deg = -PI / 4f;
            float degstep = PI / 16f;
            for (int i = 0; i < 9; i++)
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
