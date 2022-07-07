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

        public void StartFire(ITransform start, ITransform target)
        {
        }

        public void EndFire(ITransform start, ITransform target)
        {
            launcherManager.OnBeforeLaunched();
            var projectile = projectileFactory.Create(
                start.Position,
                config);
            projectile?.Go(target);
            launcherManager.ShowTrajectory(false);
            launcherManager.OnLaunched();
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
