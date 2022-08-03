#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/EnemyManager", fileName = "EnemyManager")]
    public sealed partial class EnemyManagerConfig : ScriptableObject
    {
        public EnemyConfig? enemy;
        public EnemyEffectsConfig? effects;
    }
}
