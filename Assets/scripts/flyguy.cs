using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flyguy : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 5f;
    public float range = 15f;

    public float targetHeight = 33f;
    public float heightCorrectionStrength = 2f;

    public float avoidDistance = 33f;
    public float sphereRadius = 0.5f;
    public LayerMask obstacleMask;

    public float health = 8f;
    Transform player;
    FirstPersonController playerController;
    public ParticleSystem hitParticles;
    public Transform bulletAnchor;
    public GameObject bulletPrefab;

    EnemySpawner par;
    SpriteRenderer sprite;
    public Sprite[] frames;

    float fireCooldown = 2f, shootTimer = 2f;

    private Vector3[] directions =
    {
        Vector3.forward,
        new Vector3(1, 0, 1).normalized,
        new Vector3(-1, 0, 1).normalized,
        Vector3.right,
        Vector3.left,
        new Vector3(1, 0, -1).normalized,
        new Vector3(-1, 0, -1).normalized,

        new Vector3(1, 1, 1).normalized,
        new Vector3(-1, 1, 1).normalized,
        new Vector3(1, -1, 1).normalized,
        new Vector3(-1, -1, 1).normalized,
        Vector3.up,
        Vector3.down
    };

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sprite = GetComponentInChildren<SpriteRenderer>();
        playerController = player.GetComponent<FirstPersonController>();
        StartCoroutine(FlyAnim());
        par=transform.parent.gameObject.GetComponent<EnemySpawner>();
        health+=par.healthIncrement;
    }
    void Update()
    {
        Vector3 moveDirection = GetBestDirection();
        float heightDifference = targetHeight - transform.position.y;
        moveDirection.y += heightDifference * heightCorrectionStrength;
        moveDirection.Normalize();

        transform.position += moveDirection * speed * Time.deltaTime;

        Vector3 direction = player.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        if(Mathf.Abs(Vector3.Distance(player.position, transform.position)) <= range)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0){
                shootTimer = fireCooldown;
                StartCoroutine(FireAttack());
            }
        }

    }
    void FireSpread()
    {
        Vector3 center = ((player.position + Vector3.up * 1.5f) - bulletAnchor.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, center).normalized;
        float spread = 12f;
        Vector3 leftShot = (center - right * Mathf.Tan(spread * Mathf.Deg2Rad)).normalized;
        Vector3 rightShot = (center + right * Mathf.Tan(spread * Mathf.Deg2Rad)).normalized;
        Vector3[] directions = {
            leftShot,
            center,
            rightShot
        };

        foreach (Vector3 dir in directions) {
            GameObject bullet = Instantiate(
                bulletPrefab,
                bulletAnchor.position,
                Quaternion.LookRotation(dir),
                transform.parent);

            bullet.GetComponent<EnemyBullet>().Initialize(dir);
        }
    }
    IEnumerator FireAttack()
    {
        FireSpread();
        yield return new WaitForSeconds(0.5f);
        FireSpread();
    }
    Vector3 GetBestDirection()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;

        Vector3 bestDirection = Vector3.zero;
        float bestScore = float.MinValue;


        foreach (Vector3 localDirection in directions)
        {
            Vector3 direction = transform.TransformDirection(localDirection);

            if (Physics.SphereCast(
                transform.position,
                sphereRadius,
                direction,
                out RaycastHit hit,
                avoidDistance,
                obstacleMask))
            {
                continue;
            }

            float score = Vector3.Dot(direction, toPlayer);
            float heightDifference = targetHeight - transform.position.y;
            score += direction.y * Mathf.Sign(heightDifference) * heightCorrectionStrength;


            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = direction;
            }
        }

        if (bestDirection == Vector3.zero)
            bestDirection = toPlayer;

        return bestDirection;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, sphereRadius);

        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.forward * avoidDistance
        );
    }
    public void Hurt(Vector3 hitPoint)
    {
        health--;

        Destroy(Instantiate(hitParticles, hitPoint, Quaternion.identity), 2f);

        StartCoroutine(HurtAnim());

        if (health <= 0) {
            Dead();
        }
    }
    public void Dead()
    {
        playerController.ResetTime();
        SFXManager.instance.playSFX(7, transform, 1f, Random.Range(0.4f, 1.2f));
        Destroy(gameObject);
    }
    IEnumerator HurtAnim()
    {
        sprite.color = Color.red;
        SFXManager.instance.playSFX(1, transform, 1f);
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }

    IEnumerator FlyAnim()
    {
        while(true){
            sprite.sprite = frames[Random.Range(0,frames.Length)];
            yield return new WaitForSeconds(1f);
        }
    }
}