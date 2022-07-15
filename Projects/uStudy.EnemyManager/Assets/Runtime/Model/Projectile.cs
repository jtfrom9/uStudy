#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    namespace Projectile
    {
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
            public Collider? collider;
            public RaycastHit? willHit;
            public EndReason? endReason;
            public Ray? ray;
            public float? maxDistance;
            public Vector3? to;

            public EventArg(EventType type)
            {
                this.type = type;
                this.collider = null;
                this.willHit = null;
                this.endReason = null;
                this.ray = null;
                this.maxDistance = null;
                this.to = null;
            }
        }
    }

    public interface IProjectileController : IMobileObject
    {
        UniTask<bool> Move(Vector3 destRelative, float duration);
        UniTask LastMove(float speed);
        ISubject<Projectile.EventArg> OnEvent { get; }

        void Initialize(Vector3 initial);
    }

    public struct ProjectileOption
    {
        public bool? destroyAtEnd;

        public bool DestroyAtEnd { get => destroyAtEnd ?? true; }
    }

    public interface IProjectile : IMobileObject
    {
        IProjectileController controller { get; }
        Projectile.EndReason EndReason { get; }

        ISubject<Unit> OnStarted { get; }
        ISubject<Unit> OnEnded { get; }
        ISubject<Unit> OnDestroy { get; }
        void Start(ITransform target, in ProjectileOption? option = null);

        TrajectoryMap? trajectoryMap { get; }
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, ProjectileConfig config);
        ISubject<IProjectile> OnCreated { get; }
    }
}
