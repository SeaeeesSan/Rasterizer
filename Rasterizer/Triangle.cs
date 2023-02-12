using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Triangle
    {
        public Point A;
        public Point B;
        public Point C;
        
        public Triangle(Point a, Point b, Point c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }
    }
}