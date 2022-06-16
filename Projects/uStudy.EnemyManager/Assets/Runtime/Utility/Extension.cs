using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Utility
{
    public static class UtilityExtension
    {
        public static DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>
            DORotateAround(this Transform transform, Vector3 point, Vector3 axis, float endValue, float duration)
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
}
