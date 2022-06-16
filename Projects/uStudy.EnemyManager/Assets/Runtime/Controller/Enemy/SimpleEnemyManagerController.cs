using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Enemy
{
    public class SimpleEnemyManagerController : MonoBehaviour, IEnemyManager
    {
        [SerializeField]
        GameObject enemyPrefab;

        List<IEnemy> _enemies = new List<IEnemy>();

        void OnEnemyAttacked(DamageEvent e) {
            Debug.Log($"onAttacked: {e.enemy.Name}, {e.damage}");
        }

        void OnEnemyDeath(IEnemy enemy) {
            Debug.Log($"onDeath: {enemy.Name}");
        }

        void AddEnemy(IEnemy enemy) {
            _enemies.Add(enemy);
            enemy.OnAttacked.Subscribe(OnEnemyAttacked).AddTo(this);
            enemy.OnDeath.Subscribe(OnEnemyDeath).AddTo(this);

            var ec = enemy as IEnemyControl;
            if(ec!=null) {
                ec.SetHealth(100);
            }
        }

        public void Awake()
        {
            var children = transform.GetComponentsInChildren<SimpleEnemyController>();
            foreach(var enemy in children) {
                AddEnemy(enemy);
            }

            Debug.Log($"enemies: {_enemies.Select(e => e.Name).Aggregate((a, b) => $"{a},{b}")}");
        }

        #region IEnemyManager

        public IReadOnlyCollection<IEnemy> Enemies { get => _enemies; }

        #endregion
    }
}
