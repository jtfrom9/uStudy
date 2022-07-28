#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class SelectiveSelection : Selection<ISelectable>
    {
        public SelectiveSelection(IReadOnlyList<ISelectable> selectables) :
            base(selectables)
        {
            Current.Select(true);
        }

        public override void Select(int index)
        {
            if (Index != index)
            {
                Current.Select(false);
            }
            base.Select(index);
            Current.Select(true);
        }
    }
}