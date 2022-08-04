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
        ILauncherHandlerEvent handlerEvent;
        IProjectileFactory projectileFactory;
        ProjectileObject projectileObject;

        CancellationTokenSource cts = new CancellationTokenSource();

        public void Fire(ITransform start, ITransform target)
        {
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            UniTask.Create(async () =>
            {
                handlerEvent.OnBeforeFire();
                while (true)
                {
                    var projectile = projectileFactory.Create(
                        start.Position,
                        projectileObject);
                    if (projectile == null)
                    {
                        Debug.LogError($"fail to create projectile");
                        break;
                    }
                    projectile.Start(target);
                    handlerEvent.OnFired(projectile);
                    try
                    {
                        await UniTask.Delay(100, cancellationToken: cts.Token);
                    }
                    catch
                    {
                        break;
                    }
                }
                handlerEvent.OnAfterFire();
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
            ILauncherHandlerEvent handlerEvent,
            IProjectileFactory projectileFactory,
            ProjectileObject projectileObject)
        {
            this.handlerEvent = handlerEvent;
            this.projectileFactory = projectileFactory;
            this.projectileObject = projectileObject;
        }
    }
}
