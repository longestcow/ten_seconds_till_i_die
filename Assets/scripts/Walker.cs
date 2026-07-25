using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Walker : MonoBehaviour
{
    NavMeshAgent agent;
    Transform player;
    FirstPersonController playerController;
    SpriteRenderer sprite;
    public bool melee;
    public Transform bulletAnchor;
    public GameObject bulletPrefab;
    float fireCooldown = 1f, shootTimer = 0;
    public float range = 3.5f, speed = 3.5f, health = 10f;
    float updateRate = 0.2f;
    float timer;
    [SerializeField] private ParticleSystem hitParticles;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed * (melee?1.8f:1);
        agent.stoppingDistance = (melee)?0:range;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<FirstPersonController>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        agent.SetDestination(player.position);
        timer -= Time.deltaTime;

        if(timer <= 0) {
            timer = updateRate;
            NavMeshHit hit;
            if(NavMesh.SamplePosition(player.position, out hit, 15f, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                agent.updateRotation = false;

                Vector3 direction = player.position - transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed);
                }

                if (!melee)
                {
                    shootTimer -= Time.deltaTime;
                    if (shootTimer <= 0){
                        shootTimer = fireCooldown;
                        Vector3 direction1 = ((player.position+new Vector3(0,1.5f,0)) - bulletAnchor.position).normalized;
                        GameObject bullet = Instantiate(bulletPrefab, bulletAnchor.position, bulletAnchor.rotation);
                        bullet.GetComponent<Bullet>().Initialize(direction1);
                    }
                }
            }
        else
            agent.updateRotation = true;
        
        
    }


    public void Hurt(Vector3 hitPoint)
    {
        Debug.Log("OUCH");
        health--;

        Instantiate(hitParticles, hitPoint, Quaternion.identity);

        StartCoroutine(HurtAnim());

        if (health <= 0)
        {
            playerController.ResetTime();
            SFXManager.instance.playSFX(7, transform, 1f, Random.Range(0.5f, 1.5f));
            Destroy(gameObject);
        }
    }

    IEnumerator HurtAnim()
    {
        sprite.color = Color.red;
        SFXManager.instance.playSFX(1, transform, 1f);
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }
}
