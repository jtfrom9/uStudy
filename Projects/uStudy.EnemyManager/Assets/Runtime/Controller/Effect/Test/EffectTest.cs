using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using UniRx;
using VContainer;
using VContainer.Unity;

using Hedwig.Runtime;

public class EffectTest : LifetimeScope
{
    [SerializeField]
    Setting setting;

    [SerializeField]
    List<GameObject> targets = new List<GameObject>();

    [SerializeField]
    Button hitButton;

    [SerializeField]
    Toggle continuousToggle;

    [Inject]
    IEffectFactory factory;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<IEffectFactory>(setting);

        foreach (var target in targets)
        {
            var mr = target.transform.gameObject.GetComponent<MeshRenderer>();
            mr.material.color = new Color(0.1f, 0.1f, 0.1f);
        }
    }

    IEffect[] createEffects(IMobileObject target, int damage, Vector3 position)
    {
        return new IEffect[] {
            factory.CreateDamageEffect(target, damage),
            factory.CreateHitEffect(target, position, Vector3.zero)
        }.Where(e => e != null).ToArray();
    }

    Vector3 hitPosition(IMobileObject target) {
        var position = Vector3.zero;
        if (target != null)
        {
            var hit = new RaycastHit();
            var origin = Camera.main.transform.position;
            var dir = target.transform.Position - origin;
            if (!Physics.Raycast(origin, dir, out hit, 100))
            {
                Debug.LogError("failed not raycast");
                return Vector3.zero;
            }
            return hit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    class DummyMobileObject : MonoBehaviour, IMobileObject
    {
        ITransform _transform = new CachedTransform();
        string IMobileObject.Name { get => gameObject.name; }
        ITransform IMobileObject.transform { get => _transform; }

        public void Dispose() { Destroy(gameObject); }
        void Awake()
        {
            _transform.Initialize(transform);
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

            var _targets = targets.Select(target =>
            {
                return target.AddComponent<DummyMobileObject>() as IMobileObject;
            });

            var tasks = new List<UniTask>();

            foreach (var target in _targets)
            {
                var e = createEffects(target, Random.Range(1, 30), hitPosition(target));
                tasks.Add(e.PlayAndDispose());
            }
            await UniTask.WhenAll(tasks);

            if (!continuousToggle.isOn)
            {
                hitButton.interactable = true;
            }
        }).AddTo(this);
    }
}
