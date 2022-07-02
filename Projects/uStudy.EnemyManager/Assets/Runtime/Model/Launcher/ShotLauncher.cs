#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class ShotLauncher: ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        ILauncherController launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;

        public async UniTask Fire()
        {
            if(launcherController.target==null)
                return;
            launcherManager.OnBeforeLaunched();
            for (var i = 0; i < config.successionCount; i++)
            {
                var projectile = projectileFactory.Create(
                    launcherController.mazzle.Position,
                    config);
                Debug.Log($"[{i}] {launcherController.target.transform.Position}");
                projectile?.Go(launcherController.target);

                if (config.successionCount > 1)
                {
                    await UniTask.Delay(config.successionInterval);
                }
            }
            launcherManager.OnLaunched();
        }

        public void StartFire()
        {
            Debug.Log($"StartFire: {config.chargable}");
            if (config.chargable)
            {
                trajectoryVisualizer?.Show(true);
            }
        }

        public void EndFire()
        {
            Debug.Log("EndFire");
            if (config.chargable)
            {
                trajectoryVisualizer?.Show(false);
            }
        }

        public void Dispose()
        {
        }

        public ShotLauncher(
            ILauncherManager  launcherManager,
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
