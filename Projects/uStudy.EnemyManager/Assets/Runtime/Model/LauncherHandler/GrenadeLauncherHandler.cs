#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class GrenadeLauncherHandler : ILauncherHandler
    {
        ILauncherHandlerEvent handlerEvent;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        ITransform? _start = null;
        ITransform? _target = null;

        public void Fire(ITransform start, ITransform target)
        {
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            handlerEvent.OnShowTrajectory(true);
            _start = start;
            _target = target;
        }

        public void TriggerOff()
        {
            if(_start==null || _target==null)
                return;

            handlerEvent.OnBeforeFire();
            var projectile = projectileFactory.Create(
                _start.Position,
                config);
            if (projectile == null)
            {
                Debug.LogError($"fiail to create projectile");
                return;
            }
            projectile.Start(_target);
            handlerEvent.OnFired(projectile);
            handlerEvent.OnShowTrajectory(false);
            handlerEvent.OnAfterFire();
        }

        public void Error()
        {
        }

        public void Dispose()
        {
        }

        public GrenadeLauncherHandler(
            ILauncherHandlerEvent handlerEvent,
            IProjectileFactory projectileFactory,
            ProjectileConfig config)
        {
            this.handlerEvent = handlerEvent;
            this.projectileFactory = projectileFactory;
            this.config = config;
        }
    }
}
