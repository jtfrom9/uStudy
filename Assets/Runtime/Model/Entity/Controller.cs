using System.Linq;
using UnityEngine;

namespace Hedwig.RTSCore
{
    public class Controller : MonoBehaviour
    {
        public static T Find<T>() where T : class
        {
            var objects = UnityEngine.Object.FindObjectsOfType<Controller>();
            foreach (var _obj in objects)
            {
                if (_obj is T)
                    return _obj as T;
            }
            return null;
        }
    }

    public static class ControllerTransformExtension
    {
        public static T[] GetControllersInChildren<T>(this Transform transform) where T : class
        {
            return transform.GetComponentsInChildren<Controller>()
                .Select(controller => controller as T)
                .Where(controller => controller != null)
                .ToArray();
        }
    }
}