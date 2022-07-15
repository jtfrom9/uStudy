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

        public void Fire(ITransform start, ITransform target)
        {
            launcherManager.ShowTrajectory(true);
        }

        public void TriggerOn(ITransform start, ITransform target)
        {
        }

        public void TriggerOff(ITransform start, ITransform target)
        {
            launcherManager.BeforeFire();
            var projectile = projectileFactory.Create(
                start.Position,
                config);
            if (projectile == null)
            {
                Debug.LogError($"fiail to create projectile");
                return;
            }
            projectile?.Start(target);
            launcherManager.ShowTrajectory(false);
            launcherManager.AfterFire();
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
