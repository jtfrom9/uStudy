using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class SimpleEnemyManagerController : MonoBehaviour, IEnemyManager
    {
        [SerializeField]
        GameObject enemyPrefab;

        List<IEnemy> _enemies = new List<IEnemy>();
        IEffectFactory effectFactory;

        void OnEnemyAttacked(DamageEvent e) {
            Debug.Log($"onAttacked: {e.enemy.Name}, {e.damage}");
            if (effectFactory != null)
            {
                var effect = effectFactory.CreateDamageEffect(
                    e.enemy.transform,
                    e.damage);
                effect.Play().Forget();
            }
        }

        async void OnEnemyDeath(IEnemy enemy) {
            Debug.Log($"onDeath: {enemy.Name}");

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            _enemies.Remove(enemy);
            enemy.Dispose();
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
        }

        #region IEnemyManager

        public IReadOnlyList<IEnemy> Enemies { get => _enemies; }

        public void SetEffectFactory(IEffectFactory factory) {
            this.effectFactory = factory;
        }

        #endregion
    }
}
