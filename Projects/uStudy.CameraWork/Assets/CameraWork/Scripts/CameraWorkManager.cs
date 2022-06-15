using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class CameraWorkManager : MonoBehaviour
{
    AgentController current;

    AgentController[] agents;

    [SerializeField]
    Transform _camera;

    [SerializeField]
    Dropdown dropdown;
    [SerializeField]
    Button prevButton;
    [SerializeField]
    Button nextButton;
    [SerializeField]
    Toggle birdCamToggle;

    [SerializeField]
    Boolean birdCam = false;

    void SetTarget(AgentController agent)
    {
        var move = Vector3.zero;
        var rot = Vector3.zero;
        if (birdCam)
        {
            move = new Vector3(0, 15, -5);
            rot = new Vector3(60, 0, 0);
        }
        else
        {
            move = new Vector3(0, 1, -3);
            rot = Vector3.zero;
        }

        this.current = agent;

        // _camera.SetParent(this.target, false);
        // _camera.localPosition = new Vector3(0, 1, -3);
        // _camera.localRotation = Quaternion.identity;

        _camera.SetParent(this.current.transform);
        if (DOTween.IsTweening(_camera))
        {
            _camera.DOKill();
        }
        _camera.DOLocalMove(move, 1);
        _camera.DOLocalRotate(rot, 1);

        dropdown.value = Array.IndexOf(agents, agent);
    }

    void Start() {
        agents = FindObjectsOfType<AgentController>();
        foreach (var agent in agents.OrderBy(agent => agent.agentName)) {
            dropdown.AddOptions(new List<string>() { agent.agentName });
        }

        dropdown.OnValueChangedAsObservable().Subscribe(index =>
        {
            SetTarget(agents[index]);
        }).AddTo(this);

        nextButton.OnClickAsObservable().Subscribe(_ =>
        {
            var index = (current == null) ? -1 : Array.IndexOf(agents, current);
            var next = index != agents.Length - 1 ? index + 1 : 0;
            Debug.Log($"{index},{next}");
            SetTarget(agents[next]);
        }).AddTo(this);

        prevButton.OnClickAsObservable().Subscribe(_ =>
        {
            var index = (current == null) ? -1 : Array.IndexOf(agents, current);
            var prev = index != 0 ? index - 1 : agents.Length - 1;
            SetTarget(agents[prev]);
        }).AddTo(this);

        birdCamToggle.OnValueChangedAsObservable().Subscribe(v =>
        {
            this.birdCam = v;
            SetTarget(current);
        }).AddTo(this);
    }
}
