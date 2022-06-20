#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

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

        void Update()
        {
            if (lineRenderer == null) return;
            if (this.target!.position == Vector3.zero) return;

            lineRenderer.SetPositions(new Vector3[] {
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
            if(projectileFactory==null) return;
            if(mazzle==null) return;
            var bullet = projectileFactory.Create(mazzle.position, target, 20);
            bullet?.Go();
        }
    }
}