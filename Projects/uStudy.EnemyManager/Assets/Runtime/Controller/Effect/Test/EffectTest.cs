using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using UniRx;

using Hedwig.Runtime;

public class EffectTest : MonoBehaviour
{
    [SerializeField]
    List<Transform> targets = new List<Transform>();

    [SerializeField]
    Button hitButton;
    [SerializeField]
    Toggle continuousToggle;

    IEffect[] createEffects(IEffectFactory factory, Transform target, int damage, Vector3 position)
    {
        return new IEffect[] {
            factory.CreateDamageEffect(target, damage),
            factory.CreateHitEffect(target, position, Vector3.zero)
        }.Where(e => e != null).ToArray();
    }

    Vector3 hitPosition(Transform target) {
        var position = Vector3.zero;
        if (target != null)
        {
            var hit = new RaycastHit();
            var origin = Camera.main.transform.position;
            var dir = target.position - origin;
            if (!Physics.Raycast(origin, dir, out hit, 100))
            {
                Debug.LogError("failed not raycast");
                return Vector3.zero;
            }
            Debug.Log($"{target.gameObject.name}: {hit.point}");
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    void Awake()
    {
        foreach(var target in targets) {
            var mr = target.gameObject.GetComponent<MeshRenderer>();
            mr.material.color = new Color(0.1f, 0.1f, 0.1f);
        }
    }

    void Start()
    {
        hitButton.OnClickAsObservable().Subscribe(async _ =>
        {
            if (!continuousToggle.isOn)
            {
                hitButton.interactable = false;
            }

            var factory = FindObjectOfType<EffectFactory>();
            var _targets = new Transform[] { null }.Concat(targets.ToArray());

            var tasks = new List<UniTask>();

            foreach (var target in _targets)
            {
                var e = createEffects(factory, target, Random.Range(1, 30), hitPosition(target));
                tasks.Add(e.PlayAndDispose());
            }
            // Debug.Break();
            await UniTask.WhenAll(tasks);

            if (!continuousToggle.isOn)
            {
                hitButton.interactable = true;
            }
        }).AddTo(this);
    }
}
