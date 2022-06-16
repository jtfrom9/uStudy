using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Effect
{
    public class HitEffectController : MonoBehaviour, IHitEffect
    {
        [SerializeField]
        ParticleSystem _particleSystem;

        Transform gazeTarget;
        EffectCompleteHandler completeHandler;

        class EffectCompleteHandler : MonoBehaviour {
            void OnParticleSystemStopped()
            {
                onComplete.OnNext(Unit.Default);
            }
            public Subject<Unit> onComplete = new Subject<Unit>();
        }

        void Awake()
        {
            completeHandler = _particleSystem.gameObject.AddComponent<EffectCompleteHandler>();
        }

        #region IDamageEffect
        public void Initialize(Transform parent, Transform gazeTarget, Vector3 position, Vector3 normal)
        {
            this.gazeTarget = gazeTarget;
            transform.position = position;
            transform.SetParent(parent, true);
        }

        public UniTask Play()
        {
            _particleSystem?.Play();
            return completeHandler.onComplete.ToUniTask(true);
        }
        #endregion

        #region IDisposable
        public void Dispose() 
        {
            Destroy(gameObject);
        }
        #endregion

    }
}
