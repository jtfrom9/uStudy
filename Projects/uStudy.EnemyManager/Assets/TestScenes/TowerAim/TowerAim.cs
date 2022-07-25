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

using Hedwig.Runtime;
using Hedwig.Runtime.InputObservable;

public class TowerAim : LifetimeScope
{
    // UI
    [SerializeField] TextMeshProUGUI? textMesh;

    // Inject
    [SerializeField] Setting? setting;
    [SerializeField] List<ProjectileConfig> configs = new List<ProjectileConfig>();
    [SerializeField] InputObservableMouseHandler? inputObservableCusrorManager;

    [Inject] IEnemyManager? enemyManager;
    [Inject] IMouseOperation? mouseOperation;
    [Inject] ICursorFactory? cursorFactory;
    [Inject] ILauncher? launcher;

    CompositeDisposable disposables = new CompositeDisposable();

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<Setting>(setting!)
            .AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
        builder.RegisterInstance<InputObservableMouseHandler>(inputObservableCusrorManager!)
            .AsImplementedInterfaces();
        builder.Register<LauncherImpl>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
    }

    void Start()
    {
        if (enemyManager == null) return;
        enemyManager.Initialize();

        if(launcher==null) return;
        launcher.Initialize();

        if(cursorFactory==null) return;
        if(mouseOperation==null) return;

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

        setupMouse(mouseOperation, launcher, cursorFactory);

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

    void setupMouse(IMouseOperation mouseOperation, ILauncher launcher, ICursorFactory cursorFactory)
    {
        IFreeCursor? cursor = null;
        mouseOperation.OnMove.Subscribe(e =>
        {
            switch (e.type)
            {
                case MouseMoveEventType.Enter:
                    if(cursor==null) {
                        cursor = cursorFactory.CreateFreeCusor();
                        cursor?.Move(e.position);
                        launcher.SetTarget(cursor);
                    }
                    break;
                case MouseMoveEventType.Over:
                    cursor?.Move(e.position);
                    break;
                case MouseMoveEventType.Exit:
                    if(cursor!=null) {
                        cursor.Dispose();
                        cursor = null;
                    }
                    break;
            }
        }).AddTo(this);

        mouseOperation.OnLeftClick.Subscribe(_ => {
            if (launcher.CanFire.Value)
            {
                launcher.Fire();
            }
            else
            {
                Debug.LogWarning("Cannot Fire Now");
            }
        }).AddTo(this);

        mouseOperation.OnLeftTrigger.Subscribe(trigger =>
        {
            if (trigger)
            {
                launcher.TriggerOn();
            }
            else
            {
                launcher.TriggerOff();
            }
        }).AddTo(this);
    }
}
