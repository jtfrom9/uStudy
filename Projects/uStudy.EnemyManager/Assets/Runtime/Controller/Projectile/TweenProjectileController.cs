#nullable enable

using System;
using System.Threading;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using UniRx;

using Hedwig.Runtime.Projectile;

namespace Hedwig.Runtime
{
    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        CachedTransform _transform = new CachedTransform();
        bool _disposed = false;
        ProjectileConfig? config;
        Status _status = Status.Init;
        EndReason _endReson = EndReason.Expired;
        Vector3 _direction = Vector3.zero;
        float _speed = 0;
        Subject<IProjectile> _onUpdate = new Subject<IProjectile>();

        CancellationTokenSource cts = new CancellationTokenSource();

        void Awake() {
            _transform.Initialize(transform);
        }

        void OnDestroy()
        {
            _onUpdate.OnCompleted();
        }

        void OnTriggerEnter(Collider other)
        {
            var hit = false;
            if (other.gameObject.CompareTag(HitTag.CharacterT))
            {
                _endReson = EndReason.TargetHit;
                hit = true;
            }
            if (other.gameObject.CompareTag(HitTag.Environment))
            {
                _endReson = EndReason.OtherHit;
                hit = true;
            }
            if (hit)
            {
                cts.Cancel();
            }
        }

        // void hitHandler(RaycastHit hit)
        // {
        //     if (hit.collider.gameObject.CompareTag(Collision.CharacterTag))
        //     {
        //         _endReson = EndReason.TargetHit;
        //         var handler = hit.collider.gameObject.GetComponent<ICollisionHandler>();
        //         if (handler != null)
        //         {
        //             onTriggerInNextFrame.OnNext(new CollisionEvent()
        //             {
        //                 handler = handler,
        //                 pos = hit.point,
        //                 projectile = this
        //             });
        //             cts.Cancel();
        //         }
        //     }
        //     if (hit.collider.gameObject.CompareTag(Collision.EnvironmentTag))
        //     {
        //         _endReson = EndReason.OtherHit;
        //         var handler = hit.collider.gameObject.GetComponent<ICollisionHandler>();
        //         if (handler != null)
        //         {
        //             onTriggerInNextFrame.OnNext(new CollisionEvent()
        //             {
        //                 handler = handler,
        //                 pos = hit.point,
        //                 projectile = this
        //             });
        //             cts.Cancel();
        //         }
        //     }
        // }

        // void hitTest(Vector3 pos, Vector3 dir, float speed) {
        //     // var hits = Physics.RaycastAll(pos, dir, speed * Time.deltaTime );
        //     // if(hits.Length > 0) {
        //     //     Debug.Log($"hits: {hits.Length}");
        //     //     RaycastHit? nearest = null;
        //     //     foreach(var hit in hits) {
        //     //         if (!nearest.HasValue) { nearest = hit; }
        //     //         else {
        //     //             if(nearest.Value.distance > hit.distance)
        //     //                 nearest = hit;
        //     //         }
        //     //     }
        //     //     hitHandler(nearest!.Value);
        //     // }

        //     var hit = new RaycastHit();
        //     if (Physics.Raycast(pos, dir, out hit, speed * Time.deltaTime))
        //     {
        //         hitHandler(hit);
        //     }
        // }

        void _update(Vector3 direction, float speed) {
            this._direction = direction;
            this._speed = speed;
            this._onUpdate.OnNext(this);
        }

        async UniTask<bool> move(Vector3 destRelative, float duration)
        {
            var dir = destRelative.normalized;
            var speed = destRelative.magnitude / duration;
            try
            {
                var tween = transform.DOMove(destRelative, duration)
                    .SetRelative(true)
                    .SetUpdate(UpdateType.Fixed)
                    .SetEase(Ease.Linear);
                tween = tween.OnUpdate(() => _update(dir, speed));
                await tween.ToUniTask(cancellationToken: cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        async UniTask mainLoop(ProjectileConfig config, ITransform target)
        {
            var prevDir = Vector3.zero;
            var distance = config.distance;
            for (var i = 0; i < config.NumAdjust; i++)
            {
                var start = transform.position;
                var rand = config.MakeRandom(target);
                var end = target.Position + rand;
                var dir = end - start;

                if (i > 0 && config.adjustMaxAngle.HasValue)
                {
                    var angle = Vector3.Angle(dir, prevDir);
                    if (config.adjustMaxAngle.Value < angle)
                    {
                        var cross = Vector3.Cross(dir, prevDir);
                        dir = Quaternion.AngleAxis(-config.adjustMaxAngle.Value, cross) * prevDir;
                    }
                }

                var destRelative = dir.normalized * config.speed * config.EachDuration;
                var result = await move(destRelative, config.EachDuration);
                distance -= destRelative.magnitude;

                if (!result)
                    break;

                prevDir = dir;
            }
        }

        async UniTaskVoid go(ProjectileConfig config, ITransform target)
        {
            // var stopwatch = new System.Diagnostics.Stopwatch();
            // stopwatch.Start();
            await mainLoop(config, target);
            // stopwatch.Stop();
            // Debug.Log($"elapsed: {stopwatch.ElapsedMilliseconds}");

            _status = Status.End;
            if (config.endType == EndType.Destroy)
            {
                Dispose();
            }
        }

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (DOTween.IsTweening(transform))
            {
                transform.DOKill();
            }
            Destroy(gameObject);
        }
        #endregion

        #region IMobileObject
        ITransform IMobileObject.transform { get => _transform; }
        Vector3 IMobileObject.diretion { get => _direction; }
        float IMobileObject.speed { get => _speed; }

        void IMobileObject.OnHit(IMobileObject target, Vector3 posision)
        {
            if (target is IEnemy)
                _endReson = EndReason.TargetHit;
            else
                _endReson = EndReason.OtherHit;
            cts.Cancel();
        }
        #endregion

        #region IProjectile

        Status IProjectile.status { get=> _status; }
        EndReason IProjectile.endRegion { get => _endReson; }

        ISubject<IProjectile> IProjectile.OnUpdate { get => _onUpdate; }

        void IProjectile.Initialize(Vector3 initial, ProjectileConfig config)
        {
            transform.position = initial;
            this.config = config;
        }

        void IProjectile.Go(ITransform target)
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

        #endregion
    }
}