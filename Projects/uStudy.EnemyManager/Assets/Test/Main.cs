using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Enemy;

public class Main : MonoBehaviour
{
    [SerializeField]
    SimpleEnemyManagerController enemyManagerController;

    async UniTaskVoid RnadomMoveEnemy()
    {
        while (true)
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
        while (true)
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
    }
}
