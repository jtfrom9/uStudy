using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using UniRx;
using Effect;

public class DamegeEffectTest : MonoBehaviour
{
    [SerializeField]
    List<Transform> targets = new List<Transform>();

    [SerializeField]
    Button button;

    [SerializeField]
    [Range(0.5f, 3.0f)]
    float duration = 1.0f;

    void Start()
    {
        button.OnClickAsObservable().Subscribe(async _ =>
        {
            button.interactable = false;
            var factory = FindObjectOfType<Effect.EffectFactory>();
            var effect = factory.CreateDamageEffect(null, Vector3.zero, 123);

            var tasks = new List<UniTask>() {
                effect.PlayAndDispose(duration)
            };
            foreach (var target in targets)
            {
                var e = factory.CreateDamageEffect(target, Vector3.up * 0.5f, 9);
                tasks.Add(e.PlayAndDispose(duration));
            }
            await UniTask.WhenAll(tasks);
            button.interactable = true;
        }).AddTo(this);
    }
}
