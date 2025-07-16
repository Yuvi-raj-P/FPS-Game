using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEditor.Analytics;
using UnityEngine.Scripting.APIUpdating;

public class ProceduralWorld : MonoBehaviour
{
    public Transform player;
    public int chunkSize = 10;
    public int renderDistance = 5;
    public int maxPropsPerChunk = 4;
    public float maxPropSize = 2f;

    public float navMeshUpdateDelay = 0.5f;
    public bool useAsyncNavMeshUpdate = true;

    private NavMeshSurface navMeshSurface;
    private Vector2Int currentPlayerChunk;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private bool isUpdatingNavMesh = false;
    private Coroutine navMeshUpdateCoroutine;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface component not forund");
            this.enabled = false;
            return;
        }
        player = PlayerManager.Instance.player.transform;
        if (player == null)
        {
            Debug.LogError("Player is not found please fix this RIGHT NOW!");
            this.enabled = false;
            return;
        }
        UpdateChunks();
    }
    void Update()
    {
        if (player == null)
        {
            return;
        }
        Vector2Int playerChunkCoord = GetChunkCoordFromPosition(player.position);
        if (playerChunkCoord != currentPlayerChunk)
        {
            UpdateChunks();
        }
    }
    Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }
    void UpdateChunks()
    {
        currentPlayerChunk = GetChunkCoordFromPosition(player.position);
        List<Vector2Int> chunksToRemove = new List<Vector2Int>(activeChunks.Keys);

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(currentPlayerChunk.x + x, currentPlayerChunk.y + z);

                if (activeChunks.ContainsKey(chunkCoord))
                {
                    chunksToRemove.Remove(chunkCoord);
                }
                else
                {
                    GenerateChunk(chunkCoord);
                }
            }
        }
        foreach (var chunkCoord in chunksToRemove)
        {
            if (activeChunks.ContainsKey(chunkCoord))
            {
                Destroy(activeChunks[chunkCoord]);
                activeChunks.Remove(chunkCoord);
            }
        }
        if (navMeshUpdateCoroutine != null)
        {
            StopCoroutine(navMeshUpdateCoroutine);
        }
        navMeshUpdateCoroutine = StartCoroutine(DelayedNavMeshUpdate());
    }
    IEnumerator DelayedNavMeshUpdate()
    {
        yield return new WaitForSeconds(navMeshUpdateDelay);
        if (!isUpdatingNavMesh)
        {
            yield return StartCoroutine(UpdateNavMeshCoroutine());
        }
    }
    IEnumerator UpdateNavMeshCoroutine()
    {
        isUpdatingNavMesh = true;
        var agentStates = new Dictionary<NavMeshAgent, AgentState>();
        NavMeshAgent[] agents = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);

        foreach (var agent in agents)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agentStates[agent] = new AgentState
                {
                    position = agent.transform.position,
                    velocity = agent.velocity,
                    destination = agent.destination,
                    hasPath = agent.hasPath,
                    isStopped = agent.isStopped,

                };
                agent.isStopped = true;
            }
        }

        yield return new WaitForEndOfFrame();

        if (useAsyncNavMeshUpdate)
        {
            AsyncOperation bakeOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            while (!bakeOperation.isDone)
            {
                yield return null;
            }
        }
        else
        {
            navMeshSurface.BuildNavMesh();
        }

        yield return new WaitForEndOfFrame();

        foreach (var kvp in agentStates)
        {
            var agent = kvp.Key;
            var state = kvp.Value;

            if (agent != null && agent.isActiveAndEnabled)
            {
                if (agent.Warp(state.position))
                {
                    agent.isStopped = state.isStopped;

                    if (state.hasPath && !state.isStopped)
                    {
                        agent.SetDestination(state.destination);
                    }
                }
                else
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(state.position, out hit, 1f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                        agent.isStopped = state.isStopped;

                        if (state.hasPath && !state.isStopped)
                        {
                            agent.SetDestination(state.destination);
                        }
                    }
                }
            }
        }

        isUpdatingNavMesh = false;
        Debug.Log("NavMesh updated successfully.");

    }

    void GenerateChunk(Vector2Int coord)
    {
        GameObject chunkObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObject.transform.parent = transform;
        chunkObject.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = chunkObject.transform;
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(chunkSize / 10f, 1, chunkSize / 10f);

        int numberOfProps = Random.Range(0, maxPropsPerChunk + 1);
        for (int i = 0; i < numberOfProps; i++)
        {
            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.transform.parent = chunkObject.transform;

            float randomScale = Random.Range(0.5f, maxPropSize);

            prop.transform.localScale = Vector3.one * randomScale;

            float randomX = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            float randomZ = Random.Range(-chunkSize / 2f, chunkSize / 2f);

            prop.transform.localPosition = new Vector3(randomX, randomScale / 2f, randomZ);
            prop.transform.rotation = Random.rotation;
        }
        activeChunks.Add(coord, chunkObject);

    }

    private class AgentState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 destination;
        public bool hasPath;
        public bool isStopped;
        
    }
    

}
