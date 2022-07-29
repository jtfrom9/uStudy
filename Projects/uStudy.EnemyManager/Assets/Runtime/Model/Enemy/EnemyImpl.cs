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
        ICursor cursor;

        int health;
        Subject<DamageEvent> onAttacked = new Subject<DamageEvent>();
        Subject<IEnemy> onDeath = new Subject<IEnemy>();

        void _onAttacked(DamageEvent e)
        {
            var actualDamage = e.damage - _def.Deffence;
            this.health -= actualDamage;
            if (health <= 0)
            {
                this.health = 0;
                onDeath.OnNext(this);
            }
            else
            {
                onAttacked.OnNext(e);
            }
        }

        #region IEnemyControllerEvent
        void IEnemyControllerEvent.OnAttacked(Vector3 position) =>
            _onAttacked(new DamageEvent()
            {
                enemy = this,
                damage = 10,
                position = position
            });
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

        void IEnemy.Attacked(int damage) {
            _onAttacked(new DamageEvent()
            {
                enemy = this,
                damage = damage,
                position = enemyController.transform.Position
            });
        }
        void IEnemy.ResetPos() => enemyController.ResetPos();

        public IObservable<DamageEvent> OnAttacked => onAttacked;
        public IObservable<IEnemy> OnDeath => onDeath;
        #endregion

        public override string ToString()
        {
            return $"{controller.name}.Impl";
        }

        public EnemyImpl(EnemyDef def, IEnemyController enemyController, ICursor cursor)
        {
            this._def = def;
            this.enemyController = enemyController;
            this.cursor = cursor;
            this.health = def.MaxHealth;

            enemyController.Initialize(this);
        }
    }
}