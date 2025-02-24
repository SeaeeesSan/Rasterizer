using System.Numerics;

namespace Rasterizer.Object.Component.Custom;

public class RotateAnimation : Core.Component
{
    private float _angle;
    private float _speed;
    
    public RotateAnimation(float speed)
    {
        _speed = speed;
    }
    
    public override void Update()
    {
        _angle += _speed;
        MyObject.Transform.Rotation = new Vector3(0, _angle, 0);
    }
}