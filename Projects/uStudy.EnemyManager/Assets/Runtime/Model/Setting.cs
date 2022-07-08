#nullable enable

using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName ="Hedwig/Setting", fileName ="HedwigRuntimeSetting")]
    public class Setting : ScriptableObject, IProjectileFactory, IEffectFactory, ICursorFactory
    {
        #region  IProjectileFactory
        [SerializeField, InterfaceType(typeof(IProjectileController))]
        Component? projectilePrefab;

        Subject<IProjectile> onCreated = new Subject<IProjectile>();

        ISubject<IProjectile> IProjectileFactory.OnCreated { get => onCreated; }

        IProjectile? IProjectileFactory.Create(Vector3 start,ProjectileConfig config)
        {
            if (projectilePrefab == null) return null;
            IProjectile? projectile = null;
            var projectileController = Instantiate(projectilePrefab) as IProjectileController;
            if (projectileController != null)
            {
                projectileController.Initialize(start);
                projectile = new ProjectileManager(projectileController, config);
                onCreated.OnNext(projectile);
            }
            return projectile;
        }
        #endregion

        #region  IEffectFactory
        [SerializeField, InterfaceType(typeof(IDamageEffect))]
        Component? damageEffectPrefab;

        [SerializeField, InterfaceType(typeof(IHitEffect))]
        Component? hitEffectPrefab;

        [SerializeField]
        DamageEffectParameter? damageEffectParameter;

        public IDamageEffect? CreateDamageEffect(IMobileObject parent, int damage)
        {
            if (damageEffectPrefab == null)
            {
                return null;
            }
            var effect = Instantiate(damageEffectPrefab) as IDamageEffect;
            effect?.Initialize(parent,
                damageEffectParameter!,
                damage);
            return effect;
        }

        public IHitEffect? CreateHitEffect(IMobileObject parent, Vector3 position, Vector3 normal)
        {
            if (hitEffectPrefab == null)
            {
                return null;
            }
            var effect = Instantiate(hitEffectPrefab) as IHitEffect;
            effect?.Initialize(parent,
                position,
                normal);
            return effect;
        }
        #endregion

        #region ICursorFactory
        [SerializeField, InterfaceType(typeof(ITargetCursor))]
        Component? targetCursorPrefab;

        [SerializeField, InterfaceType(typeof(IFreeCursor))]
        Component? freeCursorPrefab;

        ITargetCursor? ICursorFactory.CreateTargetCusor(ICharactor charactor)
        {
            if (targetCursorPrefab == null)
            {
                return null;
            }
            var cursor = Instantiate(targetCursorPrefab) as ITargetCursor;
            cursor?.Initialize(charactor, charactor.distanceToGround);
            return cursor;
        }

        IFreeCursor? ICursorFactory.CreateFreeCusor()
        {
            if (freeCursorPrefab==null)
            {
                return null;
            }
            var cursor = Instantiate(freeCursorPrefab) as IFreeCursor;
            cursor?.Initialize();
            return cursor;
        }
        #endregion
    }
}