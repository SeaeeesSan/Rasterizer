using System.Drawing;
using System.Drawing.Imaging;

namespace Rasterizer.Core;

#pragma warning disable CA1416
public class Texture
{
    public Bitmap TextureImage;
    public int Width;
    public int Height;
    
    private byte[] _src;
    private byte[] _dst;

    public Texture(string path)
    {
        TextureImage = new Bitmap(path);
        Width = TextureImage.Width;
        Height = TextureImage.Height;
        
        LoadTexture(TextureImage);
    }
    
    private void LoadTexture(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
            ImageLockMode.ReadOnly, 
            PixelFormat.Format32bppArgb);
        var ptr = bitmapData.Scan0;
        _src = new byte[Math.Abs(bitmapData.Stride) * bitmapData.Height];
        _dst = new byte[Math.Abs(bitmapData.Stride) * bitmapData.Height];
        
        System.Runtime.InteropServices.Marshal.Copy(ptr, _src, 0, _src.Length);
    }
    
    public Color GetColor(int x, int y, bool repeat = true)
    {
        if (repeat)
        {
            // 負の数を考慮して正しい範囲に戻す
            x = (x % Width + Width) % Width;
            y = (y % Height + Height) % Height;
        }
        else
        {
            // 範囲外の値を0または最大値に収める
            x = x < 0 ? 0 : x >= Width ? Width - 1 : x;
            y = y < 0 ? 0 : y >= Height ? Height - 1 : y;
        }

        // インデックスの計算
        var index = (x + y * Width) * 4;
    
        // インデックスが範囲内かチェック（例外が発生しないように）
        if (index + 3 >= _src.Length) 
        {
            throw new IndexOutOfRangeException("インデックスが範囲外です");
        }
    
        return Color.FromArgb(_src[index + 3], _src[index + 2], _src[index + 1], _src[index]);
    }


}
#pragma warning restore CA1416
