#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class GrenadeLauncherHandler : ILauncherHandler
    {
        ILauncherManager launcherManager;
        IProjectileFactory projectileFactory;
        ProjectileConfig config;

        ITransform? _start = null;
        ITransform? _target = null;

        public void Fire(ITransform start, ITransform target)
        {
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
            launcherManager.ShowTrajectory(true);
            _start = start;
            _target = target;
        }

        public void TriggerOff()
        {
            if(_start==null || _target==null)
                return;

            launcherManager.BeforeFire();
            var projectile = projectileFactory.Create(
                _start.Position,
                config);
            if (projectile == null)
            {
                Debug.LogError($"fiail to create projectile");
                return;
            }
            projectile.Start(_target);
            launcherManager.OnFired(projectile);
            launcherManager.ShowTrajectory(false);
            launcherManager.AfterFire();
        }

        public void Error()
        {
        }

        public void Dispose()
        {
        }

        public GrenadeLauncherHandler(
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
