#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface IProjectile : System.IDisposable
    {
        void Initialize(Vector3 start, Vector3 end, float duration);
        void Go();
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, Vector3 end, float duration);
    }
}
