using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int enemiesPerWave = 5;
    public float waveInterval = 8f;

    public float spawnRadius = 30f;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true)
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
                Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
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
}