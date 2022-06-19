#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public struct DamageEvent
    {
        public IEnemy enemy;
        public int damage;
        public Vector3 position;

        public DamageEvent(IEnemy enemy, int damage, Vector3 position)
        {
            this.enemy = enemy;
            this.damage = damage;
            this.position = position;
        }
    }

    public interface IEnemy: ICharactor, System.IDisposable
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

    public interface IEnemyManager: System.IDisposable
    {
        IReadOnlyList<IEnemy> Enemies { get; }
        void AddEnemy(IEnemy eney);
    }
}
