using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public class EffectFactory : MonoBehaviour, IEffectFactory
    {
        [SerializeField]
        GameObject damageEffectPrefab;

        [SerializeField]
        GameObject hitEffectPrefab;

        [SerializeField]
        Transform gazeTarget;

        [SerializeField]
        DamageEffectParameter damageEffect;

        #region  IEffectFactory
        public IDamageEffect CreateDamageEffect(Transform parent, int damage)
        {
            var effect = Instantiate(damageEffectPrefab).GetComponent<IDamageEffect>();
            effect.Initialize(parent,
                gazeTarget != null ? gazeTarget : Camera.main.transform,
                damageEffect,
                damage);
            return effect;
        }

        public IHitEffect CreateHitEffect(Transform parent, Vector3 position, Vector3 normal)
        {
            var effect = Instantiate(hitEffectPrefab).GetComponent<IHitEffect>();
            effect.Initialize(parent,
                gazeTarget != null ? gazeTarget : Camera.main.transform,
                position,
                normal);
            return effect;
        }
        #endregion
    }
}