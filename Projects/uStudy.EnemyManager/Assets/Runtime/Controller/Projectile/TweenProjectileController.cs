#nullable enable

using System.Threading;
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

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        // void OnCollisionEnter(Collision collision)
        // {
        //     if (collision.gameObject.CompareTag("Character"))
        //     {
        //         if (DOTween.IsTweening(transform))
        //         {
        //             transform.DOKill();
        //             active = false;
        //             // Destroy(gameObject);
        //         }
        //     }
        // }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Character") || other.gameObject.CompareTag("Environment"))
            {
                if (DOTween.IsTweening(transform))
                {
                    tokenSource.Cancel();
                }
            }
        }

        async UniTaskVoid go()
        {
            if (this.target == null) return;

            int max = (int)(duration);
            float speed = 10.0f;

            Vector3 prevDir = Vector3.zero;
            for (var i = 0; i < max; i++)
            {
                var start = transform.position;
                var end = target.position + target.up * Random.Range(-0.3f, 0.3f) + target.right * Random.Range(-0.3f, 0.3f);
                var dir = end - start;
                if (prevDir != Vector3.zero)
                {
                    var angle = Vector3.Angle(dir, prevDir);
                    if (angle > 45)
                    {
                        var cross = Vector3.Cross(dir, prevDir);
                        dir = Quaternion.AngleAxis(-45, cross) * prevDir;

                        var recalc = Vector3.Angle(dir, prevDir);
                        if (recalc > 45)
                        {
                            Debug.Log($"angle: {recalc}");
                        }
                    }
                }
                prevDir = dir;

                await transform.DOMove(dir.normalized * speed, 1.0f)
                    .SetRelative(true)
                    .SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: tokenSource.Token);
                if (tokenSource.IsCancellationRequested)
                    break;
            }
            Destroy(gameObject);
        }

        #region IProjectile
        void IProjectile.Initialize(Vector3 initial, Transform target, float duration)
        {
            transform.position = initial;
            this.target = target;
            this.duration = duration;
        }

        void IProjectile.Go()
        {
            go().Forget();
        }
        #endregion

        #region IDisposable
        void System.IDisposable.Dispose()
        {
            transform.DOKill();
            Destroy(gameObject);
        }
        #endregion

    }
}