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
    [Inject] SimpleCursorManager? cursorManager;
    [Inject] Launcher? launcher;

    CompositeDisposable disposables = new CompositeDisposable();
    InputObservableContext? context;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<SimpleCursorManager>(Lifetime.Singleton);
        builder.Register<Launcher>(Lifetime.Singleton);

        context = this.DefaultInputContext();
    }

    void Start()
    {
        if (configs.Count == 0) { return; }
        var config = configs[0];
        if (enemyManager == null) return;
        enemyManager.Setup();

        if(cursorManager==null) return;
        if(launcher==null) return;

        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();

         var selection = new SingleSelection(enemyManager.Enemies);
        selection.onCurrentChanged.Subscribe(selectable =>
        {
            launcher.SetTarget(selectable as IEnemy);
        }).AddTo(this);
        selection.SelectExclusive(0);

        launcher.OnConfigChanged.Subscribe(config => {
            Debug.Log($"config changed: {config?.name ?? "n/a"}");
            var style = InputStyle.Normal;
            if (config != null)
            {
                switch (config.type)
                {
                    case ProjectileType.Fire:
                        style = InputStyle.Normal;
                        break;
                    case ProjectileType.Burst:
                        style = InputStyle.MoveOnly;
                        break;
                }
                setupMouse(launcher, cursorManager, style);
            }else {
                disableMouse();
            }
        }).AddTo(this);

        launcher.OnCanFireChanged.Subscribe(v => {
            Debug.Log($"CanLaunch: {v}");
        }).AddTo(this);

        launcher.OnRecastTimeUpdated.Subscribe(v => {
            Debug.Log($"Recast: {v}");
        }).AddTo(this);

        setupKey(selection,launcher);

        (cursorManager as ICursorManager).OnCursorCreated.Subscribe(cursor => {
            launcher.SetTarget(cursor);
        }).AddTo(this);

        launcher.SetProjectileConfig(config);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        disposables.Dispose();
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
            // if(Input.GetKeyDown(KeyCode.Space))
            // {
            //     if (launcher.CanLaunch)
            //         launcher.Launch();
            //     // launcher.ShowTrajectory(!launcher.trajectory);
            // }
        }).AddTo(this);
    }

    void setupNormalShotStyle(IInputObservable input, Launcher launcher, SimpleCursorManager cursorManager)
    {
        input.OnBegin.Subscribe(async e =>
        {
            cursorManager.Move(e.position);
            if (launcher.CanFire)
            {
                await launcher.Fire();
                cursorManager.Reset();
            }
            else
            {
                Debug.LogWarning("recasting");
            }
        }).AddTo(disposables);
    }

    void setupLongPressStyle(IInputObservable input, Launcher launcher, SimpleCursorManager cursorManager)
    {
        input.Keep(100, () => true).Subscribe(e =>
        {
            launcher.StartFire();
        }).AddTo(disposables);

        input.OnEnd.Subscribe(e =>
        {
            launcher.EndFire();
        }).AddTo(disposables);
    }

    void setupMoveOnly(IInputObservable input, Launcher launcher, SimpleCursorManager cursorManager)
    {
        input.Any().Where(e => e.type != InputEventType.End).Subscribe(e =>
        {
            cursorManager.Move(e.position);
        }).AddTo(disposables);
        input.OnEnd.Subscribe(e =>
        {
            cursorManager.Reset();
        }).AddTo(disposables);
    }

    enum InputStyle
    {
        Normal,
        MoveOnly
    };
    void setupMouse(Launcher launcher, SimpleCursorManager cursorManager, InputStyle style)
    {
        if(context==null) {
            Debug.LogError("No Input Context");
            return;
        }
        var input = context.GetObservable(0);
        disposables.Clear();
        if (style == InputStyle.Normal)
        {
            setupNormalShotStyle(input, launcher, cursorManager);
            launcher.ShowTrajectory(false);
        }
        setupLongPressStyle(input, launcher, cursorManager);
        if (style == InputStyle.MoveOnly)
        {
            setupMoveOnly(input, launcher, cursorManager);
            launcher.ShowTrajectory(true);
        }
    }
    void disableMouse() {
        disposables.Clear();
    }
}
