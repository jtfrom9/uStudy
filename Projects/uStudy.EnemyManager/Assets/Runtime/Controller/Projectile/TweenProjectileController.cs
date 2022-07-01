#nullable enable

using System;
using System.Threading;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;

using Hedwig.Runtime.Projectile;

namespace Hedwig.Runtime
{
    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        CachedTransform _transform = new CachedTransform();
        ProjectileConfig? config;
        Status _status = Status.Init;
        EndReason _endReson = EndReason.Expired;
        GameObject? lastHit;

        CancellationTokenSource cts = new CancellationTokenSource();

        void Awake() {
            _transform.Initialize(transform);
        }

        void OnTriggerEnter(Collider other)
        {
            var hit = false;
            if (other.gameObject.CompareTag(Collision.CharacterTag))
            {
                _endReson = EndReason.TargetHit;
                hit = true;
            }
            if (other.gameObject.CompareTag(Collision.EnvironmentTag))
            {
                _endReson = EndReason.OtherHit;
                hit = true;
            }
            if (hit)
            {
                cts.Cancel();
            }
        }

        // void hitTest(Vector3 pos, Vector3 dir, float speed) {
        //     var hit = new RaycastHit();
        //     if(Physics.Raycast(pos, dir, out hit, speed *Time.deltaTime)) {
        //         if(hit.collider.gameObject.CompareTag(Collision.CharacterTag)) {
        //             Debug.Log($"would hit {hit.collider.gameObject.name} at next frame");
        //             Debug.Break();
        //             lastHit = hit.collider.gameObject;
        //             cts.Cancel();
        //         }
        //         if (hit.collider.gameObject.CompareTag(Collision.EnvironmentTag))
        //         {
        //             Debug.Log($"would hit {hit.collider.gameObject.name} at next frame");
        //             Debug.Break();
        //         }
        //     }
        // }

        async UniTask<bool> move(Vector3 destRelative, float duration) {
            var dir = destRelative.normalized;
            var speed = destRelative.magnitude / duration;
            try
            {
                await transform.DOMove(destRelative, duration)
                    .SetRelative(true)
                    .SetEase(Ease.Linear)
                    // .OnUpdate(() => hitTest(transform.position, dir, speed))
                    .ToUniTask(cancellationToken: cts.Token);
                return true;
            } catch (OperationCanceledException) {
                return false;
            }
        }

        async UniTask mainLoop(ProjectileConfig config, IMobileObject target)
        {
            var prevDir = Vector3.zero;
            var distance = config.distance;
            for (var i = 0; i < config.NumAdjust; i++)
            {
                var start = transform.position;
                var rand = config.MakeRandom(target.transform);
                Debug.Log($"rand: {rand}");
                var end = target.transform.Position + rand;
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

            // if (lastHit != null)
            // {
            //     await UniTask.NextFrame();
            //     var enemy = lastHit.GetComponent<IEnemy>();
            //     if (enemy != null)
            //     {
            //         enemy.Attacked(10);
            //     }
            // }
        }

        async UniTaskVoid go(ProjectileConfig config, IMobileObject target)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            await mainLoop(config, target);
            stopwatch.Stop();
            Debug.Log($"elapsed: {stopwatch.ElapsedMilliseconds}");

            _status = Status.End;
            if (config.endType == EndType.Destroy)
            {
                Destroy(gameObject);
            }
        }

        #region IDisposable
        void IDisposable.Dispose()
        {
            if (DOTween.IsTweening(transform))
            {
                transform.DOKill();
            }
            Destroy(gameObject);
        }
        #endregion

        #region IMobileObject
        ITransform IMobileObject.transform { get => _transform; }
        #endregion

        #region IProjectile

        Status IProjectile.status { get=> _status; }
        EndReason IProjectile.endRegion { get => _endReson; }

        void IProjectile.Initialize(Vector3 initial, ProjectileConfig config)
        {
            transform.position = initial;
            this.config = config;
        }

        void IProjectile.Go(IMobileObject target)
        {
            if (config == null)
            {
                throw new InvalidConditionException($"config: ${config}");
            }
            if (_status != Status.Init)
            {
                throw new InvalidConditionException($"status: ${_status}");
            }
            go(config, target).Forget();
        }

        void IProjectile.Go(Vector3 target)
        {}
        #endregion
    }
}