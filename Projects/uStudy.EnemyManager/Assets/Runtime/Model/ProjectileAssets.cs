#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/ProjectileAssets", fileName = "ProjectileAsset")]
    public class ProjectileAssets : ScriptableObject, IProjectileFactory
    {
        [SerializeField, InterfaceType(typeof(IProjectile))]
        Component? projectilePrefab;

        IProjectile? IProjectileFactory.Create(Vector3 start, Vector3 end, float duration)
        {
            if (projectilePrefab == null) return null;
            var projectile = Instantiate(projectilePrefab) as IProjectile;
            projectile?.Initialize(start, end, duration);
            return projectile;
        }
    }
}
