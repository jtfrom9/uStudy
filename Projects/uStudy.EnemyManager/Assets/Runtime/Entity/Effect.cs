using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Effect
{
    public static class EffectExtension {
        public static async UniTask PlayAndDispose(this IEffect effect, float duration)
        {
            await effect.Play(duration);
            effect.Dispose();
        }
    }

    public interface IEffect : System.IDisposable
    {
        UniTask Play(float duration);
    }

    public interface IDamageEffect : IEffect
    {
        void Initialize(Transform parent, Vector3 pos, Transform gazeTarget, int damage);
    }

    public interface IEffectFactory
    {
        IDamageEffect CreateDamageEffect(Transform parent, Vector3 pos, int damage);
    }
}