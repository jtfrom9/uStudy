using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchCamera : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera trackingVCamera;
    [SerializeField]
    CinemachineVirtualCamera topDownVCamera;

    bool tracking = false;

    void Start()
    {
        if(trackingVCamera==null || topDownVCamera==null) {
            throw new System.Exception("invalid");
        }
        tracking = trackingVCamera.Priority > topDownVCamera.Priority;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            if (tracking)
            {
                trackingVCamera.Priority -= 2;
                tracking = false;
            } else {
                trackingVCamera.Priority += 2;
                tracking = true;
            }
        }
    }
}
