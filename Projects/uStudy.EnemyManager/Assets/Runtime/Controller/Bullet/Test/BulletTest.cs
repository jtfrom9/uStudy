using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class BulletTest : MonoBehaviour
{
    public GameObject bulletPrefab;

    public Transform start;
    public Transform end;
    Tweener tweener;

    void Start()
    {
    }

    void Shot()
    {
        if (tweener != null) return;
        var go = Instantiate(bulletPrefab);
        go.transform.position = start.position;

        var dir = end.position - start.position;
        var p1 = start.position + dir.normalized * (dir.magnitude / 3);
        var p2 = start.position + dir.normalized * (dir.magnitude / 3 * 2);

        tweener = go.transform.DOPath(new Vector3[]{
                p1 + Vector3.up,
                p2 + Vector3.up * 2,
                end.position
            }, 5, PathType.CatmullRom)
            .OnComplete(() =>
            {
                tweener = null;
            }).SetEase(Ease.Linear);
    }

    async void Stop() 
    {
        if(tweener==null) return;
        var x = tweener.Pause();
        Debug.Log(tweener.IsPlaying());
        await UniTask.Delay(1000);
        x = tweener.Play();
        Debug.Log(tweener.IsPlaying());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shot();
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Stop();
        }
    }
}
