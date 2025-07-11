using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemies : MonoBehaviour
{
    public float lookRadius = 10f;
    public Transform target;
    public int attackDamage = 10;
    public float attackRate = 0.5f;
    private float nextAttackTime = 0f;

    [Header("Flying Settings")]
    public float flySpeed = 5f;
    public float hoverHeight = 3f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;

    private Vector3 startPosition;
    private NavMeshAgent agent;
    private Vector3 baseDestination;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = PlayerManager.Instance.player.transform;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            Debug.LogWarning("Target not assigned, using Player as target. FIX THIS ASAP");
        }
        agent = GetComponent<NavMeshAgent>();
        agent.speed = flySpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.updateRotation = false;
        agent.updatePosition = false;

        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition;
        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);
            FaceTarget();

            if (distance <= 3f)
            {
                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
        }
        else
        {
            agent.SetDestination(startPosition);
        }
        Vector3 agentPathPoint = agent.nextPosition;
        float desiredHeight = hoverHeight + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        float targetHeight = (distance <= agent.stoppingDistance) ? target.position.y + hoverHeight : agentPathPoint.y + desiredHeight;
        desiredPosition = new Vector3(agentPathPoint.x, targetHeight, agentPathPoint.z);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * flySpeed);
    }
    void Attack()
    {
        if (target != null)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
                Debug.Log("Flying enemy attacked the player for " + attackDamage + "damage.");

            }
            else
            {
                Debug.LogWarning("Target does not have a Health component, BRO FIX THIS RIGHT NOW");
            }
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);

        if (target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(target.position + Vector3.up * hoverHeight, Vector3.one * 0.5f);
        }
    }
}
