#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class EnemyImpl : IEnemy, IEnemyControllerEvent, ISelectable
    {
        EnemyConfig _def;
        IEnemyController enemyController;
        IEnemyEvent enemyEvent;
        ICursor cursor;

        ReactiveProperty<int> health;

        int calcDamage(IHitObject hitObject) {
            return hitObject.attack;
        }

        int calcActualDamage(int damage)
        {
            return Math.Max(damage - _def.Deffence, 0);
        }

        void applyDamage(int actualDamage)
        {
            this.health.Value -= actualDamage;
            if(this.health.Value <0) this.health.Value = 0;
            Debug.Log($"{this}: applyDamage: actualDamage={actualDamage}, health={health}");
        }

        void doDamage(int damage, out DamageEvent damageEvent)
        {
            var actualDamage = calcActualDamage(damage);
            applyDamage(actualDamage);
            damageEvent = new DamageEvent(damage, actualDamage: actualDamage);
        }

        void doDamage(IHitObject hitObject, out DamageEvent damageEvent)
        {
            var damage = calcDamage(hitObject);
            doDamage(damage, out damageEvent);
        }

        void raiseEvent(IHitObject? hitObject, in DamageEvent damageEvent)
        {
            enemyEvent.OnAttacked(this, hitObject, damageEvent);

            if (health.Value == 0)
            {
                enemyEvent.OnDeath(this);
            }
        }

        void damaged(int damage)
        {
            doDamage(damage, out DamageEvent damageEvent);
            raiseEvent(null, damageEvent);
        }

        #region IEnemyControllerEvent
        void IEnemyControllerEvent.OnHit(IHitObject hitObject)
        {
            doDamage(hitObject, out DamageEvent damageEvent);
            raiseEvent(hitObject, damageEvent);
        }
        #endregion

        #region ISelectable
        void ISelectable.Select(bool v)
        {
            cursor.Show(v);
        }
        bool ISelectable.selected { get => cursor.visible; }
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            enemyController.Dispose();
        }
        #endregion

        #region ICharactor
        public IReadOnlyReactiveProperty<int> Health { get => health; }
        #endregion

        #region IEnemy
        public void SetDestination(Vector3 pos) => enemyController.SetDestination(pos);
        public void Stop() => enemyController.Stop();

        public IEnemyController controller { get => enemyController; }

        void IEnemy.Damaged(int damage) => damaged(damage);
        void IEnemy.ResetPos() => enemyController.ResetPos();
        #endregion

        public override string ToString()
        {
            return $"{controller.name}.Impl";
        }

        public EnemyImpl(EnemyConfig def, IEnemyController enemyController, IEnemyEvent enemyEvent, ICursor cursor, Vector3? position)
        {
            this._def = def;
            this.enemyController = enemyController;
            this.enemyEvent = enemyEvent;
            this.cursor = cursor;
            this.health = new ReactiveProperty<int>(def.MaxHealth);

            enemyController.Initialize(this, position);
        }
    }
}