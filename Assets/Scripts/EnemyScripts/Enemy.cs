using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float lookRadius = 10f;
    public Transform target;
    public int attackDamage = 10;

    public float health = 100f;
    public int attackRate = 1;
    private NavMeshAgent agent;
    private float nextAttackTime = 0f;
    private bool isNavMeshUpdateInProgress = false;
    private Vector3 lastValidPosition;
    private Vector3 lastDestination;
    private bool hadPath;

    void Start()
    {
        target = PlayerManager.Instance.player.transform;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            Debug.LogWarning("Target not assigned, using Player as target. FIX THIS BS ASAP");
        }
        agent = GetComponent<NavMeshAgent>();
        lastValidPosition = transform.position;

    }

    void Update()
    {
        if (target == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;

        }
        if (!agent.isOnNavMesh && !isNavMeshUpdateInProgress)
        {
            StartCoroutine(HandleOffNavMesh());
            return;
        }
        if (isNavMeshUpdateInProgress)
        {
            return;
        }
        if (agent.isOnNavMesh)
        {
            lastValidPosition = transform.position;
            if (agent.hasPath)
            {
                lastDestination = agent.destination;
                hadPath = true;
            }
        }
        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);

            if (distance <= agent.stoppingDistance)
            {
                FaceTarget();
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
        }
        if (health <= 0f)
        {
            Die();
        }

    }
    public void OnNavMeshUpdateStarted()
    {
        isNavMeshUpdateInProgress = true;
        if (agent.isOnNavMesh)
        {
            lastValidPosition = transform.position;
            if (agent.hasPath)
            {
                lastDestination = agent.destination;
                hadPath = true;
            }
        }
    }
    public void OnNavMeshUpdateCompleted()
    {
        StartCoroutine(RestoreAfterNavMeshUpdate());
    }
    IEnumerator RestoreAfterNavMeshUpdate()
    {
        yield return new WaitForEndOfFrame();
        if (!agent.isOnNavMesh || ShouldGroundCheck())
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        isNavMeshUpdateInProgress = false;
    }
    private bool ShouldGroundCheck()
    {
        RaycastHit hit;
        return !Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f);
    }
    private Vector3 GetGroundedPosition(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = position + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            return hit.point;
        }
        return position;
    }
    IEnumerator HandleOffNavMesh()
    {
        isNavMeshUpdateInProgress = true;
        int attempts = 0;

        while (!agent.isOnNavMesh && attempts < 100)
        {
            NavMeshHit hit;
            Vector3 groundPos = GetGroundedPosition(lastValidPosition);

            if (NavMesh.SamplePosition(groundPos, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                break;

            }
            attempts++;
            yield return new WaitForFixedUpdate();
        }
        if (agent.isOnNavMesh && hadPath && target != null)
        {
            yield return new WaitForEndOfFrame();
            agent.SetDestination(target.position);
        }

        isNavMeshUpdateInProgress = false;
    }
    void Attack()
    {
        if (target != null)
        {
            target.GetComponent<Health>().TakeDamage(attackDamage);
            Debug.Log("Enemy attacked the player for " + attackDamage + " damage.");
        }
        else
        {
            Debug.LogWarning("Target is null, BRO FIX THIS");
        }
    }
    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
    void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }
    void Die()
    {
        this.gameObject.SetActive(false);
        Debug.Log("Enemy Died");
    }
}
