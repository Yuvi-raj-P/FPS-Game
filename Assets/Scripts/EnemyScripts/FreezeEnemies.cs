using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FreezeEnemies : MonoBehaviour
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

    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRate = 0.5f;
    public float attackRange = 2.5f;
    private float nextAttackTime = 0f;

    [Header("Flocking Settings")]
    public LayerMask enemyLayerMask;
    public float enemySeparationRadius = 3f;
    public float enemySeparationForce = 5f;

    [Header("Blackout Mechanic")]
    public float minLookTimeBeforeBlackout = 2f;
    public float maxLookTimeBeforeBlackout = 4f;
    public float blackoutDuration = 1f;
    public float blackoutSpeed = 2f;
    public float normalSpeed = 7f;

    [Header("Debug Info")]
    public bool isStuck = false;
    public float stuckCheckTime = 3f;
    public float stuckMovementThreshold = 2f;

    [Header("Stuck Recovery")]
    public float ignoreObstaclesDuration = 3f;
    public bool isIgnoringObstacles = false;

    [Header("Self Destruct")]
    public float maxStuckTime = 20f;
    public bool enableSelfDestruct = true;

    [Header("Animation Settings")]
    public Animator enemyAnimator;

    [Header("Eye UI Settings")]
    public GameObject eyesOpenImage;
    public GameObject eyesClosedImage;
    private bool wasMovingLastFrame = false;

    private float currentSpeed;
    private Vector3 moveDirection;
    public Camera playerCamera;
    private CharacterController controller;
    private Health playerHealth;

    private bool isBeingWatched = false;
    private float currentLookTime = 0f;
    private float timeUntilNextBlackout = 0f;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float ignoreObstaclesTimer = 0f;
    private LayerMask originalObstacleLayerMask;
    private float totalStuckTime = 0f;

    private Vector3 positionSampleStart;
    private float positionSampleTimer = 0f;
    private float movementSamplePeriod = 2f;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        FindPlayerCamera();
        controller = GetComponent<CharacterController>();
        currentSpeed = 0f;
        timeUntilNextBlackout = Random.Range(minLookTimeBeforeBlackout, maxLookTimeBeforeBlackout);

        originalObstacleLayerMask = obstacleLayerMask;

        maxSpeed *= Random.Range(0.9f, 1.1f);
        stoppingDistance *= Random.Range(0.9f, 1.2f);

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("FIX THE ERROR PLAYER IS NOT FOUND");
            }
        }
        if (enemyAnimator == null)
        {
            Debug.Log("The animator is assigned to a different object");
        }
        if (eyesOpenImage != null && eyesClosedImage != null)
        {
            eyesOpenImage.SetActive(false);
            eyesClosedImage.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Eyes not assigned");
        }
        
        lastPosition = transform.position;
        positionSampleStart = transform.position;
    }

    void FindPlayerCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null && player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObj != null)
            {
                playerCamera = cameraObj.GetComponent<Camera>();
            }
        }

        if (playerCamera == null)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.enabled && cam.gameObject.activeInHierarchy)
                {
                    playerCamera = cam;
                    break;
                }
            }
        }

        if (playerCamera == null)
        {
            Debug.LogWarning($"FreezeEnemies on {gameObject.name}: Could not find player camera!");
        }
    }

    void Update()
    {
        if (player == null) return;

        if (playerCamera == null)
        {
            FindPlayerCamera();
            if (playerCamera == null) return;
        }

        isBeingWatched = IsPlayerLooking();

        if (isBeingWatched && !UIManager.IsBlackoutActive)
        {
            currentLookTime += Time.deltaTime;
            if (currentLookTime >= timeUntilNextBlackout)
            {
                UIManager.Instance.TriggerBlackout(blackoutDuration);
                currentLookTime = 0f;
                timeUntilNextBlackout = Random.Range(minLookTimeBeforeBlackout, maxLookTimeBeforeBlackout);
            }
        }
        else if (!isBeingWatched)
        {
            currentLookTime = 0f;
        }

        HandleMovement();
        HandleAttack();
        CheckIfStuck();
        HandleObstacleIgnoring();
        UpdateEyeImages();
    }
    void UpdateEyeImages()
    {
        if (eyesOpenImage == null || eyesClosedImage == null) return;

        bool isCurrentlyMoving = IsEnemyMoving();

        if (isCurrentlyMoving != wasMovingLastFrame)
        {
            if (isCurrentlyMoving)
            {
                eyesOpenImage.SetActive(true);
                eyesClosedImage.SetActive(false);
            }
            else
            {
                eyesOpenImage.SetActive(false);
                eyesClosedImage.SetActive(true);
            }
            wasMovingLastFrame = isCurrentlyMoving;
        }
    }
    bool IsEnemyMoving()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool shouldMove = distanceToPlayer > stoppingDistance;
        bool canMove = !isBeingWatched || UIManager.IsBlackoutActive;
        bool hasSpeed = currentSpeed > 0.1f;

        return shouldMove && canMove && hasSpeed;
    }

    void CheckIfStuck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance && (!isBeingWatched || UIManager.IsBlackoutActive))
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
        ignoreObstaclesTimer = ignoreObstaclesDuration;

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
        Vector3 horizontalMove = Vector3.zero;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isBeingWatched && !UIManager.IsBlackoutActive)
        {
            currentSpeed = 0;
            enemyAnimator.speed = 0f;
        }
        else
        {
            maxSpeed = UIManager.IsBlackoutActive ? blackoutSpeed : normalSpeed;
            accelerationMultiplier = 30f;
            baseAcceleration = 10f;
            horizontalMove = HandleHorizontalMovement();
            enemyAnimator.speed = 1f;
        }

        if (controller.isGrounded)
        {
            moveDirection.y = -1f;
        }
        else
        {
            moveDirection.y += gravity * Time.deltaTime;
        }
        Vector3 finalMove = horizontalMove * currentSpeed + moveDirection.y * Vector3.up;
        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleAttack()
    {
        if (Time.time < nextAttackTime) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= stoppingDistance)
        {
            enemyAnimator.SetBool("Attack", true);
            AttackPlayer();
            nextAttackTime = Time.time + 1f / attackRate;
        }
        else
        {
            enemyAnimator.SetBool("Attack", false);
        }
    }

    void AttackPlayer()
    {
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

                if (leftClear)
                {
                    finalDirection = leftDirection;
                }
                else if (rightClear)
                {
                    finalDirection = rightDirection;
                }
                else
                {
                    finalDirection = -transform.forward;
                }
            }

            Vector3 avoidance = CalculateSideAvoidance();
            finalDirection = (finalDirection + avoidance).normalized;
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

        return finalDirection;
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
        }
        return separationVector.normalized;
    }

    bool IsPlayerLooking()
    {
        if (playerCamera == null || controller == null) return false;

        Vector3 enemyCenter = transform.position + controller.center;
        Vector3 directionToEnemy = (enemyCenter - playerCamera.transform.position).normalized;
        Vector3 cameraForward = playerCamera.transform.forward;

        float dot = Vector3.Dot(cameraForward, directionToEnemy);
        if (dot <= 0) return false;

        float viewAngle = 40f;
        float angle = Vector3.Angle(cameraForward, directionToEnemy);
        if (angle > viewAngle) return false;

        RaycastHit hit;
        float distanceToEnemy = Vector3.Distance(playerCamera.transform.position, enemyCenter);

        if (Physics.Raycast(playerCamera.transform.position, directionToEnemy, out hit, distanceToEnemy + 0.1f))
        {
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }

    Vector3 HandleHorizontalMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 finalDirection = Vector3.zero;

        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            finalDirection = CalculateMovementDirection(directionToPlayer);

            float distanceBasedAcceleration = baseAcceleration * (distanceToPlayer * accelerationMultiplier);
            currentSpeed += distanceBasedAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            if (finalDirection != Vector3.zero)
            {
                transform.LookAt(transform.position + finalDirection);
            }
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, 5f * Time.deltaTime);
        }
        return finalDirection;
    }

    bool CheckForObstacle(Vector3 direction, float distance)
    {
        Vector3 rayStart = transform.position + controller.center;
        return Physics.Raycast(rayStart, direction, distance, obstacleLayerMask);
    }

    Vector3 CalculateSideAvoidance()
    {
        if (isIgnoringObstacles)
            return Vector3.zero;
        Vector3 avoidance = Vector3.zero;
        Vector3 rayStart = transform.position + controller.center;

        RaycastHit leftHit, rightHit;
        bool leftObstacle = Physics.Raycast(rayStart, -transform.right, out leftHit, sideRayDistance, obstacleLayerMask);
        bool rightObstacle = Physics.Raycast(rayStart, transform.right, out rightHit, sideRayDistance, obstacleLayerMask);

        if (leftObstacle)
        {
            avoidance += transform.right * avoidanceForce;
        }
        if (rightObstacle)
        {
            avoidance -= transform.right * avoidanceForce;
        }
        return avoidance;
    }

    void OnDrawGizmosSelected()
    {
        if (controller == null) return;

        Vector3 rayStart = transform.position + controller.center;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(rayStart, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayStart, -transform.right * sideRayDistance);
        Gizmos.DrawRay(rayStart, transform.right * sideRayDistance);
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