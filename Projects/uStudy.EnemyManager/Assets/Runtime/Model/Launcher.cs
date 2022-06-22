#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ILauncherController
    {
        Vector3 mazzlePosition { get; }
        IEnemy? target { get; }
        bool CanLaunch { get; }
        void Aim(IEnemy? enemy);
    }

    public class Launcher
    {
        public bool CanLaunch { get => launcherController?.CanLaunch ?? false; }

        public void Aim(IEnemy? enemy)
        {
            this.launcherController?.Aim(enemy);
        }

        public void Launch(float duration)
        {
            if(launcherController==null || launcherController.target==null)
                return;
            var projectile = projectileFactory.Create(
                launcherController.mazzlePosition,
                launcherController.target.transform,
                duration);
            projectile?.Go();
        }

        ILauncherController? launcherController;
        IProjectileFactory projectileFactory;

        public Launcher(IProjectileFactory projectileFactory)
        {
            this.projectileFactory = projectileFactory;
            this.launcherController = Controller.Find<ILauncherController>();
        }
    }
}