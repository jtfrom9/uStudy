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
        void StartFire(ITransform start, ITransform target);
        void EndFire(ITransform start, ITransform target);
    }

    public interface ILauncher : IDisposable
    {
        void Initialize();

        ProjectileConfig? config { get; }
        void SetProjectileConfig(ProjectileConfig? config);

        IMobileObject? target { get; }
        void SetTarget(IMobileObject? target);

        IReadOnlyReactiveProperty<bool> CanFire { get; }
        void Fire();
        void StartFire();
        void EndFire();

        ISubject<ProjectileConfig?> OnConfigChanged { get; }
        ISubject<IMobileObject?> OnTargetChanged { get; }
        ISubject<float> OnRecastTimeUpdated { get; }
    }

    public interface ILauncherManager
    {
        ILauncher launcher { get; }
        void ShowTrajectory(bool v);
        void OnBeforeLaunched();
        void OnLaunched();
    }
}
