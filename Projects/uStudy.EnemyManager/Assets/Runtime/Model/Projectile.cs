#nullable enable

using System;
using UnityEngine;

namespace Hedwig.Runtime
{
    namespace Projectile
    {
        public enum EndType
        {
            Destroy,
            Invisible
        }

        public enum Status
        {
            Init, Active, End
        }

        public enum EndReason
        {
            TargetHit,
            OtherHit,
            Expired
        }

        public struct eHistory
        {
        }
    }

    public interface IProjectile : IMobileObject
    {
        Projectile.Status status { get; }
        Projectile.EndReason endRegion { get; }
        void Initialize(Vector3 initial, ProjectileConfig config);
        void Go(ITransform target);
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, ProjectileConfig config);
    }
}
