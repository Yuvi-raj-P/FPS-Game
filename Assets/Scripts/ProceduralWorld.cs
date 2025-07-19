using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
public class ProceduralWorld : MonoBehaviour
{
    public Transform player;
    public int chunkSize = 10;
    public int renderDistance = 5;
    public int maxPropsPerChunk = 4;
    public float maxPropSize = 2f;
    private Vector2Int currentPlayerChunk;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    public Quaternion Quarentation { get; private set; }

    void Start()
    {
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
    }
    void GenerateChunk(Vector2Int coord)
    {
        GameObject chunkObject = new GameObject($"Chunk_{coord.x}_{coord.y}");
        chunkObject.transform.parent = transform;
        chunkObject.transform.position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.layer = LayerMask.NameToLayer("Ground");
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
            prop.layer = LayerMask.NameToLayer("Obstacle");
        }
        activeChunks.Add(coord, chunkObject);

    }

    /* No more NavMesh baking for the game. The agents have been changed to a manual follow script.
    IEnumerator UpdateNavMeshCoroutine()
    {
        isUpdatingNavMesh = true;
        Debug.Log("Starting NavMesh update...");

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        NavMeshAgent[] agents = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.OnNavMeshUpdateStarted();
            }
        }

        var agentStates = new Dictionary<NavMeshAgent, AgentState>();

        foreach (var agent in agents)
        {
            if (agent != null && agent.isActiveAndEnabled)
            {
                AgentState state = new AgentState
                {
                    position = agent.transform.position,
                    velocity = Vector3.zero,
                    destination = Vector3.zero,
                    hasPath = false,
                    isStopped = agent.isStopped
                };
                if (agent.isOnNavMesh)
                {
                    state.velocity = agent.velocity;
                    state.hasPath = agent.hasPath;

                    if (agent.hasPath)
                    {
                        state.destination = agent.destination;
                    }
                }
                agentStates[agent] = state;
                agent.isStopped = true;
            }
        }



        yield return new WaitForSeconds(0.1f);

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

        yield return new WaitForSeconds(0.2f);

        foreach (var kvp in agentStates)
        {
            var agent = kvp.Key;
            var state = kvp.Value;

            if (agent != null && agent.isActiveAndEnabled)
            {
                bool warpSuccessful = false;
                Vector3 targetPosition = state.position;

                if (NavMesh.SamplePosition(state.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    targetPosition = hit.position;
                    warpSuccessful = agent.Warp(targetPosition);
                }
                if (!warpSuccessful)
                {
                    RaycastHit groundHit;
                    Vector3 rayStart = state.position + Vector3.up * 10f;

                    if (Physics.Raycast(rayStart, Vector3.down, out groundHit, 20f))
                    {
                        Vector3 groundPosition = groundHit.point;

                        if (NavMesh.SamplePosition(groundPosition, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                        {
                            warpSuccessful = agent.Warp(navHit.position);
                        }
                    }
                }
                if (warpSuccessful)
                {
                    agent.isStopped = state.isStopped;

                    if (state.hasPath && !state.isStopped)
                    {
                        StartCoroutine(DelayedSetDestination(agent, state.destination));
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to warp agent {agent.name} to valid NavMesh position");
                    agent.enabled = false;
                    StartCoroutine(ReenableAgentAfterDelay(agent, 1f));
                }
            }
        }
        RestoreParkedEnemies();

        yield return new WaitForSeconds(0.1f);

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.OnNavMeshUpdateCompleted();
            }
        }

        isUpdatingNavMesh = false;
        Debug.Log("NavMesh updated successfully.");
    }
    void ParkEnemiesOnChunk(GameObject chunk)
    {
        Bounds chunkBounds = new Bounds(chunk.transform.position, new Vector3(chunkSize, 100, chunkSize));
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && chunkBounds.Contains(enemy.transform.position))
            {
                if (!parkedEnemies.Contains(enemy))
                {
                    enemy.Park();
                    parkedEnemies.Add(enemy);
                    Debug.Log($"Parking enemy {enemy.name} on chunk being removed.");
                }
            }
        }
    }
    void RestoreParkedEnemies()
    {
        for (int i = parkedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = parkedEnemies[i];
            if (enemy != null)
            {
                if (enemy.Restore())
                {
                    Debug.Log("Successfully restored parked enemy");
                    parkedEnemies.RemoveAt(i);
                }
                else
                {
                    Debug.LogWarning("Filed to restore parked enemy");
                }
            }
            else
            {
                parkedEnemies.RemoveAt(i);
            }
        }
    }

    IEnumerator ReenableAgentAfterDelay(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (agent != null)
        {
            agent.enabled = true;

            if (NavMesh.SamplePosition(agent.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

    }
    IEnumerator DelayedSetDestination(NavMeshAgent agent, Vector3 destination)
    {
        yield return new WaitForEndOfFrame();
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }*/


    /*private class AgentState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 destination;
        public bool hasPath;
        public bool isStopped;

    }*/
}

     



