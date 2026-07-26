using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;
    GameObject enemyPrefab;
    public Button[] allButtons;
    public BoxCollider spawnBox;
    Bounds bounds;
    public float spawnRadius = 30f;
    public TextMeshProUGUI waveText;
    bool stop = false;

    [Header("Diff settings")]
    int waveCount = 0;
    public float waveInterval = 8f;
    public int enemiesPerWave = 5;
    public int spawnType = 0; // 0 is melee, 1 is melee+shooter, 2 is everyone
    public int healthIncrement = 0;
    private void Start()
    {
        foreach (Button button in allButtons){
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { OnButtonHovered(); });
            trigger.triggers.Add(entry);
        }
        bounds = spawnBox.bounds;
        StartCoroutine(DifficultyRamp());
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(5f); 
        while (!stop)
        {
            SpawnWave();
            waveCount++;
            waveText.text = "WAVE "+waveCount;
            yield return new WaitForSeconds(waveInterval);
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            int rand = Random.Range(0,spawnType+1);
            enemyPrefab = enemies[rand];
            
            Vector3 spawnPoint = rand==2?GetRandomFlyPoint():GetRandomNavMeshPoint(); //change spawnpoint function if flyguy

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

    Vector3 GetRandomFlyPoint()
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        
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

    IEnumerator DifficultyRamp() // play with healthIncrement, waveInterval, enemiesPerWave
    {
        yield return new WaitForSeconds(15f);
        spawnType+=1; //shooters spawning
        yield return new WaitForSeconds(5f);
        healthIncrement+=1;
        yield return new WaitForSeconds(10f);
        spawnType+=1; //flyguys spawning
        yield return new WaitForSeconds(5f);
        healthIncrement+=1;
        enemiesPerWave+=1;
        yield return new WaitForSeconds(5f);
        waveInterval-=1;
        yield return new WaitForSeconds(10f);
        while (!stop)
        {
            healthIncrement+=1;
            enemiesPerWave+=1;
            yield return new WaitForSeconds(30f);

        }


    }



}