#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ISelectable
    {
        void Select(bool v);
        bool selected { get; }
    }

    public interface IMobileObject: IDisposable
    {
        ITransform transform { get; }
    }

    public interface ICharactor: IMobileObject
    {
        float distanceToGround { get; }
    }


    public static class MobileObjectFactory
    {
        class Impl : IMobileObject
        {
            GameObject _gameObject;
            CachedTransform _transform;

            ITransform IMobileObject.transform { get => _transform; }

            public void Dispose() { }

            public Impl(GameObject gameObject)
            {
                _gameObject = gameObject;
                _transform = new CachedTransform();
                _transform.Initialize(gameObject.transform);
            }
        }

        public static IMobileObject AsMobileObject(this GameObject gameObject)
        {
            return new Impl(gameObject);
        }
    }
}