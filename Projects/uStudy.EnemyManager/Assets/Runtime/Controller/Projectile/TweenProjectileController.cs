#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class TweenProjectileController : MonoBehaviour, IProjectile
    {
        Transform? target;
        float duration;
        bool active = true;

        #region IProjectile
        void IProjectile.Initialize(Vector3 initial, Transform target, float duration)
        {
            transform.position = initial;
            this.target = target;
            this.duration = duration;
        }

        async void IProjectile.Go()
        {
            if(this.target==null) return;

            // var start = transform.position;
            // var end = target.position;
            // var dir = end - start;
            // var p1 = start + dir.normalized * (dir.magnitude / 3);
            // var tween = transform.DOPath(new Vector3[]{
            //     p1 + Vector3.up,
            //     end
            // }, duration, PathType.CatmullRom)
            //     .SetSpeedBased(true)
            //     .SetEase(Ease.Linear);

            // await tween;
            // for (var i = 0; i < 3; i++) {
            //     await transform.DOMove(target!.position, duration)
            //         .SetSpeedBased(true)
            //         .SetEase(Ease.Linear);
            // }

            int max = (int)(duration);
            float speed = 5.0f;

            for (var i = 0; i < max; i++) {
                var start = transform.position;
                var end = target.position;
                var dir = end - start;
                // Debug.Log($"{dir.normalized * speed}");
                await transform.DOMove(dir.normalized * speed, 1.0f)
                    .SetRelative(true)
                    .SetEase(Ease.Linear)
                    .ToUniTask();
                if(!active) break;

                // await UniTask.Delay(1000);

                // transform.DOKill();

                // await t;
                // Debug.Log($"done. {transform.position}");
            }

            var reason = active ? "over" : "collision";
            Debug.Log($"Projectile: {reason}");
            Destroy(gameObject);

            // tween.OnUpdate(() =>
            //     {
            //         Debug.Log("");
            //     });
            // .OnComplete(() => Dispose());

        }
        #endregion

        #region IDisposable
        void System.IDisposable.Dispose()
        {
            Destroy(gameObject);
        }
        #endregion

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Character"))
            {
                // Debug.Log($"{collision.gameObject.name}");
                if (DOTween.IsTweening(transform))
                {
                    transform.DOKill();
                    active = false;
                    // Destroy(gameObject);
                }
            }
        }
    }
}