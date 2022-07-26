#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class TrajectoryComparison : LifetimeScope
    {
        [SerializeField]
        Factory? setting;

        [SerializeField]
        List<ProjectileConfig> projectileConfigs = new List<ProjectileConfig>();

        [SerializeField]
        GameObject? launcherPrefab;

        [SerializeField]
        GameObject? targetPrefab;

        [SerializeField]
        TextMeshProUGUI? distanceTextMesh;

        [SerializeField]
        Slider? distanceSlider;

        [SerializeField]
        Button? fireButton;

        [SerializeField]
        Button? triggerButton;

        [Inject] System.Func<(Vector3 pos, ProjectileConfig config), ILauncher>? launcherFactory;
        [Inject] IEnemyManager? enemyManager;

        List<(ILauncher launcher, IMobileObject target)> pairs = new List<(ILauncher launcher, IMobileObject target)>();

        protected override void Configure(IContainerBuilder builder)
        {
            var root = GameObject.Find("Root");
            if (root == null) { throw new InvalidConditionException("no root"); }

            if (setting == null) { Debug.LogError("setting is null"); return; }
            builder.RegisterInstance<Factory>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);

            if (launcherPrefab == null) { Debug.LogError("launcherPrefab is null"); return; }
            builder.RegisterFactory<(Vector3 pos, ProjectileConfig config), ILauncher>((resolver) =>
            {
                return (x) =>
                {
                    var go = Instantiate(launcherPrefab, x.pos, Quaternion.identity, root.transform);
                    var launcherController = go.GetComponent<ILauncherController>();
                    var projectileFactory = resolver.Resolve<IProjectileFactory>();
                    addConfigInfo(go, x.config);
                    return new LauncherImpl(projectileFactory, launcherController);
                };
            }, Lifetime.Transient);
        }

        void addConfigInfo(GameObject gameObject, ProjectileConfig config) {
            var textGameObject = new GameObject("text");
            textGameObject.transform.SetParent(gameObject.transform);
            textGameObject.transform.localPosition = Vector3.forward * (-3);
            var textMesh = textGameObject.AddComponent<TextMeshPro>();
            textMesh.fontSize = 6;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.text = config.name;
        }

        void Start()
        {
            for (var i = 0; i < this.projectileConfigs.Count; i++)
            {
                pairs.Add(createInstance(i, projectileConfigs[i]));
            }
            setupUI();

            if (enemyManager == null) { Debug.LogError("no enemyManager"); return; }
            enemyManager.Initialize();
        }

        (ILauncher, IMobileObject) createInstance(int index, ProjectileConfig config)
        {
            var root = GameObject.Find("Root");
            if (root == null) { throw new InvalidConditionException("no root"); }
            if (launcherFactory == null) { throw new InvalidConditionException("launcherFactory is null"); }
            var pos = new Vector3(index * 5, 0.5f, 0);

            if (targetPrefab == null) { throw new InvalidConditionException("targetprefab is null"); }

            var cube = Instantiate(targetPrefab, pos + Vector3.forward * 10, Quaternion.identity, root.transform);
            if (cube == null) throw new InvalidConditionException("fail to instantiate target");
            var target = cube.GetComponent<IEnemy>();
            var launcher = launcherFactory.Invoke((pos, config));
            launcher.Initialize();
            launcher.SetProjectileConfig(config, new ProjectileOption() { destroyAtEnd = false });
            launcher.SetTarget(target);


            launcher.OnFired.Subscribe(projectile =>
            {
                projectile.OnEnded.Subscribe(_ =>
                {
                    if (projectile.trajectoryMap != null)
                    {
                        string log = "";
                        foreach (var section in projectile.trajectoryMap.Sections)
                        {
                            log += $"{section} ({section.numLines} lines), ";
                        }
                        Debug.Log($"{projectile} <<Ended>> {log}");
                        projectile.Dispose();
                    }
                });
            }).AddTo(this);

            return (launcher, target);
        }

        void setupUI()
        {
            var canfire = false;
            this.ObserveEveryValueChanged(_ => this.pairs.All(pair => pair.launcher.CanFire.Value)).Subscribe(v =>
            {
                canfire = v;
                if (fireButton != null) fireButton.interactable = canfire;
                if (triggerButton != null) triggerButton.interactable = canfire;
            }).AddTo(this);

            fireButton?.OnClickAsObservable().Where(_ => canfire).Subscribe(_ =>
            {
                foreach (var pair in pairs)
                {
                    pair.launcher.Fire();
                }
            }).AddTo(this);

            var trigger = false;
            triggerButton?.OnPointerDownAsObservable().Where(_ => canfire && !trigger).Subscribe(_ =>
            {
                foreach (var pair in pairs)
                {
                    pair.launcher.TriggerOn();
                }
                trigger = true;
            }).AddTo(this);

            triggerButton?.OnPointerUpAsObservable().Where(_ => canfire && trigger).Subscribe(_ =>
            {
                foreach (var pair in pairs)
                {
                    pair.launcher.TriggerOff();
                }
                trigger = false;
            }).AddTo(this);

            distanceSlider?.OnValueChangedAsObservable().Subscribe(v => { 
                if(distanceTextMesh!=null) {
                    distanceTextMesh.text = $"Distance: {v}";
                    foreach (var pair in pairs)
                    {
                        var component = (pair.target as Component);
                        if (component != null)
                        {
                            component.transform.position = pair.target.transform.Position.Z(v);
                        }
                    }
                }
            }).AddTo(this);
        }
    }
}