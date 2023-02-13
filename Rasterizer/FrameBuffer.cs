using System.Drawing;

namespace Rasterizer
{
    public class FrameBuffer
    {
        private Color[,] _buffer;
        private int _x;
        private int _y;
        
        public FrameBuffer(int x, int y, Color color)
        {
            _x = x;
            _y = y;
            
            _buffer = new Color[x, y];
            
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    _buffer[i, j] = Color.White;
                }
            }
        }

        public void WritePixel(int x, int y, Color color)
        {
            _buffer[x, y] = color;
        }

        public Bitmap GetBitmap()
        {
            var bitmap = new Bitmap(_x, _y);
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, bitmap.Height - 1 - y, _buffer[x, y]);
                }
            }

            return bitmap;
        }
    }
}