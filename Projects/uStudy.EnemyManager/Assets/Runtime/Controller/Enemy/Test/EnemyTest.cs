using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using VContainer;
using VContainer.Unity;

using Hedwig.Runtime;

public class EnemyTest : LifetimeScope
{
    [SerializeField]
    EffectAssets effectAssets;

    [SerializeField]
    Text text;

    [Inject]
    IEnemyManager enemyManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<IEffectFactory>(effectAssets);
        builder.Register<ISelectorFactory, DummySelectorFactory>(Lifetime.Singleton);
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
    }

    async UniTaskVoid RnadomMoveEnemy()
    {
        while (enemyManager.Enemies.Count > 0)
        {
            foreach (var enemy in enemyManager.Enemies)
            {
                var x = Random.Range(-30f, 30f);
                var z = Random.Range(-30f, 30f);
                enemy.SetDestination(new Vector3(x, 0, z));
            }
            await UniTask.Delay(3000);
        }
    }

    async UniTaskVoid RandomAttach()
    {
        while (enemyManager.Enemies.Count > 0)
        {
            await UniTask.Delay(1000);
            foreach (var enemy in enemyManager.Enemies)
            {
                enemy.Attacked(Random.Range(1, 10));
            }
        }
    }

    void Start()
    {
        RnadomMoveEnemy().Forget();
        RandomAttach().Forget();

        Camera.main.transform.position = new Vector3(0, 1, -20);
        Camera.main.transform.DORotateAround(Vector3.zero, Vector3.up, 360, 10)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        this.UpdateAsObservable().Subscribe(_ =>
        {
            text.text = $"# of enemy: {enemyManager.Enemies.Count}";
            foreach(var e in enemyManager.Enemies) {
                text.text += $"\n {e.Name}: {e.Health}";
            }

            if (enemyManager.Enemies.Count == 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit ();
#endif
            }
        }).AddTo(this);
    }
}
