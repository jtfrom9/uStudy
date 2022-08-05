#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Enemy/Manager", fileName = "EnemyManager")]
    public class EnemyManagerObject : ScriptableObject
    {
        [SerializeField]
        EnemyEffectsObject? _effects;

        public IEnemyAttackedEffectFactory? effects { get => _effects; }
    }
}
