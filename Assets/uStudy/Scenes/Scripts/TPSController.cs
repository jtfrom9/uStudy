using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class TPSController : MonoBehaviour
{
    [SerializeField]
    Transform player;

    [SerializeField]
    new Transform camera;

    [SerializeField]
    float cameraDistance = 1;

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

    // void Start() {
    //     camera.localPosition = player.position + player.forward * (-cameraDistance) + Vector3.up;
    // }

    void WalkPlayerByJoystick()
    {
        var forward = camera.forward;
        forward.y = 0;
        var right = camera.right;
        right.y = 0;

        var diff = forward * moveJoystick.Vertical * zdiff + right * moveJoystick.Horizontal * zdiff;
        // player.Translate(diff);
        player.position += diff;
        Debug.Log($"diff: {diff}, pos: {player.position}");

        if (Mathf.Abs(moveJoystick.Vertical) >= 0.4 || Mathf.Abs(moveJoystick.Horizontal) > 0.4)
        {
            player.LookAt(player.position + camera.forward);
            camera.localRotation = Quaternion.Euler(Vector3.forward);
        }
    }

    void CameraManipurate()
    {

        var orig = camera.localRotation.eulerAngles;
        var after = new Vector3(orig.x - camJoystick.Vertical, orig.y + camJoystick.Horizontal * 3, 0);
        camera.localRotation = Quaternion.Euler(after);
    }

    void FixedUpdate()
    {
        WalkPlayerByJoystick();

        // camera.localPosition = player.position + player.forward * (-cameraDistance) + Vector3.up;
        Debug.Log($"forward: {player.forward}");
        // camera.localPosition = player.forward * (-cameraDistance) + Vector3.up;
        camera.localPosition = Vector3.forward * (-cameraDistance) + Vector3.up;
        Debug.Log($"campos: local: {camera.localPosition}, global: {camera.position}");

        CameraManipurate();
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

        WalkPlayerByArrawKey();
    }
}
