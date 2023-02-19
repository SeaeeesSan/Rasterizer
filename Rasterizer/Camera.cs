using System;
using System.Drawing;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Camera
    {
        public Transform Transform = new Transform();

        private CameraSettings _cameraSettings;

        public FrameBuffer FrameBuffer;

        public Camera(CameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;
            FrameBuffer = new FrameBuffer(_cameraSettings.ImageWidth, _cameraSettings.ImageHeight, Color.White);
        }

        /// <summary>
        /// フレームをレンダリング
        /// </summary>
        public void Render()
        {
            foreach (var obj in Scene.GetAllObjects())
            {
                int i = 0;
                foreach (var triangle in obj.Mesh)
                {
                    i++;
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
                                if (x > _cameraSettings.ImageWidth || x < 0 || y > _cameraSettings.ImageHeight || y < 0)
                                {
                                    continue;
                                }
                                var p = new Vector2(x, y);
                                
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
                                    if (FrameBuffer.Depth[x, y] < depth)
                                    {
                                        //デプスバッファ書き込み
                                        FrameBuffer.Depth[x, y] = depth;
                            
                                        //テクスチャ読み込み
                                         var uv = InterpolationVector2(p, a, b, c, triangle.A.UV,
                                             triangle.B.UV, triangle.C.UV);
                                         var textureColor = obj.Texture.GetPixel((int) ((uv.X) * obj.Texture.Width),
                                             obj.Texture.Height - ((int) ((uv.Y) * obj.Texture.Height)) );
                                        
                                        textureColor = Color.White;
                                        
                                        var albedo = new Vector3((float) textureColor.R / 255,
                                            (float) textureColor.G / 255, (float) textureColor.B / 255);
                                        var normal = InterpolationVector3(p, a, b, c, triangle.A.Normal,
                                            triangle.B.Normal, triangle.C.Normal);
                            
                                        //ライトのベクトル
                                        var lightVector = Vector3.Normalize(new Vector3(1, 1, 0));
                                        var ip = lightVector.X * normal.X + lightVector.Y * normal.Y +
                                                 lightVector.Z * normal.Z;
                            
                                        //視線ベクトル
                                        var viewVector = new Vector3(0, 0, 1);
                            
                                        //ハイライトの計算
                                        var refrectVector = -lightVector - (2 *
                                                                            (-lightVector.X * normal.X +
                                                                             -lightVector.Y * normal.Y +
                                                                             -lightVector.Z * normal.Z) * normal);
                                        var refre = refrectVector.X * viewVector.X + refrectVector.Y * viewVector.Y +
                                                    refrectVector.Z * viewVector.Z;
                            
                                        var ks = obj.Material.ks;
                                        var kd = obj.Material.kd;
                                        var ka = obj.Material.ka;
                                        var alpha = obj.Material.alpha;
                            
                                        //正規化
                                        if (refre < 0)
                                        {
                                            refre = 0;
                                        }
                            
                                        //色を計算
                                        var color = new Vector3
                                        {
                                            X = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.X) +
                                                ka * albedo.X,
                                            Y = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.Y) +
                                                ka * albedo.Y,
                                            Z = (float) (ks * Math.Pow(refre, alpha)) + (kd * ip * albedo.Z) +
                                                ka * albedo.Z
                                        };
                                        var pixelColor = Color.FromArgb((int) Math.Clamp(color.X * 255, 0, 255),
                                            (int) Math.Clamp(color.Y * 255, 0, 255),
                                            (int) Math.Clamp(color.Z * 255, 0, 255));
                            
                                        //pixelColor = Color.Black;
                            
                                        //フレームバッファに書き込み
                                        FrameBuffer.Color[x, y] = pixelColor;
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


        /// <summary>
        /// フレームをレンダリング
        /// </summary>
        public void RenderPoints()
        {
            foreach (var obj in Scene.GetAllObjects())
            {
                foreach (var mesh in obj.Mesh)
                {
                    try
                    {
                        var a = calc(mesh.A);
                        var b = calc(mesh.B);
                        var c = calc(mesh.C);

                        FrameBuffer.Color[(int) a.X, (int) a.Y] = Color.Black;
                        FrameBuffer.Color[(int) b.X, (int) b.Y] = Color.Black;
                        FrameBuffer.Color[(int) c.X, (int) c.Y] = Color.Black;
                    }
                    catch
                    {
                        continue;
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
                var Zmin = _cameraSettings.Zmin;
                var Zmax = _cameraSettings.Zmax;
                var ZminTilde = Zmin / Zmax;
                var d = _cameraSettings.ScreenDistance;
                var a = _cameraSettings.ScreenWidth;
                var b = _cameraSettings.ScreenHeight;
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

        public void SSAA()
        {
            var col = FrameBuffer.Color;
            var result = new Color[_cameraSettings.ImageWidth / 2, _cameraSettings.ImageWidth / 2];

            for (int x = 0; x < _cameraSettings.ImageWidth / 2; x++)
            {
                for (int y = 0; y < _cameraSettings.ImageWidth / 2; y++)
                {
                    Color a = col[x * 2, y * 2];
                    Color b = col[x * 2 + 1, y * 2];
                    Color c = col[x * 2 + 1, y * 2 + 1];
                    Color d = col[x * 2, y * 2 + 1];

                    result[x, y] = Color.FromArgb((a.R + b.R + c.R + d.R) / 4, (a.G + b.G + c.G + d.G) / 4,
                        (a.B + b.B + c.B + d.B) / 4);
                }
            }

            FrameBuffer.Color = result;
            FrameBuffer.X /= 2;
            FrameBuffer.Y /= 2;
        }
        
        public void SSAAd()
        {
            var col = FrameBuffer.Depth;
            var result = new double[_cameraSettings.ImageWidth / 2, _cameraSettings.ImageWidth / 2];

            for (int x = 0; x < _cameraSettings.ImageWidth / 2; x++)
            {
                for (int y = 0; y < _cameraSettings.ImageWidth / 2; y++)
                {
                    var a = col[x * 2, y * 2];
                    var b = col[x * 2 + 1, y * 2];
                    var c = col[x * 2 + 1, y * 2 + 1];
                    var d = col[x * 2, y * 2 + 1];

                    result[x, y] = (a + b + c + d) / 4;
                }
            }

            FrameBuffer.Depth = result;
        }
    }
}