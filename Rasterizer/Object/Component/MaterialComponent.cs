using System.Numerics;
using Rasterizer.Core;
using Rasterizer.Util;

namespace Rasterizer.Object.Component
{
    public class MaterialComponent : Core.Component
    {
        public Texture Texture;
        
        //鏡面反射係数
        public float ks = 0.3f;
        //拡散反射係数
        public float kd = 0.9f;
        //環境反射係数
        public float ka = 0.4f;
        //光沢度
        public float alpha = 20;

        public MaterialComponent(Texture texture)
        {
            Texture = texture;
        }
        
        public Vector3 GetColor(Vector2 uv, Vector3 lightDir, Vector3 normal, Vector3 viewDir)
        {
            var textureColor = Texture.GetColor((int)((uv.X) * Texture.Width),
                Texture.Height - ((int)((uv.Y) * Texture.Height)));

            var albedo = new Vector3((float)textureColor.R / 255,
                (float)textureColor.G / 255, (float)textureColor.B / 255);

            normal = Vector3.TransformNormal(normal, MyObject.Transform.ToMatrix().ConvertToSystemMatrix());
            normal = Vector3.Normalize(normal);

            //ライトのベクトル
            var ip = Vector3.Dot(lightDir, normal);
            
            //ハイライトの計算
            var refre = Vector3.Dot(Vector3.Reflect(-lightDir, normal), viewDir);

            refre = Math.Max(refre, 0);

            //色を計算
            return new Vector3
            {
                X = ks * (float)Math.Pow(refre, alpha) + kd * ip * albedo.X + ka * albedo.X,
                Y = ks * (float)Math.Pow(refre, alpha) + kd * ip * albedo.Y + ka * albedo.Y,
                Z = ks * (float)Math.Pow(refre, alpha) + kd * ip * albedo.Z + ka * albedo.Z
            };
        }
    }
}