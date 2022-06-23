using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/ProjectileConfig", fileName = "ProjectileConfig")]
    public class ProjectileConfig : ScriptableObject
    {
        [SerializeField] public bool destroyAtEnd = true;
        [SerializeField] public float speed = 10f;
        [SerializeField] public float distance = 10;

        [SerializeField] bool adjust;
        [SerializeField] float period;
        [SerializeField] float maxAngle;

        public float? adjustPeriod { get => (!adjust) ? null : period; }
        public float? adjustMaxAngle { get => (!adjust) ? null : maxAngle; }

        [SerializeField] bool random;
        [SerializeField] Vector2 range;

        public Vector2? randomRange { get => (random) ? range : Vector2.zero; }

        public float Duration { get => distance / speed; }
        public int NumAdjust { get => (!adjustPeriod.HasValue) ? 1 : (int)(Duration / adjustPeriod); }

        public float EachDuration { get => Duration / NumAdjust; }

        public Vector3 MakeRandom(Transform target)
        {
            if (!randomRange.HasValue) return Vector3.zero;
            else
            {
                var v = randomRange.Value;
                var _r = UnityEngine.Random.Range(-v.x, v.x);
                var _u = UnityEngine.Random.Range(-v.y, v.y);
                return target.right * _r + target.up * _u;
            }
        }

        public override string ToString() 
        {
            var adjust = adjustPeriod.HasValue ? adjustPeriod.ToString() : "n/a";
            var angle = adjustMaxAngle.HasValue ? adjustMaxAngle.ToString() : "n/a";
            var random = randomRange.HasValue ? $"{randomRange.Value.x}:{randomRange.Value.y}" : "n/a";
            return $"(speed: {speed}, distance: {distance}, adjust: {adjust}, angle: {angle}, rand: {random}, duration: {Duration}, Num: {NumAdjust})";
        }
    }
}