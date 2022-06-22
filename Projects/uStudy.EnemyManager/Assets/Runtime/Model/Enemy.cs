#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

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
        public static UniTask RandomWalk(this IEnemyManager manager, float min, float max, int msec, CancellationToken token)
        {
            return UniTask.Create(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var enemy in manager.Enemies)
                    {
                        var x = Random.Range(min, max);
                        var z = Random.Range(min, max);
                        var pos = new Vector3(x, 0, z);
                        enemy.SetDestination(pos);
                    }
                    await UniTask.Delay(msec, cancellationToken: token);
                }
            });
        }

        public static void StopAll(this IEnemyManager manager)
        {
            foreach(var enemy in manager.Enemies) {
                enemy.Stop();
            }
        }
    }
}
