using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer
{
    public class Object
    {
        /// <summary>
        /// オブジェクト名
        /// </summary>
        public string Name;

        /// <summary>
        /// 三角面
        /// </summary>
        public readonly Mesh[] Mesh;
        
        /// <summary>
        /// テクスチャ
        /// </summary>
        public Bitmap Texture;

        /// <summary>
        /// トランスフォーム
        /// </summary>
        public Transform Transform = new Transform();

        public Material Material = new Material();

        
        /// <summary>
        /// シーンにオブジェクトを追加
        /// </summary>
        /// <param name="name">オブジェクト名</param>
        /// <param name="mesh">メッシュデータ</param>
        /// <param name="texture">テクスチャ</param>
        public Object(string name, Mesh[] mesh, Bitmap texture)
        {
            Mesh = mesh;
            Texture = texture;
            Name = name;
        }

        /// <summary>
        /// Transformを適用
        /// </summary>
        public void UpdateTransform()
        {
            foreach (var triangle in Mesh)
            {
                triangle.MultiMatrix(Transform.ToMatrix());
            }
        }
        
        /// <summary>
        /// オブジェクトに行列を演算
        /// </summary>
        /// <param name="matrix">行列</param>
        public void MultiMatrix(DenseMatrix matrix)
        {
            foreach (var triangle in Mesh)
            {
                triangle.MultiMatrix(matrix);
            }
        }
    }
}