#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniRx.Triggers;

using Hedwig.Runtime;

public class TowerAim : LifetimeScope
{
    [SerializeField] Setting? setting;
    [SerializeField] ProjectileConfig? shot;
    [SerializeField] ProjectileConfig? bomb;

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
        enemyManager.Setup();

        if(launcher==null) return;

        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();

        launcher.SetTarget(enemyManager.Enemies[0]);
        launcher.SetProjectileConfig(shot);
        launcher.ShowTrajectory(true);
    }
}
