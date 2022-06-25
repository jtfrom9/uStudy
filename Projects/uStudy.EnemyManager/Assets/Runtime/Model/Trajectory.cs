#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ITrajectoryVisualizer
    {
        bool visible { get; }
        void SetStartTarget(Vector3 position);
        void SetEndTarget(Vector3 position);
        void SetConfig(ProjectileConfig? config);
        void Show(bool v);
    }
}