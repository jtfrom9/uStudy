using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    Joystick joystick;

    void rotate(float angleX,float angleY)
    {
        // var lp = transform.position + transform.forward;
        // lp.y = 0;
        // transform.LookAt(lp);

        // transform.Rotate(angleX, angleY, 0);

        transform.localRotation = Quaternion.Euler(0, angleY, 0);

        Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
    }

    void Update()
    {
        rotate(//joystick.Vertical,
        0,
            joystick.Horizontal * 5);
    }
}
