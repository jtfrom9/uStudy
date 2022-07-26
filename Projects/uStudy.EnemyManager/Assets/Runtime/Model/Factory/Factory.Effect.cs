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
    }
}