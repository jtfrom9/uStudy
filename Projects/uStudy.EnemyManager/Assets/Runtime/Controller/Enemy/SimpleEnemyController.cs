#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace Hedwig.Runtime
{
    public class SimpleEnemyController : MonoBehaviour, IEnemy, IEnemyControl
    {
        NavMeshAgent? _agent;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3;

            var mr = GetComponent<MeshRenderer>();
            mr.material.color = UnityEngine.Random.ColorHSV();
        }

        #region IEnemy

        public string Name { get => gameObject.name; }
        public int Health { get; private set; }

        void IEnemy.SetDestination(Vector3 pos)
        {
            Debug.Log($"{Name}: dest: ${pos}");
            _agent?.SetDestination(pos);
        }

        Subject<DamageEvent> onAttcked = new Subject<DamageEvent>();
        Subject<IEnemy> onDeath = new Subject<IEnemy>();

        public ISubject<DamageEvent> OnAttacked { get => onAttcked; }
        public ISubject<IEnemy> OnDeath { get => onDeath; }

        void IEnemy.Attacked(int damage)
        {
            Health -= damage;
            if (Health > 0)
            {
                onAttcked.OnNext(new DamageEvent(this, damage, transform.position));
            }
            else
            {
                onDeath.OnNext(this);
            }
        }

        #endregion

        #region IEnemyControl
        void IEnemyControl.SetHealth(int v) {
            this.Health = v;
        }
        #endregion


        #region IDisposable
        public void Dispose()
        {
            onAttcked.OnCompleted();
            onDeath.OnCompleted();
            Destroy(gameObject);
        }
        #endregion

    }
}