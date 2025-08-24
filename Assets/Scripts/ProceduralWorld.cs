using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class ProceduralWorld : MonoBehaviour
{
    public Transform player;
    public int chunkSize = 10;
    public int renderDistance = 5;

    [Header("Material")]
    public Material groundMaterial;
    public Material[] scatteredPlaneMaterials;

    [Header("Scattered Planes")]
    public int minPlanesPerChunk = 4;
    public int maxPlanesPerChunk = 8;
    public Vector3 planeScale = new Vector3(1f, 1f, 1f);
    public float planeSize = 0.5f;
    public float minDistanceBetweenPlanes = 1.5f;
    public bool useGridBasedGeneration = true;
    public int gridSize = 3;

    [Header("Props")]
    public GameObject[] propPrefabs;
    public int minPropsPerChunk = 1;
    public int maxPropsPerChunk = 3;
    public bool useGridBasedPropGeneration = true;
    public int propGridSize = 2;
    public float minDistanceBetweenProps = 2f;
    public float propScaleVariation = 0.3f;

    [Header("Fallback Props")]
    public bool useFallbackCubes = true;
    public float fallbackCubeMinSize = 0.5f;
    public float fallbackCubeMaxSize = 2f;

    [Header("Player Spawn Protection")]
    public float playerSpawnClearRadius = 5f;
    public bool clearAdjacentChunks = true;

    private Vector2Int currentPlayerChunk;
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int initialPlayerChunk;
    private Vector3 initialPlayerPosition;
    public Quaternion Quarentation { get; private set; }

    //Abilities
    private List<DreamAbility> availableAbilities;
    private DreamAbility currentAbility;
    private bool abilityActive = false;

    private Vector3 eyesClosedOriginalPos;
    private Coroutine currentShake;
    private ColorAdjustments colorAdjustments;

    public enum AbilityType
    {
        FogOfThought
    }
    public class DreamAbility
    {
        public AbilityType type;
    }

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        if (player == null)
        {
            Debug.LogError("Player is not found please fix this RIGHT NOW!");
            this.enabled = false;
            return;
        }

        initialPlayerPosition = player.position;
        initialPlayerChunk = GetChunkCoordFromPosition(player.position);
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
    bool IsChunkNearPlayerSpawn(Vector2Int chunkCoord)
    {
        if (chunkCoord == initialPlayerChunk)
        {
            return true;
        }
        if (clearAdjacentChunks)
        {
            int deltaX = Mathf.Abs(chunkCoord.x - initialPlayerChunk.x);
            int deltaZ = Mathf.Abs(chunkCoord.y - initialPlayerChunk.y);

            if (deltaX <= 1 && deltaZ <= 1)
            {
                return true;
            }
        }
        return false;
    }
    bool IsPositionNearPlayerSpawn(Vector3 position)
    {
        float distanceToSpawn = Vector3.Distance(position, initialPlayerPosition);
        return distanceToSpawn <= playerSpawnClearRadius;
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

        if (groundMaterial != null)
        {
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.material = groundMaterial;
            }
        }
        bool isNearPlayerSpawn = IsChunkNearPlayerSpawn(coord);
        
        if (!isNearPlayerSpawn)
        {
            GenerateScatteredPlanes(chunkObject);
            GenerateProps(chunkObject);
            //Debug.Log($"Generated content for chunk {coord} (distance from spawn: {Vector3.Distance(chunkObject.transform.position, initialPlayerPosition):F1})");
        }
        else
        {
            Debug.Log($"Skipped content generation for chunk {coord} (too close to player spawn)");
        }

        activeChunks.Add(coord, chunkObject);
    }
    void GenerateScatteredPlanes(GameObject chunkObject)
    {
        if (scatteredPlaneMaterials == null || scatteredPlaneMaterials.Length == 0)
        {
            return;
        }
        if (useGridBasedGeneration)
        {
            GeneratePlanesWithGrid(chunkObject);
        }
        else
        {
            GeneratePlanesRandomly(chunkObject);
        }

    }
    void GenerateProps(GameObject chunkObject)
    {
        if (propPrefabs == null || propPrefabs.Length == 0 && !useFallbackCubes)
        {
            return;
        }

        if (useGridBasedPropGeneration)
        {
            GeneratePropsWithGrid(chunkObject);
        }
        else
        {
            GeneratePropsRandomly(chunkObject);
        }
    }
    void GeneratePropsWithGrid(GameObject chunkObject)
    {
        int numberOfProps = Random.Range(minPropsPerChunk, maxPropsPerChunk + 1);
        List<Vector2Int> availableGridCells = new List<Vector2Int>();

        for (int x = 0; x < propGridSize; x++)
        {
            for (int z = 0; z < propGridSize; z++)
            {
                availableGridCells.Add(new Vector2Int(x, z));
            }
        }
        for (int i = 0; i < availableGridCells.Count; i++)
        {
            Vector2Int temp = availableGridCells[i];
            int randomIndex = Random.Range(i, availableGridCells.Count);
            availableGridCells[i] = availableGridCells[randomIndex];
            availableGridCells[randomIndex] = temp;
        }

        for (int i = 0; i < numberOfProps && i < availableGridCells.Count; i++)
        {
            Vector2Int gridCell = availableGridCells[i];
            Vector3 propPosition = GetPropGridCellPosition(gridCell, chunkObject.transform.position);
            if (!IsPositionNearPlayerSpawn(propPosition))
            {
                CreateProp(propPosition, chunkObject);
            }
        }
    }
    void GeneratePropsRandomly(GameObject chunkObject)
    {
        List<Vector3> occupiedPositions = new List<Vector3>();
        int numberOfProps = Random.Range(minPropsPerChunk, maxPropsPerChunk + 1);

        for (int i = 0; i < numberOfProps; i++)
        {
            Vector3 propPosition = GetValidPropPosition(occupiedPositions, chunkObject.transform.position);

            if (propPosition != Vector3.zero && !IsPositionNearPlayerSpawn(propPosition))
            {
                CreateProp(propPosition, chunkObject);
                occupiedPositions.Add(propPosition);
            }
        }
    }
    Vector3 GetPropGridCellPosition(Vector2Int gridCell, Vector3 chunkCenter)
    {
        float cellSize = (float)chunkSize / propGridSize;
        float offsetX = (gridCell.x * cellSize) - (chunkSize / 2f) + (cellSize / 2f);
        float offsetZ = (gridCell.y * cellSize) - (chunkSize / 2f) + (cellSize / 2f);

        float randomX = Random.Range(-cellSize * 0.4f, cellSize * 0.4f);
        float randomZ = Random.Range(-cellSize * 0.4f, cellSize * 0.4f);

        return chunkCenter + new Vector3(offsetX + randomX, 0, offsetZ + randomZ);
    }

    void CreateProp(Vector3 position, GameObject chunkObject)
    {
        GameObject prop;

        if (propPrefabs != null && propPrefabs.Length > 0)
        {
            GameObject selectedPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            prop = Instantiate(selectedPrefab, position, Quaternion.identity);
            
            prop.transform.parent = chunkObject.transform;

            //float scaleMultiplier = Random.Range(1f - propScaleVariation, 1f + propScaleVariation);
            
            //prop.transform.localScale *= scaleMultiplier;
        }
        else if (useFallbackCubes)
        {
            prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = "FallbackCube";
            prop.transform.parent = chunkObject.transform;
            prop.transform.position = position;
            prop.transform.rotation = Random.rotation;

            float randomSize = Random.Range(fallbackCubeMinSize, fallbackCubeMaxSize);
            prop.transform.localScale = Vector3.one * randomSize;
        }
        else
        {
            return;
        }

        prop.transform.position = new Vector3(position.x, 0, position.z);

        prop.layer = LayerMask.NameToLayer("Obstacle");
    }
    Vector3 GetValidPropPosition(List<Vector3> occupiedPositions, Vector3 chunkCenter)
    {
        int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            float randomZ = Random.Range(-chunkSize / 2f, chunkSize / 2f);

            Vector3 candidatePosition = chunkCenter + new Vector3(randomX, 0f, randomZ);

            bool validPosition = true;

            foreach (Vector3 occupiedPos in occupiedPositions)
            {
                if (Vector3.Distance(candidatePosition, occupiedPos) < minDistanceBetweenProps)
                {
                    validPosition = false;
                    break;
                }
            }
            if (validPosition)
            {
                return candidatePosition;
            }
        }
        return Vector3.zero;
    }
    Bounds GetObjectBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    void GeneratePlanesWithGrid(GameObject chunkObject)
    {
        int numberOfPlanes = Random.Range(minPlanesPerChunk, maxPlanesPerChunk + 1);
        List<Vector2Int> availableGridCells = new List<Vector2Int>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                availableGridCells.Add(new Vector2Int(x, z));
            }
        }

        for (int i = 0; i < availableGridCells.Count; i++)
        {
            Vector2Int temp = availableGridCells[i];
            int randomIndex = Random.Range(i, availableGridCells.Count);
            availableGridCells[i] = availableGridCells[randomIndex];
            availableGridCells[randomIndex] = temp;
        }

        for (int i = 0; i < numberOfPlanes && i < availableGridCells.Count; i++)
        {
            Vector2Int gridCell = availableGridCells[i];
            Vector3 planePosition = GetGridCellPosition(gridCell, chunkObject.transform.position);
            
            
            if (!IsPositionNearPlayerSpawn(planePosition))
            {
                CreateScatteredPlane(planePosition, chunkObject);
            }
        }
    }
    void GeneratePlanesRandomly(GameObject chunkObject)
    {
        List<Vector3> occupiedPositions = new List<Vector3>();
        int numberOfPlanes = Random.Range(minPlanesPerChunk, maxPlanesPerChunk + 1);

        for (int i = 0; i < numberOfPlanes; i++)
        {
            Vector3 planePosition = GetValidPlanePosition(occupiedPositions, chunkObject.transform.position);
            if (planePosition != Vector3.zero && !IsPositionNearPlayerSpawn(planePosition))
            {
                CreateScatteredPlane(planePosition, chunkObject);
                occupiedPositions.Add(planePosition);
            }
        }
    }
    Vector3 GetGridCellPosition(Vector2Int gridCell, Vector3 chunkCenter)
    {
        float cellSize = (float)chunkSize / gridSize;
        float offsetX = (gridCell.x * cellSize) - (chunkSize / 2f) + (cellSize / 2f);
        float offsetZ = (gridCell.y * cellSize) - (chunkSize / 2f) + (cellSize / 2f);

        float randomX = Random.Range(-cellSize * 0.3f, cellSize * 0.3f);
        float randomZ = Random.Range(-cellSize * 0.3f, cellSize * 0.3f);

        return chunkCenter + new Vector3(offsetX + randomX, 0.01f, offsetZ + randomZ);
    }
    void CreateScatteredPlane(Vector3 position, GameObject chunkObject)
    {
        GameObject scatteredPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        scatteredPlane.name = "ScatteredPlane";
        scatteredPlane.transform.parent = chunkObject.transform;
        scatteredPlane.transform.position = position;

        Vector3 finalScale = new Vector3(
            planeScale.x / 10f,
            planeScale.y / 10f,
            planeScale.z / 10f
        );
        scatteredPlane.transform.localScale = finalScale;

        scatteredPlane.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        MeshCollider meshCollider = scatteredPlane.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            DestroyImmediate(meshCollider);
        }
        Material randomMaterial = scatteredPlaneMaterials[Random.Range(0, scatteredPlaneMaterials.Length)];
        Renderer planeRenderer = scatteredPlane.GetComponent<Renderer>();
        if (planeRenderer != null && randomMaterial != null)
        {
            planeRenderer.material = randomMaterial;
        }

    }
    Vector3 GetValidPlanePosition(List<Vector3> occupiedPositions, Vector3 chunkCenter)
    {
        int maxAttempts = 50;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomX = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            float randomZ = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            Vector3 candidatePosition = chunkCenter + new Vector3(randomX, 0.01f, randomZ);

            bool validPosition = true;

            foreach (Vector3 occupiedPos in occupiedPositions)
            {
                if (Vector3.Distance(candidatePosition, occupiedPos) < minDistanceBetweenPlanes)
                {
                    validPosition = false;
                    break;
                }
            }
            if (validPosition)
            {
                return candidatePosition;
            }
        }
        return Vector3.zero;
    }
}

     



