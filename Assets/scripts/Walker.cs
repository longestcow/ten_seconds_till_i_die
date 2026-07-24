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
    public float range = 3.5f, speed = 3.5f, health = 10f;
    float updateRate = 0.2f;
    float timer;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = range;
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
        }
    else
        agent.updateRotation = true;
    
    
}


    public void Hurt()
    {
        Debug.Log("OUCH");
        health-=1;
        StartCoroutine(HurtAnim());
        
        if (health <= 0){
            // SFXManager.instance.playSFX(2, transform, 1f);
            playerController.ResetTime();
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
