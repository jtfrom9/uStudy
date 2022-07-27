#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Enemy", fileName = "EnemyDef")]
    public class EnemyDef : ScriptableObject
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