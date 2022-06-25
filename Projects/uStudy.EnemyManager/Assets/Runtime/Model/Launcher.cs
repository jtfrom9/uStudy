#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ILauncherController
    {
        Vector3 mazzlePosition { get; }
        Transform mazzle { get; }
        IEnemy? target { get; }
        bool CanLaunch { get; }
        void SetTarget(IEnemy? enemy);
    }

    public class Launcher
    {
        ProjectileConfig? config;

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
            if (trajectoryVisualizer != null && launcherController != null)
            {
                if (enemy != null)
                    trajectoryVisualizer.SetEndTarget(enemy.transform);
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
                launcherController.mazzlePosition,
                this.config);
            projectile?.Go(launcherController.target);
        }

        IProjectileFactory projectileFactory;
        ILauncherController? launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;

        public Launcher(IProjectileFactory projectileFactory)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = Controller.Find<ILauncherController>();
            this.trajectoryVisualizer = Controller.Find<ITrajectoryVisualizer>();

            if (trajectoryVisualizer != null)
            {
                trajectoryVisualizer.SetStartTarget(launcherController.mazzle);
            }
        }
    }
}