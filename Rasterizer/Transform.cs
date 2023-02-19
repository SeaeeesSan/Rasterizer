using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Transform
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.One;
        
        public DenseMatrix ToMatrix()
        {
            var rx = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, 0},
                {0, Math.Cos(Rotation.X), -Math.Sin(Rotation.X), 0},
                {0, Math.Sin(Rotation.X), Math.Cos(Rotation.X), 0},
                {0, 0, 0, 1}
            });

            var ry = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(Rotation.Y), 0, Math.Sin(Rotation.Y), 0},
                {0, 1, 0, 0},
                {-Math.Sin(Rotation.Y), 0, Math.Cos(Rotation.Y), 0},
                {0, 0, 0, 1}
            });

            var rz = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(Rotation.Z), -Math.Sin(Rotation.Z), 0, 0},
                {Math.Sin(Rotation.Z), Math.Cos(Rotation.Z), 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            });

            var r = rz * ry * rx;
            
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

        public Vector3 GetRotationVector()
        {
            return (new Vector3((float)Math.Cos(Rotation.X), (float)Math.Cos(Rotation.Y), (float)Math.Cos(Rotation.Z)));
        }
    }
}