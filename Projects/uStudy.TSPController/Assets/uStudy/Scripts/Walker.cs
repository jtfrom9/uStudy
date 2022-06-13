using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    public float zdiff = 0.1f;
    public float rdiff = 1.2f;

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.forward * zdiff);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(Vector3.forward * -zdiff);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(Vector3.right * zdiff);
            }
            else
            {
                transform.Rotate(Vector3.up * rdiff);
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(Vector3.right * -zdiff);
            }
            else
            {
                transform.Rotate(Vector3.up * -rdiff);
            }
        }
    }
}
