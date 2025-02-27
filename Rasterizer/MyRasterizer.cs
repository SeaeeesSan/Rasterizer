using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using Rasterizer.Core;
using Rasterizer.Object;
using Rasterizer.Object.Component;
using Rasterizer.Rendering;
using Rasterizer.Util;

namespace Rasterizer
{
    internal class Rasterizer
    {
        static void Main(string[] args)
        {
            var scene = new Scene();

            var mesh1 = MeshLoader.LoadMesh(@"F:\teapot3.obj");
            var mesh2 = MeshLoader.LoadMesh(@"F:\teapot.obj");
            var mesh3 = MeshLoader.LoadMesh(@"F:\lpshead.obj");

            var texture1 = new Texture(@"F:\kamen.png");
            var texture2 = new Texture(@"F:\spot_texture.png");
            var texture3 = new Texture(@"F:\lambertian.jpg");

            //--------------------------------------------------------------------------------
            
            //建物
            var obj1 = new MyObject("building");
            scene.AddObject(obj1);
            obj1.AddComponent(new MeshRendererComponent(mesh1));
            obj1.AddComponent(new MaterialComponent(texture1));
            obj1.Transform.Position = new Vector3(0, 0, 0);
            obj1.Transform.Rotation = new Vector3(0, 100, 0);
            obj1.Transform.Scale = new Vector3(2f, 2f, 2f);
            
            //--------------------------------------------------------------------------------
            
            //牛さん
            var obj2 = new MyObject("ball2");
            obj2.AddComponent(new MeshRendererComponent(mesh2));
            obj2.AddComponent(new MaterialComponent(texture2));
            obj2.Transform.Position = new Vector3(-5, 11, 42);
            obj2.Transform.Rotation = new Vector3(0, 120, 30);
            obj2.Transform.Scale = new Vector3(4f, 4f, 4f);
            scene.AddObject(obj2);
            
            //--------------------------------------------------------------------------------
            
            //頭
            var obj3 = new MyObject("lpshead");
            obj3.AddComponent(new MeshRendererComponent(mesh3));
            obj3.AddComponent(new MaterialComponent(texture3));
            obj3.Transform.Position = new Vector3(3.5f, 6, 42);
            obj3.Transform.Rotation = new Vector3(0, 30, 0);
            obj3.Transform.Scale = new Vector3(0.7f, 0.7f, 0.7f);
            scene.AddObject(obj3);
            
            //--------------------------------------------------------------------------------

            //カメラ
            var cameraObj = new MyObject("camera");
            var cameraComponent = new CameraComponent();
            cameraObj.AddComponent(cameraComponent);
            cameraObj.Transform.Position = new Vector3(0f, 10f, 60f);
            scene.AddObject(cameraObj);

            scene.SetTargetCamera(cameraComponent);

            //--------------------------------------------------------------------------------

            var scale = 1f;
            var renderSettings = new RenderSettings
            {
                ImageHeight = (int)(1080 * scale),
                ImageWidth = (int)(1920 * scale),
                Zmin = 1,
                Zmax = 180,
                ScreenDistance = 1.5,
                ScreenWidth = 1.77,
                ScreenHeight = 1.0,
                BackgroundColor = Color.Black,
                LightDir = Vector3.Normalize(new Vector3(-0.1f, 0.5f, 0)),
            };

            Console.WriteLine("Start rendering...");
            Console.WriteLine($" * Image size: {renderSettings.ImageWidth} x {renderSettings.ImageHeight}");
            Console.WriteLine(
                $" * Object count: {scene.GetAllObjects().Length},  Triangle count: {scene.TriangleCount()}\n");

            var sceneRenderer = new SceneRenderer(scene, renderSettings);
            var result = sceneRenderer.Render();
            result.SaveImages(@"F:\output.png", @"F:\output_d.png");

            Console.WriteLine($"Finishd! Elapsed time[ms]: {result.ElapsedTime}");
        }
    }
}