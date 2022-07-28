#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class EnemyImpl : IEnemy, IEnemyControllerEvent
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

        #region IMobileObject
        ITransform IMobileObject.transform { get => enemyController.transform; }
        #endregion

        #region ICharactor
        float ICharactor.distanceToGround { get => enemyController.distanceToGround; }
        #endregion

        #region IEnemy
        public int Health { get => health; }
        public void SetDestination(Vector3 pos) => enemyController.SetDestination(pos);
        public void Stop() => enemyController.Stop();

        void IEnemy.Attacked(int damage) {
            _onAttacked(new DamageEvent()
            {
                enemy = this,
                damage = damage,
                position = enemyController.transform.Position
            });
        }
        void IEnemy.ResetPos() => enemyController.ResetPos();

        public ISubject<DamageEvent> OnAttacked => onAttacked;
        public ISubject<IEnemy> OnDeath => onDeath;
        #endregion

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