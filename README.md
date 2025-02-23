# MyRasterizer
C#でのデプスバッファ法を用いたラスタライザの実装です。

CPUで3Dモデル（.obj）のレンダリングが可能です。

|表示|デプスバッファ|
|---|---|
|![image](https://github.com/user-attachments/assets/76f18f34-bbfd-44da-8e10-fb8b4de07cc8)|![image](https://github.com/user-attachments/assets/8e09b312-484e-432b-bd13-435a75cd5a8a)|
(1920 * 1080、△285,749、10020ms)

<img src="https://user-images.githubusercontent.com/68797964/219909379-a4353322-4a0a-4f86-bb4a-678801802bc6.gif" width="200" />

### できること
* 3Dモデル、テクスチャの読み込み（マテリアル非対応）
* 複数オブジェクト
* 簡単なコンポーネントシステム
* オブジェクトの移動、回転、スケール
* カメラの移動、回転
* レンダリング設定
* Phongライティングモデル（仮実装）
* SSAA

#### サンプル（MyRasterizer.cs）


```cs

var scene = new Scene();

var mesh1 = MeshLoader.LoadMesh(@"F:\hoge.obj");
var texture1 = new Texture(@"F:\huga.png");

//オブジェクトの追加
var obj1 = new MyObject("hogehuga");
scene.AddObject(obj1);
obj1.AddComponent(new MeshRendererComponent(mesh1));
obj1.AddComponent(new MaterialComponent(texture1));
obj1.Transform.Position = new Vector3(0, 0, 0);
obj1.Transform.Rotation = new Vector3(0, 100, 0);
obj1.Transform.Scale = new Vector3(2f, 2f, 2f);


//カメラ
var cameraObj = new MyObject("camera");
scene.AddObject(cameraObj);
var cameraComponent = new CameraComponent();
cameraObj.AddComponent(cameraComponent);
cameraObj.Transform.Position = new Vector3(0f, 10f, 60f);

scene.SetTargetCamera(cameraComponent);


var renderSettings = new RenderSettings
{
    ImageHeight = 1080,
    ImageWidth = 1920,
    Zmin = 1,
    Zmax = 180,
    ScreenDistance = 1.5,
    ScreenWidth = 1.77,
    ScreenHeight = 1.0,
    BackgroundColor = Color.Black,
    LightDir = Vector3.Normalize(new Vector3(-0.1f, 0.5f, 0)),
};

var sceneRenderer = new SceneRenderer(scene, renderSettings);
var result = sceneRenderer.Render();

result.SaveImages(@"F:\output.png", @"F:\output_d.png");
```

### 使用したもの
* Math.NET Numerics

### 既知の問題

* 画面手前のメッシュが正常にレンダリングされない
  * wの値が小さくなることでメッシュが荒ぶってしまうので、最小値を設けているが、デプスバッファとUVの補完が正常でない
* ライティングがおかしい？
