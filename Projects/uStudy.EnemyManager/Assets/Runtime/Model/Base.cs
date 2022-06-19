#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ISelectable
    {
        void Select(bool v);
        bool selected { get; }
    }

    public interface ICharactor: ISelectable
    {
        Transform transform { get; }
        float distanceToGround { get; }
    }
}