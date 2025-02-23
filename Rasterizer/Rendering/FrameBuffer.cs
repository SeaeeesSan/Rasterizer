using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#pragma warning disable CA1416

namespace Rasterizer.Rendering
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
                for (int j = Y - 1; j >= 0; j--)
                {
                    Color[i, j] = color;
                    Depth[i, j] = 1;
                }
            }
        }
        
        public Bitmap GetColorBitmap()
        {
            var bitmap = new Bitmap(X, Y);
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, X, Y),
                ImageLockMode.WriteOnly, 
                PixelFormat.Format32bppArgb
            );

            var ptr = bmpData.Scan0;
            var pixels = new byte[bmpData.Stride * Y];

            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    int pos = y * bmpData.Stride + x * 4;

                    pixels[pos] = Color[x, y].B; 
                    pixels[pos + 1] = Color[x, y].G;
                    pixels[pos + 2] = Color[x, y].R;
                    pixels[pos + 3] = Color[x, y].A;
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
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );

            var ptr = bmpData.Scan0;
            var pixels = new byte[bmpData.Stride * Y];

            for (int y = 0; y < Y; y++)
            {
                for (int x = 0; x < X; x++)
                {
                    int pos = y * bmpData.Stride + x * 4;

                    var depth = (byte)(Depth[x, y] * 255);
                    
                    pixels[pos] = depth;
                    pixels[pos + 1] = depth;
                    pixels[pos + 2] = depth;
                    pixels[pos + 3] = 255;
                }
            }

            Marshal.Copy(pixels, 0, ptr, pixels.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }
    }
}
#pragma warning restore CA1416
