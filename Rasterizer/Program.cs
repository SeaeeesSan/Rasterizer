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
            for (int i = 0; i < 360 / 3; i++)
            {
                //カメラの設定
                var cameraSettings = new CameraSettings
                {
                    ImageHeight = 2048,
                    ImageWidth = 2048,
                    Zmin = 4,
                    Zmax = 10,
                    ScreenDistance = 1.0,
                    ScreenWidth = 1.0,
                    ScreenHeight = 1.0,
                };

                //シーンにカメラを作成
                Scene.Camera = new Camera(cameraSettings);
                Stopwatch sw = Stopwatch.StartNew();
                //オブジェクトをロード
                var obj1 = ObjLoader.Load("ball1", @"X:\teapot_n.obj", @"X:\uv.png");
            
                //オブジェクトをシーンに追加
                Scene.AddObject(obj1);
                Scene.Light = new Light(new Vector3(110 * MathF.PI / 180,90 * MathF.PI / 180,0));
            
                //オブジェクトの操作
                
                obj1.Transform.Rotation = new Vector3(0, i * 3 * MathF.PI / 180, 0);

                Scene.Camera.Transform.Position = new Vector3(0, 1.5f, 8f);

                Console.WriteLine("Object init Finished:" + sw.Elapsed);

                Scene.Camera.Projection();
                Scene.Camera.Render(); 
                //Scene.Camera.RenderPoints();

            
                Console.WriteLine("Rendering Finished:" + sw.Elapsed);
            
                //Scene.Camera.RenderPoints();
                Scene.Camera.SSAA();
                Scene.Camera.SSAAd();

                Bitmap result = Scene.Camera.FrameBuffer.GetColorBitmap();
                Bitmap depth = Scene.Camera.FrameBuffer.GetDepthBitmap();

                result.Save($@"X:\d\output_{i}.png", ImageFormat.Png); 
                depth.Save($@"X:\d\output_d_{i}.png", ImageFormat.Png);

                Console.WriteLine("All tasks Finished:" + sw.Elapsed);
            }
                
        }
    }
}