using MathNet.Numerics.LinearAlgebra.Double;
using Rasterizer.Rendering;

namespace Rasterizer.Object.Component
{
    public class CameraComponent : Core.Component
    {
        public DenseMatrix GetViewMatrix()
        {
            return (DenseMatrix)MyObject.Transform.ToMatrix().Inverse();
        }

        public DenseMatrix GetProjectionMatrix(RenderSettings settings)
        {
            var aspect = settings.ScreenWidth / (float)settings.ScreenHeight;
            var fov = 2 * Math.Atan(settings.ScreenHeight / (2 * settings.ScreenDistance));

            return DenseMatrix.OfArray(new double[,]
            {
                { 1 / (Math.Tan(fov / 2) * aspect), 0, 0, 0 },
                { 0, 1 / Math.Tan(fov / 2), 0, 0 },
                { 0, 0, -(settings.Zmax + settings.Zmin) / (settings.Zmax - settings.Zmin), -2 * settings.Zmax * settings.Zmin / (settings.Zmax - settings.Zmin) },
                { 0, 0, -1, 0 }
            });
        }

    }
}