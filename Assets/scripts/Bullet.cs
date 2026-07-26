using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 5f;
    public Rigidbody rb;
    LayerMask env;
    Camera cam;
    bool parried = false;

    void Start()
    {
        cam = Camera.main;
        Destroy(gameObject, lifetime);
        env = LayerMask.NameToLayer("enemy");
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
        gameObject.layer = LayerMask.NameToLayer("parriedBullet");
    
        rb.velocity = direction.normalized * speed * 2f;

        
    }

	private void OnCollisionEnter(Collision collision)
    {
        if(parried)
        {
            if(collision.gameObject.layer == 8) //bullet
            {
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
            else if(collision.gameObject.layer == 6)//enemy
            {
                collision.gameObject.GetComponent<Walker>().Dead();
                Destroy(gameObject);

            } 
            else if(collision.gameObject.layer == 10)//fly enemy
            {
                collision.gameObject.GetComponent<flyguy>().Dead();
                Destroy(gameObject);

            } 
        }
    }
}