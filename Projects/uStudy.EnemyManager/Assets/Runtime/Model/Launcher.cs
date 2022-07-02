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
    }

    public interface ILauncherHandler : IDisposable
    {
        UniTask Fire();
        void StartFire();
        void EndFire();
    }

    public interface ILauncherManager
    {
        ProjectileConfig? config { get; }
        void SetProjectileConfig(ProjectileConfig? config);
        void SetTarget(IMobileObject? target);
        void ShowTrajectory(bool v);

        bool CanFire { get; }
        void Fire();
        void StartFire();
        void EndFire();

        ISubject<ProjectileConfig?> OnConfigChanged { get; }
        ISubject<bool> OnCanFireChanged { get; }
        ISubject<float> OnRecastTimeUpdated { get; }

        void OnBeforeLaunched();
        void OnLaunched();
    }
}
