using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rasterizer
{
    public class FrameBuffer
    {
        public Color[,] Color;
        public double[,] Depth;
        public int X;
        public int Y;

        public FrameBuffer(int x, int y, Color color)
        {
            X = x;
            Y = y;

            Color = new Color[x, y];
            Depth = new double[x, y];

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    Color[i, j] = System.Drawing.Color.Gray;
                    Depth[i, j] = 0;
                }
            }
        }
        
        public Bitmap GetColorBitmap()
        {
            var bitmap = new Bitmap(X, Y);
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, X, Y),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat
            );

            var ptr = bmpData.Scan0;
            var pixels = new byte[bmpData.Stride * bitmap.Height];
            Marshal.Copy(ptr, pixels, 0, pixels.Length);

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    var pos = j * bmpData.Stride + i * 4;

                    pixels[pos] = Color[i, bitmap.Height - 1 - j].B;
                    pixels[pos + 1] = Color[i, bitmap.Height - 1 - j].G;
                    pixels[pos + 2] = Color[i, bitmap.Height - 1 - j].R;
                    pixels[pos + 3] = Color[i, bitmap.Height - 1 - j].A;
                }
            }

            Marshal.Copy(pixels, 0, ptr, pixels.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
        
        public Bitmap GetDepthBitmap()
        {
            var bitmap = new Bitmap(X, Y);
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, X, Y),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat
            );

            var ptr = bmpData.Scan0;
            var pixels = new byte[bmpData.Stride * bitmap.Height];
            Marshal.Copy(ptr, pixels, 0, pixels.Length);

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    var pos = j * bmpData.Stride + i * 4;
                    
                    pixels[pos] = (byte)Math.Clamp(Depth[i, bitmap.Height - 1 - j] * 255.0, 0, 255);
                    pixels[pos + 1] = (byte)Math.Clamp(Depth[i, bitmap.Height - 1 - j] * 255.0, 0, 255);
                    pixels[pos + 2] = (byte)Math.Clamp(Depth[i, bitmap.Height - 1 - j] * 255.0, 0, 255);
                    pixels[pos + 3] = 255;
                }
            }

            Marshal.Copy(pixels, 0, ptr, pixels.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
    }
}