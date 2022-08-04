#nullable enable

using System;
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

        IObservable<IProjectile> IProjectileFactory.OnCreated { get => onCreated; }

        private IProjectileController? createController(ProjectileObject config)
        {
            if (config.prefab != null)
            {
                return Instantiate(config.prefab) as IProjectileController;
            }
            return Instantiate(projectilePrefab) as IProjectileController;
        }

        IProjectile? IProjectileFactory.Create(Vector3 start, ProjectileObject projectileObject)
        {
            if (projectilePrefab == null) return null;
            IProjectile? projectile = null;

            var projectileController = createController(projectileObject);
            if (projectileController != null)
            {
                projectileController.Initialize(start);
                projectile = new ProjectileImpl(projectileController, projectileObject);
                onCreated.OnNext(projectile);
            }
            return projectile;
        }
    }
}
