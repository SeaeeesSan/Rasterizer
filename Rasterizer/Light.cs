using System.Numerics;

namespace Rasterizer
{
    public class Light
    {
        public Transform Transform = new Transform();

        public Light(Vector3 rotation)
        {
            Transform.Rotation = rotation;
        }
    }
}