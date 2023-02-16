using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    /// <summary>
    /// 三角面
    /// </summary>
    public class Mesh
    {
        //各点
        public Vertex A;
        public Vertex B;
        public Vertex C;

        public Mesh(Vertex a, Vertex b, Vertex c)
        {
            A = a; 
            B = b;
            C = c;
        }

        /// <summary>
        /// 頂点に行列を演算
        /// </summary>
        /// <param name="matrix">行列</param>
        public void MultiMatrix(DenseMatrix matrix)
        {
            A.Position = matrix * A.Position;
            B.Position = matrix * B.Position;
            C.Position = matrix * C.Position;
        }

        public Vertex[] GetVertex()
        {
            return new[] {A, B, C};
        }
    }
}