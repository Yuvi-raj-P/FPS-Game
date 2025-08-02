using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;
using UnityEditor;

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

    [Header("Spawn settings")]
    public LayerMask groundLayerMask = -1;
    public float spawnHeight = 50f;
    public int maxSpawnAttempts = 10;

    [Header("Spawn Distribution")]
    public int spawnSectors = 8;
    public float sectorAngleVariation = 20f;

    [Header("Difficulty Scaling")]
    public int baseEnemyCount = 2;
    public float countMultiplier = 0.8f;
    public float baseSpawnRate = 0.5f;
    public float rateMultiplier = 0.05f;
    public int wavesPerNewEnemy = 3;

    [Header("Special Enemy Settings")]
    public int specialEnemyStartWave = 4;
    public int specialEnemyFrequency = 2;
    [Tooltip("Enemy types that are 'special' - only one can exist at a time")]
    public List<int> specialEnemyIndices = new List<int>();

    [Header("New Enemy Introduction")]
    public int maxNewEnemiesPerWave = 1;
    public float newEnemyPercentage = 0.3f;

    private Transform playerTransform;
    private int waveNumber = 0;
    private float waveCountdown;
    private float searchCountdown = 1f;
    private SpawnState state = SpawnState.COUNTING;
    private int currentSpawnIndex = 0;

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

        int totalEnemiesInWave = baseEnemyCount + Mathf.FloorToInt(waveNumber * countMultiplier);
        totalEnemiesInWave = Mathf.Min(totalEnemiesInWave, 8 + waveNumber);

        int availableEnemyTypes = Mathf.Min(enemyPrefabs.Count, (waveNumber / wavesPerNewEnemy) + 1);

        bool shouldSpawnSpecialEnemy = ShouldSpawnSpecialEnemy();
        int specialEnemyToSpawn = -1;

        if (shouldSpawnSpecialEnemy)
        {
            specialEnemyToSpawn = GetSpecialEnemyToSpawn();
            if (specialEnemyToSpawn != -1)
            {
                wave.enemiesToSpawn.Add(enemyPrefabs[specialEnemyToSpawn]);
                Debug.Log($"SPECIAL ENEMY INCOMING: {enemyPrefabs[specialEnemyToSpawn].name}!");
                totalEnemiesInWave--;
            }
        }

        int basicEnemyCount = Mathf.Max(1, totalEnemiesInWave / 2);

        for (int i = 0; i < basicEnemyCount; i++)
        {
            wave.enemiesToSpawn.Add(enemyPrefabs[0]);
        }

        int remainingEnemies = totalEnemiesInWave - basicEnemyCount;

        for (int i = 0; i < remainingEnemies; i++)
        {
            int selectedEnemyType = SelectRegularEnemyType(availableEnemyTypes);
            wave.enemiesToSpawn.Add(enemyPrefabs[selectedEnemyType]);
        }

        if (ShouldIntroduceNewRegularEnemy(availableEnemyTypes))
        {
            IntroduceNewRegularEnemy(wave, availableEnemyTypes);
        }

        ShuffleWave(wave);

        Debug.Log($"Wave {waveNumber + 1}: {wave.enemiesToSpawn.Count} enemies, {availableEnemyTypes} types available");

        return wave;
    }

    bool ShouldSpawnSpecialEnemy()
    {
        if (waveNumber < specialEnemyStartWave) return false;

        if (IsSpecialEnemyAlive()) return false;

        return (waveNumber - specialEnemyStartWave) % specialEnemyFrequency == 0;
    }

    int GetSpecialEnemyToSpawn()
    {
        List<int> availableSpecialEnemies = new List<int>();

        foreach (int specialIndex in specialEnemyIndices)
        {
            int enemyUnlockWave = specialIndex * wavesPerNewEnemy;
            if (waveNumber >= enemyUnlockWave && specialIndex < enemyPrefabs.Count)
            {
                availableSpecialEnemies.Add(specialIndex);
            }
        }

        if (availableSpecialEnemies.Count == 0) return -1;

        return availableSpecialEnemies[Random.Range(0, availableSpecialEnemies.Count)];
    }

    bool IsSpecialEnemyAlive()
    {
        foreach (int specialIndex in specialEnemyIndices)
        {
            if (specialIndex < enemyPrefabs.Count)
            {
                string specialEnemyName = enemyPrefabs[specialIndex].name;
                GameObject specialEnemy = GameObject.Find(specialEnemyName + "(Clone)");
                if (specialEnemy != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    int SelectRegularEnemyType(int availableTypes)
    {
        List<int> regularEnemyTypes = new List<int>();
        for (int i = 0; i < availableTypes; i++)
        {
            if (!specialEnemyIndices.Contains(i))
            {
                regularEnemyTypes.Add(i);
            }
        }

        if (regularEnemyTypes.Count == 0) return 0;

        float[] weights = new float[regularEnemyTypes.Count];

        for (int i = 0; i < regularEnemyTypes.Count; i++)
        {
            int enemyIndex = regularEnemyTypes[i];
            if (enemyIndex == 0)
            {
                weights[i] = 10f;
            }
            else
            {
                float waveProgress = Mathf.Min(waveNumber / 10f, 1f);
                weights[i] = 1f + (waveProgress * 3f * (1f / (enemyIndex + 1)));
            }
        }

        int selectedWeightIndex = WeightedRandomSelect(weights);
        return regularEnemyTypes[selectedWeightIndex];
    }

    int WeightedRandomSelect(float[] weights)
    {
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return i;
            }
        }
        return 0;
    }

    bool ShouldIntroduceNewRegularEnemy(int availableTypes)
    {
        return availableTypes > 1 && (waveNumber % wavesPerNewEnemy == 0) && waveNumber > 0;
    }

    void IntroduceNewRegularEnemy(Wave wave, int availableTypes)
    {
        int newEnemyIndex = -1;
        for (int i = availableTypes - 1; i >= 0; i--)
        {
            if (!specialEnemyIndices.Contains(i))
            {
                newEnemyIndex = i;
                break;
            }
        }

        if (newEnemyIndex == -1 || newEnemyIndex == 0) return;

        int newEnemyCount = Mathf.Max(1, Mathf.FloorToInt(wave.enemiesToSpawn.Count * newEnemyPercentage));
        for (int i = wave.enemiesToSpawn.Count - 1; i >= 0 && newEnemyCount > 0; i--)
        {
            if (wave.enemiesToSpawn[i] == enemyPrefabs[0])
            {
                wave.enemiesToSpawn.RemoveAt(i);
                newEnemyCount--;
            }
        }
        newEnemyCount = Mathf.Max(1, Mathf.FloorToInt(wave.enemiesToSpawn.Count * newEnemyPercentage));
        for (int i = 0; i < newEnemyCount; i++)
        {
            wave.enemiesToSpawn.Add(enemyPrefabs[newEnemyIndex]);
        }

        Debug.Log($"NEW REGULAR ENEMY INTRODUCED: {enemyPrefabs[newEnemyIndex].name}!");
    }

    void ShuffleWave(Wave wave)
    {
        for (int i = 0; i < wave.enemiesToSpawn.Count; i++)
        {
            GameObject temp = wave.enemiesToSpawn[i];
            int randomIndex = Random.Range(i, wave.enemiesToSpawn.Count);
            wave.enemiesToSpawn[i] = wave.enemiesToSpawn[randomIndex];
            wave.enemiesToSpawn[randomIndex] = temp;
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave " + (waveNumber + 1) + " completed!");
        waveNumber++;
        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;
        currentSpawnIndex = 0;
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
        Debug.Log("Spawning Wave: " + _wave.name + " with " + _wave.enemiesToSpawn.Count + " enemies");
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
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 spawnDirection = GetDistributedSpawnDirection();
            float randomDistance = Random.Range(minSpawnRadius, spawnRadius);

            Vector3 randomPoint = playerTransform.position + (spawnDirection * randomDistance);
            randomPoint.y += spawnHeight;

            RaycastHit hit;
            if (Physics.Raycast(randomPoint, Vector3.down, out hit, spawnHeight + 10f, groundLayerMask))
            {
                spawnPosition = hit.point + Vector3.up * 0.5f;
                validPositionFound = true;
                break;
            }
        }
        if (validPositionFound)
        {
            GameObject spawnEnemy = Instantiate(_enemy, spawnPosition, Quaternion.identity);
            Debug.Log("Spawned enemy: " + _enemy.name + " at position: " + spawnPosition);
        }
        else
        {
            Vector3 fallbackDirection = GetDistributedSpawnDirection();
            Vector3 fallbackPosition = playerTransform.position + (fallbackDirection * Random.Range(5f, 10f));
            fallbackPosition.y += 2f;

            GameObject fallbackEnemy = Instantiate(_enemy, fallbackPosition, Quaternion.identity);
            Debug.LogWarning("Failed to find valid spawn position after " + maxSpawnAttempts + " attempts. Using distributed fallback position.");
        }

        currentSpawnIndex++;
    }

    Vector3 GetDistributedSpawnDirection()
    {
        float sectorAngle = 360f / spawnSectors;
        float baseSectorAngle = (currentSpawnIndex % spawnSectors) * sectorAngle;

        float randomVariation = Random.Range(-sectorAngleVariation, sectorAngleVariation);
        float finalAngle = baseSectorAngle + randomVariation;

        float angleInRadians = finalAngle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));

        return direction.normalized;
    }

    void OnDrawGizmosSelected()
    {
        Transform drawTarget = playerTransform;
        if (drawTarget == null && PlayerManager.Instance != null && PlayerManager.Instance.player != null)
        {
            drawTarget = PlayerManager.Instance.player.transform;
        }
        if (drawTarget == null)
        {
            drawTarget = transform;
        }

        Vector3 centerPosition = drawTarget.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPosition, minSpawnRadius);

        Gizmos.color = Color.cyan;
        float sectorAngle = 360f / spawnSectors;
        for (int i = 0; i < spawnSectors; i++)
        {
            float angle = i * sectorAngle;
            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));

            Gizmos.DrawLine(centerPosition, centerPosition + direction * spawnRadius);

            Vector3 arcStart = centerPosition + direction * minSpawnRadius;
            Vector3 arcEnd = centerPosition + direction * spawnRadius;
            Gizmos.DrawLine(arcStart, arcEnd);
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawSphere(centerPosition, spawnRadius);

        Gizmos.color = new Color(0f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(centerPosition, minSpawnRadius);
    }
    // --- IGNORE ---
}
