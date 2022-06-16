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

        #region  IEffectFactory
        public IEffect Create(Vector3 pos, Vector3 lookat) {
            var effect = Instantiate(effectPrefab).GetComponent<IEffect>();
            effect.Initialize(pos, lookat);
            return effect;
        }
        #endregion
    }
}