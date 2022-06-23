#nullable enable

using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        [SerializeField]
        List<ProjectileConfig> projectileConfigs = new List<ProjectileConfig>();

        int projectileConfigIndex = -1;

        [SerializeField]
        Button? moveButton;

        [Inject] IEnemyManager? enemyManager;
        [Inject] Launcher? launcher;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<Setting>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.Register<Launcher>(Lifetime.Singleton);
        }

        void setupUI(IEnemyManager enemyManager)
        {
            var tmp = moveButton?.GetComponentInChildren<TextMeshProUGUI>();
            var move = false;
            // var token = this.GetCancellationTokenOnDestroy();
            var cts = new CancellationTokenSource();
            moveButton?.OnClickAsObservable().Subscribe(_ => {
                move = !move;
                tmp!.text = (move) ? "Stop" : "Move";
                if (move)
                {
                    enemyManager.RandomWalk(-30, 30, 3000, cts.Token).Forget();
                } else {
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                }
            }).AddTo(this);
        }

        void Start()
        {
            if (enemyManager == null) return;
            if (launcher == null) return;

            setupUI(enemyManager);

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

            this.projectileConfigIndex = projectileConfigs.Count - 1;
        }

        void _update(Launcher launcher, IEnemy enemy, SingleSelection selection)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (launcher.CanLaunch && projectileConfigIndex >=0)
                {
                    var config = projectileConfigs[projectileConfigIndex];
                    launcher.Launch(config);
                }
            }
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                selection.Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selection.Prev();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (projectileConfigIndex < 0) return;
                projectileConfigIndex++;
                if (projectileConfigIndex == projectileConfigs.Count - 1) projectileConfigIndex = 0;
            }
            if(Input.GetKeyDown(KeyCode.UpArrow)) 
            {
                if (projectileConfigIndex < 0) return;
                projectileConfigIndex--;
                if (projectileConfigIndex < 0) projectileConfigIndex = projectileConfigs.Count - 1;
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