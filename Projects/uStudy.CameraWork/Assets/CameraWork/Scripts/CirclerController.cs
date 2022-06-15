using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class CirclerController : MonoBehaviour
{
    void Start()
    {
        var _transform = GetComponent<Image>();
        _transform.DOFillAmount(1, 3).SetEase(Ease.InQuad);
    }
}
