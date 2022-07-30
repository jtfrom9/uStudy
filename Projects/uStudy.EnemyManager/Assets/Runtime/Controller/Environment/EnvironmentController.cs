#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public class EnvironmentController : Controller, IEnvironmentController, IHitHandler
    {
        string _name = "";
        IEnvironmentEvent? environmentEvent = null;

        void IHitHandler.OnHit(IHitObject hitObject)
        {
            environmentEvent?.OnHit(hitObject);
        }

        static int count = 0;
        [RuntimeInitializeOnLoadMethod]
        void _InitializeOnEnterPlayMode()
        {
            count = 0;
        }

        string IEnvironmentController.name { get => _name; }
        void IEnvironmentController.Initialize(IEnvironmentEvent environmentEvent)
        {
            if(gameObject.name=="") {
                gameObject.name = $"Environment({count})";
                count++;
            }
            _name = gameObject.name;
            this.environmentEvent = environmentEvent;
        }
    }
}