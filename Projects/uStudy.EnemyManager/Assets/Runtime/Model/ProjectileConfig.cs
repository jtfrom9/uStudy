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
        [SerializeField] public Projectile.EndType endType = Projectile.EndType.Destroy;
        [SerializeField] GameObject? endEffectPrefab;

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

        [SerializeField] public float speed = 10f;
        [SerializeField] public float distance = 10;

        [SerializeField] bool adjust;
        [SerializeField] float period;
        [SerializeField] float maxAngle;

        public float? adjustPeriod { get => (!adjust) ? null : period; }
        public float? adjustMaxAngle { get => (!adjust) ? null : maxAngle; }

        [SerializeField] public TrajectoryBase? trajectory;

        public float Duration { get => distance / speed; }
        public int NumAdjust { get => (!adjustPeriod.HasValue) ? 1 : (int)(Duration / adjustPeriod); }

        public float EachDuration { get => Duration / NumAdjust; }

        public Vector3 MakeRandom(ITransform target)
        {
            if(shake==0f) {
                return Vector3.zero;
            } else
            {
                var _r = UnityEngine.Random.Range(-shake, shake);
                var _u = UnityEngine.Random.Range(-shake, shake);
                return target.Right * _r + target.Up * _u;
            }
        }

        public override string ToString() 
        {
            var adjust = adjustPeriod.HasValue ? adjustPeriod.ToString() : "n/a";
            var angle = adjustMaxAngle.HasValue ? adjustMaxAngle.ToString() : "n/a";
            return $"(speed: {speed}, distance: {distance}, adjust: {adjust}, angle: {angle}, duration: {Duration}, Num: {NumAdjust})";
        }
    }
}