#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class ProjectileTest : LifetimeScope
    {
        [SerializeField]
        SelectorAssets? selectorAssets;

        [SerializeField]
        ProjectileAssets? projectileAssets;

        [SerializeField, InterfaceType(typeof(ILauncher))]
        BasicLauncher? basicLauncher;

        [Inject] IEnemyManager? enemyManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEffectFactory, DummyEffectFactory>(Lifetime.Singleton);
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.RegisterInstance<ISelectorFactory>(selectorAssets!);
            builder.RegisterInstance<IProjectileFactory>(projectileAssets!);
        }

        void Start()
        {
            if(enemyManager==null) return;
            var launcher = basicLauncher as ILauncher;
            if (launcher == null) return;

            var token = this.GetCancellationTokenOnDestroy();
            var selection = new SingleSelection(enemyManager.Enemies);
            selection.SelectExclusive(0);

            enemyManager.RandomWalk(-30, 30, 3000, token).Forget();

            this.UpdateAsObservable().Subscribe(_ => {
                var enemy = selection.Current as IEnemy;
                if (enemy == null) return;
                _update(launcher, enemy);
            }).AddTo(this);
        }

        void _update(ILauncher launcher, IEnemy enemy)
        {
            launcher.Aim(enemy.transform);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                launcher.Launch();
            }
        }

        // async void Stop() 
        // {
        //     if(tweener==null) return;
        //     var x = tweener.Pause();
        //     Debug.Log(tweener.IsPlaying());
        //     await UniTask.Delay(1000);
        //     x = tweener.Play();
        //     Debug.Log(tweener.IsPlaying());
        // }

        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         Shot();
        //     }
        //     if(Input.GetKeyDown(KeyCode.LeftShift))
        //     {
        //         Stop();
        //     }
        // }
    }

}