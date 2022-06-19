#nullable enable

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ISelector: System.IDisposable
    {
        void Initialize(Transform target, float distanceToGround);
        bool visible { get; }
        void Show(bool v);
    }

    public interface ISelectorFactory
    {
        ISelector? Create(ICharactor charactor);
    }

    // public static class SelectorExtension
    // {
    //     public static int SelectedIndex(this ICollection<ISelector> selectors) {
    //         return selectors.Select((selector, index) => (selector, index))
    //             .First(v => v.selector.visible)
    //             .index;
    //     }
    // }
}