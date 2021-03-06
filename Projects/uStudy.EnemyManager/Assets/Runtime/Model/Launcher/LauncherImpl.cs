#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    public class LauncherImpl : ILauncher, ILauncherHandlerEvent
    {
        ProjectileConfig? _config;
        IMobileObject? _target;
        CompositeDisposable disposable = new CompositeDisposable();

        bool initialized = false;
        bool recasting = false;
        bool triggerReq = false;
        bool triggered = false;

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
            setCanFire();
            handleError();
            onConfigChanged.OnNext(config);
        }

        void setTarget(IMobileObject? target)
        {
            if (!initialized)
            {
                throw new InvalidConditionException("LauncherManager is not Initalized");
            }
            _target = target;
            trajectoryVisualizer?.SetEndTarget(target?.transform);
            setCanFire();
            if(_target!=null) {
                handleTriggerOn();
            } else {
                handleError();
            }
            handleError();
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

        void handleTriggerOn()
        {
            // Debug.Log($"handleTrigerOn. {_target}, req:{triggerReq}");
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            if (triggerReq && canFire.Value)
            {
                launcherHandler.TriggerOn(launcherController.mazzle, _target.transform);
                triggered = true;
            }
        }

        void handleError()
        {
            // Debug.Log($"handleError. error:{!canFire.Value}");
            if (launcherHandler == null)
                return;
            if (!canFire.Value) 
            {
                launcherHandler.Error();
            }
        }

        void triggerOn()
        {
            if (!initialized)
                throw new InvalidConditionException("LauncherManager is not Initalized");
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            if(triggerReq)
                return;
            triggerReq = true;
            if(!canFire.Value) {
                return;
            }
            launcherHandler.TriggerOn(launcherController.mazzle, _target.transform);
            triggered = true;
        }

        void triggerOff()
        {
            if (!initialized)
                throw new InvalidConditionException("LauncherManager is not Initalized");
            if (launcherHandler == null)
                return;
            if(!triggerReq)
                return;
            triggerReq = false;
            if (triggered)
            {
                launcherHandler.TriggerOff();
                triggered = false;
            }
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
                handleTriggerOn();
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
        void ILauncherHandlerEvent.OnShowTrajectory(bool v) => trajectoryVisualizer?.Show(v);
        void ILauncherHandlerEvent.OnBeforeFire() => onBeforeLaunched();
        void ILauncherHandlerEvent.OnAfterFire() => onAfterFire();
        void ILauncherHandlerEvent.OnFired(IProjectile projectile) => onFired.OnNext(projectile);
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

        public LauncherImpl(IProjectileFactory projectileFactory, ILauncherController launcherController)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = launcherController;
            this.trajectoryVisualizer = Controller.Find<ITrajectoryVisualizer>();
        }
    }
}
