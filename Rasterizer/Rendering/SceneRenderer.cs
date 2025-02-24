using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Rasterizer.Core;
using Rasterizer.Util;

namespace Rasterizer.Rendering;

public class SceneRenderer
{
    private Scene _scene;
    private RenderSettings _renderSettings;
    private FrameBuffer _frameBuffer;

    //最小のw値
    private const double MIN_W = 0.1;

    public FrameBuffer FrameBuffer => _frameBuffer;

    public SceneRenderer(Scene scene, RenderSettings renderSettings)
    {
        _scene = scene;
        _renderSettings = renderSettings;
        _frameBuffer = new FrameBuffer(
            _renderSettings.ImageWidth, 
            _renderSettings.ImageHeight, 
            _renderSettings.BackgroundColor);
    }
    
    public RenderResult Render()
    {
        var sw = new Stopwatch();
        sw.Start();
            
        Projection();
        RenderFrame();

        sw.Stop();

        var result = new RenderResult(
            _frameBuffer.GetColorBitmap(),
            _frameBuffer.GetDepthBitmap(),
            sw.ElapsedMilliseconds);

        return result;
    }

    /// <summary>
    /// フレームをレンダリング
    /// </summary>
    public void RenderFrame()
    {
        foreach (var obj in _scene.GetMeshRenderers())
        {
            var material = obj.MyObject.GetMaterial();
            var i = 0;

            foreach (var triangle in obj.Mesh)
            {
                i++;

                //ビューポート変換
                var a = ViewportProjection(triangle.A);
                var b = ViewportProjection(triangle.B);
                var c = ViewportProjection(triangle.C);

                //メッシュの最大、最小の座標を取得
                var xMax = (int)Math.Ceiling(Math.Max(Math.Max(a.X, b.X), c.X));
                var xMin = (int)Math.Floor(Math.Min(Math.Min(a.X, b.X), c.X));
                var yMax = (int)Math.Ceiling(Math.Max(Math.Max(a.Y, b.Y), c.Y));
                var yMin = (int)Math.Floor(Math.Min(Math.Min(a.Y, b.Y), c.Y));

                //描画範囲外
                if (xMax < 0 || xMin > _renderSettings.ImageWidth ||
                    yMax < 0 || yMin > _renderSettings.ImageHeight)
                {
                    continue;
                }

                //裏面カリング
                if (RenderUtil.IsBackFace(a, b, c))
                    continue;

                var aw = Math.Max(triangle.A.Position[3, 0], MIN_W);
                var bw = Math.Max(triangle.B.Position[3, 0], MIN_W);
                var cw = Math.Max(triangle.C.Position[3, 0], MIN_W);

                var aDepth = (float)(triangle.A.Position[2, 0]);
                var bDepth = (float)(triangle.B.Position[2, 0]);
                var cDepth = (float)(triangle.C.Position[2, 0]);

                var xMinClamped = Math.Max(xMin, 0);
                var xMaxClamped = Math.Min(xMax, _renderSettings.ImageWidth);
                var yMinClamped = Math.Max(yMin, 0);
                var yMaxClamped = Math.Min(yMax, _renderSettings.ImageHeight);


                for (int x = xMinClamped; x < xMaxClamped; x++)
                {
                    for (int y = yMinClamped; y < yMaxClamped; y++)
                    {
                        //画面上の座標
                        var p = new Vector2(x, y);

                        //深度を算出
                        var depth = RenderUtil.InterpolationDoublePerspectiveCorrected(p, a, b, c,
                            aDepth, bDepth, cDepth,
                            (float)aw, (float)bw, (float)cw);

                        //ZminとZmaxの間にない場合は描画しない
                        if (depth < _renderSettings.Zmin || depth > _renderSettings.Zmax)
                            continue;

                        //三角形内に点が存在するか
                        if (!RenderUtil.IsPointInTriangle(p, a, b, c))
                            continue;

                        //深度値の正規化
                        var normalizedDepth = (depth - _renderSettings.Zmin) /
                                              (_renderSettings.Zmax - _renderSettings.Zmin);
                        if (normalizedDepth < 0 || normalizedDepth > 1)
                            continue;

                        //深度がデプスバッファよりも大きいか
                        if (!(_frameBuffer.Depth[x, y] > normalizedDepth))
                            continue;

                        //デプスバッファ書き込み
                        _frameBuffer.Depth[x, y] = normalizedDepth;

                        //uv計算
                        var uv = RenderUtil.InterpolationVector2PerspectiveCorrected(
                            p, a, b, c,
                            triangle.A.UV, triangle.B.UV, triangle.C.UV,
                            (float)aw, (float)bw, (float)cw);

                        //色計算
                        var normal = RenderUtil.InterpolationVector3(p, a, b, c, triangle.A.Normal,
                            triangle.B.Normal, triangle.C.Normal);
                        var viewVector = new Vector3(0, 0, 1);
                        var lightVector = _renderSettings.LightDir;

                        var color = material.GetColor(uv, lightVector, normal, viewVector);

                        var pixelColor = Color.FromArgb((int)Math.Clamp(color.X * 255, 0, 255),
                            (int)Math.Clamp(color.Y * 255, 0, 255),
                            (int)Math.Clamp(color.Z * 255, 0, 255));

                        //フレームバッファに書き込み
                        _frameBuffer.Color[x, y] = pixelColor;
                    }
                }
            }
        }
    }

    public void RenderPoints()
    {
        foreach (var obj in _scene.GetMeshRenderers())
        {
            foreach (var mesh in obj.Mesh)
            {
                try
                {
                    var a = ViewportProjection(mesh.A);
                    var b = ViewportProjection(mesh.B);
                    var c = ViewportProjection(mesh.C);

                    var xMax = (int)Math.Max(Math.Max(a.X, b.X), c.X) + 1;
                    var xMin = (int)Math.Min(Math.Min(a.X, b.X), c.X) - 1;
                    var yMax = (int)Math.Max(Math.Max(a.Y, b.Y), c.Y) + 1;
                    var yMin = (int)Math.Min(Math.Min(a.Y, b.Y), c.Y) - 1;

                    var aDepth = (float)(mesh.A.Position[2, 0]);
                    var bDepth = (float)(mesh.B.Position[2, 0]);
                    var cDepth = (float)(mesh.C.Position[2, 0]);

                    var xMinClamped = (int)Math.Max(xMin, 1);
                    var xMaxClamped = (int)Math.Min(xMax, _renderSettings.ImageWidth - 1);
                    var yMinClamped = (int)Math.Max(yMin, 1);
                    var yMaxClamped = (int)Math.Min(yMax, _renderSettings.ImageHeight - 1);


                    for (int x = xMinClamped; x < xMaxClamped; x++)
                    {
                        for (int y = yMinClamped; y < yMaxClamped; y++)
                        {
                            var p = new Vector2(x, y);

                            var depth = RenderUtil.InterpolationDouble(p, a, b, c, aDepth, bDepth, cDepth);

                            //描画範囲外
                            if (depth < _renderSettings.Zmin || depth > _renderSettings.Zmax)
                            {
                            }
                            else
                            {
                                _frameBuffer.Color[(int)a.X, (int)a.Y] = Color.Black;
                                _frameBuffer.Color[(int)b.X, (int)b.Y] = Color.Black;
                                _frameBuffer.Color[(int)c.X, (int)c.Y] = Color.Black;
                            }
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// MVP変換
    /// </summary>
    public void Projection()
    {
        var camera = _scene.GetCamera();

        foreach (var obj in _scene.GetMeshRenderers())
        {
            obj.ResetMesh();
            
            // モデリング変換（M）
            obj.UpdateTransform();

            // カメラのビュー行列（V）を計算
            obj.MultiMatrix(camera.GetViewMatrix());

            // プロジェクション行列（P）を計算
            obj.MultiMatrix(camera.GetProjectionMatrix(_renderSettings));
        }
    }

    /// <summary>
    /// ビューポート変換
    /// </summary>
    private Vector2 ViewportProjection(Vertex v)
    {
        double w = v.Position[3, 0];
        w = Math.Max(w, MIN_W);

        double ndcX = v.Position[0, 0] / w;
        double ndcY = v.Position[1, 0] / w;

        float screenX = (float)((ndcX + 1.0) * 0.5 * _renderSettings.ImageWidth);
        float screenY = (float)((1.0 - ndcY) * 0.5 * _renderSettings.ImageHeight);

        return new Vector2(screenX, screenY);
    }

    /// <summary>
    /// 簡易的なSSAA
    /// </summary>
    public void SSAA()
    {
        var col = _frameBuffer.Color;
        var result = new Color[_renderSettings.ImageWidth / 2, _renderSettings.ImageWidth / 2];

        for (int x = 0; x < _renderSettings.ImageWidth / 2; x++)
        {
            for (int y = 0; y < _renderSettings.ImageWidth / 2; y++)
            {
                Color a = col[x * 2, y * 2];
                Color b = col[x * 2 + 1, y * 2];
                Color c = col[x * 2 + 1, y * 2 + 1];
                Color d = col[x * 2, y * 2 + 1];

                result[x, y] = Color.FromArgb((a.R + b.R + c.R + d.R) / 4, (a.G + b.G + c.G + d.G) / 4,
                    (a.B + b.B + c.B + d.B) / 4);
            }
        }

        _frameBuffer.Color = result;
        _frameBuffer.X /= 2;
        _frameBuffer.Y /= 2;
    }
    
    public void Clear()
    {
        _frameBuffer = new FrameBuffer(
            _renderSettings.ImageWidth, 
            _renderSettings.ImageHeight, 
            _renderSettings.BackgroundColor);
    }
}