#nullable enable

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Search;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Effect/Environment", fileName = "EnvironmentEffect")]
    public sealed partial class EnvironmentObject : ScriptableObject
    {
        [SerializeField, SearchContext("t:prefab effect")]
        GameObject? prefab;

        IEnumerable<IEffect?> createEffects(IEnvironment environment, Vector3 position, Vector3 direction)
        {
            if(prefab==null) yield break;
            var effect = Instantiate(prefab).GetComponent<IHitEffect>();
            effect?.Initialize(environment.controller, position, direction);
            yield return effect;
        }

        public IEffect[] CreateEffects(IEnvironment environment, Vector3 position, Vector3 direction)
            => createEffects(environment, position, direction)
                .WhereNotNull()
                .ToArray();
    }
}