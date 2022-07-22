#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    public class LauncherManager : ILauncher, ILauncherManager
    {
        ProjectileConfig? _config;
        IMobileObject? _target;
        CompositeDisposable disposable = new CompositeDisposable();

        bool initialized = false;
        bool recasting = false;
        bool trigger = false;

        ReactiveProperty<bool> canFire = new ReactiveProperty<bool>();
        Subject<ProjectileConfig?> onConfigChanged = new Subject<ProjectileConfig?>();
        Subject<IMobileObject?> onTargetChanged = new Subject<IMobileObject?>();
        Subject<float> onRecastTimeUpdated = new Subject<float>();
        Subject<IProjectile> onFired = new Subject<IProjectile>();

        // injected
        IProjectileFactory projectileFactory;
        ILauncherController launcherController;

        // find
        ITrajectoryVisualizer? trajectoryVisualizer;

        ILauncherHandler? launcherHandler;

        void initialize()
        {
            if (trajectoryVisualizer != null)
            {
                trajectoryVisualizer.SetStartTarget(launcherController.mazzle);
            }
            launcherController.Initialize(this);
            initialized = true;
        }

        void setCanFire()
        {
            var v = _config != null &&
                _target != null &&
                !recasting;
            canFire.Value = v;
        }

        void changeRecastState(bool v)
        {
            recasting = v;
            setCanFire();
        }

        async UniTask stepRecast(int recast, int step, int index)
        {
            await UniTask.Delay(step);
            var elapsed = index * step;
            onRecastTimeUpdated.OnNext((float)elapsed / (float)recast);
        }

        void setConfig(ProjectileConfig? config, ProjectileOption? option)
        {
            if (!initialized)
            {
                throw new InvalidConditionException("LauncherManager is not Initalized");
            }
            if(recasting) {
                throw new InvalidConditionException("LauncherManager is Recasting");
            }
            this._config = config;
            this.trajectoryVisualizer?.SetConfig(config);

            // reset laouncher handler
            this.launcherHandler?.Dispose();
            this.launcherHandler = null;

            if (config != null)
            {
                switch (config.type)
                {
                    case ProjectileType.Fire:
                        this.launcherHandler = new ShotLauncherHandler(this, projectileFactory, config, option);
                        break;
                    case ProjectileType.Burst:
                        this.launcherHandler = new BurstLauncherHandler(this, projectileFactory, config);
                        break;
                    case ProjectileType.Grenade:
                        this.launcherHandler = new GrenadeLauncherHandler(this, projectileFactory, config);
                        break;
                }
            }
            onConfigChanged.OnNext(config);
            setCanFire();
        }

        void setTarget(IMobileObject? target)
        {
            if (!initialized)
            {
                throw new InvalidConditionException("LauncherManager is not Initalized");
            }
            if (recasting)
            {
                throw new InvalidConditionException("LauncherManager is Recasting");
            }
            _target = target;
            trajectoryVisualizer?.SetEndTarget(target?.transform);
            setCanFire();
            onTargetChanged.OnNext(_target);
        }

        void fire()
        {
            if (!initialized)
                throw new InvalidConditionException("LauncherManager is not Initalized");
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            if (_config == null)
                return;
            if (!canFire.Value)
                return;
            launcherHandler.Fire(launcherController.mazzle, _target.transform);
        }

        void triggerOn()
        {
            if (!initialized)
                throw new InvalidConditionException("LauncherManager is not Initalized");
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            if (!canFire.Value)
                return;
            if(trigger)
                return;
            launcherHandler.TriggerOn(launcherController.mazzle, _target.transform);
            trigger = true;
        }

        void triggerOff()
        {
            if (!initialized)
                throw new InvalidConditionException("LauncherManager is not Initalized");
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            if (!trigger)
                return;
            launcherHandler.TriggerOff(launcherController.mazzle,_target.transform);
            trigger = false;
        }

        void onBeforeLaunched()
        {
            changeRecastState(true);
        }

        void onAfterFire()
        {
            if(_config==null) {
                throw new InvalidConditionException("ProjectileConfig was modified unexpectedly");
            }
            UniTask.Create(async () => {
                for (var i = 0; i < _config.recastTime; i += 100)
                {
                    await stepRecast(_config.recastTime, 100, i);
                }
                changeRecastState(false);
            }).Forget();
        }

        #region ILauncher
        void ILauncher.Initialize() => initialize();
        ProjectileConfig? ILauncher.config { get => _config; }
        void ILauncher.SetProjectileConfig(ProjectileConfig? config, ProjectileOption? option) => setConfig(config, option);
        IMobileObject? ILauncher.target { get => _target; }
        void ILauncher.SetTarget(IMobileObject? target) => setTarget(target);

        IReadOnlyReactiveProperty<bool> ILauncher.CanFire { get => canFire; }
        void ILauncher.Fire() => fire();
        void ILauncher.TriggerOn() => triggerOn();
        void ILauncher.TriggerOff() => triggerOff();

        ISubject<ProjectileConfig?> ILauncher.OnConfigChanged { get => onConfigChanged; }
        ISubject<IMobileObject?> ILauncher.OnTargetChanged { get => onTargetChanged; }
        ISubject<float> ILauncher.OnRecastTimeUpdated { get => onRecastTimeUpdated; }
        ISubject<IProjectile> ILauncher.OnFired { get => onFired; }
        #endregion

        #region ILauncherManager
        ILauncher ILauncherManager.launcher { get => this; }
        void ILauncherManager.ShowTrajectory(bool v) => trajectoryVisualizer?.Show(v);
        void ILauncherManager.BeforeFire() => onBeforeLaunched();
        void ILauncherManager.AfterFire() => onAfterFire();
        void ILauncherManager.OnFired(IProjectile projectile) => onFired.OnNext(projectile);
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            onConfigChanged.OnCompleted();
            onTargetChanged.OnCompleted();
            onRecastTimeUpdated.OnCompleted();
            launcherHandler?.Dispose();
            canFire.Dispose();
            disposable.Dispose();
        }
        #endregion

        public LauncherManager(IProjectileFactory projectileFactory, ILauncherController launcherController)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = launcherController;
            this.trajectoryVisualizer = Controller.Find<ITrajectoryVisualizer>();
        }
    }
}
