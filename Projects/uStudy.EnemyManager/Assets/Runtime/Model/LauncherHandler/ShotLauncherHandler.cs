#nullable enable

using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class ShotLauncherHandler: ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        CancellationTokenSource cts = new CancellationTokenSource();

        public void Fire(ITransform start, ITransform target)
        {
            UniTask.Create(async () =>
            {
                launcherManager.OnBeforeLaunched();
                for (var i = 0; i < config.successionCount; i++)
                {
                    var projectile = projectileFactory.Create(
                        start.Position,
                        config);
                    projectile?.Start(target);

                    if (config.successionCount > 1)
                    {
                        await UniTask.Delay(config.successionInterval, cancellationToken: cts.Token);
                    }
                }
                launcherManager.OnLaunched();
            }).Forget();
        }

        public void StartFire(ITransform start, ITransform target)
        {
            Debug.Log($"StartFire: {config.chargable}");
            if (config.chargable)
            {
                launcherManager.ShowTrajectory(true);
            }
        }

        public void EndFire(ITransform start, ITransform target)
        {
            Debug.Log("EndFire");
            if (config.chargable)
            {
                launcherManager.ShowTrajectory(false);
            }
        }

        public void Dispose()
        {
            cts.Cancel();
        }

        public ShotLauncherHandler(
            ILauncherManager  launcherManager,
            IProjectileFactory projectileFactory,
            ProjectileConfig config)
        {
            this.launcherManager = launcherManager;
            this.projectileFactory = projectileFactory;
            this.config = config;
        }
    }
}
