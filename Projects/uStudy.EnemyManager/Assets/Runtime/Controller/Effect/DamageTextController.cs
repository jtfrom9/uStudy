using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public class DamageTextController : MonoBehaviour, IEffect
    {
        int damage = 0;
        TextMeshPro tmp;

        UniTask t1;
        UniTask t2;

        void Awake() {
            tmp = GetComponentInChildren<TextMeshPro>();
        }

        void play()
        {
            var token = this.GetCancellationTokenOnDestroy();
            t1 = transform.DOLocalMoveY(1, 0.5f).ToUniTask(cancellationToken: token);
            t2 = DOTween.To(
                () => 255,
                (value) =>
                {
                    tmp.text = $"<alpha=#{(int)value:X02}>{damage}";
                },
                0, 0.5f).ToUniTask(cancellationToken: token);
        }

        #region IEffect
        public void Initialize(Vector3 pos, Vector3 lookat)
        {
            transform.position = pos;
            transform.LookAt(lookat);
        }
        public UniTask Play()
        {
            play();
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
