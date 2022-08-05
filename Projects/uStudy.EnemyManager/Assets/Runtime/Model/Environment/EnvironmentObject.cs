#nullable enable

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Search;
using UnityExtensions;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Environment", fileName = "Environment")]
    public class EnvironmentObject : ScriptableObject
    {
        [SerializeField, SearchContext("t:prefab environment")]
        GameObject? prefab;

        [SerializeField, InspectInline]
        public EnvironmentEffectsObject? environmentEffects;

        public IEnvironment? Create()
        {
            if (prefab == null) return null;
            var environmentController = Instantiate(prefab).GetComponent<IEnvironmentController>();
            if (environmentController == null) return null;
            return new EnvironmentImpl(this, environmentController);
        }
    }
}