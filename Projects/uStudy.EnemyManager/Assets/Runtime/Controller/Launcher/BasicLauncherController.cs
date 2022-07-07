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

        MeshRenderer? mazzleMeshRenderer;

        CachedTransform _tranform = new CachedTransform();
        IDisposable? _disposable;

        void Awake()
        {
            if (mazzle != null)
            {
                _tranform.Initialize(mazzle);
                mazzleMeshRenderer = mazzle.GetComponent<MeshRenderer>();
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

        void setupHandler(IMobileObject? target)
        {
            if (target != null)
            {
                _disposable = target.transform.OnPositionChanged.Subscribe(pos =>
                {
                    transform.LookAt(pos);
                }).AddTo(this);
            }
        }

        #region ILauncher

        ITransform ILauncherController.mazzle { get => _tranform; }

         void ILauncherController.Initialize(ILauncherManager launcherManager)
        {
            launcherManager.launcher.CanFire.Subscribe(v => {
                if(mazzleMeshRenderer!=null) {
                    mazzleMeshRenderer.material.color = (!v) ? Color.red : Color.white;
                }
            }).AddTo(this);

            launcherManager.launcher.OnTargetChanged.Subscribe(v =>
            {
                this.clearHandler();
                this.setupHandler(v);
            });
        }

        #endregion
    }
}