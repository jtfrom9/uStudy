#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface IProjectile : IMobileObject
    {
        void Initialize(Vector3 initial, ProjectileConfig config);
        void Go(IMobileObject target);
        void Go(Vector3 target);
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, ProjectileConfig config);
    }
}
