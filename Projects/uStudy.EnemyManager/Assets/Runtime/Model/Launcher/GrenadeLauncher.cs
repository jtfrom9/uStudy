#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class GrenadeLauncher : ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        ILauncherController launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;

        public UniTask Fire()
        {
            trajectoryVisualizer?.Show(true);
            return UniTask.CompletedTask;
        }

        public void StartFire()
        {
        }

        public void EndFire()
        {
            if (launcherController.target == null)
                return;
            launcherManager.OnBeforeLaunched();
            var projectile = projectileFactory.Create(
                launcherController.mazzle.Position,
                config);
            projectile?.Go(launcherController.target);
            trajectoryVisualizer?.Show(false);
            launcherManager.OnLaunched();
        }

        public void Dispose()
        {
        }

        public GrenadeLauncher(
            ILauncherManager launcherManager,
            IProjectileFactory projectileFactory,
            ProjectileConfig config,
            ILauncherController launcherController,
            ITrajectoryVisualizer? trajectoryVisualizer)
        {
            this.launcherManager = launcherManager;
            this.projectileFactory = projectileFactory;
            this.config = config;

            this.launcherController = launcherController;
            this.trajectoryVisualizer = trajectoryVisualizer;
        }
    }
}
