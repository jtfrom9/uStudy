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

        Vector3 initialPosition;
        Quaternion initialRotation;
        Vector3 initialScale;
        float? _distanceToGround;

        bool _selected;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3;

            var mr = GetComponent<MeshRenderer>();
            mr.material.color = UnityEngine.Random.ColorHSV();
        }

        void Start()
        {
            this.initialPosition = transform.position;
            this.initialRotation = transform.rotation;
            this.initialScale = transform.localScale;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(Collision.ProjectileTag))
            {
                var projectile = other.gameObject.GetComponent<IProjectile>();
                var pos = other.ClosestPointOnBounds(this.transform.position);
                var damage = new DamageEvent(this, 10, pos);
                this.onAttcked.OnNext(damage);
            }
        }

        #region ISelectable
        void ISelectable.Select(bool v)
        {
            selector?.Show(v);
            _selected = v;
        }
        bool ISelectable.selected { get => _selected; }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            onAttcked.OnCompleted();
            onDeath.OnCompleted();
            selector?.Dispose();
            Destroy(gameObject);
        }
        #endregion

        #region IMobileObject
        Transform IMobileObject.transform => transform;
        #endregion

        #region ICharactor
        float ICharactor.distanceToGround {
            get {
                if(_distanceToGround==null) {
                    var mr = GetComponent<MeshRenderer>();
                    _distanceToGround = mr.bounds.extents.y;
                }
                return _distanceToGround.Value;
            }
        }
        #endregion

        #region IEnemy
        public string Name { get => gameObject.name; }
        public int Health { get; private set; }
        ISelector? selector;

        void IEnemy.SetDestination(Vector3 pos)
        {
            _agent!.isStopped = false;
            _agent?.SetDestination(pos);
        }

        void IEnemy.Stop()
        {
            _agent!.isStopped = true;
            _agent?.SetDestination(transform.position);
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

        IEnemyControl IEnemy.GetControl() => this;

        #endregion

        #region IEnemyControl
        void IEnemyControl.SetHealth(int v) {
            this.Health = v;
        }
        void IEnemyControl.SetSelector(ISelector? selector) {
            this.selector = selector;
        }
        void IEnemyControl.ResetPos() {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            transform.localScale = initialScale;
        }
        #endregion

        [ContextMenu("Select")]
        void Context_Select() {
            var selectable = (this as ISelectable);
            if(selectable!=null) {
                selectable.Select(!selectable.selected);
            }
        }
    }
}