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

    public interface ICharactor: IDisposable
    {
        Transform transform { get; }
        float distanceToGround { get; }
    }
}