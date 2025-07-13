using UnityEngine;
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
    public float noiseScale = 0.1f;
    public float heightMultiplier = 5f;


    [Header("NavMesh Settings")]
    public NavMeshSurface navMeshSurface;
    private Transform playerTransform;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentPlayerChunk;
    private Vector2Int lastPlayerChunk;
    void Start()
    {
        if (PlayerManager.Instance != null)
        {
            playerTransform = PlayerManager.Instance.player.transform;
        }
        else
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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
        UpdateChunks();
        Invoke("BakeNavMesh", 0.5f);
    }

    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }
        currentPlayerChunk = GetChunkCoordinate(playerTransform.position);
        if (currentPlayerChunk != lastPlayerChunk)
        {
            UpdateChunks();
            lastPlayerChunk = currentPlayerChunk;
            Invoke("BakeNavMesh", 0.1f);
        }
    }
    Vector2Int GetChunkCoordinate(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
        int z = Mathf.FloorToInt(worldPosition.z / chunkSize);
        return new Vector2Int(x, z);
    }
    void UpdateChunks()
    {
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);
                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    GenerateChunk(chunkCoord);
                }
            }
        }
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            Vector2Int chunkCoord = chunk.Key;
            float distance = Vector2Int.Distance(chunkCoord, currentPlayerChunk);
            if (distance > renderDistance + 1)
            {
                chunksToRemove.Add(chunkCoord);
            }
        }
        foreach (var chunkCoord in chunksToRemove)
        {
            if (activeChunks.ContainsKey(chunkCoord))
            {
                DestroyImmediate(activeChunks[chunkCoord]);
                activeChunks.Remove(chunkCoord);
            }
        }
    }
    void GenerateChunk(Vector2Int chunkCoord)
    {
        Vector3 chunkWorldPos = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
        GameObject chunkObject = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObject.transform.parent = transform;

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
        terrainQuad.transform.position = chuckWorldPos;
        terrainQuad.transform.localScale = Vector3.one * (chunkSize / 10f);

        if (terrainMaterial != null)
        {
            terrainQuad.GetComponent<Renderer>().material = terrainMaterial;
        }
    }
    void GenerateProps(GameObject chunkParent, Vector3 chunkPos)
    {
        if (propPrefabs = null || propPrefabs.Length == 0)
        {
            GenerateRandomCubes(chunkParent, chunkPos);
            return;
        }

        int propCount = Random.Range(0, Mathf.RoundToInt(chunkSize * propDensity));
        for (int i = 0; i < propCount; i++)
        {
            Vector3 randomPos = chunkPos + new Vector3(Random.Range(0, chunkSize), 0, Random.Range(0, chunkSize));
            GameObject propPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            GameObject prop = Instantiate(propPrefab, randomPos, Quaternion.identity, chunckParent.transform);
            prop.transform.rotation = Quaternion.Euler(0, Random.Ramge(0, 360), 0);
            float scale = Random.Range(0.8f, 1.2f);
            prop.transform.localScale = Vector3.one * scale;
        }
    }
    void GenerateRandomCubes(GameObject chunkParent, Vector3 chunkPos)
    {
        int cubeCount = Random.Range(0, Mathf.RoundToInt(chunkSize * propDensity));
        for (int i = 0; i < cubeCount; i++)
        {
            Vector3 randomPos = chunkPos + new Vector3(Random.Range(0, chunkSize), 1f, Random.Range(0, chunkSize));
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "RandomProp";
            cube.transform.position = chunkParent.transform;
            cube.transform.position = randomPos;
            cube.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float scale = Random.Range(0.5f, 2f);
            cube.transform.localScale = Vector3.one * scale;

            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material.color = new Color(Random.value, Random.value, Random.value);

        }
    }

    void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh baked successfully.");
        }
    }

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
    }*/
}