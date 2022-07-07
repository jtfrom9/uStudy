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

        bool recasting = false;

        ReactiveProperty<bool> canFire = new ReactiveProperty<bool>();
        Subject<ProjectileConfig?> onConfigChanged = new Subject<ProjectileConfig?>();
        Subject<IMobileObject?> onTargetChanged = new Subject<IMobileObject?>();
        Subject<float> onRecastTimeUpdated = new Subject<float>();

        // injected
        IProjectileFactory projectileFactory;
        ILauncherController launcherController;

        // find
        ITrajectoryVisualizer? trajectoryVisualizer;

        ILauncherHandler? launcherHandler;

        void setCanFire()
        {
            var v = _config != null &&
                _target != null &&
                !recasting;
            canFire.Value = v;
        }

        void initializeController()
        {
            if (trajectoryVisualizer != null)
            {
                trajectoryVisualizer.SetStartTarget(launcherController.mazzle);
            }
            launcherController.Initialize(this);
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

        void setConfig(ProjectileConfig? config)
        {
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
                        this.launcherHandler = new ShotLauncherHandler(this, projectileFactory, config);
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

        void setTarget(IMobileObject? target) {
            _target = target;
            trajectoryVisualizer?.SetEndTarget(target?.transform);
            setCanFire();
            onTargetChanged.OnNext(_target);
        }

        void fire()
        {
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

        void startFire()
        {
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            launcherHandler.StartFire(launcherController.mazzle, _target.transform);
        }

        void endFire()
        {
            if (_target == null)
                return;
            if (launcherHandler == null)
                return;
            launcherHandler.EndFire(launcherController.mazzle,_target.transform);
        }

        void onBeforeLaunched()
        {
            changeRecastState(true);
        }

        void onLaunched()
        {
            if(_config==null) return;
            UniTask.Create(async () => {
                for (var i = 0; i < _config.recastTime; i += 100)
                {
                    await stepRecast(_config.recastTime, 100, i);
                }
                changeRecastState(false);
            }).Forget();
        }

        #region ILauncher
        ProjectileConfig? ILauncher.config { get => _config; }
        void ILauncher.SetProjectileConfig(ProjectileConfig? config) => setConfig(config);
        IMobileObject? ILauncher.target { get => _target; }
        void ILauncher.SetTarget(IMobileObject? target) => setTarget(target);

        IReadOnlyReactiveProperty<bool> ILauncher.CanFire { get => canFire; }
        void ILauncher.Fire() => fire();
        void ILauncher.StartFire() => startFire();
        void ILauncher.EndFire() => endFire();

        ISubject<ProjectileConfig?> ILauncher.OnConfigChanged { get => onConfigChanged; }
        ISubject<IMobileObject?> ILauncher.OnTargetChanged { get => onTargetChanged; }
        ISubject<float> ILauncher.OnRecastTimeUpdated { get => onRecastTimeUpdated; }
        #endregion

        #region ILauncherManager
        ILauncher ILauncherManager.launcher { get => this; }
        void ILauncherManager.ShowTrajectory(bool v) => trajectoryVisualizer?.Show(v);
        void ILauncherManager.OnBeforeLaunched() => onBeforeLaunched();
        void ILauncherManager.OnLaunched() => onLaunched();
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
            this.initializeController();
        }
    }
}
