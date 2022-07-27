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
        ICursorFactory cursorFactory;

        void OnEnemyAttacked(DamageEvent e)
        {
            Debug.Log($"onAttacked: {e.enemy}, {e.damage}, {e.enemy.Health}");
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

        void addEnemy(IEnemyController enemyController)
        {
            var def = ScriptableObject.CreateInstance<EnemyDef>();
            def.MaxHealth = 100;
            def.Deffence = 0;
            def.Attack = 0;
            var cursor = cursorFactory.CreateTargetCusor(enemyController);
            if (cursor == null)
            {
                return;
            }
            var enemy = new EnemyImpl(def, enemyController, cursor);
            enemyController.Initialize(enemy);

            _enemies.Add(enemy);

            enemy.OnAttacked.Subscribe(OnEnemyAttacked).AddTo(disposable);
            enemy.OnDeath.Subscribe(OnEnemyDeath).AddTo(disposable);

            onCreated.OnNext(enemy);
        }

        // ctor
        public EnemyManager(IEffectFactory effectFactory, ICursorFactory cursorFactory)
        {
            this.effectFactory = effectFactory;
            this.cursorFactory = cursorFactory;
        }

        #region IEnemyManager

        IReadOnlyList<IEnemy> IEnemyManager.Enemies { get => _enemies; }

        void IEnemyManager.Initialize()
        {
            var enemyRepository = Controller.Find<IEnemyControllerRepository>();
            if (enemyRepository != null)
            {
                foreach (var enemyController in enemyRepository.GetEnemyController())
                {
                    addEnemy(enemyController);
                }
            }
        }

        ISubject<IEnemy> IEnemyManager.OnCreated { get => onCreated; }
        #endregion

        #region IDisposable
        void IDisposable.Dispose() {
            this.disposable.Dispose();
        }
        #endregion
    }
}