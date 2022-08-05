#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Search;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Enemy", fileName = "Enemy")]
    public class EnemyObject : ScriptableObject
    {
        [SerializeField, SearchContext("t:prefab Enemy")]
        public GameObject? prefab;

        [SerializeField]
        public int MaxHealth;

        [SerializeField]
        public int Attack;

        [SerializeField]
        public int Deffence;

        public IEnemy? Create(IEnemyEvent enemyEvent, ICursorFactory cursorFactory, Vector3? position)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab);
            var enemyController = go.GetComponent<IEnemyController>();
            if (enemyController == null)
            {
                Destroy(go);
                return null;
            }
            var cursor = cursorFactory.CreateTargetCusor(enemyController, enemyController.GetProperty());
            if (cursor == null)
            {
                Destroy(go);
                return null;
            }
            var enemy = new EnemyImpl(this, enemyController, enemyEvent, cursor);
            enemyController.Initialize(enemy, position);
            return enemy;
        }
    }
}