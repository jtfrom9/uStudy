#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Trajectory/Parabola", fileName = "ParabolaTrajectory")]
    public class ParabolaTrajectory : TrajectoryBase
    {
        // Vector3 start;
        // Vector3 end;

        // [SerializeField] Vector3 control;

        // public ParabolaTrajectory(Vector3 start, Vector3 end)
        // {
        //     this.start = start;
        //     this.end = end;
        //     this.control = ((start + end) / 2) + Vector3.up * 5;
        // }

        Vector3 sampleCurve(Vector3 start, Vector3 end, float t)
        {
            var control = ((start + end) / 2) + Vector3.up * 5;
            Vector3 Q0 = Vector3.Lerp(start, control, t);
            Vector3 Q1 = Vector3.Lerp(control, end, t);
            Vector3 Q2 = Vector3.Lerp(Q0, Q1, t);
            return Q2;
        }

        public override Vector3[] MakePoints(Vector3 start, Vector3 end)
        {
            var points = new List<Vector3>() {
                start
            };
            for (var i = 1; i <= 10; i++)
            {
                var mid = sampleCurve(start, end, (float)i / 10);
                points.Add(mid);
            }
            points.Add(end);
            return points.ToArray();
        }
    }
}
