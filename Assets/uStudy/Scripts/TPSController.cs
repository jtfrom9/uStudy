using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    Joystick moveJoystick;

    [SerializeField]
    float zdiff = 0.1f;

    [SerializeField]
    float rdiff = 1.2f;

    void diffToCameraMove(Vector2 diff, float hratio, float vratio)
    {
        var e = diff.ToEulerAngle(hratio, vratio);
        camera.RotateAround(player.position, Vector3.up, -e.y);
        camera.Rotate(-e.x, 0, 0);
    }

    void Start() {
        var context = this.DefaultInputContext(EventSystem.current);
        var hratio = -90.0f / Screen.width;
        var vratio = -90.0f / Screen.height;
        context.GetObservable(0).Difference().Subscribe(diff => diffToCameraMove(diff, hratio, vratio)).AddTo(this);
        context.GetObservable(1).Difference().Subscribe(diff => diffToCameraMove(diff, hratio, vratio)).AddTo(this);
    }

    void WalkPlayerByJoystick()
    {
        var forward = camera.forward;
        forward.y = 0;
        var right = camera.right;
        right.y = 0;

        var diff = forward * moveJoystick.Vertical * zdiff + right * moveJoystick.Horizontal * zdiff;
        // player.Translate(diff);
        player.position += diff;
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
