#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/EnemyManager", fileName = "EnemyManager")]
    public sealed partial class EnemyManagerConfig : ScriptableObject, IEnemyManagerConfig
    {
        [SerializeField]
        public EnemyConfig? enemyDef;

        EnemyConfig? IEnemyManagerConfig.EnemyDef { get => enemyDef; }
    }
}
