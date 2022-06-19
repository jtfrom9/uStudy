using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Hedwig.Runtime;

public class EffectTestLifetimeScope : LifetimeScope
{
    [SerializeField]
    EffectAssets effectFactory;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<IEffectFactory>(effectFactory);
    }
}
