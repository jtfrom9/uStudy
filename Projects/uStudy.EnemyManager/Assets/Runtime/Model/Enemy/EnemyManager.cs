#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Hedwig.Runtime
{
    public class EnemyManager: IEnemyManager
    {
        List<IEnemy> _enemies = new List<IEnemy>();
        CompositeDisposable disposable = new CompositeDisposable();
        Subject<IEnemy> onCreated = new Subject<IEnemy>();

        IEffectFactory effectFactory;
        ICursorFactory selectorFactory;

        void OnEnemyAttacked(DamageEvent e)
        {
            // Debug.Log($"onAttacked: {e.enemy.Name}, {e.damage}");
            // var effect = effectFactory.CreateDamageEffect(
            //     e.enemy.transform,
            //     e.damage);
            // effect.Play().Forget();
            var effects = new IEffect?[] {
                    effectFactory.CreateDamageEffect(
                        e.enemy,
                        e.damage),
                    effectFactory.CreateHitEffect(
                        e.enemy,
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
            Debug.Log($"onDeath: {enemy}");

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

            _enemies.Remove(enemy);
            enemy.Dispose();
        }

        void addEnemy(IEnemy enemy) {
            _enemies.Add(enemy);
            enemy.OnAttacked.Subscribe(OnEnemyAttacked).AddTo(disposable);
            enemy.OnDeath.Subscribe(OnEnemyDeath).AddTo(disposable);

            var ctrl = enemy.GetControl();
            ctrl.SetHealth(100);
            ctrl.SetSelector(selectorFactory.CreateTargetCusor(enemy));

            onCreated.OnNext(enemy);
        }

        // ctor
        public EnemyManager(IEffectFactory effectFactory, ICursorFactory selectorFactory)
        {
            this.effectFactory = effectFactory;
            this.selectorFactory = selectorFactory;
        }

        #region IEnemyManager

        IReadOnlyList<IEnemy> IEnemyManager.Enemies { get => _enemies; }

        void IEnemyManager.Initialize()
        {
            var enemyRepository = Controller.Find<IEnemyRepository>();
            if (enemyRepository != null)
            {
                foreach (var enemy in enemyRepository.GetEnemies())
                {
                    addEnemy(enemy);
                }
            }
        }

        void IEnemyManager.AddEnemy(IEnemy enemy) => addEnemy(enemy);

        ISubject<IEnemy> IEnemyManager.OnCreated { get => onCreated; }
        #endregion

        #region IDisposable
        void IDisposable.Dispose() {
            this.disposable.Dispose();
        }
        #endregion
    }
}