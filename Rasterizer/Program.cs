using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer // Note: actual namespace depends on the project name.
{
    internal class Rasterizer
    {
        static void Main(string[] args)
        {
            //レンダリング設定
            var xSize = 1028;
            var ySize = 1028;

            Color[,] pixelColor = new Color[xSize, ySize];
            double[,] depth = new double[xSize, ySize];

            for (var i = 0; i < xSize; i++)
            {
                for (var j = 0; j < ySize; j++)
                {
                    pixelColor[i, j] = Color.White;
                }
            }

            //モデル三角面読み込み
            Console.WriteLine("Loading model...");
            Triangle[] obj = ObjLoader.Load(@"X:\teapot3.obj");

            //テクスチャ読み込み
            Console.WriteLine("Loading texture...");
            Bitmap texture = new Bitmap(Image.FromFile(@"X:\tex.png"));


            Console.WriteLine("Rendering...");

            DenseMatrix objrot = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(-30 * Math.PI / 180), 0, Math.Sin(-30 * Math.PI / 180), 0},
                {0, 1, 0, 0},
                {-Math.Sin(-30 * Math.PI / 180), 0, Math.Cos(-30 * Math.PI / 180), 0},
                {0, 0, 0, 1}
            });
            foreach (var triangle in obj)
            {
                triangle.A.Position = objrot * triangle.A.Position;
                triangle.B.Position = objrot * triangle.B.Position;
                triangle.C.Position = objrot * triangle.C.Position;
            }

            //視野変換
            Vector3 camPos = new Vector3(0, 1.1f, 2.5f);
            DenseMatrix camPosMatrix = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, -camPos.X},
                {0, 1, 0, -camPos.Y},
                {0, 0, 1, -camPos.Z},
                {0, 0, 0, 1}
            });
            foreach (var triangle in obj)
            {
                triangle.A.Position = camPosMatrix * triangle.A.Position;
                triangle.B.Position = camPosMatrix * triangle.B.Position;
                triangle.C.Position = camPosMatrix * triangle.C.Position;
            }

            //ビューポート変換
            double Zmin = 1;
            double Zmax = 15;
            double ZminTilde = Zmin / Zmax;
            double d = 1;
            double a = 1;
            double b = 1;
            DenseMatrix p = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1.0 / (1.0 - ZminTilde), -(ZminTilde / (1.0 - ZminTilde))},
                {0, 0, 1, 0}
            });
            DenseMatrix s = DenseMatrix.OfArray(new double[,]
            {
                {d / (a * Zmax), 0, 0, 0},
                {0, d / (b * Zmax), 0, 0},
                {0, 0, 1.0 / Zmax, 0},
                {0, 0, 0, 1}
            });

            DenseMatrix t = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, -1, 0},
                {0, 0, 0, 1}
            });
            foreach (var triangle in obj)
            {
                triangle.A.Position = p * s * t * triangle.A.Position;
                triangle.B.Position = p * s * t * triangle.B.Position;
                triangle.C.Position = p * s * t * triangle.C.Position;
            }

            foreach (var triangle in obj)
            {
                try
                {
                    Vector3 A, B, C;
                    A = new Vector3((int) (triangle.A.Position[0, 0] / triangle.A.Position[3, 0] * xSize) + xSize / 2,
                        (int) (triangle.A.Position[1, 0] / triangle.A.Position[3, 0] * ySize) + ySize / 2,
                        (float) (triangle.A.Position[2, 0] / triangle.A.Position[3, 0]));
                    B = new Vector3((int) (triangle.B.Position[0, 0] / triangle.B.Position[3, 0] * xSize) + xSize / 2,
                        (int) (triangle.B.Position[1, 0] / triangle.B.Position[3, 0] * ySize) + ySize / 2,
                        (float) (triangle.B.Position[2, 0] / triangle.B.Position[3, 0]));
                    C = new Vector3((int) (triangle.C.Position[0, 0] / triangle.C.Position[3, 0] * xSize) + xSize / 2,
                        (int) (triangle.C.Position[1, 0] / triangle.C.Position[3, 0] * ySize) + ySize / 2,
                        (float) (triangle.C.Position[2, 0] / triangle.C.Position[3, 0]));

                    pixelColor[(int) A.X, (int) A.Y] = Color.Black;
                    pixelColor[(int) B.X, (int) B.Y] = Color.Black;
                    pixelColor[(int) C.X, (int) C.Y] = Color.Black;

                    //
                    //
                    // pixel[(int) ((triangle.A[0, 0] / triangle.A[3, 0]) * xSize) + 256,
                    //     (int) (triangle.A[1, 0] / triangle.A[3, 0] * ySize) + 256,0] = 1;
                    // pixel[(int) (triangle.B[0, 0] / triangle.B[3, 0] * xSize) + 256,
                    //     (int) (triangle.B[1, 0] / triangle.B[3, 0] * ySize) + 256,0] = 1;
                    // pixel[(int) (triangle.C[0, 0] / triangle.C[3, 0] * xSize) + 256,
                    //     (int) (triangle.C[1, 0] / triangle.C[3, 0] * ySize) + 256,0] = 1;
                }
                catch
                {
                    continue;
                }
            }


            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Vector3 P = new Vector3(x, y, 0);
                    foreach (var triangle in obj)
                    {
                        try
                        {
                            Vector3 A, B, C;
                            A = new Vector3(
                                (int) (triangle.A.Position[0, 0] / triangle.A.Position[3, 0] * xSize) + xSize / 2,
                                (int) (triangle.A.Position[1, 0] / triangle.A.Position[3, 0] * ySize) + ySize / 2,
                                (float) (triangle.A.Position[2, 0] / triangle.A.Position[3, 0]));
                            B = new Vector3(
                                (int) (triangle.B.Position[0, 0] / triangle.B.Position[3, 0] * xSize) + xSize / 2,
                                (int) (triangle.B.Position[1, 0] / triangle.B.Position[3, 0] * ySize) + ySize / 2,
                                (float) (triangle.B.Position[2, 0] / triangle.B.Position[3, 0]));
                            C = new Vector3(
                                (int) (triangle.C.Position[0, 0] / triangle.C.Position[3, 0] * xSize) + xSize / 2,
                                (int) (triangle.C.Position[1, 0] / triangle.C.Position[3, 0] * ySize) + ySize / 2,
                                (float) (triangle.C.Position[2, 0] / triangle.C.Position[3, 0]));

                            //Console.WriteLine(A.X);

                            Vector3 AB = (B - A);
                            Vector3 AP = (P - A);

                            Vector3 BC = (C - B);
                            Vector3 BP = (P - B);

                            Vector3 CA = (A - C);
                            Vector3 CP = (P - C);

                            double abap = AB.X * AP.Y - AB.Y * AP.X;
                            double bcbp = BC.X * BP.Y - BC.Y * BP.X;
                            double cacp = CA.X * CP.Y - CA.Y * CP.X;

                            double calcuatedDepth = 1 - calcDepth(new Vector2(P.X, P.Y), A, B, C);
                            if (abap >= 0 && bcbp >= 0 && cacp >= 0)
                            {
                                if (depth[x, y] < calcuatedDepth)
                                {
                                    depth[x, y] = calcuatedDepth;

                                    Vector2 color = calcUV(new Vector2(P.X, P.Y), A, B, C, triangle.A.UV,
                                        triangle.B.UV,
                                        triangle.C.UV);

                                    pixelColor[x, y] = texture.GetPixel((int) (color.X * texture.Width),
                                        texture.Height - (int) (color.Y * texture.Height));
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

            Console.WriteLine("Writint Pixels...");

            //ピクセルの書き込み
            Bitmap renderResult = new Bitmap(xSize, ySize);
            for (int x = 0; x < renderResult.Width; x++)
            {
                for (int y = 0; y < renderResult.Height; y++)
                {
                    renderResult.SetPixel(x, renderResult.Height - 1 - y, pixelColor[x, y]);
                }
            }

            //ファイル保存
            renderResult.Save($@"X:\output.png", ImageFormat.Png);

            Console.WriteLine("Done!");
        }

        static double calcDepth(Vector2 p, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector2 p0 = new Vector2(A.X, A.Y);
            Vector2 p1 = new Vector2(B.X, B.Y);
            Vector2 p2 = new Vector2(C.X, C.Y);

            double c0 = A.Z;
            double c1 = B.Z;
            double c2 = C.Z;

            Vector2 eu = p1 - p0;
            Vector2 ev = p2 - p0;

            double d = eu.X * ev.Y - ev.X * eu.Y;
            double a = p.Y - p0.Y;
            double b = p.X - p0.X;

            double u = (-ev.X * a + ev.Y * b) / d;
            double v = (eu.X * a - eu.Y * b) / d;

            return c0 + u * (c1 - c0) + v * (c2 - c0);
        }

        static Vector2 calcUV(Vector2 p, Vector3 A, Vector3 B, Vector3 C, Vector2 Au, Vector2 Bu, Vector2 Cu)
        {
            Vector2 p0 = new Vector2(A.X, A.Y);
            Vector2 p1 = new Vector2(B.X, B.Y);
            Vector2 p2 = new Vector2(C.X, C.Y);

            Vector2 eu = p1 - p0;
            Vector2 ev = p2 - p0;

            double d = eu.X * ev.Y - ev.X * eu.Y;
            double a = p.Y - p0.Y;
            double b = p.X - p0.X;

            double u = (-ev.X * a + ev.Y * b) / d;
            double v = (eu.X * a - eu.Y * b) / d;

            return Au + (float) u * (Bu - Au) + (float) v * (Cu - Au);
        }

        Vector3 ColorToVector3(Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        Color Vector3ToColor(Vector3 vector3)
        {
            return Color.FromArgb((int) vector3.X, (int) vector3.Y, (int) vector3.Z);
        }
    }
}