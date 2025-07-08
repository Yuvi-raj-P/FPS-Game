using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public float lookRadius = 10f;

    public Transform target;
    NavMeshAgent agent;
    public int attackDamage = 10;

    public float health = 100f;
    public int attackRate = 1;
    private float nextAttackTime = 0f;

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            Debug.LogWarning("Target not assigned, using Player as target. FIX THIS BS ASAP");
        }
        agent = GetComponent<NavMeshAgent>();

    }

    void Update()
    {
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
