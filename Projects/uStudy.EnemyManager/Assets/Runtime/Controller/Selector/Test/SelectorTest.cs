#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UniRx;
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

        // [SerializeField]
        // Toggle? trackCamToggle;

        // [SerializeField]
        // Toggle? birdViewToggle;

        [SerializeField]
        TMP_Dropdown? dropdown;
        // Dropdown? dropdown;


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

        void resetCam()
        {
            Camera.main.transform.SetParent(null);
            Camera.main.transform.position = new Vector3(0, 1, -10);
            Camera.main.transform.rotation = Quaternion.identity;
        }

        void trackCam(int index)
        {
            if(DOTween.IsTweening(Camera.main.transform)) {
                Camera.main.transform.DOKill();
            }
            try {
                if(towerView) {
                    var enemy = enemyManager!.Enemies[index];
                    if (enemy != null)
                    {
                        if (Camera.main.transform.position != tower)
                        {
                            Camera.main.transform.SetParent(null);
                            Camera.main.transform.DOMove(tower, 1)
                                .OnUpdate(() =>
                                {
                                    Camera.main.transform.LookAt(enemy.transform.position);
                                });
                        }else
                        {
                            Camera.main.transform.DOLookAt(enemy.transform.position, 1);
                        }
                    }
                }else
                {
                    var enemy = enemyManager!.Enemies[index];
                    Camera.main.transform.SetParent(enemy.transform, true);
                    var pos = new Vector3(0, 3f, -3);
                    var rot = new Vector3(30, 0, 0);
                    if (birdView)
                    {
                        pos = new Vector3(0, 10, -3);
                        rot = new Vector3(80, 0, 0);
                    }
                    Camera.main.transform.DOLocalMove(pos, 1);
                    Camera.main.transform.DOLocalRotate(rot, 1);
                }
            }catch {
                resetCam();
            }
        }

        // bool tracking { get => trackCamToggle?.isOn ?? false; }
        // bool birdView { get => birdViewToggle?.isOn ?? false; }

        bool tracking { get => dropdown!.options[dropdown.value].text != "Init"; }
        bool birdView { get => dropdown!.options[dropdown.value].text == "Bird"; }
        bool towerView { get => dropdown!.options[dropdown.value].text == "Tower"; }

        void selectNext(IEnemyManager enemyManager)
        {
            var cur = enemyManager.SelectedIndex();
            var next = enemyManager.Enemies.Count - 1 == cur ? 0 : cur + 1;
            Debug.Log($"{cur} -> {next}");
            enemyManager.SelectExclusive(next);
            if(tracking) trackCam(next);
        }

        void selectPrev(IEnemyManager enemyManager)
        {
            var cur = enemyManager.SelectedIndex();
            var prev = cur == 0 ? enemyManager.Enemies.Count - 1 : cur - 1;
            enemyManager.SelectExclusive(prev);
            if (tracking) trackCam(prev);
        }

        void Start()
        {
            if (enemyManager == null)
            {
                Debug.LogError($"enemyManager: {enemyManager}");
                return;
            }
            bool go = false;
            var tmp = goButton!.GetComponentInChildren<TextMeshProUGUI>();
            goButton?.OnClickAsObservable().Subscribe(_ =>
            {
                go = !go;
                tmp!.text = (go) ? "Stop" : "Go";
                if (!go)
                {
                    foreach (var enemy in enemyManager!.Enemies)
                    {
                        enemy.Stop();
                    }
                }
            }).AddTo(this);

            resetButton?.OnClickAsObservable().Subscribe(_ =>
            {
                foreach (var enemy in enemyManager!.Enemies)
                {
                    enemy.GetControl().ResetPos();
                }
            }).AddTo(this);

            // trackCamToggle?.OnValueChangedAsObservable().Subscribe(v => { 
            //     if(v) {
            //         trackCam(enemyManager.SelectedIndex());
            //     }else {
            //         resetCam();
            //     }
            // }).AddTo(this);

            // birdViewToggle?.OnValueChangedAsObservable().Subscribe(v =>
            // {
            //     trackCam(enemyManager.SelectedIndex());
            // }).AddTo(this);

            UniTask.Create(async () =>
            {
                while (true)
                {
                    if (go)
                    {
                        foreach (var enemy in enemyManager!.Enemies)
                        {
                            var x = Random.Range(-10f, 10f);
                            var z = Random.Range(-10f, 10f);
                            var pos = new Vector3(x, 0, z);
                            Debug.Log($"{enemy.Name}: {pos}");
                            enemy.SetDestination(pos);
                        }
                    }
                    await UniTask.Delay(3000);
                }
            }).Forget();

            if(dropdown!=null) {
                dropdown.options.Add(new TMP_Dropdown.OptionData("Init"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Track"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Bird"));
                dropdown.options.Add(new TMP_Dropdown.OptionData("Tower"));
            }
            dropdown.ObserveEveryValueChanged(v => v!.value).Subscribe(v => { 
                Debug.Log(dropdown!.options[v].text);
                Debug.Log(tracking);
                if(tracking) {
                    trackCam(enemyManager.SelectedIndex());
                }else {
                    resetCam();
                }
            }).AddTo(this);

            enemyManager.SelectExclusive(0);
        }

        void shot() {
            if(!towerView) return;
            if(bulletPrefab==null) return;
            var e = enemyManager?.Selected();
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

            // var rb = go.GetComponent<Rigidbody>();
            // var dir = e.transform.position - go.transform.position;
            // rb.velocity = dir.normalized * 10;
        }

        void aim() {
            if (!towerView) return;
            var e = enemyManager?.Selected();
            if(e==null) return;
            Debug.DrawLine(tower, e.transform.position, Color.red, 100);
        }

        void Update()
        {
            if (enemyManager == null) return;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectNext(enemyManager);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                selectPrev(enemyManager);
            }
            if(Input.GetKeyDown(KeyCode.Space)) {
                shot();
            }
            aim();
        }
    }
}
