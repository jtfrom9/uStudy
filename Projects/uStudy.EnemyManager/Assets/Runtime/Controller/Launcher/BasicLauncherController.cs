#nullable enable

using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class BasicLauncherController : Controller, ILauncherController
    {
        [SerializeField]
        Transform? mazzle;

        CachedTransform _tranform = new CachedTransform();
        IEnemy? _target;
        IDisposable? _disposable;

        void Start()
        {
            if (mazzle != null)
            {
                _tranform.Initialize(mazzle);
            }
            // this.UpdateAsObservable().Subscribe(_ =>
            // {
            //     _update();
            // }).AddTo(this);
        }

        // void _update()
        // {
        //     if (this._target == null) return;
        //     if (this._target.transform.position == Vector3.zero) return;
        //     if (_target != null)
        //     {
        //         transform.LookAt(_target.transform.position);
        //     }
        // }

        #region ILauncher

        ITransform ILauncherController.mazzle { get => _tranform; }

        IEnemy? ILauncherController.target { get => this._target; }

        bool ILauncherController.CanLaunch { get => this._target != null && this.mazzle != null; }

        void ILauncherController.SetTarget(IEnemy? enemy)
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
            this._target = enemy;

            if(this._target!=null) {
                _disposable = this._target.transform.OnPositionChanged.Subscribe(pos =>
                {
                    this.transform.LookAt(pos);
                }).AddTo(this);
            }
        }

        #endregion
    }
}