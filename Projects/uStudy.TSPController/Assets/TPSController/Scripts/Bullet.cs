using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Bullet : MonoBehaviour
{
    public Subject<Vector3> subject = new Subject<Vector3>();

    void Update()
    {
        if(transform.position.y < -1) {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.CompareTag("Environment")) {
            foreach(var contact in collision.contacts) {
                // subject.OnNext(contact.point);
                subject.OnNext(transform.position);
                Destroy(this.gameObject);
            }
        }
    }
}
