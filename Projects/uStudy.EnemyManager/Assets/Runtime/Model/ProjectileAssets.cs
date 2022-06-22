#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/ProjectileAssets", fileName = "ProjectileAsset")]
    public class ProjectileAssets : ScriptableObject, IProjectileFactory
    {
        [SerializeField, InterfaceType(typeof(IProjectile))]
        Component? projectilePrefab;

        IProjectile? IProjectileFactory.Create(Vector3 start, Transform target, float duration)
        {
            if (projectilePrefab == null) return null;
            // var projectile = Instantiate(projectilePrefab, start, Quaternion.identity) as IProjectile;
            var projectile = Instantiate(projectilePrefab) as IProjectile;
            projectile?.Initialize(start, target, duration);
            return projectile;
        }
    }
}
