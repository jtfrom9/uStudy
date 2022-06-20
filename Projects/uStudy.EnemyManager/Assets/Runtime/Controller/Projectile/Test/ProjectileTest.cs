#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

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
            var token = this.GetCancellationTokenOnDestroy();
            enemyManager.SelectExclusive(0);
            enemyManager.RandomWalk(-30, 30, 3000, token).Forget();
        }

        void Update()
        {
            var enemy = enemyManager?.Selected();
            if(enemy==null) return;

            var launcher = basicLauncher as ILauncher;
            if(launcher==null) return;
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