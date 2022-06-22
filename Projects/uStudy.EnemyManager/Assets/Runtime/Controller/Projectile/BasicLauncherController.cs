#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class BasicLauncherController : Controller, ILauncherController
    {
        [SerializeField]
        Transform? mazzle;
        LineRenderer? lineRenderer;

        IEnemy? _target;

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
            if (this._target == null) return;
            if (this._target.transform.position == Vector3.zero) return;

            lr.SetPositions(new Vector3[] {
                transform.position,
                this._target.transform.position
            });
            if (_target != null)
            {
                transform.LookAt(_target.transform.position);
            }
        }

        #region ILauncher

        Vector3 ILauncherController.mazzlePosition { get => mazzle?.transform.position ?? Vector3.zero; }

        IEnemy? ILauncherController.target { get => this._target; }

        bool ILauncherController.CanLaunch { get => this._target != null && this.mazzle != null; }

        void ILauncherController.Aim(IEnemy? enemy)
        {
            this._target = enemy;
        }

        #endregion
    }
}