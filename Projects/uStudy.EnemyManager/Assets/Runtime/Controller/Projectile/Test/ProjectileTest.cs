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
        ProjectileAssets? projectileAssets;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEffectFactory, DummyEffectFactory>(Lifetime.Singleton);
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.Register<ISelectorFactory, DummySelectorFactory>(Lifetime.Scoped);
            builder.RegisterInstance<IProjectileFactory>(projectileAssets!);
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