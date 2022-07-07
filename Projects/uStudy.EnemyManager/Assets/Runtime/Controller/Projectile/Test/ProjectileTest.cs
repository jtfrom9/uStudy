#nullable enable

using System.Threading;
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
    public class ProjectileTest : LifetimeScope
    {
        [SerializeField]
        Setting? setting;

        [SerializeField]
        List<ProjectileConfig> projectileConfigs = new List<ProjectileConfig>();

        [Inject] IEnemyManager? enemyManager;
        [Inject] ILauncher? launcher;
        [Inject] IProjectileFactory? projectileFactory;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<Setting>(setting!)
                .AsImplementedInterfaces();
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.Register<LauncherManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterInstance<ILauncherController>(Controller.Find<ILauncherController>());
        }

        void Start()
        {
            if (enemyManager == null) return;
            enemyManager.Initialize();
            if (launcher == null) return;
            launcher.Initialize();
            if(projectileFactory==null) return;

            setupDebug(projectileFactory);

            var enemySelection = new SelectiveSelection(enemyManager.Enemies);
            enemySelection.OnCurrentChanged.Subscribe(selectable =>
            {
                launcher.SetTarget(selectable as IEnemy);
                // launcher.ShowTrajectory(true);
            }).AddTo(this);

            enemySelection.SelectExclusive(0);

            var configSelection = new Selection<ProjectileConfig>(projectileConfigs);
            configSelection.OnCurrentChanged.Subscribe(config =>
            {
                launcher.SetProjectileConfig(config);
            }).AddTo(this);
            configSelection.Select(0);

            this.UpdateAsObservable().Subscribe(_ =>
            {
                var enemy = enemySelection.Current as IEnemy;
                if (enemy == null) return;
                _update(launcher, enemy, enemySelection, configSelection);
            }).AddTo(this);

            launcher.CanFire.Subscribe(can =>
            {
                if (can)
                {
                    if (launcher.config!.type == ProjectileType.Fire)
                    {
                        launcher.Fire();
                    }
                }
            }).AddTo(this);

        }

        void OnApplicationQuit() {
            Debug.Log($"Quit: frame:{Time.frameCount}");
            launcher?.Dispose();
        }

        void _update(ILauncher launcher, IEnemy enemy, SelectiveSelection enemySelection, Selection<ProjectileConfig> configSelection)
        {
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                enemySelection.Next();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                enemySelection.Prev();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                configSelection.Next();
            }
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                configSelection.Prev();
            }
        }

        void setupDebug(IProjectileFactory projectileFactory)
        {
            projectileFactory.OnCreated.Subscribe(projectile =>
            {
                int willHitFrame = 0;
                projectile.OnEvent.Subscribe(e => {
                    var lateCount = (willHitFrame == 0) ? "-" : (Time.frameCount - willHitFrame).ToString();
                    var stopwatch = new System.Diagnostics.Stopwatch();
                    switch(e.type) {
                        case Projectile.EventType.BeforeLoop:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} BeforeLoop");
                            stopwatch.Start();
                            break;
                        case Projectile.EventType.AfterLoop:
                            stopwatch.Stop();
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} AfterLoop @({projectile.transform.Position}, elapsed:{stopwatch.ElapsedMilliseconds}ms");
                            break;
                        case Projectile.EventType.Destroy:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} Destroy reson:{projectile.EndReason}");
                            break;
                        case Projectile.EventType.BeforeMove:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} BeforeMove @{projectile.transform.Position}");
                            break;
                        case Projectile.EventType.AfterMove:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} AfterMove @{projectile.transform.Position} (late:{lateCount})");
                            break;
                        case Projectile.EventType.BeforeLastMove:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} BeforeLastMove @{projectile.transform.Position} -> {e.willHit!.Value.point} (late:{lateCount})");
                            break;
                        case Projectile.EventType.AfterLastMove:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} AfterLastMove @{projectile.transform.Position} (late:{lateCount})");
                            break;
                        case Projectile.EventType.WillHit:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} WillHit @{projectile.transform.Position} ray: {e.ray}, maxDist: {e.maxDistance!.Value} point: {e.willHit!.Value.point} distance: {e.willHit!.Value.distance}");
                            willHitFrame = Time.frameCount;
                            break;
                        case Projectile.EventType.Trigger:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} Trigger with <{e.collider!.gameObject.name}> @{projectile.transform.Position} (late:{lateCount})");
                            break;
                        case Projectile.EventType.OnKill:
                            Debug.Log($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} OnKill (late:{lateCount})");
                            break;
                        case Projectile.EventType.OnComplete:
                            Debug.Log($"[{GetHashCode():x}] frame:{Time.frameCount} OnComplete (late:{lateCount})");
                            break;
                        case Projectile.EventType.OnPause:
                            Debug.Log($"[{GetHashCode():x}] frame:{Time.frameCount} OnPause (late:{lateCount})");
                            break;
                    }
                }).AddTo(this);

                projectile.transform.OnPositionChanged.Subscribe(pos => {
                    if (pos.z > 1)
                    {
                        Debug.LogWarning($"[{projectile.GetHashCode():x}] frame:{Time.frameCount} {pos}");
                        // Debug.Break();
                        projectile.Dispose();
                    }
                }).AddTo(this);

            }).AddTo(this);
        }
    }
}