#nullable enable

using System.Threading;
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
    [SerializeField] EnemyManagerObject? enemyManagerObject;
    [SerializeField] EnemyObject? defaultEnemyObject;
    [SerializeField] EnvironmentObject? environmentObject;
    [SerializeField] VisualizerObject? visualizerObject;
    [SerializeField] List<ProjectileObject> projectiles = new List<ProjectileObject>();
    [SerializeField] InputObservableMouseHandler? inputObservableCusrorManager;
    [SerializeField] Transform? cameraTarget;
    [SerializeField] List<Vector3> spawnPoints = new List<Vector3>();
    [SerializeField] bool randomWalk = true;
    [SerializeField] int spawnCondition = 10;

    [Inject] IEnemyManager? enemyManager;
    [Inject] IMouseOperation? mouseOperation;
    [Inject] ICursorFactory? cursorFactory;
    [Inject] ILauncher? launcher;
    [Inject] IEnvironment? environment;

    CompositeDisposable disposables = new CompositeDisposable();

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<EnemyManagerObject>(enemyManagerObject!);
        builder.RegisterInstance<EnvironmentObject>(environmentObject!);
        builder.RegisterInstance<VisualizerObject>(visualizerObject!).AsImplementedInterfaces();
        builder.Register<IEnemyManager, EnemyManagerImpl>(Lifetime.Singleton);
        builder.RegisterInstance<InputObservableMouseHandler>(inputObservableCusrorManager!)
            .AsImplementedInterfaces();
        builder.Register<LauncherImpl>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
        builder.Register<EnvironmentImpl>(Lifetime.Singleton)
            .AsImplementedInterfaces();
    }

    void Start()
    {
        if (enemyManager == null || defaultEnemyObject==null) return;
        enemyManager.Initialize(defaultEnemyObject);

        if(launcher==null) return;
        launcher.Initialize();

        if(cursorFactory==null) return;
        if(mouseOperation==null) return;
        if(cameraTarget==null) return;

        var token = this.GetCancellationTokenOnDestroy();
        if (randomWalk)
        {
            enemyManager.RandomWalk(-10f, 10f, 3000, token).Forget();
        }else
        {
            var gameSenario = new GameSenario(enemyManager,
                defaultEnemyObject,
                spawnPoints.ToArray(),
                Vector3.zero,
                spawnCondition);
            var cts = new CancellationTokenSource();
            gameSenario.Run(cts.Token).Forget();
        }

        var projectileSelection = new Selection<ProjectileObject>(projectiles);
        projectileSelection.OnCurrentChanged.Subscribe(p =>
        {
            launcher.SetProjectile(p);
        }).AddTo(this);

        setupKey(projectileSelection, launcher);

        launcher.OnProjectilehanged.Subscribe(p =>
        {
            showInfo(p);
        }).AddTo(this);

        setupMouse(mouseOperation, launcher, cursorFactory);

        projectileSelection.Select(projectileSelection.Index);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        disposables.Dispose();
    }

    void showInfo(ProjectileObject? projectile)
    {
        if(textMesh==null) return;
        if (projectile != null)
        {
            textMesh.text = @$"
Name: {projectile.name}
Type: {projectile.type}
Speed: {projectile.baseSpeed}
Distance: {projectile.range}
";
        } else {
            textMesh.text = "";
        }
    }

    void setupKey(Selection<ProjectileObject> configSelection, ILauncher launcher)
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
                        if(cursor==null) {
                            throw new InvalidConditionException("fail to create cursor");
                        }
                        cursor.Move(e.position);
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

        if (this.cameraTarget != null)
        {
            var speed = 20f;
            this.UpdateAsObservable().Where(_ => cursor != null).Subscribe(_ =>
            {
                var diff = cursor!.transform.Position - cameraTarget.position;
                if(diff.magnitude < 0.1f) {
                    cameraTarget.position = cursor!.transform.Position;
                }else
                {
                    cameraTarget.position += diff.normalized * Time.deltaTime * speed;
                }
            }).AddTo(this);
        }
    }
}
