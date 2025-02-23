using System.Numerics;
using Rasterizer.Core;

namespace Rasterizer.Object;

public class MyObject
{
    public string Name { get; set; }
    public Transform Transform { get; set; }
    
    private List<Core.Component> _components = new List<Core.Component>();
    
    public MyObject(string name)
    {
        Name = name;
        Transform = new Transform(Vector3.Zero);
    }
    
    /// <summary>
    /// コンポーネントを追加
    /// </summary>
    /// <param name="components">コンポーネント</param>
    public void AddComponent(params Core.Component[] components)
    {
        foreach (var component in components)
        {
            component.MyObject = this;
            _components.Add(component);
        }
    }
    
    public T? GetComponent<T>() where T : Core.Component
    {
        return _components.OfType<T>().FirstOrDefault();
    }
}