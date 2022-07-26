using UnityEngine;

namespace Hedwig.Runtime
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
}