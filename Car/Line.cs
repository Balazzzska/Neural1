using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
}
