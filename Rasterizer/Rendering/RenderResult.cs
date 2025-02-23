using System.Drawing;

namespace Rasterizer.Rendering;

public class RenderResult
{
    public Bitmap FrameBitmap { get; set; }
    public Bitmap DepthBitmap { get; set; }
    
    public float ElapsedTime { get; set; }
    
    public RenderResult(Bitmap frameBitmap, Bitmap depthBitmap, float elapsedTime)
    {
        FrameBitmap = frameBitmap;
        DepthBitmap = depthBitmap;
        ElapsedTime = elapsedTime;
    }
    
    public void SaveImages(string framePath, string depthPath)
    {
        FrameBitmap.Save(framePath);
        DepthBitmap.Save(depthPath);
    }
    
    public void Dispose()
    {
        FrameBitmap.Dispose();
        DepthBitmap.Dispose();
    }
}