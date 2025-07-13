using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[System.Serializable]
public class Wave
{
    public string name;
    public List<GameObject> enemiesToSpawn;
    public float spawnRate;
}
public class WavesManager : MonoBehaviour
{
    public enum SpawnState { COUNTING, SPAWNING, WAITING };
    public static WavesManager Instance;
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;
    public float spawnRadius = 30f;
    public float minSpawnRadius = 20f;

    [Header("Difficulty Scaling")]
    public int baseEnemyCount = 3;
    public float countMultiplier = 1.5f;
    public float baseSpawnRate = 1f;
    public float rateMultiplier = 0.1f;
    public int wavesPerNewEnemy = 3;

    private Transform playerTransform;
    private int waveNumber = 0;
    private float waveCountdown;
    private float searchCountdown = 1f;
    private SpawnState state = SpawnState.COUNTING;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one WavesManager in the scene. FIX THIS BROOO! Destroying this for now");
            Destroy(this.gameObject);
        }
    }
    void Start()
    {
        playerTransform = PlayerManager.Instance.player.transform;
        if (playerTransform == null)
        {
            Debug.LogError("No player transform assigned in WavesManager.");
            this.enabled = false;
        }
        waveCountdown = timeBetweenWaves;
    }
    void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (!EnemyIsAlive())
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }
        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(GenerateWave()));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }
    Wave GenerateWave()
    {
        Wave wave = new Wave();
        wave.name = "Wave " + (waveNumber + 1);
        wave.spawnRate = baseSpawnRate + (waveNumber * rateMultiplier);
        wave.enemiesToSpawn = new List<GameObject>();

        int totalEnemiesInWave = Mathf.FloorToInt(baseEnemyCount + (waveNumber * countMultiplier));
        int availableEnemyTypes = Mathf.Min(enemyPrefabs.Count, (waveNumber / wavesPerNewEnemy) + 1);

        for (int i = 0; i < totalEnemiesInWave; i++)

        {
            List<float> enemyWeights = new List<float>();
            float totalWeights = 0f;

            for (int enemyIndex = 0; enemyIndex < availableEnemyTypes; enemyIndex++)
            {
                float weight;
                if (enemyIndex == 0)
                {
                    weight = 10f - (waveNumber * 0.2f);
                    weight = Mathf.Max(3f, weight);
                }
                else
                {
                    int wavesAvailable = waveNumber - (enemyIndex * wavesPerNewEnemy);
                    if (wavesAvailable < 0)
                    {
                        wavesAvailable = 0;
                    }
                    float difficultyMultiplier = 1f + (enemyIndex * 0.5f);
                    weight = (wavesAvailable * 0.3f + 1f) * difficultyMultiplier;

                    weight += (waveNumber * 0.1f * enemyIndex);
                }
                enemyWeights.Add(weight);
                totalWeights += weight;
            }

            float randomValue = Random.value * totalWeights;
            float currentWeight = 0f;

            for (int j = 0; j < enemyWeights.Count; j++)
            {
                currentWeight += enemyWeights[j];
                if (randomValue <= currentWeight)
                {
                    wave.enemiesToSpawn.Add(enemyPrefabs[j]);
                    break;
                }
            }
        }

        return wave;
    }
    void WaveCompleted()
    {
        Debug.Log("Wave " + (waveNumber + 1) + " completed!");
        waveNumber++;
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;
    }
    bool EnemyIsAlive()
    {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f)
        {
            searchCountdown = 1f;
            if (GameObject.FindGameObjectWithTag("Enemy") == null)
            {
                return false;
            }
        }
        return true;
    }
    IEnumerator SpawnWave(Wave _wave)
    {
        Debug.Log("Spawning Wave: " + _wave.name + " with " + _wave.enemiesToSpawn.Count);
        state = SpawnState.SPAWNING;

        foreach (GameObject enemyToSpawn in _wave.enemiesToSpawn)
        {
            SpawnEnemy(enemyToSpawn);
            yield return new WaitForSeconds(1f / _wave.spawnRate);
        }
        state = SpawnState.WAITING;
        yield break;
    }

    void SpawnEnemy(GameObject _enemy)
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, spawnRadius);
        Vector3 randomPoint = playerTransform.position + new Vector3(randomDirection2D.x, 0, randomDirection2D.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
        {
            Vector3 spawnPosition = hit.position;
            Instantiate(_enemy, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Could not find a valid spawn position on the NavMesh for an enemy.");
        }

    }
    /*IEnumerator SpawnEnemyCoroutine(GameObject _enemy)
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, spawnRadius);
        Vector3 randomPoint = playerTransform.position + new Vector3(randomDirection2D.x, 0, randomDirection2D.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(Random.position, out hit, 10f, NavMesh.AllAreas))
        {
            Vector3 spawnPosition = hit.position;
            GameObject enemyInstance = Instantiate(_enemy, spawnPosition, Quaternion.identity);
            NevMeshAgent agent = enemyInstance.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(spawnPosition);
                agent.enabled = true;
                Debug.Log("Spawned enemy: " + _enemy.name + " at position: " + spawnPosition);
                StartCoroutine(SpawnEnemyCoroutine(_enemy));
            }
            else
            {
                Debug.LogWarning("Spawned enemy does not have a NavMeshAgent component. Please add one to the enemy prefab.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find a valid spawn position on the NavMesh for an enemy.");
        }
        yield return null;
        spawnCountdown = 1f / _wave.spawnRate;
        yield return new WaitForSeconds(spawnCountdown);

    }
    Wave GenerateWave2()
    {
        Wave wave = new Wave();
        wave.name = "Wave " + (waveNumber + 1);
        wave.spawnRate = baseSpawnRate + (waveNumber * rateMultiplier);
        wave.enemiesToSpawn = new List<GameObject>();

        int wavePoints = 10 + (waveNumber * 5);
        int availableEnemyTypes = Mathf.Min(enemyPrefabs.Count, (waveNumber / wavesPerNewEnemy) + 1);

        List<int> enemyCosts = new List<int>();
        for (int i = 0; i < availableEnemyTypes; i++)
        {
            enemyCosts.Add(1 + (i * 2));
        }

        List<GameObjects> waveComposition = new List<GameObject>();

        int guaranteedBasicPoints = Mathf.Max(3, wavePoints / 3);
        int basicEnemiesCount = guaranteedBasicPoints / enemyCosts[0];
        for (int i = 0; i < basicEnemiesCount; i++)
        {
            waveComposition.Add(enemyPrefabs[0]);
        }
        wavesPoints -= basicEnemiesCount * enemyCosts[0];

        // Basically adds enemies based on the wave number with unites style
        if (waveNumber >= 5 && availableEnemyTypes > 2)
        {
            int eliteChance = Mathf.Min(30, 10 + waveNumber * 2);
            if (Random.Range(0, 100) < eliteChance)
            {
                int eliteIndex = availableEnemyTypes - 1;
                if (wavePoints >= enemyCosts[eliteIndex])
                {
                    waveComposition.Add(enemyPrefabs[eliteIndex]);
                    wavePoints -= enemyCosts[eliteIndex];
                }
            }
        }
        // Clustered enemies spawning and filling the points
        while (wavePoints > 0)
        {
            List<int> affordableEnemies = new List<int>();
            for (int i = 0; i < availableEnemyTypes; i++)
            {
                if (enemyCosts[i] <= wavePoints)
                {
                    affordableEnemies.Add(i);
                }
            }
            if (affordableEnemies.Count == 0) break;

            int selectedIndex;
            if (affordableEnemies.Count == 1)
            {
                selectedIndex = affordableEnemies[0];
            }
            else
            {
                float rand = Rnadom.value;
                if (rand < 0.3f && affordableEnemies.Contains(0))
                {
                    selectedOndex = 0;
                }
                else if (rand < 0.9f && affordableEnemies.Count > 1)
                {
                    int midTierIndex = Mathf.Min(affordableEnemies.Count - 1, Random.Range(1, 3));
                    selectedIndex = affordableEnemies[midTierIndex];
                }
                else
                {
                    selectedIndex = affordableEnemies[affordableEnemies.Count - 1];
                }
            }
            waveComposition.Add(enemyPrefabs[selectedIndex]);
            wavePoints -= enemyCosts[selectedIndex];

            if (Random.value < 0.4f && wavePoints >= enemyCosts[selectedIndex])
            {
                waveComposition.Add(enemyPrefabs[selectedIndex]);
                wavePoints -= enemyCosts[selectedIndex];
            }
        }
        for (int i = 0; i < waveComposition.Count; i++)
        {
            GameObject temp = waveComposition[i];
            int randomIndex = Random.Range(i, waveComposition.Count);
            waveComposition[i] = waveComposition[randomIndex];
            waveComposition[randomIndex] = temp;
        }
        //special wave patterns
        if ((waveNumber + 1) % 5 == 0 && availableEnemyTypes > 1)
    {
        // Boss wave - add extra elite enemies
        int extraElites = Random.Range(1, 3);
        for (int i = 0; i < extraElites; i++)
        {
            int eliteIndex = Random.Range(1, availableEnemyTypes);
            waveComposition.Add(enemyPrefabs[eliteIndex]);
        }
        Debug.Log("BOSS WAVE! Extra elites incoming!");
    }
        waves.enemiesToSpawn = waveComposition;
        return wave;
    }*/
}

