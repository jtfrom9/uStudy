#nullable enable

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Visualizer/TargetVisualizers", fileName = "TargetVisualizers")]
    public class TargetVisualizersObject : ScriptableObject, ITargetVisualizerFactory
    {
        [SerializeField, InspectInline]
        List<TargetVisualizerObject> visualizerObjects = new List<TargetVisualizerObject>();

        IEnumerable<ITargetVisualizer?> createVisualizers(ITransformProvider target)
        {
            foreach (var vobj in visualizerObjects)
            {
                yield return vobj.Create(target);
            }
        }

        IEnumerable<ITargetVisualizer> ITargetVisualizerFactory.CreateVisualizers(ITransformProvider target)
            => createVisualizers(target).WhereNotNull();
    }
}
