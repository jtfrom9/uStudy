using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Hedwig.Runtime
{
    public class QuadCursorController : MonoBehaviour, ICursor
    {
        #region ISelector
        void ICursor.Initialize(IMobileObject target, float distanceToGround)
        {
            // transform.position = target.position.Y(0.01f);
            // transform.SetParent(target, true);
            transform.SetParent(target.transform);
            transform.localPosition = Vector3.down * distanceToGround;
            // transform.position = transform.position.Y(0.01f);

            // var r = target.GetComponent<Renderer>();
            // Debug.Log(r.bounds.extents);

            // transform.localPosition = Vector3.up * (-r.bounds.extents.y);
            // transform.localPosition = Vector3.up * 0.01f;

            transform.DOScale(Vector3.one * 1.5f, 1).SetLoops(-1, LoopType.Yoyo);
            gameObject.SetActive(false);
        }

        bool ICursor.visible { get => gameObject.activeSelf; }

        void ICursor.Show(bool v)
        {
            gameObject.SetActive(v);
        }
        #endregion

        void System.IDisposable.Dispose() {
            Destroy(gameObject);
        }
    }
}