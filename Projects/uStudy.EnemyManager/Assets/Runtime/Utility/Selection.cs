#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class Selection<T>
    {
        protected IReadOnlyList<T> list;
        protected int _index = -1;
        protected readonly ISubject<T> onCurrentChanged = new Subject<T>();

        public int Index { get => _index; }
        public T Current { get => this.list[_index]; }
        public ISubject<T> OnCurrentChanged { get => onCurrentChanged; }

        public virtual void Select(int index)
        {
            if (list.Count > 0 && index >= 0 && index < list.Count)
            {
                _index = index;
                onCurrentChanged.OnNext(Current);
            }
        }

        public void Next()
        {
            var cur = Index;
            var next = list.Count - 1 == cur ? 0 : cur + 1;
            Select(next);
        }

        public void Prev()
        {
            var cur = Index;
            var prev = cur == 0 ? list.Count - 1 : cur - 1;
            Select(prev);
        }

        public Selection(IReadOnlyList<T> list)
        {
            if(list.Count<=0) {
                throw new InvalidConditionException("no entries in list");
            }
            this.list = list;
            this._index = 0;
        }
    }
}
