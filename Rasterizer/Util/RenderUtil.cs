using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasterizer.Util;

public static class RenderUtil
{
    public static Matrix4x4 ConvertToSystemMatrix(this DenseMatrix matrix)
    {
        return new Matrix4x4(
            (float) matrix[0, 0], (float) matrix[0, 1], (float) matrix[0, 2], (float) matrix[0, 3],
            (float) matrix[1, 0], (float) matrix[1, 1], (float) matrix[1, 2], (float) matrix[1, 3],
            (float) matrix[2, 0], (float) matrix[2, 1], (float) matrix[2, 2], (float) matrix[2, 3],
            (float) matrix[3, 0], (float) matrix[3, 1], (float) matrix[3, 2], (float) matrix[3, 3]
        );
    }
    
    /// <summary>
    /// 三角形abcが背面を向いているかどうか
    /// </summary>
    public static bool IsBackFace(Vector2 a, Vector2 b, Vector2 c)
    {
        var ab = b - a;
        var ac = c - a;
        var normal = ab.X * ac.Y - ab.Y * ac.X;

        return normal > 0;
    }
    
    /// <summary>
    /// 重心座標系で、点pが三角形abcの内部にあるかどうかを判定する
    /// </summary>
    public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        var ab = b - a;
        var ap = p - a;
        var bc = c - b;
        var bp = p - b;
        var ca = a - c;
        var cp = p - c;

        var abap = ab.X * ap.Y - ab.Y * ap.X;
        var bcbp = bc.X * bp.Y - bc.Y * bp.X;
        var cacp = ca.X * cp.Y - ca.Y * cp.X;

        return abap <= 0 && bcbp <= 0 && cacp <= 0;
    }


    public static double InterpolationDouble(
        Vector2 p, Vector2 a, Vector2 b, Vector2 c,
        float av, float bv, float cv)
    {
        var eu = b - a;
        var ev = c - a;

        var u = (-ev.X * (p.Y - a.Y) + ev.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);
        var v = (eu.X * (p.Y - a.Y) - eu.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);

        return av + u * (bv - av) + v * (cv - av);
    }
    
    public static double InterpolationDoublePerspectiveCorrected(
        Vector2 p, Vector2 a, Vector2 b, Vector2 c, 
        float av, float bv, float cv, 
        float aw, float bw, float cw)
    {
        var v0 = b - a;
        var v1 = c - a;
        var v2 = p - a;

        var d00 = v0.X * v0.X + v0.Y * v0.Y;
        var d01 = v0.X * v1.X + v0.Y * v1.Y;
        var d11 = v1.X * v1.X + v1.Y * v1.Y;
        var d20 = v2.X * v0.X + v2.Y * v0.Y;
        var d21 = v2.X * v1.X + v2.Y * v1.Y;

        var denom = d00 * d11 - d01 * d01;
        if (Math.Abs(denom) < 1e-6f) return 0;

        var v = (d20 * d11 - d21 * d01) / denom;
        var w = (d21 * d00 - d20 * d01) / denom;
        var u = 1.0f - v - w;

        var interpolatedW = 1.0f / (u / aw + v / bw + w / cw);
        var interpolatedValue = (u * av / aw + v * bv / bw + w * cv / cw) * interpolatedW;

        return interpolatedValue;
    }
    
    public static Vector2 InterpolationVector2PerspectiveCorrected(
        Vector2 p, Vector2 a, Vector2 b, Vector2 c, 
        Vector2 av, Vector2 bv, Vector2 cv,
        float aw, float bw, float cw)
    {
        var interpX = (float)InterpolationDoublePerspectiveCorrected(
            p, a, b, c, 
            av.X, bv.X, cv.X, 
            aw, bw, cw);
        var interpY = (float)InterpolationDoublePerspectiveCorrected(
            p, a, b, c, 
            av.Y, bv.Y, cv.Y, 
            aw, bw, cw);
        
        return new Vector2(interpX, interpY);
    }
    
    public static Vector3 InterpolationVector3(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector3 av, Vector3 bv,
        Vector3 cv)
    {
        var eu = b - a;
        var ev = c - a;

        var u = (-ev.X * (p.Y - a.Y) + ev.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);
        var v = (eu.X * (p.Y - a.Y) - eu.Y * (p.X - a.X)) / (eu.X * ev.Y - ev.X * eu.Y);

        return av + u * (bv - av) + v * (cv - av);
    }
    
    
}