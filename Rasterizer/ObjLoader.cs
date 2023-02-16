using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
namespace Rasterizer
{
    public static class ObjLoader
    {
        public static Object Load(string name, string objectPath, string texturePath, Transform transform = null)
        {
            var model = LoadPolygon(objectPath);
            var texture = new Bitmap(Image.FromFile(texturePath));
            
            var obj = new Object(name, model, texture);
            obj.Transform ??= transform;
            
            return obj;
        }
        
        private static Mesh[] LoadPolygon(string path)
        {
            var triangles = new List<Mesh>(4000);

            var verts = new List<DenseMatrix>(4000);
            var uvs = new List<Vector2>(4000);
            var normals = new List<Vector3>(4000);

            var faces = new List<Vector3>(4000);
            var uvIndex = new List<Vector3>(4000);
            var normalIndex = new List<Vector3>(4000);

            foreach (string line in File.ReadLines(path))
            {
                string[] t = line.Split(' ', '/');
                string type = t[0];
                switch (type)
                {
                    case "v":
                    {
                        verts.Add(DenseMatrix.OfArray(new double[,]
                        {
                            {double.Parse(t[1])}, {double.Parse(t[2])}, {double.Parse(t[3])}, {1}
                        }));
                        break;
                    }
                    case "vt":
                    {
                        uvs.Add(new Vector2(float.Parse(t[1]), float.Parse(t[2])));
                        break;
                    }
                    case "vn":
                    {
                        normals.Add(new Vector3(float.Parse(t[1]), float.Parse(t[2]), float.Parse(t[3])));
                        break;
                    }
                    case "f":
                    {
                        faces.Add(new Vector3(int.Parse(t[1]), int.Parse(t[4]), int.Parse(t[7])));
                        uvIndex.Add(new Vector3(int.Parse(t[2]), int.Parse(t[5]), int.Parse(t[8])));
                        normalIndex.Add(new Vector3(int.Parse(t[3]), int.Parse(t[6]), int.Parse(t[9])));
                        break;
                    }
                }
            }

            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];

                var a = new Vertex
                {
                    Position = verts[(int) face.X - 1],
                    UV = uvs[(int) uvIndex[i].X - 1],
                    Normal = normals[(int) normalIndex[i].X - 1]
                };

                var b = new Vertex
                {
                    Position = verts[(int) face.Y - 1],
                    UV = uvs[(int)uvIndex[i].Y - 1],
                    Normal = normals[(int) normalIndex[i].Y - 1]
                };

                var c = new Vertex
                {
                    Position = verts[(int) face.Z - 1],
                    UV = uvs[(int) uvIndex[i].Z - 1],
                    Normal = normals[(int) normalIndex[i].Z - 1]
                };

                var tri = new Mesh(a,b,c);
                triangles.Add(tri);
            }

            return triangles.ToArray();
        }
    }
}