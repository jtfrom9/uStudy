using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public static class DOTweenRotateAroundExtension {

    public static DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> DORotateAround(this Transform transform, Vector3 point, Vector3 axis, float endValue, float duration)
    {
        float anglePrev = 0;
        return DOTween.To(
            () => 0,
            (angle) =>
            {
                transform.RotateAround(point, axis, angle - anglePrev);
                anglePrev = angle;
            },
            endValue,
            duration);
    }
}

public class ObstacleController : MonoBehaviour
{
    [SerializeField]
    float duration = 3;

    void Start()
    {
        // transform.DOLocalMoveZ(0, 3)
        //     .SetEase(Ease.InQuad)
        //     .SetLoops(-1, LoopType.Yoyo)
        //     .OnComplete(() => Debug.Log("done"));

        // var cur = transform.position;
        // transform.DOLocalPath(new[] {
        //     Vector3.zero,
        //     new Vector3(10,cur.y,10),
        //     new Vector3(0, cur.y, 10)
        // }, 5, PathType.CatmullRom);

        transform.DORotateAround(Vector3.zero, Vector3.up, 360, duration)
        .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // float prev = 0;
        // DOTween.To(() => 0,
        //  (v) =>
        //  {
        //      transform.RotateAround(Vector3.zero, Vector3.up, v - prev);
        //      prev = v;
        //  }
        // , 360f, 10).SetEase(Ease.InOutQuad);
    }
}
