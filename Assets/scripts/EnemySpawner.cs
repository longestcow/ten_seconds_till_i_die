using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnemySpawner : MonoBehaviour
{
    public GameObject melee,shooter,flyguy;
    GameObject enemyPrefab;
    public Button[] allButtons;
    public int enemiesPerWave = 5;
    public float waveInterval = 8f;

    public float spawnRadius = 30f;
    bool stop = false;
    private void Start()
    {
        foreach (Button button in allButtons){
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { OnButtonHovered(); });
            trigger.triggers.Add(entry);
        }
        StartCoroutine(DifficultyRamp());
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
            int rand = Random.Range(0,3);
            enemyPrefab = rand==0?shooter:melee;
            Vector3 spawnPoint = GetRandomNavMeshPoint(); //change spawnpoint function if flyguy

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

    public void againb()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Playground");
    }
    public void menub()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    void OnButtonHovered()
    {
        SFXManager.instance.playSFX(8, transform, 1f);
    }

    IEnumerator DifficultyRamp()
    {
        yield return new WaitForSeconds(1f);
    }



}