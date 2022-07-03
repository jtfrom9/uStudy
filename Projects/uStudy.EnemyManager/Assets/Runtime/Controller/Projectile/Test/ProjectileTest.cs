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

        [Inject] IEnemyManager? enemyManager;
        [Inject] ILauncherManager? launcher;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<Setting>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.Register<ILauncherManager, LauncherManager>(Lifetime.Singleton);
            builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
        }

        void Start()
        {
            if (enemyManager == null) return;
            enemyManager.Setup();
            if (launcher == null) return;

            var enemySelection = new SelectiveSelection(enemyManager.Enemies);
            enemySelection.OnCurrentChanged.Subscribe(selectable =>
            {
                launcher.SetTarget(selectable as IEnemy);
                launcher.ShowTrajectory(true);
            }).AddTo(this);

            enemySelection.SelectExclusive(0);

            var configSelection = new Selection<ProjectileConfig>(projectileConfigs);
            configSelection.OnCurrentChanged.Subscribe(config =>
            {
                launcher.SetProjectileConfig(config);
            }).AddTo(this);
            configSelection.Select(0);

            this.UpdateAsObservable().Subscribe(_ =>
            {
                var enemy = enemySelection.Current as IEnemy;
                if (enemy == null) return;
                _update(launcher, enemy, enemySelection, configSelection);
            }).AddTo(this);

            launcher.CanFire.Subscribe(can =>
            {
                Debug.Log($"can: {can}");
                if (can)
                {
                    if (launcher.config!.type == ProjectileType.Fire)
                    {
                        launcher.Fire();
                    }
                }
            }).AddTo(this);
        }

        void _update(ILauncherManager launcher, IEnemy enemy, SelectiveSelection enemySelection, Selection<ProjectileConfig> configSelection)
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     if (launcher.CanFire)
            //     {
            //         launcher.Fire();
            //     }
            // }
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                enemySelection.Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                enemySelection.Prev();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                configSelection.Next();
            }
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                configSelection.Prev();
            }
        }
    }
}