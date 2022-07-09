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

    // public struct ProjectileCommand {
    //     public int index;
    //     public Trajectory.SectionType type;
    //     public float startFactor;
    //     public float endFactor;
    //     public Vector3? point;
    //     public TrajectorySectionMap sectionMap;
    //     public TrajectorySectionMap endSectionMap;
    //     public Func<Vector3, Vector3, float, Vector3>? pointMaker;

    //     Vector3 getPoint(float factor) {
    //         if (type == Trajectory.SectionType.Chase)
    //         {
    //             throw new InvalidConditionException("Invalid Type");
    //         }
    //         if (pointMaker != null)
    //         {
    //             var localfactor = (factor - sectionMap.minfactor) / sectionMap.factorRatio;
    //             // return pointMaker(sectionMap.start, sectionMap.end, localfactor);
    //             if (localfactor <= 1.0f)
    //             {
    //                 return pointMaker(sectionMap.start, sectionMap.end, localfactor);
    //             }
    //             else
    //             {
    //                 return pointMaker(endSectionMap.start, endSectionMap.end, localfactor - 1.0f);
    //             }
    //         }
    //         if (point != null)
    //         {
    //             return point.Value;
    //         }
    //         throw new InvalidCastException("Invalid Command");
    //     }
    //     public Vector3 GetStartPoint() => getPoint(startFactor);
    //     public Vector3 GetEndPoint() => getPoint(endFactor);
    //     public (Vector3, Vector3) GetPoints() => (getPoint(startFactor), getPoint(endFactor));

    //     public override string ToString()
    //     {
    //         return $"(ProjectileCommand({index}, {type}, {startFactor}-{endFactor})";
    //     }
    // }

    // public struct ProjectileCommand {
    //     public int index;
    //     public float fromFactor;
    //     public float totFactor;
    //     public TrajectorySectionMap sectionMap;
    //     public TrajectorySectionMap? nextMap;
    //     public Func<Vector3, Vector3, float, List<Vector2>?, Vector3> pointMaker;

    //     bool isCrossMap { get => nextMap != null; }

    //     Vector3 getPoint(float factor, TrajectorySectionMap sectionMap)
    //     {
    //         return pointMaker(sectionMap.start, sectionMap.end, factor, sectionMap.section.controls);
    //     }

    //     public Vector3 GetFromPoint()
    //     {
    //         return getPoint(fromFactor, sectionMap);
    //     }

    //     public Vector3 GetToPoint()
    //     {
    //         return getPoint(totFactor, (nextMap == null) ? sectionMap : nextMap);
    //     }

    //     public (Vector3, Vector3) GetPoints() => (GetFromPoint(), GetToPoint());
    // }

    public interface IProjectile : IMobileObject
    {
        IProjectileController controller { get; }
        Projectile.EndReason EndReason { get; }

        ISubject<Unit> OnStarted { get; }
        ISubject<Unit> OnEnded { get; }
        ISubject<Unit> OnDestroy { get; }
        void Start(ITransform target);
    }

    public interface IProjectileFactory
    {
        IProjectile? Create(Vector3 start, ProjectileConfig config);
        ISubject<IProjectile> OnCreated { get; }
    }
}
