#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public interface ICharactor
    {
        IReadOnlyReactiveProperty<int> Health { get; }
    }
}