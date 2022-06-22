#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName ="Hedwig/Setting", fileName ="HedwigRuntimeSetting")]
    public class Setting : ScriptableObject, IProjectileFactory, IEffectFactory, ISelectorFactory
    {
        #region  IProjectileFactory
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
        #endregion

        #region  IEffectFactory
        [SerializeField, InterfaceType(typeof(IDamageEffect))]
        Component? damageEffectPrefab;

        [SerializeField, InterfaceType(typeof(IHitEffect))]
        Component? hitEffectPrefab;

        [SerializeField]
        DamageEffectParameter? damageEffectParameter;

        public IDamageEffect? CreateDamageEffect(Transform parent, int damage)
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

        public IHitEffect? CreateHitEffect(Transform parent, Vector3 position, Vector3 normal)
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

        #region ISelectorFactory
        [SerializeField, InterfaceType(typeof(ISelector))]
        Component? selectorPrefab;

        ISelector? ISelectorFactory.Create(ICharactor charactor)
        {
            if (selectorPrefab == null)
            {
                return null;
            }
            var selector = Instantiate(selectorPrefab) as ISelector;
            selector?.Initialize(charactor.transform, charactor.distanceToGround);
            return selector;
        }
        #endregion
    }
}