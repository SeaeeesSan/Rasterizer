using Rasterizer.Object.Component;

namespace Rasterizer.Util;

public static class ObjectUtil
{
    public static MaterialComponent? GetMaterial(this Object.MyObject obj)
    {
        return obj.GetComponent<MaterialComponent>();
    }
}