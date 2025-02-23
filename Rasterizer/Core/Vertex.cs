using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer.Core
{
    public struct Vertex
    {
        public DenseMatrix Position;
        public Vector2 UV;
        public Vector3 Normal;
    }
}