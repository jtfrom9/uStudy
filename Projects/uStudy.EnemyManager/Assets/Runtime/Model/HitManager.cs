#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class HitManager : IHitManager
    {
        IProjectileFactory projectileFactory;
        IEnemyManager enemyManager;

        CompositeDisposable disposables = new CompositeDisposable();

        void IDisposable.Dispose()
        {
            disposables.Dispose();
        }

        void IHitManager.Setup(MonoBehaviour monoBehaviour)
        {
            monoBehaviour.OnDestroyAsObservable().Subscribe(_ =>
            {
                (this as IDisposable).Dispose();
            });
        }

        async UniTask raiseHitEvent(GameObject gameObject, IProjectile projectile, Vector3 position)
        {
            var mobileObject = gameObject.GetComponent<IMobileObject>();
            if (mobileObject != null)
            {
                await UniTask.NextFrame();
                mobileObject.OnHit(projectile, position);
                projectile.OnHit(mobileObject, position);
            }
        }

        async UniTask hitHandler(IProjectile projectile, RaycastHit hit)
        {
            var gameObject = hit.collider.gameObject;
            if (gameObject.CompareTag(HitTag.CharacterT) ||
                gameObject.CompareTag(HitTag.Environment))
            {
                await raiseHitEvent(gameObject, projectile, hit.point);
            }
        }

        void handleProjectileHit(IProjectile projectile)
        {
            // var hits = Physics.RaycastAll(pos, dir, speed * Time.deltaTime );
            // if(hits.Length > 0) {
            //     Debug.Log($"hits: {hits.Length}");
            //     RaycastHit? nearest = null;
            //     foreach(var hit in hits) {
            //         if (!nearest.HasValue) { nearest = hit; }
            //         else {
            //             if(nearest.Value.distance > hit.distance)
            //                 nearest = hit;
            //         }
            //     }
            //     hitHandler(nearest!.Value);
            // }

            var hit = new RaycastHit();
            var origin = projectile.transform.Position;
            var direction = projectile.diretion;
            var distance = projectile.speed * Time.deltaTime;
            if (Physics.Raycast(origin, direction, out hit, distance))
            {
                hitHandler(projectile, hit).Forget();
            }
        }

        public HitManager(IProjectileFactory projectileFactory, IEnemyManager enemyManager)
        {
            this.projectileFactory = projectileFactory;
            this.enemyManager = enemyManager;

            this.projectileFactory.OnCreated.Subscribe(projectile =>
            {
                projectile.OnUpdate.Subscribe(data =>
                {
                    handleProjectileHit(projectile);
                }).AddTo(disposables);
            }).AddTo(disposables);
        }
    }
}