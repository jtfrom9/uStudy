#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public interface ITransform: IDisposable
    {
        Vector3 position { get; }
        Quaternion rotation { get; }

        ISubject<Vector3> OnPositionChanged { get; }
        ISubject<Quaternion> OnRotationChanged { get; }
    }

    public class CachedTransform: ITransform
    {
        Transform _transform;

        Subject<Vector3>? onPositionChanged;
        Subject<Quaternion>? onRotationChanged;

        CompositeDisposable disposable = new CompositeDisposable();

        #region ITransform
        Vector3 ITransform.position { get => _transform.position; }
        Quaternion ITransform.rotation { get => _transform.rotation; }

        ISubject<Vector3> ITransform.OnPositionChanged {
            get
            {
                if (onPositionChanged == null)
                {
                    onPositionChanged = new Subject<Vector3>();
                    this._transform.ObserveEveryValueChanged(t => t.position)
                    .Subscribe(_ =>
                    {
                        onPositionChanged.OnNext(_transform.position);
                    }).AddTo(disposable);
                }
                return onPositionChanged;
            }
        }

        ISubject<Quaternion> ITransform.OnRotationChanged {
            get
            {
                if (onRotationChanged == null)
                {
                    onRotationChanged = new Subject<Quaternion>();
                    this._transform.ObserveEveryValueChanged(t => t.rotation)
                    .Subscribe(_ =>
                    {
                        onRotationChanged.OnNext(_transform.rotation);
                    }).AddTo(disposable);
                }
                return onRotationChanged;
            }
        }

        #endregion

        void IDisposable.Dispose()
        {
            disposable.Dispose();
        }

        internal CachedTransform(Transform transform)
        {
            _transform = transform;

            transform.OnDestroyAsObservable().Subscribe(_ =>
            {
                disposable.Dispose();
            }).AddTo(transform);
        }
    }

    public static class CachedTransformFactory
    {
        public static ITransform CachedTransform(this MonoBehaviour monoBehaviour)
        {
            return new CachedTransform(monoBehaviour.transform);
        }

        public static ITransform CachedTransform(this GameObject gameObject)
        {
            return new CachedTransform(gameObject.transform);
        }
    }

}
