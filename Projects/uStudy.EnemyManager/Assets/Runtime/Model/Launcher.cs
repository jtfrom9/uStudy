#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public interface ILauncherController
    {
        ITransform mazzle { get; }
        IEnemy? target { get; }
        bool CanLaunch { get; }
        void SetTarget(IEnemy? enemy);
    }

    public class Launcher: System.IDisposable
    {
        ProjectileConfig? config;
        CompositeDisposable disposable = new CompositeDisposable();

        public bool CanLaunch { get => launcherController?.CanLaunch ?? false && config != null; }

        public void SetProjectileConfig(ProjectileConfig? config)
        {
            this.config = config;
            if(this.trajectoryVisualizer!=null) {
                this.trajectoryVisualizer.SetConfig(config);
            }
        }

        public void ShowTrajectory(bool v)
        {
            this.trajectoryVisualizer?.Show(v);
        }

        public void SetTarget(IEnemy? enemy)
        {
            launcherController?.SetTarget(enemy);
            if (enemy != null)
            {
                if (launcherController != null && trajectoryVisualizer != null)
                {
                    enemy.transform.OnPositionChanged.Subscribe(pos =>
                    {
                        trajectoryVisualizer.SetEndTarget(pos);
                    }).AddTo(disposable);
                }
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

        public void Dispose() {
            disposable.Dispose();
        }

        IProjectileFactory projectileFactory;
        ILauncherController? launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;

        public Launcher(IProjectileFactory projectileFactory)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = Controller.Find<ILauncherController>();
            this.trajectoryVisualizer = Controller.Find<ITrajectoryVisualizer>();

            if (trajectoryVisualizer != null && launcherController!=null)
            {
                launcherController.mazzle.OnPositionChanged.Subscribe(position => {
                    trajectoryVisualizer.SetStartTarget(position);
                }).AddTo(disposable);
            }
        }
    }
}