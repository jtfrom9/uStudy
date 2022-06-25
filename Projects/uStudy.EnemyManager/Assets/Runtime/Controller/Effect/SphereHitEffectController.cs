using System.Collections;
using System.Collections.Generic;
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

        void System.IDisposable.Dispose() {
            Destroy(this.gameObject);
        }

        // async void Start()
        // {
        //     await (this as IEffect).Play();
        //     Debug.Log("done");
        // }
    }
}