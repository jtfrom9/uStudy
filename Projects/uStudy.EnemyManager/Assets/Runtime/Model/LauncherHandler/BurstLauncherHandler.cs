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
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            UniTask.Create(async () =>
            {
                launcherManager.BeforeFire();
                while (true)
                {
                    var projectile = projectileFactory.Create(
                        start.Position,
                        config);
                    if (projectile == null)
                    {
                        Debug.LogError($"fail to create projectile");
                        break;
                    }
                    projectile.Start(target);
                    launcherManager.OnFired(projectile);
                    try
                    {
                        await UniTask.Delay(100, cancellationToken: cts.Token);
                    }
                    catch
                    {
                        break;
                    }
                }
                launcherManager.AfterFire();
            }).Forget();
        }

        public void TriggerOff()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        public void Error()
        {
            Debug.Log("BurstLauncherHandler.Error. raise TriggerOff");
            TriggerOff();
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
