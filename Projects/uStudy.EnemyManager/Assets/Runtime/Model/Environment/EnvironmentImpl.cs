#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class EnvironmentImpl : IEnvironment, IEnvironmentEvent
    {
        IEnvironmentController environmentController;
        IEffectFactory effectFactory;

        void IEnvironmentEvent.OnHit(IHitObject hitObject)
        {
            Debug.Log($"{this}: OnHit");
            var effect = effectFactory.CreateEnvironmentHitEffect(environmentController,
                hitObject.position,
                -hitObject.direction);
            effect?.PlayAndDispose().Forget();
        }

        public override string ToString()
        {
            return $"{environmentController.name}.Impl";
        }

        public EnvironmentImpl(IEffectFactory effectFactory)
        {
            this.effectFactory = effectFactory;
            environmentController = Controller.Find<IEnvironmentController>();
            environmentController.Initialize(this);
        }
    }
}