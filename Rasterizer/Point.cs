using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public struct Point
    {
        public DenseMatrix Position;
        public Vector2 UV;
    }
}