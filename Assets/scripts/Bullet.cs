using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed = 8f, lifetime = 5f;
    public Rigidbody rb;

    public void Initialize(Vector3 dir)
    {
        transform.forward = dir.normalized;
        Destroy(gameObject, lifetime);
        rb.velocity = dir.normalized * speed;
    }

}
