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
        }

        public void SelectExclusive(int index)
        {
            if (list.Count > 0 && index >= 0 && index < list.Count)
            {
                foreach (var (s, i) in list.Select((s, i) => (s, i)))
                {
                    list[i].Select(index == i);
                }
            }
            base.Select(index);
        }
    }
}