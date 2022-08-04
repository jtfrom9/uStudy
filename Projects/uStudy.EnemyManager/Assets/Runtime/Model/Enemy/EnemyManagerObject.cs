#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/EnemyManager", fileName = "EnemyManager")]
    public class EnemyManagerObject : ScriptableObject
    {
        public EnemyObject? enemy;
        public EnemyEffectsObject? effects;
    }
}
