#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    public sealed partial class Factory : ICursorFactory
    {
        [Header("Cursor Settings")]

        [SerializeField, InterfaceType(typeof(ITargetCursor))]
        Component? targetCursorPrefab;

        [SerializeField, InterfaceType(typeof(IFreeCursor))]
        Component? freeCursorPrefab;

        ITargetCursor? ICursorFactory.CreateTargetCusor(IMobileObject target, ICharactor charactor)
        {
            if (targetCursorPrefab == null)
            {
                return null;
            }
            var cursor = Instantiate(targetCursorPrefab) as ITargetCursor;
            cursor?.Initialize(target, charactor.distanceToGround);
            return cursor;
        }

        IFreeCursor? ICursorFactory.CreateFreeCusor()
        {
            if (freeCursorPrefab == null)
            {
                return null;
            }
            var cursor = Instantiate(freeCursorPrefab) as IFreeCursor;
            cursor?.Initialize();
            return cursor;
        }

    }
}