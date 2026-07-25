using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;
    public Rigidbody rb;

    Camera cam;
    bool parried = false;

    void Start()
    {
        cam = Camera.main;
        Destroy(gameObject, lifetime);
    }

    void LateUpdate()
    {
        if (cam != null)
            transform.LookAt(cam.transform);
    }

    public void Initialize(Vector3 dir)
    {
        rb.velocity = dir.normalized * speed;
    }

    public void Parry(Vector3 direction)
    {
        if (parried)
            return;

        parried = true;

    
        rb.velocity = direction.normalized * speed * 2f;

        
    }
}