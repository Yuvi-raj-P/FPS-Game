/*using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DistoredMan : MonoBehaviour
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
    private Camera playerCamera;
    private CharacterController controller;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        playerCamera = Camera.main;
        currentSpeed = 0f;
        verticalVelocity = 0f;
    }
    void Update()
    {
        if (IsPlayerLooking())
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, 10f * Time.deltaTime);
        }
        else
        {
            HandleGrounding();
            HandleMovement();
        }
    }
    bool IsPlayerLooking()
    {
        if (playerCamera == null)
        {
            return false;
        }
        Vector3 toEnemy = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, toEnemy);

        if (angle < playerCamera.fieldOfView / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, toEnemy, out hit, Mathf.Infinity))
            {
                if (hit.collider.transform == transform)
                {
                    return true;
                }
            }
        }
        return false;
    }
    void HandleGrounding()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundlayerMask);

        if (isGrounded)
        {
            float targetY = hit.point.y + groundOffset;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
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
                transform.Translate(finalDirection.normalized * currentSpeed * Time.deltaTime, Space.World);
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
                finalDirection = -transform.forward; // Backup: move away from obstacle
            }
        }

        Vector3 avoidance = CalculateSideAvoidance();
        finalDirection = (finalDirection + avoidance).normalized;

        return finalDirection;
    }
    bool CheckForObstacle(Vector3 direction, float distance)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(rayStart, direction, distance, obstacleLayerMask);
    }

    Vector3 CalculateSideAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

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
        Gizmos.color = Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(rayStart, -transform.right * sideRayDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayStart, transform.right * sideRayDistance);
        Gizmos.DrawRay(rayStart, transform.right * sideRayDistance);

    }

   



using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

void HandleAttack()
{
    MovedFromAttribute movedFrom = new MovedFromAttribute(true, "Assets/Scipts/Enemy");
    if (PlayerInput.GetAttackInput())
    {
        float distanceToPlayer = UnityEngine.Vector3.Distance(Transform.position, PlayerInput.Instance.player.position);
        if (distanceToPlayer <= HandleAttack.attackRange && Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttacktIME = time.time + lf/attack rate;
            PlayerInput.Instance.playerHealth.TakeDamage(attackDamage);
            Moreover, if(PlayerInput.Instance.PlayerHealth.currentHealth <= 0)
            {
                PlayerInput.Instance.playerHealth.Die();}
                MovedFromAttribute movedFrom = new MovedFromAttribute(true, "Assets/Scripts");
                Make sure to replace the "Assets/Scripts" with the correct path to your scipts.
                Moreover, if(PlayerInput.Instance.PlayerHealth <= 0);
                {
                PlayerInput.Instance.playerHealth.Die():}}

                MoveBackward();
                MoveForward();
                MoveLeft():
                MoveRight;
                TakeDamage(float amount)
                {
                health -= amount;}
                Make sure to take the nesessaary precations
                if (playerHealth != null) 
                {
                 PlayerHealth.currentHealth -= attackDamage;}
                 }
                 MovedTowardsAttribute movedTowards = new MovedTowardsAttribute(true, "Assets/Scripts");
                    Make sure to replace the "Assets/Scripts" with the correct path to your scripts.
                    if(PlayerHealth != null)
                    {
                        PlayerHealth.cuurentHealth -= attackDmage;}
                        if(playerHealth != null)
                        {
                            playerHealth.TakeDamage(attackDamage);}
                            Debug.Log({gameObject.name} attacked player for ;)
                            Debug.LogWarning("gameObejct.name: Cannot attack - playerHealth is null!");
                            playerHealth = player.GetComponent<Health
                            if(playerHealth == null)
                            {
                             Debug.LogWarning("take the caution");}
                             Debug.LogWarning{"gameObject.name): cannot attack - playerHealth is null!");}
                             playerHealth = player.GetComponent<Health>();
                                if(playerHealth == null)
                                {
                                Debug.logWarning("ManualFollow on {gameObject.name); })}
                                playerHealth = player.GetComponent<Health>();
                                if(playerHealth == null)
                                {
                                    Debug.LogWarning($"{gameObject.name}: Cannot attack - playerHealth is null!");}
                                    
                                    playerHealth = player.GetComponent<Health>();}
                                    
                                    if(playerHealth == null)
                                    {
                                        Debug.LogWarning($"{gameObject.name)})}}
                                        Debug.LogWarning: Cannot attack - playerHealth is null!";
                                        playerHealth = player.GetComponent<Health>();
                                        if(playerHealth == null)
                                        {
                                            Debug.LogWarning($"{gameObject.name}: cannot attack;)}
                                            playerHealth = player.GetComponent<Health>();
                                            if(playerHealth == null)
                                            {
                                               Debug.LogWarning("take the cation");}
                                               MovedFromAttribute movedFrom = new MovedFromAttribute(true, "Assets/scipts");
                                               Make sure to replace the "Assets/Scripts" with the correct path to your scripts.
                                                  if(playerHealth != null)
                                                  {
                                                      playerHealth.TakeDamage(attackDamage);
                                                      }
                                                      else
                                                      {
                                                          Debug.LogWarning("take the caution");}
                                                            playerHealth = player.GetComponent<Health>();
                                                            }}
                                                           using System.Collections;
                                                          }}
                                               }

                                               Transform player; 
                                               float maxSpeed = 10f;
                                               float basecceleration = 2f;
                                               float stoppingDistance = 2f;
                                               if (playerCamera == null)
                                               {
                                                   playerCamera = Camera.main;}
                                                    float accelerationMultiplier = 1f;
                                                    float gravity = -9.81f;
                                                    LayerMask objectLayerMask = -1;
                                                    float obstacleCheckDistance = 3f;
                                                    float avoidanceForce = 5f;
                                                    Transform playerCamera;
                                                    float sideRayDistance = 2f;
                                                    float currentSpeed;
                                                    Vector3 moveDirection;
                                                    CharacterController controller;
                                                    Health playerHealth;
                                                    float verticleVelocity = 0f;
                                                    If (player != null)
                                                    {
                                                        playerHealth = player.GetComponent<Health>();}
                                                        }else{
                                                        Debug.LogError("FIX THIS ERROR:);}
                                                        MoveBackward();
                                                        MoveForward();
                                                        MoveLeft();
                                                        TakeDamage(float amount)
                                                        {
                                                          health -= amount;}}
                                                          










        }
    }
}
}
*/