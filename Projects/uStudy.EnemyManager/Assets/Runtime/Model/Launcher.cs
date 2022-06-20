using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ILauncher
    {
        Vector3 muzzlePosition { get; }
        Vector3 direction { get; }
        void Aim(Transform target);

        void Launch();
    }
}