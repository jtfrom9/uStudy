using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class CameraWorkManager : MonoBehaviour
{
    Transform target;
    AgentController[] agents;

    [SerializeField]
    Transform _camera;

    [SerializeField]
    Dropdown dropdown;

    void SetTarget(AgentController agent) {
        var target = agent.gameObject.transform;
        if(this.target != target) {
            this.target = target;
            _camera.SetParent(this.target, false);
            _camera.localPosition = new Vector3(0, 1, -3);
            _camera.localRotation = Quaternion.identity;

            dropdown.value = Array.IndexOf(agents, agent);
        }
    }

    [SerializeField]
    AgentController _agent;

    void Start() {
        agents = FindObjectsOfType<AgentController>();
        foreach (var agent in agents) {
            Debug.Log($"agent: {agent.agentName}");
            dropdown.AddOptions(new List<string>() { agent.agentName });
        }

        dropdown.OnValueChangedAsObservable().Subscribe(index =>
        {
            SetTarget(agents[index]);
        });


        if (_agent)
        {
            SetTarget(_agent);
        }
    }
}
