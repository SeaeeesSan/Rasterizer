using MathNet.Numerics.LinearAlgebra.Double;
using Rasterizer.Core;

namespace Rasterizer.Object.Component;

public class MeshRendererComponent : Core.Component
{
    public Mesh[] OriginalMesh;
    public Mesh[] Mesh;
    
    public MeshRendererComponent(Mesh[] mesh)
    {
        OriginalMesh = mesh;
    }
    
    /// <summary>
    /// Transformを適用
    /// </summary>
    public void UpdateTransform()
    {
        Mesh = OriginalMesh.Select(x => new Mesh(x.A, x.B, x.C)).ToArray();
        
        foreach (var triangle in Mesh)
        {
            triangle.MultiplyMatrix(MyObject.Transform.ToMatrix());
        }
    }
        
    /// <summary>
    /// オブジェクトに行列を乗算
    /// </summary>
    /// <param name="matrix">行列</param>
    public void MultiMatrix(DenseMatrix matrix)
    {
        foreach (var triangle in Mesh)
        {
            triangle.MultiplyMatrix(matrix);
        }
    }

    public void ResetMesh()
    {
        Mesh = OriginalMesh.Select(x => new Mesh(x.A, x.B, x.C)).ToArray();
    }
}