#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class EffectFactory : MonoBehaviour, IEffectFactory
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
            if(damageEffectParameter==null) {
                return null;
            }
            var effect = Instantiate(damageEffectPrefab) as IDamageEffect;
            effect?.Initialize(parent,
                damageEffectParameter,
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