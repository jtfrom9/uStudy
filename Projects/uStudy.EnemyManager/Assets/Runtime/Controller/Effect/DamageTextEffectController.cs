using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public class DamageTextEffectController : MonoBehaviour, IDamageEffect
    {
        Transform gazeTarget;
        float duration;
        int damage;
        TextMeshPro tmp;

        void Awake() {
            tmp = GetComponentInChildren<TextMeshPro>();
        }

        UniTask _play()
        {
            var token = this.GetCancellationTokenOnDestroy();

            // t1 = transform.DOLocalMoveY(1, duration).ToUniTask(cancellationToken: token);
            var t1 = transform.DOLocalMoveY(1, duration).OnUpdate(() =>
            {
                transform.LookAt(gazeTarget);
            }).ToUniTask(cancellationToken: token);

            var t2 = DOTween.To(
                () => 255,
                (value) =>
                {
                    tmp.text = $"<alpha=#{(int)value:X02}>{damage}";
                },
                0, duration).ToUniTask(cancellationToken: token);
            return UniTask.WhenAll(t1, t2);
        }

        #region IDamageEffect
        public void Initialize(Transform parent, Transform gazeTarget, DamageEffectParameter param, int damage)
        {
            this.gazeTarget = gazeTarget;
            this.duration = param.duration;
            this.damage = damage;
            transform.SetParent(parent, false);
        }
        public UniTask Play() => _play();
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Destroy(this.gameObject);
        }
        #endregion
    }
}
