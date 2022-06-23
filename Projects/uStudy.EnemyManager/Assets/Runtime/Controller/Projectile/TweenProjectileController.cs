#nullable enable

using System;
using System.Threading;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        Transform? target;
        ProjectileConfig? config;

        CancellationTokenSource cts = new CancellationTokenSource();

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(Collision.CharacterTag) ||
                other.gameObject.CompareTag(Collision.EnvironmentTag))
            {
                cts.Cancel();
            }
        }

        async UniTask<bool> move(Vector3 destRelative, float duration) {
            try
            {
                await transform.DOMove(destRelative, duration)
                    .SetRelative(true)
                    .SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: cts.Token);
                return true;
            } catch (OperationCanceledException) {
                return false;
            }
        }

        async UniTaskVoid go(ProjectileConfig config)
        {
            if (this.target == null) return;

            var stopwatch = new System.Diagnostics.Stopwatch();
            Debug.Log($"{config}");

            stopwatch.Start();

            var prevDir = Vector3.zero;
            var distance = config.distance;
            for (var i = 0; i < config.NumAdjust; i++)
            {
                var start = transform.position;
                var rand = config.MakeRandom(target);
                Debug.Log($"rand: {rand}");
                var end = target.position + rand;
                var dir = end - start;

                if (i > 0 && config.adjustMaxAngle.HasValue)
                {
                    var angle = Vector3.Angle(dir, prevDir);
                    if (config.adjustMaxAngle.Value < angle)
                    {
                        var cross = Vector3.Cross(dir, prevDir);
                        dir = Quaternion.AngleAxis(-config.adjustMaxAngle.Value, cross) * prevDir;
                    }

                    // var recalc = Vector3.Angle(dir, prevDir);
                    // if (recalc > config.maxAngle)
                    // {
                    //     Debug.Log($"angle: {recalc}");
                    // }
                }

                var destRelative = dir.normalized * config.speed * config.EachDuration;
                // var distRelative = destRelative.magnitude;
                // if(distRelative > dir.magnitude) {
                //     destRelative = dir;
                //     Debug.Log($"near: ${destRelative}");
                // }
                Debug.Log($"DOMove: {destRelative} {config.EachDuration}");
                var result = await move(destRelative, config.EachDuration);
                distance -= destRelative.magnitude;

                if (!result)
                    break;

                prevDir = dir;
            }

            stopwatch.Stop();
            Debug.Log($"elapsed: {stopwatch.Elapsed.TotalMilliseconds}");
            if(config.destroyAtEnd)
                Destroy(gameObject);
        }

        #region IProjectile
        void IProjectile.Initialize(Vector3 initial, Transform target, ProjectileConfig config)
        {
            transform.position = initial;
            this.target = target;
            this.config = config;
        }

        void IProjectile.Go()
        {
            if(config==null) return;
            go(config).Forget();
        }
        #endregion

        #region IDisposable
        void System.IDisposable.Dispose()
        {
            transform.DOKill();
            Destroy(gameObject);
        }
        #endregion

    }
}