#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

using UniRx;
using UniRx.Triggers;
using VContainer;
using VContainer.Unity;

namespace Hedwig.Runtime
{
    public class KnockbackTest : LifetimeScope
    {
        [SerializeField]
        Factory? setting;

        [SerializeField]
        EnemyManagerObject? enemyManagerObject;

        [SerializeField]
        List<ProjectileObject> projectileObjects = new List<ProjectileObject>();

        [SerializeField]
        TextMeshProUGUI? textMesh;

        [Inject] IEnemyManager? enemyManager;
        [Inject] ILauncher? launcher;
        [Inject] IProjectileFactory? projectileFactory;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<Factory>(setting!)
                .AsImplementedInterfaces();
            builder.RegisterInstance<EnemyManagerObject>(enemyManagerObject!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManagerImpl>(Lifetime.Singleton);
            builder.Register<ILauncher, LauncherImpl>(Lifetime.Singleton);
            builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
        }

        void Start()
        {
            Assert.IsNotNull(enemyManager);
            Assert.IsNotNull(launcher);
            Assert.IsNotNull(projectileFactory);
            Assert.IsNotNull(textMesh);
            _start(enemyManager!, launcher!, projectileFactory!);
        }

        void _start(IEnemyManager enemyManager, ILauncher launcher, IProjectileFactory projectileFactory)
        {
            enemyManager.Initialize();
            launcher.Initialize();

            var configSelection = new Selection<ProjectileObject>(projectileObjects);
            configSelection.OnCurrentChanged.Subscribe(config =>
            {
                launcher.SetProjectile(config);
                updateText(config);
            }).AddTo(this);
            configSelection.Select(0);

            var enemy = enemyManager.Enemies[0];
            enemy.SetDestination(Vector3.zero);
            launcher.SetTarget(enemy.controller);

            setupKey(configSelection, launcher, enemy);
        }

        void setupKey(Selection<ProjectileObject> configSelection, ILauncher launcher, IEnemy enemy)
        {
            this.UpdateAsObservable().Subscribe(_ =>
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    configSelection.Prev();
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    configSelection.Next();
                }
                if(Input.GetKey(KeyCode.Space))
                {
                    if(launcher.CanFire.Value)
                        launcher.Fire();
                }
                if (Input.GetKey(KeyCode.Escape))
                {
                    enemy.ResetPos();
                }
            }).AddTo(this);
        }

        void updateText(ProjectileObject config)
        {
            if (textMesh != null)
            {
                if (config.weaponData != null)
                {
                    textMesh.text = $"{config.name}: attack: {config.weaponData.attack}, power: {config.weaponData.power}";
                }
            }
        }
    }
}