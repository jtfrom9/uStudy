#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public interface ICursor: IDisposable
    {
        bool visible { get; }
        void Show(bool v);
    }

    public interface ITargetCursor : ICursor
    {
        void Initialize(ITransformProvider target, float distanceToGround);
    }

    public interface IFreeCursor : ICursor, ITransformProvider
    {
        void Initialize();
        void Move(Vector3 pos);
    }

    public interface ICursorFactory
    {
        ITargetCursor? CreateTargetCusor(ITransformProvider target, ICharactor charactor);
        IFreeCursor? CreateFreeCusor();
    }
}