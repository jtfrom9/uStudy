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
    [SerializeField]
    CinemachineVirtualCamera towerVCamera;


    public enum CameraType
    {
        Tracking,
        TopDown,
        Tower
    };

    [SerializeField]
    CameraType cameraType = CameraType.Tracking;

    void setPriority()
    {
        switch (cameraType)
        {
            case CameraType.Tracking:
                trackingVCamera.Priority = 11;
                topDownVCamera.Priority = 10;
                towerVCamera.Priority = 10;
                break;
            case CameraType.TopDown:
                trackingVCamera.Priority = 10;
                topDownVCamera.Priority = 11;
                towerVCamera.Priority = 10;
                break;
            case CameraType.Tower:
                trackingVCamera.Priority = 10;
                topDownVCamera.Priority = 10;
                towerVCamera.Priority = 11;
                break;
        }
    }

    void switchCameraType()
    {
        switch (cameraType) {
            case CameraType.Tracking:
                cameraType = CameraType.TopDown;
                break;
            case CameraType.TopDown:
                cameraType = CameraType.Tower;
                break;
            case CameraType.Tower:
                cameraType = CameraType.Tracking;
                break;
        }
        setPriority();
    }

    void Start()
    {
        if(trackingVCamera==null || topDownVCamera==null) {
            throw new System.Exception("invalid");
        }
        setPriority();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            switchCameraType();
        }
    }
}
