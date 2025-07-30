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

    private float currentSpeed;
    private Vector3 moveDirection; 
    private CharacterController controller;
    private Health playerHealth;

    [Header("Attack Settings")]
    public int attackDamage = 25;
    public float attackRate = 0.5f;
    public float attackRange = 2.5f;
    private float nextAttackTime = 0f;

    // Unused variables can be removed.
    // private Vector3 avoidanceDirection;
    // private float avoidanceTimer;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        controller = GetComponent<CharacterController>();
        currentSpeed = 0f;

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth == null)
            {
                Debug.LogWarning($"ManualFollow on {gameObject.name}: Could not find Health component on player!");
            }
        }
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
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
        return finalDirection.normalized; 
    }
    bool CheckForObstacle(Vector3 direction, float distance)
    {
        Vector3 rayStart = transform.position + Vector3.up * (controller.height / 2);
        return Physics.Raycast(rayStart, direction, distance, obstacleLayerMask);
    }
    Vector3 CalculateAvoidanceVector()
    {
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

        Gizmos.color = Color.blue;
        Vector3 leftDiag = Quaternion.Euler(0, -45, 0) * transform.forward;
        Vector3 rightDiag = Quaternion.Euler(0, 45, 0) * transform.forward;
        Gizmos.DrawRay(rayStart, leftDiag * obstacleCheckDistance);
        Gizmos.DrawRay(rayStart, rightDiag * obstacleCheckDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}