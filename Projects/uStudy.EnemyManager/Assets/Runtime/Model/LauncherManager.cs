#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    public class LauncherManager : ILauncherManager, IDisposable
    {
        ProjectileConfig? _config;
        CompositeDisposable disposable = new CompositeDisposable();

        bool recasting = false;

        Subject<ProjectileConfig?> onConfigChanged = new Subject<ProjectileConfig?>();
        Subject<bool> onCanFireChanged = new Subject<bool>();
        Subject<float> onRecastTimeUpdated = new Subject<float>();

        // injected
        IProjectileFactory projectileFactory;
        ILauncherController launcherController;

        // find
        ITrajectoryVisualizer? trajectoryVisualizer;

        ILauncherHandler? launcher;

        bool canFire
        {
            get
            {
                var v = launcherController.CanLaunch &&
                    _config != null &&
                    !recasting;
                return v;
            }
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
            onCanFireChanged.OnNext(canFire);
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
            this.launcher?.Dispose();
            this.launcher = null;

            if (config != null)
            {
                switch (config.type)
                {
                    case ProjectileType.Fire:
                        this.launcher = new ShotLauncher(this, projectileFactory, config);
                        break;
                    case ProjectileType.Burst:
                        this.launcher = new BurstLauncher(this, projectileFactory, config);
                        break;
                    case ProjectileType.Grenade:
                        this.launcher = new GrenadeLauncher(this, projectileFactory, config);
                        break;
                }
            }
            onConfigChanged.OnNext(config);
        }

        void fire()
        {
            if (launcherController.target == null)
                return;
            if (launcher == null)
                return;
            if (_config == null)
                return;
            if (!canFire)
                return;
            launcher.Fire(launcherController.mazzle,
                launcherController.target.transform);
        }

        void startFire()
        {
            if (launcherController.target == null)
                return;
            if (launcher == null)
                return;
            launcher.StartFire(launcherController.mazzle,
                launcherController.target.transform);
        }

        void endFire()
        {
            if (launcherController.target == null)
                return;
            if (launcher == null)
                return;
            launcher.EndFire(launcherController.mazzle,
                launcherController.target.transform);
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

        #region ILauncherManager
        ProjectileConfig? ILauncherManager.config { get => _config; }
        void ILauncherManager.SetProjectileConfig(ProjectileConfig? config) => setConfig(config);

        void ILauncherManager.SetTarget(IMobileObject? target)
        {
            launcherController.SetTarget(target);
            trajectoryVisualizer?.SetEndTarget(target?.transform);
        }

        void ILauncherManager.ShowTrajectory(bool v)
        {
            this.trajectoryVisualizer?.Show(v);
        }

        bool ILauncherManager.CanFire { get => canFire; }
        void ILauncherManager.Fire() => fire();
        void ILauncherManager.StartFire() => startFire();
        void ILauncherManager.EndFire() => endFire();

        ISubject<ProjectileConfig?> ILauncherManager.OnConfigChanged { get => onConfigChanged; }
        ISubject<bool> ILauncherManager.OnCanFireChanged { get => onCanFireChanged; }
        ISubject<float> ILauncherManager.OnRecastTimeUpdated { get => onRecastTimeUpdated; }

        void ILauncherManager.OnBeforeLaunched() => onBeforeLaunched();
        void ILauncherManager.OnLaunched() => onLaunched();
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            launcher?.Dispose();
            onConfigChanged.OnCompleted();
            onCanFireChanged.OnCompleted();
            onRecastTimeUpdated.OnCompleted();
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
