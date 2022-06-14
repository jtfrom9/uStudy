using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Collider _collider;
    void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public void IgnoreCollider(Collider other) {
        Physics.IgnoreCollision(_collider, other, true);
    }
}
