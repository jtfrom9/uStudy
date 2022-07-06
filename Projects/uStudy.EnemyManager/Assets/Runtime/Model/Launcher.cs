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
        IMobileObject? target { get; }
        bool CanLaunch { get; }
        void SetTarget(IMobileObject? target);
        void Initialize(ILauncherManager launcherManager);
    }

    public interface ILauncherHandler : IDisposable
    {
        void Fire(ITransform start, ITransform target);
        void StartFire(ITransform start, ITransform target);
        void EndFire(ITransform start, ITransform target);
    }

    public interface ILauncherManager: IDisposable
    {
        ProjectileConfig? config { get; }
        void SetProjectileConfig(ProjectileConfig? config);
        void SetTarget(IMobileObject? target);
        void ShowTrajectory(bool v);

        IReadOnlyReactiveProperty<bool> CanFire { get; }
        void Fire();
        void StartFire();
        void EndFire();

        ISubject<ProjectileConfig?> OnConfigChanged { get; }
        ISubject<float> OnRecastTimeUpdated { get; }

        void OnBeforeLaunched();
        void OnLaunched();
    }
}
