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
        void Initialize(ILauncherManager launcherManager);
    }

    public interface ILauncherHandler : IDisposable
    {
        void Fire(ITransform start, ITransform target);
        void TriggerOn(ITransform start, ITransform target);
        void TriggerOff(ITransform start, ITransform target);
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

    public interface ILauncherManager
    {
        ILauncher launcher { get; }
        void ShowTrajectory(bool v);
        void BeforeFire();
        void AfterFire();
        void OnFired(IProjectile projectile);
    }
}
