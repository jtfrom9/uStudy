#nullable enable

using System.Linq;
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

        Vector3 toSpearPoint(Vector3 from, Vector3 to, float range) {
            return from + ((to - from).normalized * range);
        }

        Vector3 toPoint(TrajectoryLineMap line, float range, bool spear)
        {
            if (spear)
            {
                var (from, to) = line.GetPoints();
                return toSpearPoint(from, to, range);
            }
            else
            {
                return line.GetToPoint();
            }
        }

        async UniTask<bool> curveMainLoop(TrajectorySectionMap section)
        {
            //
            // Move to the pont which premaild by trajectory setting
            //
            foreach (var line in section.Lines)
            {
                var exitLoop = await projectileController.Move(
                    line.GetToPoint(),
                    line.GetAccelatedSpeed());
                // section.speed);
                if (exitLoop)
                    return true;
            }
            return false;
        }

        async UniTask<bool> homingMainLoop(TrajectorySectionMap section, ITransform target)
        {
            var prevDir = Vector3.zero;
            var from = section.Lines.First().GetFromPoint();
            var lines = section.Lines.ToArray();
            foreach (var (line, index) in lines.Select((line, index) => (line, index)))
            {
                var to = Vector3.Lerp(from, target.Position, (float)1 / (float)(lines.Length - index));
                var dir = to - from;
                if (!line.IsFirst)
                {
                    var angle = Vector3.Angle(dir, prevDir);
                    if (section.adjustMaxAngle < angle)
                    {
                        var cross = Vector3.Cross(dir, prevDir);
                        var length = dir.magnitude;
                        dir = Quaternion.AngleAxis(-section.adjustMaxAngle, cross) * prevDir;
                        to = from + dir.normalized * length;
                    }
                }
                var exitLoop = await projectileController.Move(to, line.GetAccelatedSpeed());
                if (exitLoop)
                    return true;
                section.AddDynamic(new TrajectoryLineMap(section, index, line.fromFactor, line.toFactor));
                from = to; // update next 'from' position
                prevDir = dir;
            }
            return false;
        }

        async UniTask mainLoop(ProjectileConfig config, ITransform target)
        {
            var globalFromPoint = projectileController.transform.Position;
            var globalToPoint = target.Position + target.ShakeRandom(config.shake);

            if (config.trajectory == null) {
                //
                // linear Move if no trajectory
                //
                await projectileController.Move(
                    toSpearPoint(globalFromPoint, globalToPoint, config.range),
                    config.baseSpeed);
            }
            else
            {
                var map = config.trajectory.ToMap(globalFromPoint, globalToPoint, config.baseSpeed);
                var sections = map.Sections.ToList();

                //
                // linear Move if only one line and Fire style projectile
                //
                if(sections.Count==1 && !sections[0].IsCurve && config.type==ProjectileType.Fire) {
                    var section = sections[0];
                    await projectileController.Move(
                        toSpearPoint(globalFromPoint, globalToPoint, config.range),
                        section.baseSpeed * section.speedFactor);
                }
                else
                {
                    //
                    // mainloop for curved Move
                    //
                    foreach (var section in sections)
                    {
                        var exitLoop = false;
                        Debug.Log($"{section}");
                        if (!section.IsHoming)
                        {
                            exitLoop = await curveMainLoop(section);
                        } else {
                            exitLoop = await homingMainLoop(section, target);
                        }
                        if(exitLoop)
                            break;
                    }
                }
            }

            //
            // last one step move to the object will hit
            //
            await projectileController.LastMove(config.baseSpeed);
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