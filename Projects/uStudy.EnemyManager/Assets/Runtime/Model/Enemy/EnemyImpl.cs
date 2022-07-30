#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class EnemyImpl : IEnemy, IEnemyControllerEvent, ISelectable
    {
        EnemyDef _def;
        IEnemyController enemyController;
        IEnemyEvent enemyEvent;
        ICursor cursor;

        int health;

        int calcDamage(IHitObject hitObject) {
            return 10;
        }

        void makeDamageEvent(IHitObject hitObject, out DamageEvent damageEvent)
        {
            damageEvent = new DamageEvent(damage: calcDamage(hitObject));
        }

        void applyDamage(in DamageEvent damageEvent)
        {
            var actualDamage = damageEvent.damage - _def.Deffence;
            this.health -= actualDamage;
        }

        void raiseEvent(IHitObject? hitObject, in DamageEvent damageEvent)
        {
            if (health <= 0)
            {
                this.health = 0;
                enemyEvent.OnDeath(this);
            }
            else
            {
                enemyEvent.OnAttacked(this, hitObject, damageEvent);
            }
        }

        void damaged(int damage)
        {
            var e = new DamageEvent(damage: damage);
            applyDamage(e);
            raiseEvent(null, e);
        }

        #region IEnemyControllerEvent
        void IEnemyControllerEvent.OnHit(IHitObject hitObject)
        {
            makeDamageEvent(hitObject, out DamageEvent damageEvent);
            applyDamage(damageEvent);
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

        #region IEnemy
        public int Health { get => health; }
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

        public EnemyImpl(EnemyDef def, IEnemyController enemyController, IEnemyEvent enemyEvent, ICursor cursor)
        {
            this._def = def;
            this.enemyController = enemyController;
            this.enemyEvent = enemyEvent;
            this.cursor = cursor;
            this.health = def.MaxHealth;

            enemyController.Initialize(this);
        }
    }
}