using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public static class Vector3Extension {
    public static Vector3 X(this Vector3 vec, float v) {
        return new Vector3(v, vec.y, vec.z);
    }

    public static Vector3 Y(this Vector3 vec, float v)
    {
        return new Vector3(vec.x, v, vec.z);
    }

    public static Vector3 Z(this Vector3 vec, float v)
    {
        return new Vector3(vec.x, vec.y, v);
    }
}

public class TPSController : MonoBehaviour
{
    [SerializeField]
    Transform player;

    [SerializeField]
    new Transform camera;

    [SerializeField]
    Vector3 cameraOffset = new Vector3(0, 1, 3);

    [SerializeField]
    float cameraWatchDistance = 3;

    [SerializeField]
    Joystick moveJoystick;

    [SerializeField]
    Joystick camJoystick;

    [SerializeField]
    float angleElevation = -10;

    [SerializeField]
    float angleDepression = 0;

    [SerializeField, Range(0.1f, 3)]
    float cameraHorizontalSensitivity = 0.1f;

    [SerializeField, Range(0.1f, 3)]
    float cameraVerticalSensitivity = 0.1f;

    public float zdiff = 0.1f;
    public float rdiff = 1.2f;

    void WalkPlayerByJoystick()
    {
        var forward = camera.forward;
        forward.y = 0;
        var right = camera.right;
        right.y = 0;

        var diff = forward * moveJoystick.Vertical * zdiff + right * moveJoystick.Horizontal * zdiff;
        player.Translate(diff);
        // player.position += diff;
    }

    void CameraManipurate()
    {
        // var orig = camera.localRotation.eulerAngles;
        // var after = new Vector3(orig.x - camJoystick.Vertical, orig.y + camJoystick.Horizontal * 3, 0);
        // camera.localRotation = Quaternion.Euler(after);

        var angleY = camJoystick.Horizontal * 3;
        camera.RotateAround(player.position, Vector3.up, angleY);
        camera.Rotate(-camJoystick.Vertical, 0, 0);
    }

    void CameraChasePlayer()
    {
        // camera.position = player.position + offset;
        // var dist = offset.normalized;
        // camera.position = player.position + camera.forward * dist.z;
        // camera.position = camera.TransformDirection(player.position + offset);

        // camera.position = player.position + camera.forward * (-3) + Vector3.up * offset.y;
        // camera.position = player.position + camera.forward * offset.z + Vector3.up * offset.y;

        var forward = camera.forward.Y(0);
        camera.position = player.position + forward * (-cameraOffset.z) + Vector3.up * cameraOffset.y;
    }

    void SyncPlayerDirection()
    {
        if (Mathf.Abs(moveJoystick.Vertical) >= 0.4 || Mathf.Abs(moveJoystick.Horizontal) > 0.4)
        {
            player.LookAt(player.position + camera.forward.Y(0));
        }
    }

    void FixedUpdate()
    {
        WalkPlayerByJoystick();
        CameraManipurate();
        CameraChasePlayer();
        SyncPlayerDirection();

        WalkPlayerByArrawKey();
    }

    void WalkPlayerByArrawKey()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            player.Translate(Vector3.forward * zdiff);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            player.Translate(Vector3.forward * -zdiff);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                player.Translate(Vector3.right * zdiff);
            }
            else
            {
                player.Rotate(Vector3.up * rdiff);
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                player.Translate(Vector3.right * -zdiff);
            }
            else
            {
                player.Rotate(Vector3.up * -rdiff);
            }
        }
    }

    void Update()
    {
        Debug.DrawLine(player.position, player.position + player.forward * 100, Color.red);
        Debug.DrawLine(camera.position, camera.position + camera.forward * 100, Color.green);
        Debug.DrawLine(player.position, player.position + camera.forward * 100, Color.yellow);
    }
}
