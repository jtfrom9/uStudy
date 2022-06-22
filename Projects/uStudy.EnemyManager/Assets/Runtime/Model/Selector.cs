#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ISelector: IDisposable
    {
        void Initialize(Transform target, float distanceToGround);
        bool visible { get; }
        void Show(bool v);
    }

    public interface ISelectorFactory
    {
        ISelector? Create(ICharactor charactor);
    }
}