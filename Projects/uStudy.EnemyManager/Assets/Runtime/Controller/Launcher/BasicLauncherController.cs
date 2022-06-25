#nullable enable

using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class BasicLauncherController : Controller, ILauncherController
    {
        [SerializeField]
        Transform? mazzle;

        IEnemy? _target;

        void Start()
        {
            this.UpdateAsObservable().Subscribe(_ =>
            {
                _update();
            }).AddTo(this);
        }

        void _update()
        {
            if (this._target == null) return;
            if (this._target.transform.position == Vector3.zero) return;
            if (_target != null)
            {
                transform.LookAt(_target.transform.position);
            }
        }

        #region ILauncher

        Vector3 ILauncherController.mazzlePosition { get => mazzle?.transform.position ?? Vector3.zero; }

        Transform ILauncherController.mazzle { get => mazzle!; }

        IEnemy? ILauncherController.target { get => this._target; }

        bool ILauncherController.CanLaunch { get => this._target != null && this.mazzle != null; }

        void ILauncherController.SetTarget(IEnemy? enemy)
        {
            this._target = enemy;
        }

        #endregion
    }
}