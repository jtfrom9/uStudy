#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Enemy/Manager", fileName = "EnemyManager")]
    public class EnemyManagerObject : ScriptableObject
    {
        [SerializeField, InspectInline]
        EnemyEffectsObject? _effects;

        public IEnemyAttackedEffectFactory? effects { get => _effects; }
    }
}
