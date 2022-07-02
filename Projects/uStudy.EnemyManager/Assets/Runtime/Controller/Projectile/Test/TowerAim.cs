#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniRx.Triggers;
using InputObservable;

using Hedwig.Runtime;

public class TowerAim : LifetimeScope
{
    // UI
    [SerializeField] TextMeshProUGUI? textMesh;

    // Inject
    [SerializeField] Setting? setting;
    [SerializeField] List<ProjectileConfig> configs = new List<ProjectileConfig>();

    [Inject] IEnemyManager? enemyManager;
    [Inject] SimpleCursorManager? cursorManager;
    [Inject] ILauncherManager? launcher;

    CompositeDisposable disposables = new CompositeDisposable();
    InputObservableContext? context;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<SimpleCursorManager>(Lifetime.Singleton);
        builder.Register<ILauncherManager, LauncherManager>(Lifetime.Singleton);
        builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
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

        launcher.OnConfigChanged.Subscribe(config =>
        {
            showConfigInfo(config);
        }).AddTo(this);
        // launcher.OnConfigChanged.Subscribe(config => {
        //     Debug.Log($"config changed: {config?.name ?? "n/a"}");
        //     var style = InputStyle.Normal;
        //     if (config != null)
        //     {
        //         switch (config.type)
        //         {
        //             case ProjectileType.Fire:
        //                 style = InputStyle.Normal;
        //                 break;
        //             case ProjectileType.Burst:
        //                 style = InputStyle.MoveOnly;
        //                 break;
        //         }
        //         setupMouse(launcher, cursorManager, style);
        //     }else {
        //         disableMouse();
        //     }
        // }).AddTo(this);
        setupMouse(launcher, cursorManager);

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

    void showConfigInfo(ProjectileConfig? config)
    {
        if(textMesh==null) return;
        if (config != null)
        {
            textMesh.text = @$"
Name: {config.name}
Type: {config.type}
Speed: {config.speed}
Distance: {config.distance}
";
        } else {
            textMesh.text = "";
        }
    }

    void setupKey(SingleSelection selection, ILauncherManager launcher) {
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

    void setupNormalShotStyle(IInputObservable input, ILauncherManager launcher, SimpleCursorManager cursorManager)
    {
        input.OnBegin.Subscribe(e =>
        {
            cursorManager.Move(e.position);
            if (launcher.CanFire)
            {
                launcher.Fire();
            }
            else
            {
                Debug.LogWarning("recasting");
            }
        }).AddTo(disposables);
    }

    void setupLongPressStyle(IInputObservable input, ILauncherManager launcher, SimpleCursorManager cursorManager)
    {
        input.Keep(100, () => true).First().TakeUntil(input.OnEnd)
            .Repeat().Subscribe(e =>
        {
            launcher.StartFire();
        }).AddTo(disposables);

        input.OnMove.Subscribe(e =>
        {
            cursorManager.Move(e.position);
        }).AddTo(disposables);

        input.OnEnd.Subscribe(e =>
        {
            launcher.EndFire();
        }).AddTo(disposables);
    }

    void setupMoveOnly(IInputObservable input, ILauncherManager launcher, SimpleCursorManager cursorManager)
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

    void setupMouse(ILauncherManager launcher, SimpleCursorManager cursorManager)
    {
        if (context == null)
        {
            Debug.LogError("No Input Context");
            return;
        }
        var input = context.GetObservable(0);
        setupNormalShotStyle(input, launcher, cursorManager);
        setupLongPressStyle(input, launcher, cursorManager);
    }


    // enum InputStyle
    // {
    //     Normal,
    //     MoveOnly
    // };
    // void setupMouse(ILauncherManager launcher, SimpleCursorManager cursorManager, InputStyle style)
    // {
    //     if(context==null) {
    //         Debug.LogError("No Input Context");
    //         return;
    //     }
    //     var input = context.GetObservable(0);
    //     disposables.Clear();
    //     if (style == InputStyle.Normal)
    //     {
    //         setupNormalShotStyle(input, launcher, cursorManager);
    //         launcher.ShowTrajectory(false);
    //     }
    //     setupLongPressStyle(input, launcher, cursorManager);
    //     if (style == InputStyle.MoveOnly)
    //     {
    //         setupMoveOnly(input, launcher, cursorManager);
    //         launcher.ShowTrajectory(true);
    //     }
    // }
    // void disableMouse() {
    //     disposables.Clear();
    // }
}
