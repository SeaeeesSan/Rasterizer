using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public struct Vertex
    {
        public DenseMatrix Position;
        public Vector2 UV;
        public Vector2 Normal;
    }
}