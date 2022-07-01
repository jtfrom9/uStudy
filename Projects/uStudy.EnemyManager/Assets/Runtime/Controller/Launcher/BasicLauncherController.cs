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
        IMobileObject? _target;
        IDisposable? _disposable;

        void Awake()
        {
            if (mazzle != null)
            {
                _tranform.Initialize(mazzle);

                // (_tranform as ITransform).OnPositionChanged.Subscribe(pos =>
                // {
                //     Debug.Log($"muzzle: {pos}");
                // });
            }
        }

        void OnDestroy()
        {
            this.clearHandler();
        }

        void clearHandler()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }

        void setupHandler()
        {
            if (this._target != null)
            {
                _disposable = this._target.transform.OnPositionChanged.Subscribe(pos =>
                {
                    transform.LookAt(pos);
                }).AddTo(this);
            }
        }

        #region ILauncher

        ITransform ILauncherController.mazzle { get => _tranform; }

        IMobileObject? ILauncherController.target { get => this._target; }

        bool ILauncherController.CanLaunch { get => this._target != null && this.mazzle != null; }

        void ILauncherController.SetTarget(IMobileObject? target)
        {
            this.clearHandler();
            this._target = target;
            this.setupHandler();
        }

        #endregion
    }
}