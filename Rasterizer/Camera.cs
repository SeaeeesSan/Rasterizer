using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Camera
    {
        public Transform Transform = new Transform();

        private CameraSettings _cameraSettings;

        public FrameBuffer FrameBuffer;
        public double[,] DepthBuffer;

        public Camera(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;

            FrameBuffer = new FrameBuffer(_cameraSettings.ImageWidth, _cameraSettings.ImageHeight, Color.White);
            DepthBuffer = new double[_cameraSettings.ImageWidth, _cameraSettings.ImageHeight];
        }

        /// <summary>
        /// フレームをレンダリング
        /// </summary>
        public void Render()
        {
            Console.Write("Rendering...");

            foreach (var obj in Scene.GetAllObjects())
            {
                foreach (var triangle in obj.Mesh)
                {
                    var a = calc(triangle.A);
                    var b = calc(triangle.B);
                    var c = calc(triangle.C);

                    var xMax = (int) Math.Max(Math.Max(a.X, b.X), c.X) + 1;
                    var xMin = (int) Math.Min(Math.Min(a.X, b.X), c.X) - 1;

                    var yMax = (int) Math.Max(Math.Max(a.Y, b.Y), c.Y) + 1;
                    var yMin = (int) Math.Min(Math.Min(a.Y, b.Y), c.Y) - 1;

                    for (int x = xMin; x < xMax; x++)
                    {
                        for (int y = yMin; y < yMax; y++)
                        {
                            try
                            {
                                var p = new Vector2(x, y);

                                if (x > _cameraSettings.ImageWidth || x < 0 || y > _cameraSettings.ImageHeight || y < 0)
                                {
                                    continue;
                                }

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

                                var depth = 1 - InterpolationFloat(p, (Vector2) a, (Vector2) b,
                                    (Vector2) c, aDepth, bDepth, cDepth);
                                if (abap >= 0 && bcbp >= 0 && cacp >= 0)
                                {
                                    if (DepthBuffer[x, y] < depth)
                                    {
                                        DepthBuffer[x, y] = depth;

                                        var uv = InterpolationVector2(p, a, b, c, triangle.A.UV,
                                            triangle.B.UV, triangle.C.UV);
                                        var albedoColor = obj.Texture.GetPixel((int) (uv.X * obj.Texture.Width),
                                            obj.Texture.Height - (int) (uv.Y * obj.Texture.Height));

                                        //albdo
                                        var albedo = new Vector3((float) albedoColor.R / 255,
                                            (float) albedoColor.G / 255, (float) albedoColor.B / 255);

                                        var color = Vector3.One;

                                        var normal = InterpolationVector3(p, a, b, c, triangle.A.Normal,
                                            triangle.B.Normal, triangle.C.Normal);

                                        Vector3 light = new Vector3(1, 1, 0.0f);
                                        float ip = light.X * normal.X + light.Y * normal.Y + light.Z * normal.Z;

                                        Vector3 view = new Vector3(0, 0, 1);

                                        Vector3 Refrect = -light - (2 *
                                                                    (-light.X * normal.X + -light.Y * normal.Y +
                                                                     -light.Z * normal.Z) * normal);

                                        float refre = Refrect.X * view.X + Refrect.Y * view.Y + Refrect.Z * view.Z;

                                        //鏡面反射係数
                                        float ks = 0.01f;
                                        //拡散反射係数
                                        float kd = 0.7f;
                                        //環境反射係数
                                        float ka = 0.3f;
                                        //光沢度
                                        float alpha = 15;

                                        if (refre < 0)
                                        {
                                            refre = 0;
                                        }

                                        //color = (float)((ka * color) + kd * ip + ks * Math.Pow(refre, alpha));
                                        color.X = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.X) +
                                                  ka * albedo.X;
                                        color.Y = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.Y) +
                                                  ka * albedo.Y;
                                        color.Z = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.Z) +
                                                  ka * albedo.Z;

                                        Color col = Color.FromArgb((int) Math.Clamp(color.X * 255, 0, 255),
                                            (int) Math.Clamp(color.Y * 255, 0, 255),
                                            (int) Math.Clamp(color.Z * 255, 0, 255));

                                        FrameBuffer.WritePixel(x, y, col);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }

            Console.WriteLine("done");
        }


        /// <summary>
        /// フレームをレンダリング
        /// </summary>
        public void RenderPoints()
        {
            Console.Write("Rendering(point only)...");
            foreach (var obj in Scene.GetAllObjects())
            {
                foreach (var mesh in obj.Mesh)
                {
                    try
                    {
                        var a = calc(mesh.A);
                        var b = calc(mesh.B);
                        var c = calc(mesh.C);

                        FrameBuffer.WritePixel((int) a.X, (int) a.Y, Color.Black);
                        FrameBuffer.WritePixel((int) b.X, (int) b.Y, Color.Black);
                        FrameBuffer.WritePixel((int) c.X, (int) c.Y, Color.Black);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            Console.WriteLine("done");
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
                var Zmin = _cameraSettings.Zmin;
                var Zmax = _cameraSettings.Zmax;
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

        /// <summary>
        /// 三角形の線形補完
        /// </summary>
        private static Vector3 InterpolationVector3(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector3 av, Vector3 bv,
            Vector3 cv)
        {
            var eu = b - a;
            var ev = c - a;

            var u = (-ev.X * (p.Y - a.Y) + ev.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);
            var v = (eu.X * (p.Y - a.Y) - eu.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);

            return av + u * (bv - av) + v * (cv - av);
        }
    }
}