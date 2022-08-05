#nullable enable

using UnityEngine;
using UnityEngine.Search;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName ="Hedwig/Visualizer/TargetVisualizer", fileName ="TargetVisualizer")]
    public class TargetVisualizerObject : ScriptableObject
    {
        [SerializeField, SearchContext("t:prefab visualizer")]
        GameObject? prefab;

        public ITargetVisualizer? Create(ITransformProvider target)
        {
            if (prefab == null) return null;
            var visualizer = Instantiate(prefab).GetComponent<ITargetVisualizer>();
            visualizer?.Initialize(target);
            return visualizer;
        }
    }
}