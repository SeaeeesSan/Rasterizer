using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Rasterizer
{
    public static class Scene
    {
        /// <summary>
        /// シーンにあるオブジェクト
        /// </summary>
        private static List<Object> _objects = new List<Object>(10);
        public static Camera Camera;
        public static Light Light;

        /// <summary>
        /// シーンにオブジェクトを追加
        /// </summary>
        /// <param name="objects">追加するオブジェクト</param>
        public static void AddObject(params Object[] objects)
        {
            foreach (var obj in objects)
            {
                _objects.Add(obj);
            }
        }

        /// <summary>
        /// シーンにあるすべてのオブジェクトを取得
        /// </summary>
        /// <returns></returns>
        public static Object[] GetAllObjects()
        {
            return _objects.ToArray();
        }
    }
}