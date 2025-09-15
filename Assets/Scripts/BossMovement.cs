using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BossMovement : MonoBehaviour
{
    [Header("Boss Movement Settings")]
    public Transform player;
    public float maxSpeed = 6f;
    public float baseAcceleration = 1.5f;
    public float stoppingDistance = 8f;
    public float accelerationMultiplier = 1f;
    public float flightHeight = 15f;

    [Header("Boss Flying Animation Settings")]
    public float bobbingAmplitude = 0.8f;
    public float bobbingFrequency = 1.5f;
    public float bankingAngle = 30f;
    public float bankingSpeed = 2f;
    public float heightAdjustmentSpeed = 2f;
    public float naturalDrift = 0.5f;

    [Header("Avoidance Settings")]
    public LayerMask obstacleLayerMask = -1;
    public float obstacleCheckDistance = 6f;
    public float sideRayDistance = 4f;
    public bool ignoreObstacleCollisions = true;

    [Header("Boss Flocking Settings")]
    public LayerMask enemyLayerMask;
    public float enemySeparationRadius = 6f;
    public float enemySeparationForce = 3f;
    public float targetOffsetUpdateTime = 3f;
    private Vector3 targetPositionOffset;

    [Header("Boss Attack Settings")]
    public GameObject projectilePrefab;
    public Transform[] projectileSpawnPoints; 
    public int attackDamage = 40;
    public float attackRate = 2f;
    public float attackRange = 20f;
    public float projectileSpeed = 25f;
    public float projectileLifetime = 8f;
    private float nextAttackTime = 0f;

    [Header("Boss Special Abilities")]
    public bool enableMultiShot = true;
    public int multiShotCount = 3;
    public float multiShotSpread = 15f;
    public float specialAttackCooldown = 8f;
    private float nextSpecialAttackTime = 0f;

    [Header("Boss Aggression")]
    public float aggressionLevel = 1f;
    public float rageHealthThreshold = 0.3f;
    public float rageSpeedMultiplier = 1.5f;
    public float rageAttackRateMultiplier = 2f;
    private bool isInRageMode = false;

    [Header("Debug Info")]
    public bool isStuck = false;
    public float stuckCheckTime = 3f;
    public float stuckMovementThreshold = 0.1f;

    [Header("Boss Self Destruct")]
    public float maxStuckTime = 30f;
    public bool enableSelfDestruct = false;

    private float currentSpeed;
    private CharacterController controller;
    private Health playerHealth;
    private BossController bossController;

    private float bobbingTimer;
    private float baseFlightHeight;
    private Vector3 currentVelocity;
    private Vector3 lastMoveDirection;
    private float bankingRotation;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float totalStuckTime = 0f;

    void Start()
    {
        if (player == null)
        {
            player = PlayerManager.Instance?.player?.transform;
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }
        }

        controller = GetComponent<CharacterController>();
        bossController = GetComponent<BossController>();
        currentSpeed = 0f;
        baseFlightHeight = flightHeight;

        maxSpeed *= Random.Range(0.95f, 1.05f);
        stoppingDistance *= Random.Range(0.95f, 1.1f);

        bobbingTimer = Random.Range(0f, Mathf.PI * 2f);
        bobbingFrequency *= Random.Range(0.9f, 1.1f);
        bobbingAmplitude *= Random.Range(0.8f, 1.2f);

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("Player does not have a Health component.");
            }
        }

        lastPosition = transform.position;
        InvokeRepeating(nameof(UpdateTargetOffset), 0, targetOffsetUpdateTime);

        Debug.Log("Boss Movement initialized!");
    }

    void UpdateTargetOffset()
    {
        targetPositionOffset = Random.insideUnitSphere * (stoppingDistance * 0.7f);
        targetPositionOffset.y = Random.Range(-2f, 3f); 
    }

    void Update()
    {
        CheckRageMode();
        HandleMovement();
        HandleFlyingAnimation();
        HandleBossAttack();
        IgnoreCollisions();
        CheckIfStuck();
    }

    void CheckRageMode()
    {
        if (bossController != null)
        {
            float healthPercentage = bossController.GetHealthPercentage();
            bool shouldBeInRage = healthPercentage <= rageHealthThreshold;
            
            if (shouldBeInRage && !isInRageMode)
            {
                EnterRageMode();
            }
            else if (!shouldBeInRage && isInRageMode)
            {
                ExitRageMode();
            }
        }
    }

    void EnterRageMode()
    {
        isInRageMode = true;
        Debug.Log("Boss entered RAGE MODE!");
        
        maxSpeed *= rageSpeedMultiplier;
        attackRate *= rageAttackRateMultiplier;
        
        bobbingAmplitude *= 1.5f;
        bobbingFrequency *= 1.3f;
    }

    void ExitRageMode()
    {
        isInRageMode = false;
        Debug.Log("Boss exited rage mode");
        
        maxSpeed /= rageSpeedMultiplier;
        attackRate /= rageAttackRateMultiplier;
        
        bobbingAmplitude /= 1.5f;
        bobbingFrequency /= 1.3f;
    }

    void CheckIfStuck()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckMovementThreshold)
            {
                stuckTimer += Time.deltaTime;

                if (enableSelfDestruct)
                {
                    totalStuckTime += Time.deltaTime;

                    if (totalStuckTime >= maxStuckTime)
                    {
                        SelfDestruct();
                        return;
                    }
                }
                if (stuckTimer >= stuckCheckTime)
                {
                    isStuck = true;
                }
            }
            else
            {
                stuckTimer = 0f;
                totalStuckTime = 0f;
                isStuck = false;
            }
        }
        else
        {
            isStuck = false;
            stuckTimer = 0f;
            totalStuckTime = 0f;
        }
        lastPosition = transform.position;
    }

    void SelfDestruct()
    {
        Debug.LogWarning("Boss self-destructed due to being stuck!");
        if (ignoreObstacleCollisions)
        {
            int obstacleLayer = GetObstacleLayerFromMask(obstacleLayerMask);
            if (obstacleLayer != -1)
            {
                Physics.IgnoreLayerCollision(gameObject.layer, obstacleLayer, false);
            }
        }
                if (bossController != null)
        {
            bossController.TakeDamage(bossController.maxHealth);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void IgnoreCollisions()
    {
        if (!ignoreObstacleCollisions) return;
        
        int obstacleLayer = GetObstacleLayerFromMask(obstacleLayerMask);
        if (obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(gameObject.layer, obstacleLayer, true);
        }
    }

    int GetObstacleLayerFromMask(LayerMask mask)
    {
        int layerIndex = 0;
        int maskValue = mask.value;

        while (maskValue > 1)
        {
            maskValue >>= 1;
            layerIndex++;
        }
        return maskValue == 1 ? layerIndex : -1;
    }

    void HandleMovement()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + targetPositionOffset;
        bobbingTimer += Time.deltaTime * bobbingFrequency;
        float bobbingOffset = Mathf.Sin(bobbingTimer) * bobbingAmplitude;
        targetPosition.y = player.position.y + baseFlightHeight + bobbingOffset;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        Vector3 finalMoveDirection;

        if (distanceToTarget > stoppingDistance)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            finalMoveDirection = CalculateMovementDirection(directionToTarget);

            Vector3 driftDirection = new Vector3(
                Mathf.Sin(Time.time * 0.7f) * naturalDrift, 
                Mathf.Sin(Time.time * 0.3f) * naturalDrift * 0.5f, 
                Mathf.Cos(Time.time * 0.5f) * naturalDrift
            );
            finalMoveDirection = (finalMoveDirection + driftDirection).normalized;

            float distanceBasedAcceleration = baseAcceleration * (distanceToTarget * accelerationMultiplier * aggressionLevel);
            currentSpeed += distanceBasedAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        else
        {
            finalMoveDirection = Vector3.zero;
            currentSpeed = Mathf.Lerp(currentSpeed, 0, 3f * Time.deltaTime);
        }

        currentVelocity = Vector3.Lerp(currentVelocity, finalMoveDirection * currentSpeed, Time.deltaTime * 4f);
        controller.Move(currentVelocity * Time.deltaTime);

        lastMoveDirection = finalMoveDirection;
    }

    void HandleFlyingAnimation()
    {
        float horizontalMovement = Vector3.Dot(lastMoveDirection, transform.right);
        float targetBanking = -horizontalMovement * bankingAngle;
        bankingRotation = Mathf.Lerp(bankingRotation, targetBanking, Time.deltaTime * bankingSpeed);

        if (lastMoveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            Quaternion bankingRotationQ = Quaternion.Euler(0, 0, bankingRotation);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * bankingRotationQ, Time.deltaTime * 2f);
        }
    }

    void HandleBossAttack()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            if (HasLineOfSight())
            {
                if (enableMultiShot && Random.Range(0f, 1f) < 0.3f)
                {
                    ShootMultiProjectile();
                }
                else
                {
                    ShootProjectile();
                }
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        if (Time.time >= nextSpecialAttackTime && distanceToPlayer <= attackRange * 1.5f)
        {
            if (HasLineOfSight())
            {
                PerformSpecialAttack();
                nextSpecialAttackTime = Time.time + specialAttackCooldown;
            }
        }
    }

    bool HasLineOfSight()
    {
        if (projectileSpawnPoints == null || projectileSpawnPoints.Length == 0) return false;
        
        Transform spawnPoint = projectileSpawnPoints[0];
        Vector3 directionToPlayer = (player.position - spawnPoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(spawnPoint.position, player.position);

        return !Physics.Raycast(spawnPoint.position, directionToPlayer, distanceToPlayer, obstacleLayerMask);
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoints == null || projectileSpawnPoints.Length == 0)
        {
            Debug.LogWarning($"Boss on {gameObject.name}: Missing projectile prefab or spawn points!");
            return;
        }

        Transform spawnPoint = projectileSpawnPoints[Random.Range(0, projectileSpawnPoints.Length)];
        Vector3 targetPosition = PredictPlayerPosition();
        Vector3 shootDirection = (targetPosition - spawnPoint.position).normalized;

        CreateProjectile(spawnPoint.position, shootDirection);

        Debug.Log($"Boss {gameObject.name} shot projectile at player!");
    }

    void ShootMultiProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoints == null || projectileSpawnPoints.Length == 0) return;

        Transform spawnPoint = projectileSpawnPoints[Random.Range(0, projectileSpawnPoints.Length)];
        Vector3 baseDirection = (PredictPlayerPosition() - spawnPoint.position).normalized;

        for (int i = 0; i < multiShotCount; i++)
        {
            float angle = (i - (multiShotCount - 1) * 0.5f) * multiShotSpread;
            Vector3 shootDirection = Quaternion.Euler(0, angle, 0) * baseDirection;
            
            CreateProjectile(spawnPoint.position, shootDirection);
        }

        Debug.Log($"Boss {gameObject.name} fired multishot with {multiShotCount} projectiles!");
    }

    void PerformSpecialAttack()
    {
        if (projectileSpawnPoints == null || projectileSpawnPoints.Length == 0) return;

        Transform spawnPoint = projectileSpawnPoints[Random.Range(0, projectileSpawnPoints.Length)];
        int circularShotCount = 8;
        
        for (int i = 0; i < circularShotCount; i++)
        {
            float angle = (360f / circularShotCount) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            CreateProjectile(spawnPoint.position, direction);
        }

        Debug.Log($"Boss {gameObject.name} performed special circular barrage attack!");
    }

    void CreateProjectile(Vector3 spawnPosition, Vector3 direction)
    {
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = direction * projectileSpeed;
        }

        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<EnemyProjectile>();
        }

        projectileScript.Initialize(attackDamage, projectileLifetime);
    }

    Vector3 PredictPlayerPosition()
    {
        if (player == null) return Vector3.zero;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        CharacterController playerController = player.GetComponent<CharacterController>();

        Vector3 playerVelocity = Vector3.zero;

        if (playerRb != null)
        {
            playerVelocity = playerRb.linearVelocity;
        }
        else if (playerController != null)
        {
            playerVelocity = playerController.velocity;
        }

        float timeToTarget = Vector3.Distance(transform.position, player.position) / projectileSpeed;
        return player.position + playerVelocity * timeToTarget;
    }

    Vector3 CalculateMovementDirection(Vector3 directionToTarget)
    {
        Vector3 finalDirection = directionToTarget;

        if (CheckForObstacle(transform.forward, obstacleCheckDistance))
        {
            Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * transform.forward;
            Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * transform.forward;
            Vector3 upDirection = Quaternion.Euler(-30, 0, 0) * transform.forward;

            if (!CheckForObstacle(upDirection, obstacleCheckDistance))
            {
                finalDirection = upDirection;
            }
            else if (!CheckForObstacle(leftDirection, obstacleCheckDistance))
            {
                finalDirection = leftDirection;
            }
            else if (!CheckForObstacle(rightDirection, obstacleCheckDistance))
            {
                finalDirection = rightDirection;
            }
            else
            {
                finalDirection = Vector3.up;
            }
        }

        Vector3 separation = CalculateSeparationVector();
        if (separation != Vector3.zero)
        {
            finalDirection = (finalDirection + separation * enemySeparationForce).normalized;
        }

        return finalDirection.normalized;
    }

    Vector3 CalculateSeparationVector()
    {
        Vector3 separationVector = Vector3.zero;
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, enemySeparationRadius, enemyLayerMask);

        if (nearbyEnemies.Length > 1)
        {
            foreach (var enemyCollider in nearbyEnemies)
            {
                if (enemyCollider.gameObject == gameObject) continue;

                Vector3 directionFromOther = transform.position - enemyCollider.transform.position;
                float distance = directionFromOther.magnitude;
                if (distance > 0)
                {
                    float force = enemyCollider.gameObject.name.ToLower().Contains("boss") ? 1f : 2f;
                    separationVector += (directionFromOther.normalized / distance) * force;
                }
            }
            separationVector /= (nearbyEnemies.Length - 1);
        }
        return separationVector.normalized;
    }

    bool CheckForObstacle(Vector3 direction, float distance)
    {
        return Physics.Raycast(transform.position, direction, distance, obstacleLayerMask);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemySeparationRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 leftDiag = Quaternion.Euler(0, -45, 0) * transform.forward;
        Vector3 rightDiag = Quaternion.Euler(0, 45, 0) * transform.forward;
        Vector3 upDiag = Quaternion.Euler(-30, 0, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDiag * obstacleCheckDistance);
        Gizmos.DrawRay(transform.position, rightDiag * obstacleCheckDistance);
        Gizmos.DrawRay(transform.position, upDiag * obstacleCheckDistance);

        if (projectileSpawnPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var spawnPoint in projectileSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                }
            }
        }
    }
}