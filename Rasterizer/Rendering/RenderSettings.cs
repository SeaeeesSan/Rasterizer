using System.Drawing;
using System.Numerics;

namespace Rasterizer.Rendering
{
    public struct RenderSettings
    {
        public int ImageHeight;
        public int ImageWidth;
        public double Zmin;
        public double Zmax;
        public double ScreenDistance;
        public double ScreenWidth;
        public double ScreenHeight;
        public Color BackgroundColor;
        public Vector3 LightDir;
    }
}