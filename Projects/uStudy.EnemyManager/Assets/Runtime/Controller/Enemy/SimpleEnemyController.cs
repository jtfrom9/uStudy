#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace Hedwig.Runtime
{
    public class SimpleEnemyController : Controller, IEnemyController, ICharactor, IHitHandler
    {
        string _name = "";
        IEnemyControllerEvent? controllerEvent;
        ITransform _transform = new CachedTransform();
        NavMeshAgent? _agent;
        Rigidbody? _rigidbody;

        Vector3 initialPosition;
        Quaternion initialRotation;
        Vector3 initialScale;
        float? _distanceToGround;

        void Awake()
        {
            _transform.Initialize(transform);
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3;

            _rigidbody = GetComponent<Rigidbody>();

            var mr = GetComponent<MeshRenderer>();
            mr.material.color = UnityEngine.Random.ColorHSV();
        }

        void OnDestroy()
        {
        }

        void Start()
        {
            this.initialPosition = transform.position;
            this.initialRotation = transform.rotation;
            this.initialScale = transform.localScale;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(HitTag.Projectile))
            {
                var projectile = other.gameObject.GetComponent<IProjectileController>();
                var position = other.ClosestPointOnBounds(_transform.Position);
                Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} Hit({gameObject.name}) @{position}");
            }
        }

        // void OnTriggerStay(Collider other)
        // {
        //     if (other.gameObject.CompareTag(Collision.ProjectileTag))
        //     {
        //         var projectile = other.gameObject.GetComponent<IProjectile>();
        //         var posision = other.ClosestPointOnBounds(this.transform.position);
        //         onTrigger(projectile, posision);
        //     }
        // }

        #region IDisposable
        void IDisposable.Dispose()
        {
            Destroy(gameObject);
        }
        #endregion

        #region
        void IHitHandler.OnHit(IHitObject hitObject)
        {
            controllerEvent?.OnHit(hitObject);
        }
        #endregion

        #region IMobileObject
        ITransform ITransformProvider.transform { get => _transform; }
        #endregion

        #region ICharactor
        float ICharactor.distanceToGround
        {
            get {
                if(_distanceToGround==null) {
                    var mr = GetComponent<MeshRenderer>();
                    _distanceToGround = mr.bounds.extents.y;
                }
                return _distanceToGround.Value;
            }
        }
        #endregion

        #region IEnemyController
        string IEnemyController.name { get => _name; }
        void IEnemyController.SetDestination(Vector3 pos)
        {
            _agent!.isStopped = false;
            _agent?.SetDestination(pos);
        }
        void IEnemyController.Stop()
        {
            _agent!.isStopped = true;
            _agent?.SetDestination(_transform.Position);
        }
        void IEnemyController.ResetPos()
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            transform.localScale = initialScale;
        }
        void IEnemyController.Knockback(Vector3 direction, float power)
        {
            Debug.Log($"AddShock: ${_rigidbody}");
            _rigidbody?.AddForce(direction * power, ForceMode.Impulse);
        }

        ICharactor IEnemyController.GetCharactor()
        {
            return this;
        }

        static int count = 0;
        [RuntimeInitializeOnLoadMethod]
        void _InitializeOnEnterPlayMode()
        {
            count = 0;
        }

        void IEnemyController.Initialize(IEnemyControllerEvent controllerEvent)
        {
            if (gameObject.name == "")
            {
                gameObject.name = $"SimpleEnemyController({count})";
                count++;
            }
            _name = gameObject.name;
            this.controllerEvent = controllerEvent;
        }
        #endregion
    }
}