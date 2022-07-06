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
        string Name { get; }
        ITransform transform { get; }
    }

    public interface ICharactor: IMobileObject
    {
        float distanceToGround { get; }
    }
}