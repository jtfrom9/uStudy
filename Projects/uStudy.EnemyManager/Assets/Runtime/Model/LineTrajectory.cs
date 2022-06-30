#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Trajectory/Line", fileName = "LineTrajectory")]
    public class LineTrajectory : TrajectoryBase
    {
        public override Vector3[] MakePoints(Vector3 start, Vector3 end)
        {
            var points = new List<Vector3>() {
                start,
                end
            };
            return points.ToArray();
        }
    }
}
