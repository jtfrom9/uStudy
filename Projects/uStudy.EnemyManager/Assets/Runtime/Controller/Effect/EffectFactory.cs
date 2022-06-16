using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public class EffectFactory : MonoBehaviour, IEffectFactory
    {
        [SerializeField]
        GameObject effectPrefab;

        [SerializeField]
        Transform gazeTarget;

        #region  IEffectFactory
        public IDamageEffect CreateDamageEffect(Transform parent, Vector3 pos, int damage)
        {
            var effect = Instantiate(effectPrefab).GetComponent<IDamageEffect>();
            effect.Initialize(parent,
                pos,
                gazeTarget != null ? gazeTarget : Camera.main.transform,
                damage);
            return effect;
        }
        #endregion
    }
}