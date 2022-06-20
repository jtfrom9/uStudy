#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

using Hedwig.Runtime;

public class TowerAim : LifetimeScope
{
    [SerializeField]
    Transform? launcherPoint;

    [Inject] IEnemyManager? enemyManager;

    LineRenderer? lr;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IEffectFactory, DummyEffectFactory>(Lifetime.Singleton);
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<ISelectorFactory, DummySelectorFactory>(Lifetime.Singleton);
    }

    void Start()
    {
        if (enemyManager == null)
        {
            Debug.LogError($"enemyManager: {enemyManager}");
            return;
        }
        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();
        enemyManager.SelectExclusive(0);

        TryGetComponent(out lr);
    }

    void Update()
    {
        if(launcherPoint==null || enemyManager==null) {
            return;
        }
        var i = enemyManager.SelectedIndex();
        Debug.Log(i);
        var cur = enemyManager.Selected();
        Debug.Log(cur);
        if(cur==null) return;
        // Debug.DrawLine(launcherPoint.position, cur.transform.position, Color.red, 100);

        if (lr != null)
        {
            lr.SetPositions(new Vector3[] { launcherPoint.position, cur.transform.position });
            // lr.startWidth = 0.01f;
            // lr.endWidth = 0.01f;
        }
    }
}
