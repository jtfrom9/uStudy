#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace Hedwig.Runtime
{
    public class SimpleEnemyController : MonoBehaviour, IEnemyController, ICharactor
    {
        string _name = "";
        IEnemyControllerEvent? controllerEvent;
        ITransform _transform = new CachedTransform();
        NavMeshAgent? _agent;

        Vector3 initialPosition;
        Quaternion initialRotation;
        Vector3 initialScale;
        float? _distanceToGround;

        void Awake()
        {
            _transform.Initialize(transform);
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3;

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
                var posision = other.ClosestPointOnBounds(_transform.Position);
                onHit(projectile, posision);
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

        void onHit(IMobileObject target, Vector3 position)
        {
            Debug.Log($"[{target.GetHashCode():x}] frame:{Time.frameCount} Hit({gameObject.name}) @{position}");
            onAttacked(position);
        }

        void onAttacked(Vector3 position)
        {
            controllerEvent?.OnAttacked(position);
        }

        #region IDisposable
        void IDisposable.Dispose()
        {
            Destroy(gameObject);
        }
        #endregion

        #region IMobileObject
        ITransform IMobileObject.transform { get => _transform; }
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