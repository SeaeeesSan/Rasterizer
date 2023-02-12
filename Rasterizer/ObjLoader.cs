using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Double;

namespace Rasterizer
{
    public static class ObjLoader
    {
        public static Triangle[] Load(string path)
        {
            List<Triangle> tris = new List<Triangle>(4000);

            List<DenseMatrix> verts = new List<DenseMatrix>(4000);
            List<Vector2> uv = new List<Vector2>(4000);
            List<Vector3> faces = new List<Vector3>(4000);
            List<Vector3> uvFaces = new List<Vector3>(4000);

            foreach (string line in File.ReadLines(path))
            {
                string[] t = line.Split(' ', '/');
                string type = t[0];
                if (type == "v")
                {
                    //頂点座標
                    double a, b, c;
                    TryParse(t[1], out a);
                    TryParse(t[2], out b);
                    TryParse(t[3], out c);
                    verts.Add(DenseMatrix.OfArray(new double[,]
                    {
                        {(float) a}, {(float) b}, {(float) c}, {1}
                    }));
                }
                else if (type == "vt")
                {
                    double a, b;
                    TryParse(t[1], out a);
                    TryParse(t[2], out b);
                    uv.Add(new Vector2((float) a, (float) b));
                }
                else if (type == "f")
                {
                    //頂点座標
                    double a, b, c;
                    TryParse(t[1], out a);
                    TryParse(t[4], out b);
                    TryParse(t[7], out c);
                    faces.Add(new Vector3((float) a, (float) b, (float) c));
                    
                    double au, bu, cu;
                    TryParse(t[2], out au);
                    TryParse(t[5], out bu);
                    TryParse(t[8], out cu);
                    uvFaces.Add(new Vector3((float) au, (float) bu, (float) cu));
                }
            }

            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];

                var a = new Point();
                a.Position = verts[(int)face.X - 1];
                a.UV = uv[(int)uvFaces[i].X - 1];
                
                var b = new Point();
                b.Position = verts[(int)face.Y - 1];
                b.UV = uv[(int)uvFaces[i].Y - 1];
                
                var c = new Point();
                c.Position = verts[(int)face.Z - 1];
                c.UV = uv[(int)uvFaces[i].Z - 1];
                
                Triangle tri = new Triangle(a,b,c);
                tris.Add(tri);
            }

            return tris.ToArray();
        }
    }
}