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
        EnvironmentObject environmentObject;

        void IEnvironmentEvent.OnHit(IHitObject hitObject)
        {
            Debug.Log($"{this}: OnHit");
            var effects = environmentObject.CreateEffects(this, 
                hitObject.position, 
                -hitObject.direction);
            // effect?.PlayAndDispose().Forget();
            foreach(var effect in effects) {
                effect.PlayAndDispose().Forget();
            }
        }

        IEnvironmentController IEnvironment.controller { get => environmentController; }

        public override string ToString()
        {
            return $"{environmentController.name}.Impl";
        }

        public EnvironmentImpl(EnvironmentObject environmentObject)
        {
            this.environmentObject = environmentObject;
            environmentController = Controller.Find<IEnvironmentController>();
            environmentController.Initialize(this);
        }
    }
}