using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class ManualFollow : MonoBehaviour
{
    public Transform player;
    public float maxSpeed = 10f;
    public float baseAcceleration = 2f;
    public float stoppingDistance = 2f;
    public float accelerationMultiplier = 1f;
    public float gravity = -9.81f;
    public LayerMask obstacleLayerMask = -1;
    public float obstacleCheckDistance = 3f;
    public float avoidanceForce = 5f;
    public float sideRayDistance = 2f;

    [Header("Flocking Settings")]
    public LayerMask enemyLayerMask;
    public float enemySeparationRadius = 3f;
    public float enemySeparationForce = 5f;
    private Vector3 targetPositionOffset;
    public float targetOffsetUpdateTime = 2f;

    [Header("Animation Settings")]
    public Animator animator;

    private float currentSpeed;
    private Vector3 moveDirection;
    private CharacterController controller;
    private Health playerHealth;

    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRate = 0.5f;
    public float attackRange = 2.5f;
    private float nextAttackTime = 0f;

    [Header("Debug info")]
    public bool isStuck = false;
    public float stuckCheckTime = 2f;
    public float stuckMovementThreshold = 1f; // Minimum units to move per second

    public float igonareObstaclesDuration = 3f;
    public bool isIgnoringObstacles = false;

    

    [Header("Self Destruct")]
    public float maxStuckTime = 20f;
    public bool enableSelfDestruct = true;
    public float totalStuckTime = 0f;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float ignoreObstaclesTimer = 0f;
    private LayerMask originalObstacleLayerMask;

    private Vector3 positionSampleStart;
    private float positionSampleTimer = 0f;
    private float movementSamplePeriod = 1f;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        controller = GetComponent<CharacterController>();
        currentSpeed = 0f;

        maxSpeed *= Random.Range(0.9f, 1.1f);
        stoppingDistance *= Random.Range(0.9f, 1.2f);

        originalObstacleLayerMask = obstacleLayerMask;
        
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogWarning($"ManualFollow on {gameObject.name}: Could not find Health component on player!");
            }
        }
        lastPosition = transform.position;
        positionSampleStart = transform.position;
        InvokeRepeating(nameof(UpdateTargetOffset), 0, targetOffsetUpdateTime);
    }

    void UpdateTargetOffset()
    {
        targetPositionOffset = Random.insideUnitSphere * (stoppingDistance * 0.5f);
        targetPositionOffset.y = 0;
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
        CheckIfStuck();
        HandleObstacleIgnoring();
    }

    void CheckIfStuck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            positionSampleTimer += Time.deltaTime;

            if (positionSampleTimer >= movementSamplePeriod)
            {
                float totalDistanceMoved = Vector3.Distance(transform.position, positionSampleStart);
                
                if (totalDistanceMoved < stuckMovementThreshold)
                {
                    stuckTimer += movementSamplePeriod;
                    
                    if (stuckTimer >= stuckCheckTime)
                    {
                        if (!isStuck)
                        {
                            StartIgnoringObstacles();
                            Debug.Log($"{gameObject.name} detected as stuck - moved only {totalDistanceMoved:F2} units in {movementSamplePeriod} second(s)");
                        }
                        isStuck = true;
                        
                        if (enableSelfDestruct)
                        {
                            totalStuckTime += movementSamplePeriod;
                            if (totalStuckTime >= maxStuckTime)
                            {
                                SelfDestruct();
                                return;
                            }
                        }
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    totalStuckTime = 0f;
                    isStuck = false;
                }
                
                // Reset sample tracking
                positionSampleStart = transform.position;
                positionSampleTimer = 0f;
            }
        }
        else
        {
            isStuck = false;
            stuckTimer = 0f;
            totalStuckTime = 0f;
            positionSampleStart = transform.position;
            positionSampleTimer = 0f;
        }
        lastPosition = transform.position;
    }

    void SelfDestruct()
    {
        if (isIgnoringObstacles)
        {
            StopIgnoringObstacles();
        }
        Debug.Log($"{gameObject.name} is self-destructing after being stuck for too long!");
        Destroy(this.gameObject);
    }

    void HandleObstacleIgnoring()
    {
        if (isIgnoringObstacles)
        {
            ignoreObstaclesTimer -= Time.deltaTime;

            if (ignoreObstaclesTimer <= 0f || !isStuck)
            {
                StopIgnoringObstacles();
            }
        }
    }

    void StartIgnoringObstacles()
    {
        isIgnoringObstacles = true;
        ignoreObstaclesTimer = igonareObstaclesDuration;

        int obstacleLayer = GetObstacleLayerFromMask(originalObstacleLayerMask);
        if (obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(gameObject.layer, obstacleLayer, true);
        }
    }

    void StopIgnoringObstacles()
    {
        isIgnoringObstacles = false;
        ignoreObstaclesTimer = 0f;

        int obstacleLayer = GetObstacleLayerFromMask(originalObstacleLayerMask);
        if (obstacleLayer != -1)
        {
            Physics.IgnoreLayerCollision(gameObject.layer, obstacleLayer, false);
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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 horizontalDirection = Vector3.zero;

        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            horizontalDirection = CalculateMovementDirection(directionToPlayer);

            float distanceBasedAcceleration = baseAcceleration * (distanceToPlayer * accelerationMultiplier);
            currentSpeed += distanceBasedAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            if (horizontalDirection != Vector3.zero)
            {
                transform.LookAt(transform.position + horizontalDirection);
            }
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, 5f * Time.deltaTime);
        }
        if (!controller.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -1f;
        }

        Vector3 finalMove = horizontalDirection * currentSpeed + moveDirection.y * Vector3.up;

        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= stoppingDistance && Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + 1f / attackRate;
        }
        else
        {
            animator.SetBool("Attack", false);
        }
    }

    void AttackPlayer()
    {
        animator.SetBool("Attack", true);
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} attacked player for {attackDamage} damage!");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Cannot attack - playerHealth is null!");
        }
    }

    Vector3 CalculateMovementDirection(Vector3 directionToPlayer)
    {
        Vector3 finalDirection = directionToPlayer;
        if (!isIgnoringObstacles)
        {
            if (CheckForObstacle(transform.forward, obstacleCheckDistance))
            {
                Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * directionToPlayer;
                Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * directionToPlayer;

                bool leftClear = !CheckForObstacle(leftDirection, obstacleCheckDistance);
                bool rightClear = !CheckForObstacle(rightDirection, obstacleCheckDistance);

                if (leftClear && rightClear)
                {
                    float leftDot = Vector3.Dot(leftDirection, directionToPlayer);
                    float rightDot = Vector3.Dot(rightDirection, directionToPlayer);

                    finalDirection = leftDot > rightDot ? leftDirection : rightDirection;
                }
                else if (leftClear)
                {
                    finalDirection = leftDirection;
                }
                else if (rightClear)
                {
                    finalDirection = rightDirection;
                }
                else
                {
                    Vector3 sharpLeft = Quaternion.Euler(0, -90, 0) * directionToPlayer;
                    Vector3 sharpRight = Quaternion.Euler(0, 90, 0) * directionToPlayer;

                    if (!CheckForObstacle(sharpLeft, obstacleCheckDistance))
                    {
                        finalDirection = sharpLeft;
                    }
                    else if (!CheckForObstacle(sharpRight, obstacleCheckDistance))
                    {
                        finalDirection = sharpRight;
                    }
                    else
                    {
                        finalDirection = -transform.forward * 0.5f;
                    }
                }
            }

            Vector3 avoidance = CalculateAvoidanceVector();
            if (avoidance != Vector3.zero)
            {
                finalDirection = (finalDirection + avoidance).normalized;
            }
        }
        else
        {
            finalDirection = directionToPlayer;
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
                    separationVector += directionFromOther.normalized / distance;
                }
            }
            separationVector /= (nearbyEnemies.Length - 1);
        }
        return separationVector.normalized;
    }

    bool CheckForObstacle(Vector3 direction, float distance)
    {
        Vector3 rayStart = transform.position + Vector3.up * (controller.height / 2);
        return Physics.Raycast(rayStart, direction, distance, obstacleLayerMask);
    }

    Vector3 CalculateAvoidanceVector()
    {
        if (isIgnoringObstacles)
            return Vector3.zero;

        Vector3 avoidance = Vector3.zero;

        Vector3 leftDirection = -transform.right;
        Vector3 rightDirection = transform.right;

        RaycastHit leftHit, rightHit;
        Vector3 rayStart = transform.position + controller.center;
        bool leftObstacle = Physics.Raycast(rayStart, leftDirection, out leftHit, sideRayDistance, obstacleLayerMask);
        bool rightObstacle = Physics.Raycast(rayStart, rightDirection, out rightHit, sideRayDistance, obstacleLayerMask);

        if (leftObstacle)
        {
            float avoidanceStrength = 1f - (leftHit.distance / sideRayDistance);
            avoidance += rightDirection * avoidanceStrength;
        }
        if (rightObstacle)
        {
            float avoidanceStrength = 1f - (rightHit.distance / sideRayDistance);
            avoidance += leftDirection * avoidanceStrength;
        }
        return avoidance;
    }

    void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        Gizmos.color = Color.red;
        Vector3 rayStart = transform.position + Vector3.up * (controller.height / 2);

        Gizmos.DrawRay(rayStart, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.yellow;
        Vector3 sideRayStart = transform.position + controller.center;
        Gizmos.DrawRay(sideRayStart, -transform.right * sideRayDistance);
        Gizmos.DrawRay(sideRayStart, transform.right * sideRayDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, enemySeparationRadius);

        Gizmos.color = Color.blue;
        Vector3 leftDiag = Quaternion.Euler(0, -45, 0) * transform.forward;
        Vector3 rightDiag = Quaternion.Euler(0, 45, 0) * transform.forward;
        Gizmos.DrawRay(rayStart, leftDiag * obstacleCheckDistance);
        Gizmos.DrawRay(rayStart, rightDiag * obstacleCheckDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnDestroy()
    {
        if (isIgnoringObstacles)
        {
            int obstacleLayer = GetObstacleLayerFromMask(originalObstacleLayerMask);
            if (obstacleLayer != -1)
            {
                Physics.IgnoreLayerCollision(gameObject.layer, obstacleLayer, false);
            }
        }
    }
}