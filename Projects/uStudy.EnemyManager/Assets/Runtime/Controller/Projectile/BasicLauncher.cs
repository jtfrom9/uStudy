#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class BasicLauncher : MonoBehaviour, ILauncher
    {
        [SerializeField]
        Transform? mazzle;

        LineRenderer? lineRenderer;

        void Awake()
        {
            TryGetComponent(out lineRenderer);
        }

        void Start()
        {
            if (lineRenderer == null) return;

            this.UpdateAsObservable().Subscribe(_ =>
            {
                _update(lineRenderer);
            }).AddTo(this);
        }

        void _update(LineRenderer lr)
        {
            if (this.target == null) return;
            if (this.target!.position == Vector3.zero) return;

            lr.SetPositions(new Vector3[] {
                transform.position,
                this.target!.position
            });
        }

        Transform? target;

        public Vector3 muzzlePosition { get => transform.position; }
        public Vector3 direction { get => target!.position - muzzlePosition; }

        public void Aim(Transform target)
        {
            this.target = target;
            transform.LookAt(target.position);
        }

        [Inject] IProjectileFactory? projectileFactory;

        public void Launch()
        {
            if (projectileFactory == null) return;
            if (mazzle == null) return;
            if (target == null) return;
            var bullet = projectileFactory.Create(mazzle.position, target, 20);
            bullet?.Go();
        }
    }
}