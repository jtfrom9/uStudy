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
        Vector3 diretion { get; }
        float speed { get; }

        void OnHit(IMobileObject other, Vector3 position);
    }

    public interface ICharactor: IMobileObject
    {
        float distanceToGround { get; }
    }
}