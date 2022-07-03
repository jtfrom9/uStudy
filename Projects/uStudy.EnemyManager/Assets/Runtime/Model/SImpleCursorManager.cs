#nullable enable

using System;
using UnityEngine;
using UniRx;

namespace Hedwig.Runtime
{
    public class SimpleCursorManager : ICursorManager
    {
        ICursorFactory cursorFactory;
        Subject<IFreeCursor?> onCursorCreated = new Subject<IFreeCursor?>();
        IFreeCursor? cursor;

        #region ICursorManager
        ISubject<IFreeCursor?> ICursorManager.OnCursorCreated { get => onCursorCreated; }
        #endregion

        public SimpleCursorManager(ICursorFactory cursorFactory)
        {
            this.cursorFactory = cursorFactory;
        }

        void setCursor(Vector3? pos)
        {
            if (pos.HasValue)
            {
                if (cursor == null)
                {
                    cursor = cursorFactory?.CreateFreeCusor();
                    onCursorCreated.OnNext(cursor);
                }
                cursor?.Move(pos.Value);
            }
            else
            {
                cursor?.Dispose();
                cursor = null;
                onCursorCreated.OnNext(null);
            }
        }

        public void Move(Vector2 pos)
        {
            var ray = Camera.main.ScreenPointToRay(pos);
            var hits = Physics.RaycastAll(ray, 100);
            var highestY = 0f;
            Vector3? result = null;
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.CompareTag(Hedwig.Runtime.HitTag.Environment))
                {
                    var y = hit.point.y;
                    if (result == null || y > highestY)
                    {
                        result = hit.point;
                        highestY = y;
                    }
                }
            }
            if (result.HasValue)
            {
                setCursor(result.Value);
            }
        }

        public void Reset()
        {
            setCursor(null);
        }
    }
}