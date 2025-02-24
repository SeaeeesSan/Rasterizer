namespace Rasterizer.Core;

public class Component
{
    public Object.MyObject MyObject { get; set; }
    
    public virtual void Update() { }
    
    public virtual void DeltaUpdate(float deltaTime) { }
}