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
    [SerializeField]
    Setting? setting;

    [SerializeField]
    Transform? launcherPoint;

    [Inject] IEnemyManager? enemyManager;

    LineRenderer? lr;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
    }

    void Start()
    {
        if (enemyManager == null)
        {
            return;
        }
        if (launcherPoint == null)
        {
            return;
        }
        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();
        var selection = new SingleSelection(enemyManager.Enemies);
        selection.SelectExclusive(0);

        TryGetComponent(out lr);

        this.UpdateAsObservable().Subscribe(_ =>
        {
            update(selection, launcherPoint);
        }).AddTo(this);
    }

    void update(SingleSelection selection, Transform launcherPoint)
    {
        var enemy = selection.Current as IEnemy;
        if (enemy == null) return;
        // Debug.DrawLine(launcherPoint.position, cur.transform.position, Color.red, 100);

        if (lr != null)
        {
            lr.SetPositions(new Vector3[] { launcherPoint.position, enemy.transform.position });
            // lr.startWidth = 0.01f;
            // lr.endWidth = 0.01f;
        }
    }

}
