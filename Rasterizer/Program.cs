using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace Rasterizer
{
    internal class Rasterizer
    {
        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            //カメラの設定
            var cameraSettings = new CameraSettings
            {
                ImageHeight = 1028,
                ImageWidth = 1028,
                Zmin = 0.1,
                Zmax = 10.0,
                ScreenDistance = 1.0,
                ScreenWidth = 1.0,
                ScreenHeight = 1.0,
            };

            //シーンにカメラを作成
            Scene.Camera = new Camera(cameraSettings);

            //オブジェクトをロード
            var obj1 = ObjLoader.Load("ball1", @"X:\sphere.obj", @"X:\obj2.png");
            //var obj2 = ObjLoader.Load("ball2", @"X:\obj2.obj", @"X:\obj2.png");
            
            //オブジェクトをシーンに追加
            Scene.AddObject(obj1);
            
            // //オブジェクトの操作
            // obj1.Transform.Rotation = new Vector3(10 * MathF.PI / 180, -30 * MathF.PI / 180, 0);
            // obj1.Transform.Position = new Vector3(0, 0.5f, 0f);
            //
            // obj2.Transform.Rotation = new Vector3(90 * MathF.PI / 180, 0, 0);
            // obj2.Transform.Position = new Vector3(0,0,-0.5f);

            Scene.Camera.Transform.Position = new Vector3(0, 2.4f, 7f);
            
            Scene.Camera.Projection();
            Scene.Camera.Render();
            
            //Scene.Camera.RenderPoints();
            
            Bitmap result = Scene.Camera.FrameBuffer.GetBitmap();
            result.Save($@"X:\output.png", ImageFormat.Png);

            Console.WriteLine(sw.Elapsed);
        }
    }
}