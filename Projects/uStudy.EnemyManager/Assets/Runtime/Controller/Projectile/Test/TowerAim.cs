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
    [Inject] ILauncher? launcher;

    CompositeDisposable disposables = new CompositeDisposable();

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.Register<SimpleCursorManager>(Lifetime.Singleton);
        builder.Register<LauncherManager>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
    }

    void Start()
    {
        if (enemyManager == null) return;
        enemyManager.Initialize();

        if(cursorManager==null) return;
        if(launcher==null) return;
        launcher.Initialize();

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
Speed: {config.baseSpeed}
Distance: {config.range}
";
        } else {
            textMesh.text = "";
        }
    }

    void setupKey(Selection<ProjectileConfig> configSelection, ILauncher launcher)
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

    void setupNormalShotStyle(IInputObservable input, ILauncher launcher, SimpleCursorManager cursorManager)
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

    void setupLongPressStyle(IInputObservable input, ILauncher launcher, SimpleCursorManager cursorManager)
    {
        input.Keep(100, () => true).First().TakeUntil(input.OnEnd)
            .Repeat().Subscribe(e =>
        {
            launcher.TriggerOn();
        }).AddTo(disposables);

        input.OnMove.Subscribe(e =>
        {
            cursorManager.Move(e.position);
        }).AddTo(disposables);

        input.OnEnd.Subscribe(e =>
        {
            launcher.TriggerOff();
        }).AddTo(disposables);
    }

    void setupMoveOnly(IInputObservable input, ILauncher launcher, SimpleCursorManager cursorManager)
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

    void setupMouse(SimpleCursorManager cursorManager,ILauncher launcher)
    {
        var context = this.DefaultInputContext();
        var input = context.GetObservable(0);
        setupNormalShotStyle(input, launcher, cursorManager);
        setupLongPressStyle(input, launcher, cursorManager);
    }
}
