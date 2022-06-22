#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class EnemyManager: IEnemyManager
    {
        List<IEnemy> _enemies = new List<IEnemy>();
        CompositeDisposable disposable = new CompositeDisposable();

        IEffectFactory effectFactory;
        ISelectorFactory selectorFactory;

        void OnEnemyAttacked(DamageEvent e)
        {
            Debug.Log($"onAttacked: {e.enemy.Name}, {e.damage}");
            // var effect = effectFactory.CreateDamageEffect(
            //     e.enemy.transform,
            //     e.damage);
            // effect.Play().Forget();
            var effects = new IEffect?[] {
                    effectFactory.CreateDamageEffect(
                        e.enemy.transform,
                        e.damage),
                    effectFactory.CreateHitEffect(
                        e.enemy.transform,
                        e.position,
                        Vector3.zero)
                };
            foreach (var effect in effects)
            {
                effect?.PlayAndDispose().Forget();
            }
        }

        async void OnEnemyDeath(IEnemy enemy)
        {
            Debug.Log($"onDeath: {enemy.Name}");

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            _enemies.Remove(enemy);
            enemy.Dispose();
        }

        // ctor
        public EnemyManager(IEffectFactory effectFactory, ISelectorFactory selectorFactory)
        {
            this.effectFactory = effectFactory;
            this.selectorFactory = selectorFactory;

            var enemyRepository = Controller.Find<IEnemyRepository>();
            if (enemyRepository != null)
            {
                foreach (var enemy in enemyRepository.GetEnemies())
                {
                    this.AddEnemy(enemy);
                }
            }
        }

        #region IEnemyManager

        public IReadOnlyList<IEnemy> Enemies { get => _enemies; }

        public void AddEnemy(IEnemy enemy)
        {
            _enemies.Add(enemy);
            enemy.OnAttacked.Subscribe(OnEnemyAttacked).AddTo(disposable);
            enemy.OnDeath.Subscribe(OnEnemyDeath).AddTo(disposable);

            var ctrl = enemy.GetControl();
            ctrl.SetHealth(100);
            ctrl.SetSelector(selectorFactory.Create(enemy));
        }

        #endregion

        #region IDisposable

        public void Dispose() {
            this.disposable.Dispose();
        }

        #endregion
    }
}