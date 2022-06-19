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
        float distanceToGround;

        bool _selected;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3;

            var mr = GetComponent<MeshRenderer>();
            mr.material.color = UnityEngine.Random.ColorHSV();

            this.distanceToGround = mr.bounds.extents.y;
        }

        void Start()
        {
            this.initialPosition = transform.position;
            this.initialRotation = transform.rotation;
            this.initialScale = transform.localScale;
        }

        #region ISelectable
        void ISelectable.Select(bool v)
        {
            selector?.Show(v);
            _selected = v;
        }
        bool ISelectable.selected { get => _selected; }
        #endregion

        #region ICharactor
        Transform ICharactor.transform => transform;
        float ICharactor.distanceToGround => distanceToGround;
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


        #region IDisposable
        public void Dispose()
        {
            onAttcked.OnCompleted();
            onDeath.OnCompleted();
            selector?.Dispose();
            Destroy(gameObject);
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