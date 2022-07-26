#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public enum ProjectileType
    {
        Fire,
        Burst,
        Grenade
    };

    [CreateAssetMenu(menuName = "Hedwig/Projectile", fileName = "Projectile")]
    public class ProjectileConfig : ScriptableObject
    {
        [SerializeField, InterfaceType(typeof(IProjectileController))]
        public Component? prefab;

        [SerializeField] public ProjectileType type;
        [SerializeField] public bool chargable;

        [SerializeField]
        [Range(0, 256)]
        public int successionCount = 1;

        [SerializeField]
        [Range(10, 1000)]
        public int successionInterval = 0;

        [SerializeField]
        [Range(10, 10000)]
        public int recastTime = 500;

        [SerializeField]
        [Range(0, 2.0f)]
        public float shake = 0f;

        [SerializeField]
        [Min(1)]
        public float baseSpeed = 10f;

        [SerializeField]
        [Min(1)]
        public float range = 10;

        [SerializeField] public Trajectory? trajectory;
    }
}