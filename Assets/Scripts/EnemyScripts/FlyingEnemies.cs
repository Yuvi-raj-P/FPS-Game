using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FlyingEnemies : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float maxSpeed = 8f;
    public float baseAcceleration = 2f;
    public float stoppingDistance = 5f;
    public float accelerationMultiplier = 1f;
    public float flightHeight = 10f;

    [Header("Flying Animation Settings")]
    public float bobbingAmplitude = 0.5f;
    public float bobbingFrequency = 2f;
    public float bankingAngle = 20f;
    public float bankingSpeed = 3f;
    public float heightAdjustmentSpeed = 2f;
    public float naturalDrift = 0.3f;

    [Header("Avoidance Settings")]
    public LayerMask obstacleLayerMask = -1;
    public float obstacleCheckDistance = 4f;
    public float sideRayDistance = 3f;

    [Header("Flocking Settings")]
    public LayerMask enemyLayerMask;
    public float enemySeparationRadius = 4f;
    public float enemySeparationForce = 5f;
    public float targetOffsetUpdateTime = 2f;
    private Vector3 targetPositionOffset;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public int attackDamage = 20;
    public float attackRate = 1f;
    public float attackRange = 15f;
    public float projectileSpeed = 20f;
    public float projectileLifetime = 5f;
    private float nextAttackTime = 0f;

    private float currentSpeed;
    private CharacterController controller;
    private Health playerHealth;

    private float bobbingTimer;
    private float baseFlightHeight;
    private Vector3 currentVelocity;
    private Vector3 lastMoveDirection;
    private float bankingRotation;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        controller = GetComponent<CharacterController>();
        currentSpeed = 0f;
        baseFlightHeight = flightHeight;

        maxSpeed *= Random.Range(0.9f, 1.1f);
        stoppingDistance *= Random.Range(0.9f, 1.2f);

        bobbingTimer = Random.Range(0f, Mathf.PI * 2f);
        bobbingFrequency *= Random.Range(0.8f, 1.2f);
        bobbingAmplitude *= Random.Range(0.7f, 1.3f);

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogError("Player does not have a Health component.");
            }
        }

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
        HandleFlyingAnimation();
        HandleAttack();
    }
    void HandleMovement()
    {
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

            Vector3 driftDirection = new Vector3(Mathf.Sin(Time.time * 0.7f) * naturalDrift, 0, Mathf.Cos(Time.time * 0.5f) * naturalDrift);
            finalMoveDirection = (finalMoveDirection + driftDirection).normalized;

            float distanceBasedAcceleration = baseAcceleration * (distanceToTarget * accelerationMultiplier);
            currentSpeed += distanceBasedAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
        }
        else
        {
            finalMoveDirection = Vector3.zero;
            currentSpeed = Mathf.Lerp(currentSpeed, 0, 5f * Time.deltaTime);
        }

        currentVelocity = Vector3.Lerp(currentVelocity, finalMoveDirection * currentSpeed, Time.deltaTime * 5f);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * bankingRotationQ, Time.deltaTime * 3f);

        }
    }
    void HandleAttack()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            if (HasLineOfSight())
            {
                ShootProjectile();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - projectileSpawnPoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(projectileSpawnPoint.position, player.position);

        return !Physics.Raycast(projectileSpawnPoint.position, directionToPlayer, distanceToPlayer, obstacleLayerMask);
    }
    void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"FlyingEnemies on {gameObject.name}: No projectile prefab assigned!");
            return;
        }
        Vector3 targetPosition = PredictPlayerPosition();
        Vector3 shootDirection = (targetPosition - projectileSpawnPoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(shootDirection));

        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = shootDirection * projectileSpeed;
        }

        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<EnemyProjectile>();
        }

        projectileScript.Initialize(attackDamage, projectileLifetime);

        Debug.Log($"{gameObject.name} shot projectile at player!");
    }
    Vector3 PredictPlayerPosition()
    {
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

        float timeToTarget = Vector3.Distance(projectileSpawnPoint.position, player.position) / projectileSpeed;
        return player.position + playerVelocity * timeToTarget;
    }
    Vector3 CalculateMovementDirection(Vector3 directionToTarget)
    {
        Vector3 finalDirection = directionToTarget;

        if (CheckForObstacle(transform.forward, obstacleCheckDistance))
        {
            Vector3 leftDirection = Quaternion.Euler(0, -45, 0) * transform.forward;
            Vector3 rightDirection = Quaternion.Euler(0, 45, 0) * transform.forward;

            if (!CheckForObstacle(leftDirection, obstacleCheckDistance))
            {
                finalDirection = leftDirection;
            }
            else if (!CheckForObstacle(rightDirection, obstacleCheckDistance))
            {
                finalDirection = rightDirection;
            }
            else
            {
                finalDirection = Quaternion.Euler(0, -90, 0) * transform.forward;
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
                    separationVector += directionFromOther.normalized / distance;
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

        Gizmos.color = Color.blue;
        Vector3 leftDiag = Quaternion.Euler(0, -45, 0) * transform.forward;
        Vector3 rightDiag = Quaternion.Euler(0, 45, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDiag * obstacleCheckDistance);
        Gizmos.DrawRay(transform.position, rightDiag * obstacleCheckDistance);
    }
}
