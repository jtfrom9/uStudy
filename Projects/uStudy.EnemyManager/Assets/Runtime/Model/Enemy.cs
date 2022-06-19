#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        void Stop();
        void Attacked(int damage);

        ISubject<DamageEvent> OnAttacked { get; }
        ISubject<IEnemy> OnDeath { get; }

        IEnemyControl GetControl();
    }

    public interface IEnemyControl
    {
        void SetHealth(int v);
        void SetSelector(ISelector? selector);
        void ResetPos();
    }

    public interface IEnemyManager: System.IDisposable
    {
        IReadOnlyList<IEnemy> Enemies { get; }
        void AddEnemy(IEnemy eney);
    }

    public static class EnemyManagerExtension
    {
        public static int SelectedIndex(this IEnemyManager manager)
        {
            if (manager.Enemies.Count > 0)
            {
                var list = manager.Enemies
                    .Select((enemy, index) => (enemy, index))
                    .Where(v => v.enemy.selected)
                    .Select(v => v.index)
                    .ToList();
                return list.Count > 0 ? list[0] : -1;
            }
            else
            {
                return -1;
            }
        }

        public static IEnemy? Selected(this IEnemyManager manager)
        {
            var index = manager.SelectedIndex();
            return index > 0 ? manager.Enemies[index] : null;
        }


        public static void SelectExclusive(this IEnemyManager manager, int index)
        {
            if (manager.Enemies.Count > 0 && index >= 0 && index < manager.Enemies.Count)
            {
                foreach (var (e, i) in manager.Enemies.Select((e, i) => (e, i)))
                {
                    manager.Enemies[i].Select(index == i);
                }
            }
        }
    }
}
