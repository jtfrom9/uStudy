#nullable enable

using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public enum MouseMoveEventType
    {
        Enter,
        Exit,
        Over
    }

    public struct MouseMoveEvent {
        public MouseMoveEventType type;
        public Vector3 position;
    }

    public interface IMouseOperation
    {
        ISubject<Unit> OnLeftClick { get; }
        ISubject<bool> OnLeftTrigger { get; }
        ISubject<Unit> OnRightClick { get; }
        ISubject<bool> OnRightTrigger { get; }

        ISubject<MouseMoveEvent> OnMove { get; }
    }
}
