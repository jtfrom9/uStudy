#nullable enable

using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class ParticleEffectTest : MonoBehaviour
    {
        [SerializeField]
        Button? button;

        void Start()
        {
            var effect = Controller.Find<IHitEffect>();
            Debug.Log(effect);
            var transform = gameObject.AsTransformProvider();
            effect.Initialize(transform, Vector3.zero, Vector3.up);
            button?.OnClickAsObservable().Subscribe(async _ =>
            {
                await effect.PlayAndDispose();
            }).AddTo(this);
        }
    }
}