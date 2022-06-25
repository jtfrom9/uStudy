#nullable enable

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;

public class CachedTransformTest
{
    public class HogeComponent : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("Start");
        }

        void Update()
        {
            Debug.Log("Debug");
        }
    }

    [UnityTest]
    public IEnumerator Test1() => UniTask.ToCoroutine(async () =>
    {
        var go = new GameObject("Hoge");
        var hoge = go.AddComponent<HogeComponent>();

        await UniTask.NextFrame();

        // Assert.Pass("OK");

        await UniTask.NextFrame();
    });
}
