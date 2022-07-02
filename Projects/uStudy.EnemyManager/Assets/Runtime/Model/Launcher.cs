#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public interface ILauncherController
    {
        ITransform mazzle { get; }
        IMobileObject? target { get; }
        bool CanLaunch { get; }
        void SetTarget(IMobileObject? target);
    }

    public class Launcher: IDisposable
    {
        ProjectileConfig? _config;
        CompositeDisposable disposable = new CompositeDisposable();

        bool recasting = false;

        public bool CanFire {
            get
            {
                var v = (launcherController?.CanLaunch ?? false) &&
                    config != null &&
                    !recasting;
                return v;
            }
        }

        public ProjectileConfig? config { get => _config; }

        Subject<ProjectileConfig?> onConfigChanged = new Subject<ProjectileConfig?>();

        public ISubject<ProjectileConfig?> OnConfigChanged { get => onConfigChanged; }

        Subject<bool> onCanFireChanged = new Subject<bool>();

        public ISubject<bool> OnCanFireChanged { get => onCanFireChanged; }

        Subject<float> onRecastTimeUpdated = new Subject<float>();

        public ISubject<float> OnRecastTimeUpdated { get => onRecastTimeUpdated; }

        public void SetProjectileConfig(ProjectileConfig? config)
        {
            this._config = config;
            this.trajectoryVisualizer?.SetConfig(config);
            onConfigChanged.OnNext(config);
        }

        public void ShowTrajectory(bool v)
        {
            this.trajectoryVisualizer?.Show(v);
        }

        public bool trajectory { get => trajectoryVisualizer?.visible ?? false; }

        public void SetTarget(IMobileObject? target)
        {
            launcherController?.SetTarget(target);
            trajectoryVisualizer?.SetEndTarget(target?.transform);
        }

        void setupMazzle() {
            if (trajectoryVisualizer != null && launcherController != null)
            {
                trajectoryVisualizer.SetStartTarget(launcherController.mazzle);
            }
        }

        void changeRecastState(bool v) {
            recasting = v;
            OnCanFireChanged.OnNext(this.CanFire);
        }

        async UniTask stepRecast(int recast, int step, int index)
        {
            await UniTask.Delay(step);
            var elapsed = index * step;
            onRecastTimeUpdated.OnNext((float)elapsed / (float)recast);
        }

        public async UniTask Fire()
        {
            if(launcherController==null || launcherController.target==null)
                return;
            if(config==null)
                return;
            if(!CanFire)
                return;
            changeRecastState(true);
            for (var i = 0; i < config.successionCount; i++)
            {
                var projectile = projectileFactory.Create(
                    launcherController.mazzle.Position,
                    this.config);
                Debug.Log($"[{i}] {launcherController.target.transform.Position}");
                projectile?.Go(launcherController.target);

                if (config.successionCount > 1)
                {
                    await UniTask.Delay(config.successionInterval);
                }
            }
            for (var i = 0; i < config.recastTime; i += 100)
            {
                await stepRecast(config.recastTime, 100, i);
            }
            changeRecastState(false);
        }

        public void StartFire()
        {
            // Debug.Log("StartFire");
        }
        public void EndFire()
        {
            // Debug.Log("EndFire");
        }

        void IDisposable.Dispose()
        {
            onConfigChanged.OnCompleted();
            disposable.Dispose();
        }

        // injected
        IProjectileFactory projectileFactory;

        // find
        ILauncherController? launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;

        public Launcher(IProjectileFactory projectileFactory)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = Controller.Find<ILauncherController>();
            this.trajectoryVisualizer = Controller.Find<ITrajectoryVisualizer>();
            this.setupMazzle();
        }
    }
}