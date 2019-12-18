using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Car
{
    public static class Helper
    {
        public const float PI = (float)Math.PI;
        public static float cos(double d) => (float)Math.Cos(d);
        public static float sin(double d) => (float)Math.Sin(d);
        public static float atan2(double x, double y) => (float)Math.Atan2(x, y);
        public static float linear(float x,
            float x0,
            float x1,
            float y0,
            float y1)
        {
            if ((x1 - x0) == 0)
                return (y0 + y1) / 2;
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

        public static void CreatePolygon(
            List<Vector2> pts,
            out List<Line> poly)
        {
            poly = new List<Line>();
            for (int i = 1; i < pts.Count; i++)
            {
                var a = pts[i - 1];
                var b = pts[i];
                var l = new Line(a, b);
                poly.Add(l);
            }
            poly.Add(new Line(pts.First(), pts.Last()));
        }

        public static float Map(
            this float value,
            float from1,
            float to1,
            float from2,
            float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        //http://csharphelper.com/blog/2014/08/determine-where-two-lines-intersect-in-c/
        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static void FindIntersection(Vector2 p1,
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

        public static void FindIntersection(
            Line l1,
            Line l2,
            out bool lines_intersect,
            out bool segments_intersect,
            out Vector2 intersection,
            out Vector2 close_p1,
            out Vector2 close_p2)
        {
            FindIntersection(l1.a, l1.b, l2.a, l2.b, out bool l, out bool s, out Vector2 i, out Vector2 c1, out Vector2 c2);
            lines_intersect = l;
            segments_intersect = s;
            intersection = i;
            close_p1 = c1;
            close_p2 = c2;
        }
    }
}
