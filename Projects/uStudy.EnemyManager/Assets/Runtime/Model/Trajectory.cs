#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ITrajectoryVisualizer
    {
        bool visible { get; }
        void SetStartTarget(Transform target);
        void SetEndTarget(Transform targeet);
        void SetConfig(ProjectileConfig? config);
        void Show(bool v);
    }
}