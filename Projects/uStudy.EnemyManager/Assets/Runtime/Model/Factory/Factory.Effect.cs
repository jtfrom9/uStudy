#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    public sealed partial class Factory : IEffectFactory
    {
        [Header("Effect Settings")]

        [SerializeField, InterfaceType(typeof(IDamageEffect))]
        Component? damageEffectPrefab;

        [SerializeField, InterfaceType(typeof(IHitEffect))]
        Component? hitEffectPrefab;

        [SerializeField, InterfaceType(typeof(IHitEffect))]
        Component? environmentHitPrefab;

        [SerializeField]
        DamageEffectParameter? damageEffectParameter;

        public IDamageEffect? CreateDamageEffect(ITransformProvider parent, int damage)
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

        public IHitEffect? CreateHitEffect(ITransformProvider parent, Vector3 position, Vector3 direction)
        {
            if (hitEffectPrefab == null)
            {
                return null;
            }
            var effect = Instantiate(hitEffectPrefab) as IHitEffect;
            effect?.Initialize(parent,
                position,
                direction);
            return effect;
        }

        public IHitEffect? CreateEnvironmentHitEffect(ITransformProvider parent, Vector3 position, Vector3 direction)
        {
            if (environmentHitPrefab == null)
            {
                return null;
            }
            var effect = Instantiate(environmentHitPrefab) as IHitEffect;
            effect?.Initialize(parent, position, direction);
            return effect;
        }
    }
}