using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Utility;

public class Main : MonoBehaviour
{
    [SerializeField]
    Enemy.SimpleEnemyManagerController enemyManagerController;

    [SerializeField]
    Effect.EffectFactory effectFactory;

    [SerializeField]
    Text text;

    async UniTaskVoid RnadomMoveEnemy()
    {
        while (enemyManagerController.Enemies.Count > 0)
        {
            foreach (var enemy in enemyManagerController.Enemies)
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
        while (enemyManagerController.Enemies.Count > 0)
        {
            await UniTask.Delay(1000);
            foreach (var enemy in enemyManagerController.Enemies)
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

        var em = enemyManagerController as Enemy.IEnemyManager;
        this.UpdateAsObservable().Subscribe(_ =>
        {
            text.text = $"# of enemy: {em.Enemies.Count}";
            foreach(var e in em.Enemies) {
                text.text += $"\n {e.Name}: {e.Health}";
            }

            if (em.Enemies.Count == 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit ();
#endif
            }
        }).AddTo(this);
    }

    void Awake()
    {
        var em = enemyManagerController as Enemy.IEnemyManager;
        em.SetEffectFactory(effectFactory);
    }
}
