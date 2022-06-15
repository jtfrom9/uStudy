using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using UniRx;

public class AgentController : MonoBehaviour
{
    NavMeshAgent _agent;

    [SerializeField]
    DestinationMaker destinationMaker;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = Random.Range(3, 8);

        var mr = GetComponent<MeshRenderer>();
        mr.material.color = Random.ColorHSV();
    }

    void Start()
    {
        var x = Random.Range(-50, 50);
        var z = Random.Range(-50, 50);
        transform.position = new Vector3(x, 0, z);

        destinationMaker.destinationSubject.Subscribe(pos =>
        {
            _agent?.SetDestination(pos);
        }).AddTo(this);
    }
}
