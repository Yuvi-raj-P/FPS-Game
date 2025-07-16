/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class WorldGen : MonoBehaviour
{
    [Header("World Generation Settings")]
    public GameObject[] terrainPrefabs;
    public GameObject[] propPrefabs;
    public float chunkSize = 50f;
    public int renderDistance = 3;
    public float propDensity = 0.1f;

    [Header("Terrain Settings")]
    public Material terrainMaterial;
    //public float noiseScale = 0.1f;
    //public float heightMultiplier = 5f;


    [Header("NavMesh Settings")]
    public NavMeshSurface navMeshSurface;
    private Transform playerTransform;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentPlayerChunk;
    private bool isBaking = false;

    void Start()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
        {
            playerTransform = PlayerManager.Instance.player.transform;
        }
        else
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }
        if (playerTransform == null)
        {
            Debug.LogError("No player found for WorldGen script. FIX THIS BS ASAP DAWG");
            return;
        }

        if (navMeshSurface == null)
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }
        }
        StartCoroutine(InitialGeneration());
    }
    private IEnumerator InitialGeneration()
    {
        currentPlayerChunk = GetChunkCoordinate(playerTransform.position);
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                GenerateChunk(currentPlayerChunk + new Vector2Int(x, z));
            }
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(BakeNavMeshAsync());
    }

    void Update()
    {
        if (playerTransform == null || isBaking)
        {
            return;
        }
        Vector2Int newPlayerChunk = GetChunkCoordinate(playerTransform.position);
        if (newPlayerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = newPlayerChunk;
            StartCoroutine(UpdateChunksAndBake());

        }
    }
    Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }
    IEnumerator UpdateChunksAndBake()
    {
        isBaking = true;
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                requiredChunks.Add(currentPlayerChunk + new Vector2Int(x, z));
            }
        }

        foreach (var chunkCoord in requiredChunks)
        {
            if (!activeChunks.ContainsKey(chunkCoord))
            {
                GenerateChunk(chunkCoord);
            }
        }
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(BakeNavMeshAsync());

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (!requiredChunks.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        foreach (var chunkCoord in chunksToRemove)
        {
            if (activeChunks.TryGetValue(chunkCoord, out GameObject chunkObject))
            {
                Destroy(chunkObject);
                activeChunks.Remove(chunkCoord);
            }
        }
        isBaking = false;
    }
    void GenerateChunk(Vector2Int chunkCoord)
    {
        Vector3 chunkWorldPos = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
        GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.parent = transform;
        chunkObject.transform.position = chunkWorldPos;

        if (terrainPrefabs != null && terrainPrefabs.Length > 0)
        {
            GameObject terrainPrefab = terrainPrefabs[Random.Range(0, terrainPrefabs.Length)];
            GameObject terrain = Instantiate(terrainPrefab, chunkWorldPos, Quaternion.identity, chunkObject.transform);

        }
        else
        {
            GenerateFlatTerrain(chunkObject, chunkWorldPos);
        }

        GenerateProps(chunkObject, chunkWorldPos);
        activeChunks[chunkCoord] = chunkObject;

    }
    void GenerateFlatTerrain(GameObject chunkParent, Vector3 chunkWorldPos)
    {
        GameObject terrainQuad = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrainQuad.name = "TerrainQuad";
        terrainQuad.transform.parent = chunkParent.transform;
        terrainQuad.transform.position = chunkWorldPos;
        terrainQuad.transform.localScale = Vector3.one * (chunkSize / 10f);

        if (terrainMaterial != null)
        {
            terrainQuad.GetComponent<Renderer>().material = terrainMaterial;
        }

        terrainQuad.layer = 0;
    }
    void GenerateProps(GameObject chunkParent, Vector3 chunkPos)
    {
        if (propPrefabs == null || propPrefabs.Length == 0)
        {
            GenerateRandomCubes(chunkParent, chunkPos);
            return;
        }

        int propCount = Random.Range(0, Mathf.RoundToInt(chunkSize * propDensity));
        for (int i = 0; i < propCount; i++)
        {
            Vector3 randomPos = chunkPos + new Vector3(Random.Range(0, chunkSize), 0, Random.Range(0, chunkSize));
            GameObject propPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            GameObject prop = Instantiate(propPrefab, randomPos, Quaternion.identity, chunkParent.transform);

            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float scale = Random.Range(0.8f, 1.2f);
            prop.transform.localScale = Vector3.one * scale;
        }
    }
    void GenerateRandomCubes(GameObject chunkParent, Vector3 chunkPos)
    {
        int cubeCount = Random.Range(0, Mathf.RoundToInt(chunkSize * propDensity));
        for (int i = 0; i < cubeCount; i++)
        {
            Vector3 randomPos = chunkPos + new Vector3(
            Random.Range(2f, chunkSize - 2f),
            1f,
            Random.Range(2f, chunkSize - 2f)
        );

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "RandomProp";
            cube.transform.parent = chunkParent.transform;
            cube.transform.position = randomPos;
            cube.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            float scale = Random.Range(0.5f, 2f);
            cube.transform.localScale = Vector3.one * scale;


            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material.color = new Color(Random.value, Random.value, Random.value);

        }
    }

    private IEnumerator BakeNavMeshAsync()
    {
        if (navMeshSurface == null)
        {
            yield break;
        }
        Debug.Log("Starting NavMesh bake");
        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>(FindObjectsSortMode.None);
        foreach (var agent in agents)
        {
            if (agent != null && agent.gameObject.activeInHierarchy && agent.enabled)
            {
                agent.isStopped = true;
            }
        }
        yield return null;

        AsyncOperation bakeOperation = navMeshSurfave.UpdateNavMesh(navMeshSurface.navMeshData);

        while (!bakeOperation.isDone)
        {
            yield return null;
        }
        Degub.Log("NavMesh baked successfully");

        foreach (var agent in agents)
        {
            if (agent != null && agent.gameobject.activeInHierarchy)
            {
                if (navMeshSurface.SamplePosition(agent.transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAread))
                {
                    agent.Warp(hit.position);
                    agent.isStopped = false;
                }
                else
                {
                    Debug.LogWarning($"Could not fine a valid NavMesh position for agent {agent.name}. It might b stuck.");
                }
            }
        }

    }
    
   
    /* Related to terrain management with enemies spawning and Player chunks
       Commented because the enemy generation was moved to WaveManager
    public Vector2Int CurrentPlayerChunk
    {
        get { return currentPlayerChunk; }
        set
        {
            currentPlayerChunk = value;
            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(currentPlayerChunk.x * chunkSize, playerTransform);
                playerTransform.position;
                MovePlayerToChunk(currentPlayerChunk);
                BakeNavMesh();
                PlayerManager.Instance.UpdatePlayerPosition(currentPlayerChunk);
                for (int i = 0; i < renderDistance; i++)
                {
                    UpdateChunks();
                    lastPlayerChunk = currentPlayerChunk;
                    Invoke("BakeNavMesh", 0.1f);
                    MovePlayerToChunk(currentPlayerChunk);
                    for (int x = -renderDistance; x <= renderDistance; x++)
                        for (int z = -renderDistance; z <= renderDistance; z++)
                        {
                            Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);
                            if (!activeChunks.ContainsKey(chuckCoord))
                            {
                                GenerateChunk(chunkCoord);
                            }
                        }
                    {

                    }
                }
            }
        }

    }
    /* Shifted this code to a seperate script WaveManager so dont bother with this
    void GenerateEnemies(GameObject chunkParent, Vector3 chunkPos)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            GenerateRandomCubes(chunkParent, chunkPos, enemyDensity, "Enemy");
            return;
        }
        int enemyCount = Random.Range(0, Mathf.RoundToInt(chuckSize * enemyDensity));
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randomPos = chunkPos + new Vector3(Random.Range(5, chunkSize - 5), 1f, Random.Range(5, chunkSize - 5));

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity, chunkParent.transform);
            NavMeshAgent agent = enemy.GetComponent<NaveMeshAgent>();

            if (agent == null)
            {
                agent = enemy.AddComponent<NavMeshAgent>();
            }
            agent.enabled = false;
            enemy.tag = "Enemy";

        }
    }
}
*/