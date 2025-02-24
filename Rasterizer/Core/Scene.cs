using Rasterizer.Object;
using Rasterizer.Object.Component;

namespace Rasterizer.Core
{
    public class Scene
    {
        private List<Object.MyObject> _sceneObjects = new List<Object.MyObject>(10);
        private CameraComponent _camera;

        public void AddObject(MyObject myObject)
        {
            _sceneObjects.Add(myObject);
        }
        
        public void SetTargetCamera(CameraComponent camera)
        {
            _camera = camera;
        }
        
        public CameraComponent GetCamera()
        {
            return _camera;
        }

        public Object.MyObject[] GetAllObjects()
        {
            return _sceneObjects.ToArray();
        }
        
        public MeshRendererComponent[] GetMeshRenderers()
        {
            var meshRenderers = new List<MeshRendererComponent>();
            foreach (var obj in _sceneObjects)
            {
                var meshRenderer = obj.GetComponent<MeshRendererComponent>();
                if (meshRenderer != null)
                {
                    meshRenderers.Add(meshRenderer);
                }
            }

            return meshRenderers.ToArray();
        }
        
        public int TriangleCount()
        {
            var count = 0;
            foreach (var obj in _sceneObjects)
            {
                var meshRenderer = obj.GetComponent<MeshRendererComponent>();
                if (meshRenderer != null)
                {
                    count += meshRenderer.OriginalMesh.Length;
                }
            }
            
            return count;
        }
        
        public void Update()
        {
            foreach (var obj in _sceneObjects)
            {
                foreach (var component in obj.GetComponents())
                {
                    component.Update();
                }
            }
        }   
    }
}