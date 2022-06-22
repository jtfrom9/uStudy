#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using DG.Tweening;

namespace Hedwig.Runtime
{
    public class SelectorTest : LifetimeScope
    {
        [SerializeField]
        SelectorAssets? selectorAssets;

        [SerializeField]
        Button? goButton;

        [SerializeField]
        Button? resetButton;

        [SerializeField]
        TMP_Dropdown? dropdown;

        [SerializeField]
        GameObject? bulletPrefab;

        [Inject] IEnemyManager? enemyManager;

        [SerializeField]
        Vector3 tower = new Vector3(10, 20, -10);

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEffectFactory, DummyEffectFactory>(Lifetime.Singleton);
            builder.Register<IEnemyManager, EnemyManager>(Lifetime.Singleton);
            builder.RegisterInstance<ISelectorFactory>(selectorAssets!);
        }

        void Start()
        {
            if (enemyManager == null)
            {
                Debug.LogError($"enemyManager: {enemyManager}");
                return;
            }

            var selection = new SingleSelection(enemyManager.Enemies);
            selection.SelectExclusive(0);

            setupUI(selection, enemyManager);

            this.UpdateAsObservable().Subscribe(_ =>
            {
                update(selection, enemyManager);
            }).AddTo(this);
        }

        void resetCam()
        {
            Camera.main.ResetPosition(new Vector3(0, 1, -10), Quaternion.identity);
        }

        void trackCam(ISelectable? selectable)
        {
            var enemy = selectable as IEnemy;
            if (enemy == null) return;

            if (towerView)
            {
                Camera.main.MoveWithLookAt(tower, enemy.transform.position, 1);
            }
            else
            {
                if (birdView)
                {
                    Camera.main.Tracking(enemy.transform,
                        new Vector3(0, 10, -3),
                        new Vector3(80, 0, 0),
                        1);
                }
                else
                {
                    Camera.main.Tracking(enemy.transform,
                        new Vector3(0, 3f, -3),
                        new Vector3(30, 0, 0),
                        1);
                }
            }
        }

        bool tracking { get => dropdown!.options[dropdown.value].text != "Init"; }
        bool birdView { get => dropdown!.options[dropdown.value].text == "Bird"; }
        bool towerView { get => dropdown!.options[dropdown.value].text == "Tower"; }

        void selectNext(SingleSelection selection, IEnemyManager enemyManager)
        {
            selection.Next();
            if(tracking) trackCam(selection.Current);
        }

        void selectPrev(SingleSelection selection, IEnemyManager enemyManager)
        {
            selection.Prev();
            if (tracking) trackCam(selection.Current);
        }

        void setupUI(SingleSelection selection, IEnemyManager enemyManager)
        {
            bool go = false;
            var tmp = goButton!.GetComponentInChildren<TextMeshProUGUI>();
            var ct = new CancellationTokenSource();

            goButton?.OnClickAsObservable().Subscribe(_ =>
            {
                go = !go;
                tmp!.text = (go) ? "Stop" : "Go";
                if (go) {
                    enemyManager.RandomWalk(-10f, 10f, 3000, ct.Token).Forget();
                } else
                {
                    ct.Cancel();
                    ct = new CancellationTokenSource();
                    enemyManager.StopAll();
                }
            }).AddTo(this);

            resetButton?.OnClickAsObservable().Subscribe(_ =>
            {
                foreach (var enemy in enemyManager.Enemies)
                {
                    enemy.GetControl().ResetPos();
                }
            }).AddTo(this);

            if (dropdown != null)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData("Init"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Track"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Bird"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Tower"));
            }
            dropdown.ObserveEveryValueChanged(v => v!.value).Subscribe(v =>
            {
                Debug.Log(dropdown!.options[v].text);
                Debug.Log(tracking);
                if (tracking)
                {
                    trackCam(selection.Current);
                }
                else
                {
                    resetCam();
                }
            }).AddTo(this);
        }

        void shot(SingleSelection selection)
        {
            if (!towerView) return;
            if (bulletPrefab == null) return;
            var e = selection.Current as IEnemy;
            if (e == null) return;

            var go = Instantiate(bulletPrefab);
            go.transform.position = tower;

            go.transform.DOMove(e.transform.position, 3).OnComplete(() =>
            {
                Destroy(go);
            });

            var start = go.transform.position;
            var end = e.transform.position;
            var dir = end - start;
            Debug.Log(dir.magnitude);

            go.transform.DOPath(new Vector3[]{
                (start + end) / 2 + Vector3.up * 5 + Vector3.right * Random.Range(-1f, 1f),
                end
            }, 3, PathType.CatmullRom).SetEase(Ease.InQuart);
        }

        void aim(SingleSelection selection)
        {
            if (!towerView) return;
            var e = selection.Current as IEnemy;
            if (e == null) return;
            Debug.DrawLine(tower, e.transform.position, Color.red, 100);
        }

        void update(SingleSelection selection, IEnemyManager enemyManager)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectNext(selection, enemyManager);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectPrev(selection, enemyManager);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                shot(selection);
            }
            aim(selection);
        }
    }
}
