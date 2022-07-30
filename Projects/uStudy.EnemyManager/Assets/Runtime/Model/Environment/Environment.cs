#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public interface IEnvironmentController
    {
        void Initialize(IEnvironmentEvent environmentEvent);
        string name { get; }
    }

    public interface IEnvironmentEvent
    {
        void OnHit(IHitObject hitObject);
    }

    public interface IEnvironment
    {}
}