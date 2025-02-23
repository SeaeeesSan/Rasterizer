using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer.Core
{
    public class Transform
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        
        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
        
        public Transform(Vector3 position) : this(position, Vector3.Zero, Vector3.One)
        {
        }
        
        public Transform(Vector3 position, Vector3 rotation) : this(position, rotation, Vector3.One)
        {
        }
        
        public DenseMatrix ToMatrix()
        {
            const double degToRad = Math.PI / 180.0;

            double radX = Rotation.X * degToRad;
            double radY = Rotation.Y * degToRad;
            double radZ = Rotation.Z * degToRad;

            var rx = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, 0},
                {0, Math.Cos(radX), Math.Sin(radX), 0},
                {0, -Math.Sin(radX), Math.Cos(radX), 0},
                {0, 0, 0, 1}
            });

            var ry = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(radY), 0, -Math.Sin(radY), 0},
                {0, 1, 0, 0},
                {Math.Sin(radY), 0, Math.Cos(radY), 0},
                {0, 0, 0, 1}
            });

            var rz = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(radZ), Math.Sin(radZ), 0, 0},
                {-Math.Sin(radZ), Math.Cos(radZ), 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            });

            var r = rx * ry * rz;

            var s = DenseMatrix.OfArray(new double[,]
            {
                {Scale.X, 0, 0, 0},
                {0, Scale.Y, 0, 0},
                {0, 0, Scale.Z, 0},
                {0, 0, 0, 1}
            });

            var t = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, Position.X},
                {0, 1, 0, Position.Y},
                {0, 0, 1, Position.Z},
                {0, 0, 0, 1}
            });

            return t * r * s;
        }
    }
}