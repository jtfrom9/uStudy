using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public interface IEffect: System.IDisposable
    {
        void Initialize(Vector3 pos, Vector3 lookat);
        UniTask Play();
    }

    public interface IEffectFactory
    {
        IEffect Create(Vector3 pos, Vector3 lookat);
    }
}