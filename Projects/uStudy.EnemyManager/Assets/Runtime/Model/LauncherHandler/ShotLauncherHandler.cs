#nullable enable

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    public class ShotLauncherHandler: ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;
        ProjectileOption? option;

        public void Fire(ITransform start, ITransform target)
        {
            UniTask.Create(async () =>
            {
                launcherManager.BeforeFire();
                for (var i = 0; i < config.successionCount; i++)
                {
                    var cts = new CancellationTokenSource();
                    var projectile = projectileFactory.Create(
                        start.Position,
                        config);
                    if(projectile==null) {
                        Debug.LogError($"fiail to create projectile");
                        break;
                    }
                    var disposable = projectile.OnDestroy.Subscribe(_ =>
                    {
                        cts.Cancel();
                    });
                    projectile.Start(target, in option);
                    launcherManager.OnFired(projectile);

                    if (config.successionCount > 1)
                    {
                        try
                        {
                            await UniTask.Delay(config.successionInterval, cancellationToken: cts.Token);
                        }catch(OperationCanceledException) {
                            break;
                        }finally {
                            disposable.Dispose();
                            cts.Dispose();
                        }
                    }
                }
                launcherManager.AfterFire();
            }).Forget();
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            Debug.Log($"StartFire: {config.chargable}");
            if (config.chargable)
            {
                launcherManager.ShowTrajectory(true);
            }
        }

        public void TriggerOff()
        {
            Debug.Log("EndFire");
            if (config.chargable)
            {
                launcherManager.ShowTrajectory(false);
            }
        }

        public void Error()
        {
        }

        public void Dispose()
        {
        }

        public ShotLauncherHandler(
            ILauncherManager  launcherManager,
            IProjectileFactory projectileFactory,
            ProjectileConfig config,
            ProjectileOption? option)
        {
            this.launcherManager = launcherManager;
            this.projectileFactory = projectileFactory;
            this.config = config;
            this.option = option;
        }
    }
}
