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
    Joystick joystick;

    void Start()
    {
    }

    public float zdiff = 0.1f;
    public float rdiff = 1.2f;

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

    void WalkPlayerByJoystick(){
        Debug.Log($"{joystick.Vertical}, {joystick.Horizontal}");
        player.Translate(Vector3.forward * joystick.Vertical * zdiff + Vector3.right * joystick.Horizontal * zdiff);
    }


    void Update()
    {
        // camera chase player
        camera.position = player.position + (player.forward * -1 * cameraDistance) + Vector3.up;

        var lookpoint = player.position + player.forward * cameraWatchDistance;
        camera.LookAt(new Vector3(lookpoint.x, 0, lookpoint.z));

        WalkPlayerByArrawKey();
    }

    void FixedUpdate()
    {
        WalkPlayerByJoystick();
    }
}
