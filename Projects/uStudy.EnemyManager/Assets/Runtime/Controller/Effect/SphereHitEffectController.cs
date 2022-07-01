#nullable enable

using System;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Hedwig.Runtime
{
    public class SphereHitEffectController : MonoBehaviour, IHitEffect
    {
        #region IEffect
        UniTask IEffect.Play()
        {
            return transform.DOShakeScale(1f, 2f, 20, 1, true).ToUniTask();
        }
        #endregion

        #region IHitEffect
        void IHitEffect.Initialize(IMobileObject parent, Vector3 position, Vector3 normal) {
            transform.position = position;
            transform.SetParent(parent.transform);
        }
        #endregion

        void IDisposable.Dispose() {
            Destroy(gameObject);
        }
    }
}