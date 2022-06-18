#nullable enable

using System.Collections;
using System.Collections.Generic;
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

    public interface IEffect : System.IDisposable
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
        void Initialize(Transform parent, Transform gazeTarget, DamageEffectParameter duration, int damage);
    }

    public interface IHitEffect: IEffect
    {
        void Initialize(Transform parent, Transform gazeTarget, Vector3 position, Vector3 normal);
    }

    public interface IEffectFactory
    {
        IDamageEffect? CreateDamageEffect(Transform parent, int damage);
        IHitEffect? CreateHitEffect(Transform parent, Vector3 position, Vector3 normal);
    }
}