using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int enemiesPerWave = 5;
    public float waveInterval = 8f;

    public float spawnRadius = 30f;
    bool stop = false;
    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(5f); 
        while (!stop)
        {
            SpawnWave();
            yield return new WaitForSeconds(waveInterval);
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 spawnPoint = GetRandomNavMeshPoint();

            if (spawnPoint != Vector3.zero)
            {
                Instantiate(enemyPrefab, spawnPoint, Quaternion.identity, transform);
            }
        }
    }

    Vector3 GetRandomNavMeshPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    public void StopSpawning()
    {
        stop = true;
        StopAllCoroutines();
        //destroy all children under this object
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}