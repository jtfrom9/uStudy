#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Hedwig.Runtime
{
    public class EnemyManager: IEnemyManager, IEnemyEvent
    {
        ReactiveCollection<IEnemy> _enemies = new ReactiveCollection<IEnemy>();
        CompositeDisposable disposable = new CompositeDisposable();

        IEnemyManagerConfig? enemyManagerConfig;
        IEffectFactory effectFactory;
        ICursorFactory cursorFactory;

        void equipHitTransformEffect(IEnemy enemy, IHitObject? hitObject, in DamageEvent e)
        {
            if (hitObject != null && e.actualDamage > 0)
            {
                enemy.controller.Knockback(hitObject.direction, hitObject.power);
            }
        }

        void equipHitVisualEffect(IEnemy enemy, IHitObject? hitObject, in DamageEvent e)
        {
            var effects = new IEffect?[]
            {
                effectFactory.CreateDamageEffect(enemy.controller, e.damage),
                effectFactory.CreateHitEffect(enemy.controller,
                    hitObject?.position ?? enemy.controller.transform.Position,
                    Vector3.zero)
            };
            foreach (var effect in effects)
            {
                effect?.PlayAndDispose().Forget();
            }
        }

        void onEnemyAttacked(IEnemy enemy, IHitObject? hitObject, in DamageEvent damageEvent)
        {
            equipHitVisualEffect(enemy, hitObject, damageEvent);
            equipHitTransformEffect(enemy, hitObject, damageEvent);
        }

        async void onEnemyDeath(IEnemy enemy)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            _enemies.Remove(enemy);
            enemy.Dispose();
        }

        EnemyDef getDefaultDef()
        {
            var def = ScriptableObject.CreateInstance<EnemyDef>();
            def.MaxHealth = 100;
            def.Deffence = 0;
            def.Attack = 0;
            return def;
        }

        EnemyDef getDef()
        {
            return enemyManagerConfig?.EnemyDef ?? getDefaultDef();
        }

        IEnemy? addEnemy(IEnemyController enemyController, Vector3? position = null)
        {
            var def = getDef();
            var cursor = cursorFactory.CreateTargetCusor(enemyController, enemyController.GetCharactor());
            if (cursor == null)
            {
                return null;
            }
            var enemy = new EnemyImpl(def, enemyController, this, cursor, position);
            _enemies.Add(enemy);
            return enemy;
        }

        #region IEnemyManager
        IReadOnlyReactiveCollection<IEnemy> IEnemyManager.Enemies { get => _enemies; }

        IEnemy IEnemyManager.Spawn(EnemyDef enemyDef, Vector3 position)
        {
            var enemyController = GameObject.Instantiate(enemyDef.prefab) as IEnemyController;
            if (enemyController == null)
            {
                throw new InvalidConditionException("Invalid prefab");
            }
            var enemy = addEnemy(enemyController, position);
            if (enemy == null)
            {
                throw new InvalidCastException("fail to spwawn");
            }
            return enemy;
        }

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
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            this.disposable.Dispose();
        }
        #endregion


        #region IEnemyEvent
        void IEnemyEvent.OnAttacked(IEnemy enemy, IHitObject? hitObject, in DamageEvent damageEvent)
            => onEnemyAttacked(enemy, hitObject, damageEvent);

        void IEnemyEvent.OnDeath(IEnemy enemy)
            => onEnemyDeath(enemy);
        #endregion

        // ctor
        public EnemyManager(IEnemyManagerConfig? enemyManagerConfig, IEffectFactory effectFactory, ICursorFactory cursorFactory)
        {
            this.enemyManagerConfig = enemyManagerConfig;
            this.effectFactory = effectFactory;
            this.cursorFactory = cursorFactory;
        }
    }
}