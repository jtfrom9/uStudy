#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface IProjectile : IDisposable
    {
        void Initialize(Vector3 initial, Transform target, ProjectileConfig config);
        void Go();
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, Transform target, ProjectileConfig config);
    }
}
