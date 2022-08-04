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
        ILauncherHandlerEvent handlerEvent;
        IProjectileFactory projectileFactory;
        ProjectileObject projectileObject;
        ProjectileOption? option;

        public void Fire(ITransform start, ITransform target)
        {
            UniTask.Create(async () =>
            {
                handlerEvent.OnBeforeFire();
                for (var i = 0; i < projectileObject.successionCount; i++)
                {
                    var cts = new CancellationTokenSource();
                    var projectile = projectileFactory.Create(
                        start.Position,
                        projectileObject);
                    if(projectile==null) {
                        Debug.LogError($"fiail to create projectile");
                        break;
                    }
                    var disposable = projectile.OnDestroy.Subscribe(_ =>
                    {
                        cts.Cancel();
                    });
                    projectile.Start(target, in option);
                    handlerEvent.OnFired(projectile);

                    if (projectileObject.successionCount > 1)
                    {
                        try
                        {
                            await UniTask.Delay(projectileObject.successionInterval, cancellationToken: cts.Token);
                        }catch(OperationCanceledException) {
                            break;
                        }finally {
                            disposable.Dispose();
                            cts.Dispose();
                        }
                    }
                }
                handlerEvent.OnAfterFire();
            }).Forget();
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            Debug.Log($"StartFire: {projectileObject.chargable}");
            if (projectileObject.chargable)
            {
                handlerEvent.OnShowTrajectory(true);
            }
        }

        public void TriggerOff()
        {
            Debug.Log("EndFire");
            if (projectileObject.chargable)
            {
                handlerEvent.OnShowTrajectory(false);
            }
        }

        public void Error()
        {
        }

        public void Dispose()
        {
        }

        public ShotLauncherHandler(
            ILauncherHandlerEvent  handlerEvent,
            IProjectileFactory projectileFactory,
            ProjectileObject projectileObject,
            ProjectileOption? option)
        {
            this.handlerEvent = handlerEvent;
            this.projectileFactory = projectileFactory;
            this.projectileObject = projectileObject;
            this.option = option;
        }
    }
}
