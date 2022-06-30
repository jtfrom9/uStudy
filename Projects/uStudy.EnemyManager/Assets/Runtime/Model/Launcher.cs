#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

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
        ProjectileConfig? config;
        CompositeDisposable disposable = new CompositeDisposable();

        public bool CanLaunch { get => launcherController?.CanLaunch ?? false && config != null; }

        public void SetProjectileConfig(ProjectileConfig? config)
        {
            this.config = config;
            this.trajectoryVisualizer?.SetConfig(config);
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

        public void Launch()
        {
            if(launcherController==null || launcherController.target==null)
                return;
            if(config==null)
                return;
            if(trajectoryVisualizer!=null) {
                trajectoryVisualizer.Show(false);
            }
            var projectile = projectileFactory.Create(
                launcherController.mazzle.Position,
                this.config);
            projectile?.Go(launcherController.target);
        }

        void IDisposable.Dispose()
        {
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