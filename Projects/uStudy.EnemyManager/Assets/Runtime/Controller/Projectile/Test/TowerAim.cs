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

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<SimpleCursorManager>(Lifetime.Singleton);
        builder.Register<ILauncherManager, LauncherManager>(Lifetime.Singleton);
        builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
    }

    void Start()
    {
        if (enemyManager == null) return;
        enemyManager.Setup();

        if(cursorManager==null) return;
        if(launcher==null) return;

        var token = this.GetCancellationTokenOnDestroy();
        enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();

        var configSelection = new Selection<ProjectileConfig>(configs);
        configSelection.OnCurrentChanged.Subscribe(config =>
        {
            launcher.SetProjectileConfig(config);
        }).AddTo(this);

        setupKey(configSelection, launcher);

        launcher.OnConfigChanged.Subscribe(config =>
        {
            showConfigInfo(config);
        }).AddTo(this);

        launcher.CanFire.Subscribe(v => {
            Debug.Log($"CanLaunch: {v}");
        }).AddTo(this);

        launcher.OnRecastTimeUpdated.Subscribe(v => {
            Debug.Log($"Recast: {v}");
        }).AddTo(this);

        setupMouse(cursorManager, launcher);

        (cursorManager as ICursorManager).OnCursorCreated.Subscribe(cursor => {
            launcher.SetTarget(cursor);
        }).AddTo(this);

        configSelection.Select(configSelection.Index);
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

    void setupKey(Selection<ProjectileConfig> configSelection, ILauncherManager launcher)
    {
        this.UpdateAsObservable().Subscribe(_ =>
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                configSelection.Next();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                configSelection.Prev();
            }
        }).AddTo(this);
    }

    void setupNormalShotStyle(IInputObservable input, ILauncherManager launcher, SimpleCursorManager cursorManager)
    {
        input.OnBegin.Subscribe(e =>
        {
            cursorManager.Move(e.position);
            if (launcher.CanFire.Value)
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

    void setupMouse(SimpleCursorManager cursorManager,ILauncherManager launcher)
    {
        var context = this.DefaultInputContext();
        var input = context.GetObservable(0);
        setupNormalShotStyle(input, launcher, cursorManager);
        setupLongPressStyle(input, launcher, cursorManager);
    }
}
