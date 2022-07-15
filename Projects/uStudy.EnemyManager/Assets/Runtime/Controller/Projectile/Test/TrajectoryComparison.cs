#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        Setting? setting;

        [SerializeField]
        List<ProjectileConfig> projectileConfigs = new List<ProjectileConfig>();

        [SerializeField]
        GameObject? launcherPrefab;

        [SerializeField]
        GameObject? targetPrefab;


        [SerializeField]
        Button? shotButton;

        [Inject] System.Func<Vector3, ILauncher>? launcherFactory;
        [Inject] IEnemyManager? enemyManager;

        List<(ILauncher launcher, IMobileObject target)> pairs = new List<(ILauncher launcher, IMobileObject target)>();

        protected override void Configure(IContainerBuilder builder)
        {
            if (setting == null) { Debug.LogError("setting is null"); return; }
            builder.RegisterInstance<Setting>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);

            if (launcherPrefab == null) { Debug.LogError("launcherPrefab is null"); return; }
            builder.RegisterFactory<Vector3, ILauncher>((resolver) =>
            {
                return (pos) =>
                {
                    var launcherController = Instantiate(launcherPrefab, pos, Quaternion.identity).GetComponent<ILauncherController>();
                    var projectileFactory = resolver.Resolve<IProjectileFactory>();
                    return new LauncherManager(projectileFactory, launcherController);
                };
            }, Lifetime.Transient);
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
            if(root==null) {throw new InvalidConditionException("no root"); }
            if (launcherFactory == null) { throw new InvalidConditionException("launcherFactory is null"); }
            var pos = new Vector3(index * 5, 0.5f, 0);

            if (targetPrefab == null) { throw new InvalidConditionException("targetprefab is null"); }

            var cube = Instantiate(targetPrefab, pos + Vector3.forward * 10, Quaternion.identity, root.transform);
            if(cube==null) throw new InvalidConditionException("fail to instantiate target");
            var target = cube.GetComponent<IEnemy>();
            var launcher = launcherFactory.Invoke(pos);
            launcher.Initialize();
            launcher.SetProjectileConfig(config);
            launcher.SetTarget(target);

            return (launcher, target);
        }

        void setupUI()
        {
            shotButton?.OnClickAsObservable().Subscribe(_ =>
            {
                var canFire = pairs.All(pair => pair.launcher.CanFire.Value);
                if (canFire)
                {
                    foreach (var pair in pairs)
                    {
                        pair.launcher.Fire();
                    }
                }
            }).AddTo(this);
        }
    }
}