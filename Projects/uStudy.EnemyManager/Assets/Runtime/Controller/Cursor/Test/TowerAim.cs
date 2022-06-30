#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniRx.Triggers;
using InputObservable;

using Hedwig.Runtime;

public class TowerAim : LifetimeScope
{
    [SerializeField] Setting? setting;
    [SerializeField] List<ProjectileConfig> configs = new List<ProjectileConfig>();

    [Inject] IEnemyManager? enemyManager;
    [Inject] Launcher? launcher;
    [Inject] ICursorFactory? cursorFactory;

    IFreeCursor? _cursor;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<Launcher>(Lifetime.Singleton);
    }

    void Start()
    {
        if (configs.Count == 0) { return; }
        var config = configs[0];
        if (enemyManager == null) return;
        enemyManager.Setup();

        if(launcher==null) return;

        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();

        // enemyManager.Enemies[0].Select(true);

        var selection = new SingleSelection(enemyManager.Enemies);
        selection.onCurrentChanged.Subscribe(selectable =>
        {
            launcher.SetTarget(selectable as IEnemy);
        }).AddTo(this);
        selection.SelectExclusive(0);

        launcher.OnConfigChanged.Subscribe(config => {
            Debug.Log($"config changed: {config?.name ?? "n/a"}");
        }).AddTo(this);

        setupKey(selection,launcher);
        setupMouse(launcher);

        launcher.SetProjectileConfig(config);
        launcher.ShowTrajectory(true);
    }

    void setupKey(SingleSelection selection, Launcher launcher) {
        this.UpdateAsObservable().Subscribe(_ => {
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                selection.Prev();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selection.Next();
            }
            if(Input.GetKeyDown(KeyCode.DownArrow)) {
                var cur = configs.FindIndex(0, configs.Count, (config) => config == launcher.config);
                var next = cur == configs.Count - 1 ? 0 : cur + 1;
                launcher.SetProjectileConfig(configs[next]);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                var cur = configs.FindIndex(0, configs.Count, (config) => config == launcher.config);
                var prev = cur == 0 ? configs.Count - 1 : cur - 1;
                launcher.SetProjectileConfig(configs[prev]);
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (launcher.CanLaunch)
                    launcher.Launch();
                // launcher.ShowTrajectory(!launcher.trajectory);
            }
        }).AddTo(this);
    }

    void setCursor(Vector3? pos) {
        if(pos.HasValue) {
            if (_cursor == null)
            {
                _cursor = cursorFactory?.CreateFreeCusor();
                launcher?.SetTarget(_cursor);
            }
            _cursor?.Move(pos.Value);
        }else {
            _cursor?.Dispose();
            _cursor = null;
        }
    }

    void setupMouse(Launcher launcher)
    {
        var input = this.DefaultInputContext().GetObservable(0);
        input.Any().Where(e => e.type != InputEventType.End).Subscribe(e =>
        {
            var ray = Camera.main.ScreenPointToRay(e.position);
            var hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.gameObject.CompareTag(Hedwig.Runtime.Collision.EnvironmentTag))
                {
                    setCursor(hit.point);
                }
            }
        }).AddTo(this);
        input.OnEnd.Subscribe(_ =>
        {
            setCursor(null);
        }).AddTo(this);
    }
}
