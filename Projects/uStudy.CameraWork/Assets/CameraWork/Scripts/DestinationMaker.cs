using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

public class DestinationMaker : MonoBehaviour
{
    public Subject<Vector3> destinationSubject = new Subject<Vector3>();

    [SerializeField]
    GameObject targetPrefab;
    GameObject currentTarget;

    void GenTarget(Vector3 pos) {
        if (currentTarget != null) { Destroy(currentTarget); }

        currentTarget = Instantiate(targetPrefab);
        currentTarget.transform.position = pos;
    }

    async void Start()
    {
        while (true)
        {
            await UniTask.Delay(5000);
            var x = Random.Range(-50, 50);
            var z = Random.Range(-50, 50);
            var pos = new Vector3(x, 0, z);
            GenTarget(pos);
            destinationSubject.OnNext(pos);
        }
    }
}
