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
        Setting? setting;

        [Inject] IEnemyManager? enemyManager;
        [Inject] Launcher? launcher;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<Setting>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.Register<Launcher>(Lifetime.Singleton);
        }

        void Start()
        {
            if (enemyManager == null) return;
            if (launcher == null) return;

            var selection = new SingleSelection(enemyManager.Enemies);
            selection.onCurrentChanged.Subscribe(selectable =>
            {
                launcher.Aim(selectable as IEnemy);
            }).AddTo(this);

            selection.SelectExclusive(0);

            this.UpdateAsObservable().Subscribe(_ =>
            {
                var enemy = selection.Current as IEnemy;
                if (enemy == null) return;
                _update(launcher, enemy, selection);
            }).AddTo(this);

            var token = this.GetCancellationTokenOnDestroy();
            enemyManager.RandomWalk(-30, 30, 3000, token).Forget();
        }

        void _update(Launcher launcher, IEnemy enemy, SingleSelection selection)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(launcher.CanLaunch)
                    launcher.Launch(10);
            }
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                selection.Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selection.Prev();
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