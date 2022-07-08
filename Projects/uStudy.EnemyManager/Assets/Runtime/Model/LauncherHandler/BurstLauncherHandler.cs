#nullable enable

using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class BurstLauncherHandler : ILauncherHandler
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
                while (true)
                {
                    var projectile = projectileFactory.Create(
                        start.Position,
                        config);
                    projectile?.Start(target);
                    try
                    {
                        await UniTask.Delay(100, cancellationToken: cts.Token);
                    }
                    catch
                    {
                        break;
                    }
                }
                launcherManager.OnLaunched();
            }).Forget();
        }

        public void StartFire(ITransform start, ITransform target)
        {
            Debug.Log("StartFire");
        }

        public void EndFire(ITransform start, ITransform target)
        {
            Debug.Log("EndFire");
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
        }

        public BurstLauncherHandler(
            ILauncherManager launcherManager,
            IProjectileFactory projectileFactory,
            ProjectileConfig config)
        {
            this.launcherManager = launcherManager;
            this.projectileFactory = projectileFactory;
            this.config = config;
        }
    }
}
