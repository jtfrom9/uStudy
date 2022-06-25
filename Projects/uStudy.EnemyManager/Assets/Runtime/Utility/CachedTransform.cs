#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public interface ITransform : IDisposable
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }

        Vector3 Right { get; }
        Vector3 Up { get; }
        Vector3 Forward { get; }

        ISubject<Vector3> OnPositionChanged { get; }
        ISubject<Quaternion> OnRotationChanged { get; }

        Transform? Raw { get; }
    }

    public class CachedTransform : ITransform
    {
        Transform? _transform;

        Subject<Vector3> onPositionChanged = new Subject<Vector3>();
        Subject<Quaternion> onRotationChanged = new Subject<Quaternion>();

        CompositeDisposable disposable = new CompositeDisposable();

        #region ITransform
        Vector3 ITransform.Position { get => (_transform != null) ? _transform.position : Vector3.zero; }
        Quaternion ITransform.Rotation { get => (_transform != null) ? _transform.rotation : Quaternion.identity; }

        Vector3 ITransform.Right { get => (_transform!=null) ? _transform.right : Vector3.right; }
        Vector3 ITransform.Up { get => (_transform!=null) ? _transform.up : Vector3.up; }
        Vector3 ITransform.Forward { get => (_transform != null) ? _transform.forward : Vector3.forward; }

        ISubject<Vector3> ITransform.OnPositionChanged { get => onPositionChanged; }
        ISubject<Quaternion> ITransform.OnRotationChanged { get => onRotationChanged; }

        Transform? ITransform.Raw { get => _transform; }
        #endregion

        void IDisposable.Dispose()
        {
            disposable.Dispose();
        }

        public void Initialize(Transform transform)
        {
            _transform = transform;

            _transform.ObserveEveryValueChanged(t => t.position).Subscribe(pos =>
            {
                onPositionChanged.OnNext(pos);
            }).AddTo(disposable);

            _transform.ObserveEveryValueChanged(t => t.rotation).Subscribe(rot =>
            {
                onRotationChanged.OnNext(rot);
            }).AddTo(disposable);

            transform.OnDestroyAsObservable().Subscribe(_ =>
            {
                disposable.Dispose();
            }).AddTo(transform);
        }

        public CachedTransform()
        {
        }

        internal CachedTransform(Transform transform)
        {
            Initialize(transform);
        }
    }

    public static class CachedTransformFactory
    {
        public static ITransform CachedTransform(this MonoBehaviour? monoBehaviour)
        {
            if (monoBehaviour == null)
            {
                throw new InvalidConditionException("Unable to create CachedTransform");
            }
            return new CachedTransform(monoBehaviour.transform);
        }

        public static ITransform CachedTransform(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new InvalidConditionException("Unable to create CachedTransform");
            }
            return new CachedTransform(gameObject.transform);
        }
    }

    public static class TransformExtension
    {
        public static void SetParent(this Transform transform, ITransform parent)
        {
            if (parent.Raw == null)
            {
                Debug.LogWarning($"TransformExtension.SetParent. parent.Raw is null");
            }
            transform.SetParent(parent.Raw);
        }

        public static void SetParent(this Transform transform, ITransform parent, bool worldPositionStays)
        {
            if (parent.Raw == null)
            {
                Debug.LogWarning($"TransformExtension.SetParent. parent.Raw is null");
            }
            transform.SetParent(parent.Raw, worldPositionStays);
        }
    }
}
