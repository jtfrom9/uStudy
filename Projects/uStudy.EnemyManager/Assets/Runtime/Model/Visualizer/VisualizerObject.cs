#nullable enable

using UnityEngine;
using UnityEngine.Search;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName ="Hedwig/Visualizers", fileName ="Visualizers")]
    public class VisualizerObject : ScriptableObject, ICursorFactory
    {
        [Header("Cursor Settings")]

        [SerializeField, SearchContext("t:prefab Cursor")]
        GameObject? targetCursorPrefab;

        [SerializeField, SearchContext("t:prefab Cursor")]
        GameObject? freeCursorPrefab;

        ITargetCursor? ICursorFactory.CreateTargetCusor(ITransformProvider target, IVisualProperty vproperty)
        {
            if (targetCursorPrefab == null)
            {
                return null;
            }
            var cursor = Instantiate(targetCursorPrefab).GetComponent<ITargetCursor>();
            cursor?.Initialize(target, vproperty.distanceToGround);
            return cursor;
        }

        IFreeCursor? ICursorFactory.CreateFreeCusor()
        {
            if (freeCursorPrefab == null)
            {
                return null;
            }
            var cursor = Instantiate(freeCursorPrefab).GetComponent<IFreeCursor>();
            cursor?.Initialize();
            return cursor;
        }
    }
}