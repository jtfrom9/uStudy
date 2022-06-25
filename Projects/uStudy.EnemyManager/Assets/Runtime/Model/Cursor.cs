#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ICursor: IDisposable
    {
        void Initialize(IMobileObject target, float distanceToGround);
        bool visible { get; }
        void Show(bool v);
    }

    public interface ICursorFactory
    {
        ICursor? Create(ICharactor charactor);
    }
}