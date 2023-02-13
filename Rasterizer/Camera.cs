using System.Drawing;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Camera
    {
        public Transform Transform = new Transform();

        private CameraSettings _cameraSettings;

        public FrameBuffer frameBuffer;
        public double[,] depthBuffer;

        public Camera(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;

            frameBuffer = new FrameBuffer(_cameraSettings.ImageWidth, _cameraSettings.ImageHeight, Color.White);
            depthBuffer = new double[_cameraSettings.ImageWidth, _cameraSettings.ImageHeight];
        }

        /// <summary>
        /// フレームをレンダリング
        /// </summary>
        public void Render()
        {
            for (var x = 0; x < _cameraSettings.ImageWidth; x++)
            {
                for (var y = 0; y < _cameraSettings.ImageHeight; y++)
                {
                    var p = new Vector2(x, y);
                    foreach (var obj in Scene.GetAllObjects())
                    {
                        foreach (var triangle in obj.Mesh)
                        {
                            try
                            {
                                var a = calc(triangle.A);
                                var b = calc(triangle.B);
                                var c = calc(triangle.C);

                                var aDepth = (float) (triangle.A.Position[2, 0] / triangle.A.Position[3, 0]);
                                var bDepth = (float) (triangle.B.Position[2, 0] / triangle.B.Position[3, 0]);
                                var cDepth = (float) (triangle.C.Position[2, 0] / triangle.C.Position[3, 0]);

                                var ab = (b - a);
                                var ap = (p - a);

                                var bc = (c - b);
                                var bp = (p - b);

                                var ca = (a - c);
                                var cp = (p - c);

                                var abap = ab.X * ap.Y - ab.Y * ap.X;
                                var bcbp = bc.X * bp.Y - bc.Y * bp.X;
                                var cacp = ca.X * cp.Y - ca.Y * cp.X;

                                var depth = 1 - InterpolationFloat(new Vector2(p.X, p.Y), (Vector2) a, (Vector2) b,
                                    (Vector2) c, aDepth, bDepth, cDepth);
                                if (abap >= 0 && bcbp >= 0 && cacp >= 0)
                                {
                                    if (depthBuffer[x, y] < depth)
                                    {
                                        depthBuffer[x, y] = depth;

                                        var color = InterpolationVector2(new Vector2(p.X, p.Y), a, b, c, triangle.A.UV,
                                            triangle.B.UV, triangle.C.UV);
                                        var tex = obj.Texture.GetPixel((int) (color.X * obj.Texture.Width),
                                            obj.Texture.Height - (int) (color.Y * obj.Texture.Height));

                                        frameBuffer.WritePixel(x, y, tex);
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }

        }

        private Vector2 calc(Vertex v)
        {
            return new Vector2(
                (int) (v.Position[0, 0] / v.Position[3, 0] * _cameraSettings.ImageWidth) +
                _cameraSettings.ImageWidth / 2,
                (int) (v.Position[1, 0] / v.Position[3, 0] * _cameraSettings.ImageHeight) +
                _cameraSettings.ImageHeight / 2);
        }

        /// <summary>
        /// MVP変換を行う
        /// </summary>
        public void Projection()
        {
            foreach (var obj in Scene.GetAllObjects())
            {
                //モデリング変換M
                obj.UpdateTransform();

                //視野変換V
                var camPosMatrix = DenseMatrix.OfArray(new double[,]
                {
                    {1, 0, 0, -Transform.Position.X},
                    {0, 1, 0, -Transform.Position.Y},
                    {0, 0, 1, -Transform.Position.Z},
                    {0, 0, 0, 1}
                });
                obj.MultiMatrix(camPosMatrix);

                //ビューポート変換P（透視投影）
                var Zmin = 0.01;
                var Zmax = 5.0;
                var ZminTilde = Zmin / Zmax;
                var d = 1.0;
                var a = 1.0;
                var b = 1.0;
                var p = DenseMatrix.OfArray(new double[,]
                {
                    {1, 0, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 1.0 / (1.0 - ZminTilde), -(ZminTilde / (1.0 - ZminTilde))},
                    {0, 0, 1, 0}
                });
                var s = DenseMatrix.OfArray(new double[,]
                {
                    {d / (a * Zmax), 0, 0, 0},
                    {0, d / (b * Zmax), 0, 0},
                    {0, 0, 1.0 / Zmax, 0},
                    {0, 0, 0, 1}
                });

                var t = DenseMatrix.OfArray(new double[,]
                {
                    {1, 0, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, -1, 0},
                    {0, 0, 0, 1}
                });

                obj.MultiMatrix(p * s * t);
            }
        }

        /// <summary>
        /// 三角形の線形補完
        /// </summary>
        private static double InterpolationFloat(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float av, float bv,
            float cv)
        {
            var eu = b - a;
            var ev = c - a;

            var u = (-ev.X * (p.Y - a.Y) + ev.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);
            var v = (eu.X * (p.Y - a.Y) - eu.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);

            return av + u * (bv - av) + v * (cv - av);
        }

        /// <summary>
        /// 三角形の線形補完
        /// </summary>
        private static Vector2 InterpolationVector2(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 av, Vector2 bv,
            Vector2 cv)
        {
            var eu = b - a;
            var ev = c - a;

            var u = (-ev.X * (p.Y - a.Y) + ev.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);
            var v = (eu.X * (p.Y - a.Y) - eu.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);

            return av + u * (bv - av) + v * (cv - av);
        }
    }
}