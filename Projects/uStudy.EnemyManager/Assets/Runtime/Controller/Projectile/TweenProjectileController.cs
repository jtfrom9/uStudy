#nullable enable

using System.Threading;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    using Projectile;

    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        CachedTransform _transform = new CachedTransform();
        bool _disposed = false;
        ProjectileConfig? config;
        Status status = Status.Init;
        EndReason endReson = EndReason.Expired;
        Vector3 _direction = Vector3.zero;

        bool _willHit = false;
        RaycastHit? willRaycastHit = null;

        Subject<Projectile.EventArg> onEvent = new Subject<EventArg>();
        CancellationTokenSource cts = new CancellationTokenSource();

        void Awake() {
            _transform.Initialize(transform);
        }

        void OnDestroy()
        {
            Debug.Log($"[{GetHashCode():x}] frame:{Time.frameCount} {gameObject.name} OnDestroy");

            _disposed = true;
            onEvent.OnNext(new EventArg(this, Projectile.EventType.Destroy));
            onEvent.OnCompleted();
        }

        void OnTriggerEnter(Collider other)
        {
            var _hit = false;
            if (other.gameObject.CompareTag(HitTag.Character))
            {
                endReson = EndReason.TargetHit;
                _hit = true;
            }
            if (other.gameObject.CompareTag(HitTag.Environment))
            {
                endReson = EndReason.OtherHit;
                _hit = true;
            }
            if (_hit)
            {
                onEvent.OnNext(new EventArg(this, Projectile.EventType.Trigger)
                {
                    collider = other,
                    willHit = willRaycastHit
                });
                // request cancel
                cts.Cancel();
            }
        }

        void willHit(GameObject gameObject, Ray ray, float distance, RaycastHit hit)
        {
            var mobileObject = gameObject.GetComponent<IMobileObject>();
            if (mobileObject != null)
            {
                // immediately stop tweening without cancel
                _willHit = true;
                transform.DOKill();
                onEvent.OnNext(new EventArg(this, Projectile.EventType.WillHit)
                {
                    willHit = hit,
                    ray = ray,
                    maxDistance = distance
                });
            }
        }

        void hitHandler(Ray ray, float distance, RaycastHit hit)
        {
            var gameObject = hit.collider.gameObject;
            if (gameObject.CompareTag(HitTag.Character) ||
                gameObject.CompareTag(HitTag.Environment))
            {
                willHit(gameObject, ray, distance, hit);
            }
        }

        void hitTest(Vector3 pos, Vector3 dir, float speed) {
            // var hits = Physics.RaycastAll(pos, dir, speed * Time.deltaTime );
            // if(hits.Length > 0) {
            //     Debug.Log($"hits: {hits.Length}");
            //     RaycastHit? nearest = null;
            //     foreach(var hit in hits) {
            //         if (!nearest.HasValue) { nearest = hit; }
            //         else {
            //             if(nearest.Value.distance > hit.distance)
            //                 nearest = hit;
            //         }
            //     }
            //     hitHandler(nearest!.Value);
            // }

            var hit = new RaycastHit();
            var ray = new Ray(pos, dir);
            var distance = speed * Time.deltaTime;
            if (Physics.Raycast(ray, out hit, distance))
            {
                this.willRaycastHit = hit;
                hitHandler(ray, distance, hit);
            }
        }

        async UniTask move(Vector3 destRelative, float duration, bool raycastEveryFrame)
        {
            onEvent.OnNext(new EventArg(this, Projectile.EventType.BeforeMove));

            var dir = destRelative.normalized;
            var speed = destRelative.magnitude / duration;
            var tween = transform.DOMove(destRelative, duration)
                .SetRelative(true)
                .SetUpdate(UpdateType.Fixed)
                .SetEase(Ease.Linear)
                .OnKill(() =>
                {
                    onEvent.OnNext(new EventArg(this, Projectile.EventType.OnKill));
                })
                .OnComplete(() =>
                {
                    onEvent.OnNext(new EventArg(this, Projectile.EventType.OnComplete));
                })
                .OnPause(() =>
                {
                    onEvent.OnNext(new EventArg(this, Projectile.EventType.OnPause));
                });

            if (raycastEveryFrame)
            {
                tween = tween.OnUpdate(() => hitTest(transform.position, dir, speed));
            }

            await tween.ToUniTask(cancellationToken: cts.Token);

            onEvent.OnNext(new EventArg(this, Projectile.EventType.AfterMove));
        }

        async UniTask mainLoop(ProjectileConfig config, ITransform target)
        {
            var prevDir = Vector3.zero;

            //
            // do move step loop
            //
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

                //
                // move advance by destRelative in EachDuration time, raycastEveryFrame is enabled if fast speed
                //
                await move(destRelative, config.EachDuration,
                    config.speed > 50);

                if (_willHit || cts.IsCancellationRequested)
                    break;

                prevDir = dir;
            }

            //
            // last one step move to the object will hit
            //
            if(willRaycastHit.HasValue && !cts.IsCancellationRequested) {
                onEvent.OnNext(new EventArg(this, Projectile.EventType.BeforeLastMove)
                {
                    willHit = willRaycastHit
                });

                // move to will hit point
                await transform.DOMove(willRaycastHit.Value.point, config.speed)
                    .SetSpeedBased(true)
                    .SetEase(Ease.Linear)
                    .SetUpdate(UpdateType.Fixed);
                // at this timing, onTrigger caused because hit will be supporsed,
                // one more frame wait is needed to update last move
                await UniTask.NextFrame(PlayerLoopTiming.LastFixedUpdate);

                onEvent.OnNext(new EventArg(this, Projectile.EventType.AfterLastMove));
            }
        }

        async UniTaskVoid go(ProjectileConfig config, ITransform target)
        {
            onEvent.OnNext(new EventArg(this, Projectile.EventType.BeforeLoop));
            await mainLoop(config, target);
            onEvent.OnNext(new EventArg(this, Projectile.EventType.AfterLoop));

            status = Status.End;
            if (config.endType == EndType.Destroy)
            {
                dispose();
            }
        }

        void dispose() {
            if (_disposed) return;
            // _disposed = true;

            if (DOTween.IsTweening(transform))
            {
                cts.Cancel();
                transform.DOKill();
            }
            _transform.Dispose();
            Destroy(gameObject);
        }

        #region IDisposable
        public void Dispose()
        {
            endReson = EndReason.Disposed;
            dispose();
        }
        #endregion

        #region IMobileObject
        string IMobileObject.Name { get => gameObject.name; }
        ITransform IMobileObject.transform { get => _transform; }
        #endregion

        #region IProjectile

        Status IProjectile.Status { get=> status; }
        EndReason IProjectile.EndReason { get => endReson; }
        ISubject<EventArg> IProjectile.OnEvent { get => onEvent; }

        static int count = 0;

        void IProjectile.Initialize(Vector3 initial, ProjectileConfig config)
        {
            gameObject.name = $"TweenProjectile({count})";
            count++;
            Debug.Log($"[{GetHashCode():x}] {gameObject.name} created");
            transform.position = initial;
            this.config = config;
        }

        void IProjectile.Go(ITransform target)
        {
            Debug.Log($"[{GetHashCode():x}] frame: {Time.frameCount} {gameObject.name} Go");
            if (config == null)
            {
                throw new InvalidConditionException($"config: ${config}");
            }
            if (status != Status.Init)
            {
                throw new InvalidConditionException($"status: ${status}");
            }
            go(config, target).Forget();
        }

        #endregion
    }
}