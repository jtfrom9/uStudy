using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Enemy
{
    public struct DamageEvent
    {
        public IEnemy enemy;
        public int damage;

        public DamageEvent(IEnemy enemy, int damage) {
            this.enemy = enemy;
            this.damage = damage;
        }
    }

    public interface IEnemy
    {
        string Name { get; }
        int Health { get; }
        void SetDestination(Vector3 pos);
        void Attacked(int damage);

        ISubject<DamageEvent> OnAttacked { get; }
        ISubject<IEnemy> OnDeath { get; }
    }

    public interface IEnemyControl
    {
        void SetHealth(int v);
    }

    public interface IEnemyManager
    {
        IReadOnlyCollection<IEnemy> Enemies { get; }
    }
}
