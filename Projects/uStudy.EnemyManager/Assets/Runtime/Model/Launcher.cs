#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public interface ILauncherController
    {
        ITransform mazzle { get; }
        void Initialize(ILauncher launcher);
    }

    public interface ILauncherHandler : IDisposable
    {
        void Fire(ITransform start, ITransform target);
        void TriggerOn(ITransform start, ITransform target);
        void TriggerOff();
        void Error();
    }

    public interface ILauncherHandlerEvent
    {
        void OnShowTrajectory(bool v);
        void OnBeforeFire();
        void OnAfterFire();
        void OnFired(IProjectile projectile);
    }

    public interface ILauncher : IDisposable
    {
        void Initialize();

        ProjectileConfig? config { get; }
        void SetProjectileConfig(ProjectileConfig? config, ProjectileOption? option = null);

        IMobileObject? target { get; }
        void SetTarget(IMobileObject? target);

        IReadOnlyReactiveProperty<bool> CanFire { get; }
        void Fire();
        void TriggerOn();
        void TriggerOff();

        ISubject<ProjectileConfig?> OnConfigChanged { get; }
        ISubject<IMobileObject?> OnTargetChanged { get; }
        ISubject<float> OnRecastTimeUpdated { get; }
        ISubject<IProjectile> OnFired { get; }
    }
}
