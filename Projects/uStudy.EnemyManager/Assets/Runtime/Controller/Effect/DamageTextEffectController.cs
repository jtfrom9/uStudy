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
        int damage = 0;
        Transform gazeTarget;
        TextMeshPro tmp;
        UniTask t1;
        UniTask t2;

        void Awake() {
            tmp = GetComponentInChildren<TextMeshPro>();
        }

        void play(float duration)
        {
            var token = this.GetCancellationTokenOnDestroy();
            transform.DOLocalMoveY(1, duration).OnUpdate(() =>
            {
                transform.LookAt(gazeTarget);
            }).ToUniTask();

            t1 = transform.DOLocalMoveY(1, duration).ToUniTask(cancellationToken: token);
            t2 = DOTween.To(
                () => 255,
                (value) =>
                {
                    tmp.text = $"<alpha=#{(int)value:X02}>{damage}";
                },
                0, duration).ToUniTask(cancellationToken: token);
        }

        #region IDamageEffect
        public void Initialize(Transform parent, Vector3 pos, Transform gazeTarget, int damage)
        {
            this.damage = damage;
            this.gazeTarget = gazeTarget;
            transform.SetParent(parent, false);
        }
        public UniTask Play(float duration)
        {
            play(duration);
            return UniTask.WhenAll(t1, t2);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Destroy(this.gameObject);
        }
        #endregion
    }
}
