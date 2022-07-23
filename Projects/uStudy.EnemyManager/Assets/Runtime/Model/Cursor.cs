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
        void Initialize(IMobileObject target, float distanceToGround);
    }

    public interface IFreeCursor : ICursor, IMobileObject
    {
        void Initialize();
        void Move(Vector3 pos);
    }

    public interface ICursorFactory
    {
        ITargetCursor? CreateTargetCusor(ICharactor charactor);
        IFreeCursor? CreateFreeCusor();
    }

    public interface ICursorManager
    {
        ISubject<IFreeCursor?> OnCursorCreated { get; }
        void Move(Vector2 pos);
        void Reset();
    }
}