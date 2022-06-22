#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class SingleSelection
    {
        IReadOnlyList<ISelectable> _selectables;

        public SingleSelection(IReadOnlyList<ISelectable> selectables)
        {
            this._selectables = selectables;
        }

        public int SelectedIndex { get => _selectables.SelectedIndex(); }

        public ISelectable? Current {
            get
            {
                var index = this.SelectedIndex;
                return index >= 0 ? _selectables[index] : null;
            }
        }

        public readonly ISubject<ISelectable?> onCurrentChanged = new Subject<ISelectable?>();

        public void SelectExclusive(int index)
        {
            if (_selectables.Count > 0 && index >= 0 && index < _selectables.Count)
            {
                foreach (var (s, i) in _selectables.Select((s, i) => (s, i)))
                {
                    _selectables[i].Select(index == i);
                }
            }
            this.onCurrentChanged.OnNext(Current);
        }

        public void Next()
        {
            var cur = SelectedIndex;
            var next = _selectables.Count - 1 == cur ? 0 : cur + 1;
            SelectExclusive(next);
        }

        public void Prev()
        {
            var cur = SelectedIndex;
            var prev = cur == 0 ? _selectables.Count - 1 : cur - 1;
            SelectExclusive(prev);
        }
    }

    public static class SelectionExtention
    {
        public static int SelectedIndex(this IReadOnlyList<ISelectable> selectables)
        {
            if (selectables.Count > 0)
            {
                var indecies = selectables
                    .Select((selectable, index) => (selectable, index))
                    .Where(v => v.selectable.selected)
                    .Select(v => v.index)
                    .ToList();
                return indecies.Count > 0 ? indecies[0] : -1;
            }
            else
            {
                return -1;
            }
        }
    }
}