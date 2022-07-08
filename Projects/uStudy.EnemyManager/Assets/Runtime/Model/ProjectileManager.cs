#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    using Projectile;

    public class ProjectileManager : IProjectile
    {
        IProjectileController projectileController;
        ProjectileConfig config;

        EndReason endReason = EndReason.Expired;
        CompositeDisposable disposables = new CompositeDisposable();
        Subject<Unit> onStarted = new Subject<Unit>();
        Subject<Unit> onEnded = new Subject<Unit>();
        Subject<Unit> onDestroy = new Subject<Unit>();

        async UniTask mainLoop(ProjectileConfig config, ITransform target)
        {
            var prevDir = Vector3.zero;

            //
            // do move step loop
            //
            for (var i = 0; i < config.NumAdjust; i++)
            {
                var start = projectileController.transform.Position;
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
                var exitLoop = await projectileController.Move(destRelative, config.EachDuration,
                    config.speed > 50);

                if (exitLoop)
                    break;

                prevDir = dir;
            }

            //
            // last one step move to the object will hit
            //
            await projectileController.LastMove(config.speed);
        }

        void destroy()
        {
            disposables.Clear();
            onStarted.OnCompleted();
            onEnded.OnCompleted();
            onDestroy.OnNext(Unit.Default);
            onDestroy.OnCompleted();
        }

        void dispose()
        {
            projectileController.Dispose();
        }

        async UniTaskVoid start(ProjectileConfig config, ITransform target)
        {
            onStarted.OnNext(Unit.Default);
            await mainLoop(config, target);
            if (config.endType == EndType.Destroy)
            {
                dispose();
            }
            onEnded.OnNext(Unit.Default);
        }

        #region IMobileObject
        string IMobileObject.Name { get => projectileController.Name; }
        ITransform IMobileObject.transform { get => projectileController.transform; }
        #endregion

        #region IProjectile
        IProjectileController IProjectile.controller { get => projectileController; }
        EndReason IProjectile.EndReason { get => endReason; }

        ISubject<Unit> IProjectile.OnStarted { get => onStarted; }
        ISubject<Unit> IProjectile.OnEnded { get => onEnded; }
        ISubject<Unit> IProjectile.OnDestroy { get => onDestroy; }

        void IProjectile.Start(ITransform target)
        {
            start(config, target).Forget();
        }
        #endregion


        #region IDisposable
        public void Dispose()
        {
            endReason = EndReason.Disposed;
            dispose();
        }
        #endregion

        public ProjectileManager(IProjectileController projectileController, ProjectileConfig config)
        {
            this.projectileController = projectileController;
            this.config = config;

            projectileController.OnEvent.Subscribe(e =>
            {
                switch (e.type)
                {
                    case Projectile.EventType.Trigger:
                        if (e.endReason.HasValue)
                            this.endReason = e.endReason.Value;
                        break;
                    case Projectile.EventType.Destroy:
                        destroy();
                        break;
                }
            }).AddTo(disposables);
        }
    }
}