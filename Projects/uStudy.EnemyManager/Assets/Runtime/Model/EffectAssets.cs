#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/EffectAssets", fileName = "EffectAssets")]
    public class EffectAssets : ScriptableObject, IEffectFactory
    {
        [SerializeField, InterfaceType(typeof(IDamageEffect))]
        Component? damageEffectPrefab;

        [SerializeField, InterfaceType(typeof(IHitEffect))]
        Component? hitEffectPrefab;

        [SerializeField]
        DamageEffectParameter? damageEffectParameter;

        #region  IEffectFactory
        public IDamageEffect? CreateDamageEffect(Transform parent, int damage)
        {
            if(damageEffectPrefab ==null) {
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
            if(hitEffectPrefab==null) {
                return null;
            }
            var effect = Instantiate(hitEffectPrefab) as IHitEffect;
            effect?.Initialize(parent,
                position,
                normal);
            return effect;
        }
        #endregion
    }
}