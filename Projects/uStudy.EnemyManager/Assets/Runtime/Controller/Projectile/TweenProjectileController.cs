#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Hedwig.Runtime
{
    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        Vector3 start;
        Vector3 end;
        float duration;
        void IProjectile.Initialize(Vector3 start, Vector3 end, float duration)
        {
            this.start = start;
            this.end = end;
            this.duration = duration;
        }

        void IProjectile.Go()
        {
            var dir = end - start;
            var p1 = start + dir.normalized * (dir.magnitude / 3);
            transform.DOPath(new Vector3[]{
                p1 + Vector3.up,
                end
            }, duration, PathType.CatmullRom)
            .SetEase(Ease.Linear);
        }

        public void Dispose() {
            Destroy(gameObject);
        }
    }
}