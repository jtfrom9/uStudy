#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Enemy", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField, InterfaceType(typeof(IEnemyController))]
        public Component? prefab;

        [SerializeField]
        public int MaxHealth;

        [SerializeField]
        public int Attack;

        [SerializeField]
        public int Deffence;
    }
}