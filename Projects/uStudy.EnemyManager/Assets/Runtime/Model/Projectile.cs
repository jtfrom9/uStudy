#nullable enable

using System;
using UnityEngine;
using UniRx;

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
            Expired,
            Disposed
        }

        public enum EventType
        {
            BeforeLoop,
            AfterLoop,
            BeforeMove,
            AfterMove,
            Trigger,
            WillHit,
            BeforeLastMove,
            AfterLastMove,
            OnKill,
            OnComplete,
            OnPause,
            Destroy
        }

        public struct EventArg {
            public EventType type;
            public IProjectile projectile;
            public Collider? collider;
            public RaycastHit? willHit;
            public Ray? ray;
            public float? maxDistance;

            public EventArg(IProjectile projectile, EventType type)
            {
                this.projectile = projectile;
                this.type = type;
                this.collider = null;
                this.willHit = null;
                this.ray = null;
                this.maxDistance = null;
            }
        }
    }

    public interface IProjectile : IMobileObject
    {
        Projectile.Status Status { get; }
        Projectile.EndReason EndReason { get; }
        void Initialize(Vector3 initial, ProjectileConfig config);
        void Go(ITransform target);

        ISubject<Projectile.EventArg> OnEvent { get; }
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, ProjectileConfig config);
        ISubject<IProjectile> OnCreated { get; }
    }
}
