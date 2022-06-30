#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ITrajectoryVisualizer
    {
        bool visible { get; }
        void SetStartTarget(ITransform? target);
        void SetEndTarget(ITransform? target);
        void SetConfig(ProjectileConfig? config);
        void Show(bool v);
    }

    public abstract class TrajectoryBase: ScriptableObject
    {
        public abstract Vector3[] MakePoints(Vector3 start, Vector3 end);
    }
}