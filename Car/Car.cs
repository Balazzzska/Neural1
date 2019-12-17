using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Car
{
    public class Car
    {
        //http://kidscancode.org/godot_recipes/2d/car_steering/

        public Vector2 Position;
        const float Wheelbase = 35;  // Distance from front to rear wheel
        const float Length = 50; //Total length of the car

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

        float Heading = 0;
        public float Speed;

        public Car(float posx, float posy)
        {
            Position = new Vector2(posx, posy);
            Speed = 0;
        }

        float cos(double d) => (float)Math.Cos(d);
        float sin(double d) => (float)Math.Sin(d);
        float atan2(double x, double y) => (float)Math.Atan2(x, y);
        float linear(float x, float x0, float x1, float y0, float y1)
        {
            if ((x1 - x0) == 0)
                return (y0 + y1) / 2;
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

        public void Move(float steerAngle, float throttle)
        {
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

            //Acceleration
            acceleration += throttle * EnginePower;
            Speed += acceleration;
        }

        public void Draw(Graphics g)
        {
            g.RotateTransform(Heading * 180f / (float)Math.PI);
            g.TranslateTransform(Position.X, Position.Y, MatrixOrder.Append);

            float width = Length / 1.618f;
            g.FillRectangle(Brushes.DarkOrange, -Length / 2, -width / 2, Length, width);
            g.FillRectangle(Brushes.DarkBlue, Length / 2 - 5, -width / 2, 5, width);
            g.ResetTransform();
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
                var v = new Vector2(5000, 0);
                v = Vector2.Transform(v, Matrix3x2.CreateRotation(deg + Heading, Position));

                List<Vector2> tmp = new List<Vector2>();

                foreach (var l in lines)
                {
                    FindIntersection(Position, v, l.a, l.b, out bool intersect, out bool segmentsintersect, out Vector2 intersection, out Vector2 p1, out Vector2 p2);

                    if (segmentsintersect)
                        tmp.Add(intersection);
                }

                var a = tmp.Count > 0 ? tmp.OrderBy(p => p.Length()).FirstOrDefault() : v + Position;
                intersectionpoints.Add(a);
                result.Add((a - Position).Length());

                deg += degstep;
            }

            return result;
        }

        //http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        private void FindIntersection(Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2 p4,
            out bool lines_intersect,
            out bool segments_intersect,
            out Vector2 intersection,
            out Vector2 close_p1,
            out Vector2 close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Vector2(float.NaN, float.NaN);
                close_p1 = new Vector2(float.NaN, float.NaN);
                close_p2 = new Vector2(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                / -denominator;

            // Find the point of intersection.
            intersection = new Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
                t1 = 0;
            else if (t1 > 1)
                t1 = 1;

            if (t2 < 0)
                t2 = 0;
            else if (t2 > 1)
                t2 = 1;

            close_p1 = new Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new Vector2(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }
    }
}
