#nullable enable

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public static class EffectExtension {
        public static async UniTask PlayAndDispose(this IEffect effect)
        {
            await effect.Play();
            effect.Dispose();
        }
        public static async UniTask PlayAndDispose(this IEffect[] effects)
        {
            await UniTask.WhenAll(effects.Select(async effect =>
            {
                await effect.Play();
                effect.Dispose();
            }));
        }
    }

    public interface IEffect : IDisposable
    {
        UniTask Play();
    }

    [System.Serializable]
    public class DamageEffectParameter
    {
        [SerializeField]
        [Range(0.5f, 3.0f)]
        public float duration = 1f;
    }

    public interface IDamageEffect : IEffect
    {
        void Initialize(ITransformProvider parent, DamageEffectParameter duration, int damage);
    }

    public interface IDamageSoundEffect : IEffect
    {
        void Initialize(AudioClip audioClip, float volume);
    }

    public interface IHitEffect: IEffect
    {
        void Initialize(ITransformProvider parent, Vector3 position, Vector3 direction);
    }

    public interface IEffectFactory
    {
        IDamageEffect? CreateDamageEffect(ITransformProvider parent, int damage);
        IHitEffect? CreateHitEffect(ITransformProvider parent, Vector3 position, Vector3 direction);
        IHitEffect? CreateEnvironmentHitEffect(ITransformProvider parent, Vector3 position, Vector3 direction);
    }
}