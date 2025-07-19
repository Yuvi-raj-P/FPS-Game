using UnityEngine;
using UnityEngine.AI;

public class ManualFollow : MonoBehaviour
{
    public Transform player;
    public float maxSpeed = 10f;
    public float baseAcceleration = 2f;
    public float stoppingDistance = 2f;
    public float accelerationMultiplier = 1f;
    public float gravity = -9.81f;
    public LayerMask groundLayerMask = -1;
    public LayerMask obstacleLayerMask = -1;
    public float groundCheckDistance = 1.8f;
    public float groundOffset = 0.91f;
    public float obstacleCheckDistance = 3f;
    public float avoidanceForce = 5f;
    public float sideRayDistance = 2f;

    private float currentSpeed;
    public float verticalVelocity;
    private Vector3 avoidanceDirection;
    private float avoidanceTimer;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        currentSpeed = 0f;
        verticalVelocity = 0f;
        avoidanceDirection = Vector3.zero;
    }

    void Update()
    {
        HandleGrounding();
        HandleMovement();
    }
    void HandleGrounding()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayerMask);

        if (isGrounded)
        {
            float targetY = hit.point.y + groundOffset;
            float currentY = transform.position.y;

            if (Mathf.Abs(currentY - targetY) > 0.2f)
            {
                float newY = Mathf.Lerp(currentY, targetY, 10f * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.Translate(0, verticalVelocity * Time.deltaTime, 0, Space.World);
        }
    }
    void HandleMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            Vector3 finalDirection = CalculateMovementDirection(directionToPlayer);

            float distanceBasedAcceleration = baseAcceleration * (distanceToPlayer * accelerationMultiplier);
            currentSpeed += distanceBasedAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            if (finalDirection != Vector3.zero)
            {
                transform.LookAt(transform.position + finalDirection);
                transform.Translate(finalDirection * currentSpeed * Time.deltaTime, Space.World);
            }
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, 5f * Time.deltaTime);
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
            finalDirection += (finalDirection + avoidance).normalized;
        }
        return finalDirection;
    }
    bool CheckForObstacle(Vector3 direction, float distance)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(rayStart, direction, distance, obstacleLayerMask);
    }
    Vector3 CalculateAvoidanceVector()
    {
        Vector3 avoidance = Vector3.zero;

        Vector3 leftDirection = -transform.right;
        Vector3 rightDirection = transform.right;

        RaycastHit leftHit, rightHit;
        bool leftObstacle = Physics.Raycast(transform.position, leftDirection, out leftHit, sideRayDistance, obstacleLayerMask);
        bool rightObstacle = Physics.Raycast(transform.position, rightDirection, out rightHit, sideRayDistance, obstacleLayerMask);

        if (leftObstacle)
        {
            float avoidanceStrength = 1f - (leftHit.distance / sideRayDistance);
            avoidance += rightDirection * avoidanceStrength * avoidanceForce;
        }
        if (rightObstacle)
        {
            float avoidanceStrength = 1f - (rightHit.distance / sideRayDistance);
            avoidance += leftDirection * avoidanceStrength * avoidanceForce;
        }
        return avoidance.normalized;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        Gizmos.DrawRay(rayStart, transform.forward * obstacleCheckDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.right * sideRayDistance);
        Gizmos.DrawRay(transform.position, transform.right * sideRayDistance);

        Gizmos.color = Color.blue;
        Vector3 leftDiag = Quaternion.Euler(0, -45, 0) * transform.forward;
        Vector3 rightDiag = Quaternion.Euler(0, 45, 0) * transform.forward;
        Gizmos.DrawRay(rayStart, leftDiag * obstacleCheckDistance);
        Gizmos.DrawRay(rayStart, rightDiag * obstacleCheckDistance);
        
    }

}

    