#nullable enable

using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class BurstLauncher : ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        CancellationTokenSource cts = new CancellationTokenSource();

        ILauncherController launcherController;
        ITrajectoryVisualizer? trajectoryVisualizer;


        public UniTask Fire()
        {
            if (launcherController.target == null)
                return UniTask.CompletedTask;

            launcherManager.OnBeforeLaunched();
            return UniTask.Create(async () =>
            {
                while(true) {
                    var projectile = projectileFactory.Create(
                        launcherController.mazzle.Position,
                        config);
                    projectile?.Go(launcherController.target);
                    try
                    {
                        await UniTask.Delay(100, cancellationToken: cts.Token);
                    }catch{
                        break;
                    }
                }
                launcherManager.OnLaunched();
            });
        }

        public void StartFire()
        {
            Debug.Log("StartFire");
        }

        public void EndFire()
        {
            Debug.Log("EndFire");
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
        }

        public BurstLauncher(
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
