#nullable enable

using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public sealed partial class Factory : IProjectileFactory
    {
        [Header("Projectile Settings")]

        [SerializeField, InterfaceType(typeof(IProjectileController))]
        Component? projectilePrefab;

        Subject<IProjectile> onCreated = new Subject<IProjectile>();

        ISubject<IProjectile> IProjectileFactory.OnCreated { get => onCreated; }

        IProjectile? IProjectileFactory.Create(Vector3 start, ProjectileConfig config)
        {
            if (projectilePrefab == null) return null;
            IProjectile? projectile = null;
            var projectileController = Instantiate(projectilePrefab) as IProjectileController;
            if (projectileController != null)
            {
                projectileController.Initialize(start);
                projectile = new ProjectileImpl(projectileController, config);
                onCreated.OnNext(projectile);
            }
            return projectile;
        }
    }
}
