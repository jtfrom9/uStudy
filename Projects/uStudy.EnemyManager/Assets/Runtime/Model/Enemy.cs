#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
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

    public interface IEnemy : ICharactor, ISelectable
    {
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
        void SetSelector(ICursor? selector);
        void ResetPos();
    }

    public interface IEnemyRepository
    {
        IEnemy[] GetEnemies();
    }

    public interface IEnemyManager : IDisposable
    {
        IReadOnlyList<IEnemy> Enemies { get; }
        void Setup();
        void AddEnemy(IEnemy eney);

        ISubject<IEnemy> OnCreated { get; }
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
                        var x = UnityEngine.Random.Range(min, max);
                        var z = UnityEngine.Random.Range(min, max);
                        var pos = new Vector3(x, 0, z);
                        enemy.SetDestination(pos);
                    }
                    try
                    {
                        await UniTask.Delay(msec, cancellationToken: token);
                    }catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                // manager.StopAll();
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
