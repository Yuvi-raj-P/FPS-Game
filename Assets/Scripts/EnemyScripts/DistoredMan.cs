using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

public class DistoredMan : MonoBehaviour
{
    [Header("General Settings")]
    public float lookRadius = 10f;
    public Transform target;
    public int attackDamage = 25;
    public float attackRate = 0.3f;
    private float nextAttackTime = 0f;
    public float health = 100f;

    [Header("Vision Settings")]

    public float viewAngle = 60f;
    public float maxViewDistance = 15f;

    [Header("Blackout Mechanic")]
    public float minLookTimeBeforeBlackout = 2f;
    public float maxLookTimeBeforeBlackout = 5f;
    public float blackoutDuration = 1f;
    public float blackoutSpeed = 2f;
    public float normalSpeed = 3.5f;

    private NavMeshAgent agent;
    private bool isBeingWatched = false;
    private float currentLookTime = 0f;
    private float timeUntilNextBlackout = 0f;

    //Flicker variables
    /*public float flickerInterval = 3f;
    public float flickerDuration = 0.5f;
    public float flickerSpeed = 0.1f;
    private bool isFlickering = false;
    private bool canMoveDuringFlicker = false;
    private bool hasHadFirstBlackout = false;
    private Coroutine flickerCoroutine;*/
    void Start()
    {
        target = PlayerManager.Instance.player.transform;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            Debug.LogWarning("Target not assigned, using Player as target. FIX THIS RIGHT NOW BRO");
        }
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        timeUntilNextBlackout = Random.Range(minLookTimeBeforeBlackout, maxLookTimeBeforeBlackout);
    }
    void Update()
    {

        isBeingWatched = IsPlayerLookingAtMe();

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
        else
        {
            //There is nothing in here; Intentionally left empty
        }

        if (!isBeingWatched || UIManager.IsBlackoutActive)
        {
            agent.speed = UIManager.IsBlackoutActive ? blackoutSpeed : normalSpeed;
            float distance = Vector3.Distance(target.position, transform.position);
            if (distance <= lookRadius)
            {
                agent.SetDestination(target.position);
                FaceTarget();
                if (distance <= agent.stoppingDistance)
                {
                    if (Time.time >= nextAttackTime)
                    {
                        Attack();
                        nextAttackTime = Time.time + 1f / attackRate;
                    }
                }
            }
        }
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    // After testing, the flicker effect is not going to be used in the game
    //Adds unnecssary confusing to the player and some error is causing damage without knowing
    /*IEnumerator FlickerCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(flickerInterval);

            if (isBeingWatched && !isInBlackout && darknessImage != null)
            {
                if (flickerCoroutine == null)
                {
                    flickerCoroutine = StartCoroutine(FlickerEffect());
                }
            }
        }
    }
    IEnumerator FlickerEffect()
    {
        isFlickering = true;
        float flickerTimer = 0f;

        while (flickerTimer < flickerDuration)
        {
            float alpha = Mathf.PingPong(Time.time / flickerSpeed, 1f);

            canMoveDuringFlicker = alpha > 0.7f;

            Color color = darknessImage.color;
            color.a = alpha * 0.8f;
            darknessImage.color = color;

            flickerTimer += Time.deltaTime;
            yield return null;
        }

        Color finalColor = darknessImage.color;
        finalColor.a = 0f;
        darknessImage.color = finalColor;

        isFlickering = false;
        canMoveDuringFlicker = false;
        flickerCoroutine = null;
    }*/

    bool IsPlayerLookingAtMe()
    {
        if (target == null) return false;

        Camera playerCamera = target.GetComponentInChildren<Camera>();
        if (playerCamera == null) return false;

        Vector3 directionToEnemy = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToEnemy);
        if (angle > viewAngle / 2f) return false;

        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (distance > maxViewDistance) return false;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, directionToEnemy, out hit, distance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;

    }
    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxViewDistance);
    }
    void Attack()
    {
        if (target != null)
        {
            target.GetComponent<Health>().TakeDamage(attackDamage);

        }
        else
        {
            Debug.LogWarning("Target is null, FIX THIS ASAP");
        }

    }
    bool IsPlayerLookingAtMeAcc()
    {
        if (target == null) return false;

        Camera playerCamera = target.GetComponentInChildren<Camera>();
        if (playerCamera == null) return false;

        Renderer enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null) return false;
        Bounds bounds = enemyRenderer.bounds;

        Vector3[] samplePoints = {
        bounds.center,
        bounds.min,
        bounds.max,
        new Vector3(bounds.min.x, bounds.center.y, bounds.center.z),
        new Vector3(bounds.max.x, bounds.center.y, bounds.center.z),
        new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
        new Vector3(bounds.center.x, bounds.max.y, bounds.center.z),
        new Vector3(bounds.center.x, bounds.center.y, bounds.min.z),
        new Vector3(bounds.center.x, bounds.center.y, bounds.max.z)
    };

        foreach (Vector3 point in samplePoints)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(point);

            if (screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
                screenPoint.y >= 0 && screenPoint.y <= Screen.height &&
                screenPoint.z > 0)
            {
                Vector3 direction = (point - playerCamera.transform.position).normalized;
                float distance = Vector3.Distance(playerCamera.transform.position, point);

                if (distance <= maxViewDistance)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(playerCamera.transform.position, direction, out hit, distance))
                    {
                        if (hit.collider.gameObject == gameObject)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    bool IsPlayerLookingAtMostAccurateWhenIchangetheModels()
    {
        if (target == null) return false;
        Camera playerCamera = target.GetComponentInChildren<Camera>();
        Renderer enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null) return false;

        Bounds bounds = enemyRenderer.bounds;

        Vector3[] samplePoints = {
            bounds.center,
            bounds.min,
            bounds.max,
            new Vector3(bounds.min.x, bounds.center.y, bounds.center.z),
            new Vector3(bounds.max.x, bounds.center.y, bounds.center.z),
            new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
            new Vector3(bounds.center.x, bounds.max.y, bounds.center.z),
            new Vector3(bounds.center.x, bounds.center.y, bounds.min.z),
            new Vector3(bounds.center.x, bounds.center.y, bounds.max.z)
        };
        foreach (Vector3 point in samplePoints)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(point);

            if (screenPoint.x >= 0 && screenPoint.x <= Screen.width && screenPoint.y >= 0 && screenPoint.y <= Screen.height && screenPoint.z > 0)
            {
                Vector3 direction = (point - playerCamera.transform.position).normalized;
                float distance = Vector3.Distance(playerCamera.transform.position, point);
                if (distance <= maxViewDistance)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(playerCamera.transform.position, direction, out hit, distance))
                    {
                        if (hit.collider.gameObject == gameObject)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    /*bool IsPlayerLookingDifferentMethodForMoreAccuracy()
    {
        // This method needs to be tested before its use in the game as performance issues with WebGL but best and most accurate so far
        if (target == null) return false;
        Camera playerCamera = target.GetComponentInChildren<Camera>();
        if (playerCamera == null) return false;

        Bounds bounds = GetComponent<Renderer>().bounds;
        if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerCamera), bounds)) return false;

        Vector3 enemyCenter = bounds.center;
        Vector3 enemySize = bounds.size;

        for (int x = 0; x < raycastResolution; x++)
        {
            for (int y = 0; y < raycastResolution; y++)
            {
                for (int z = 0; z < raycastResolution; z++)
                {
                    Vector3 offset = new Vector3(
                        (x / (float)(raycastResolution - 1) - 0.5f) * enemySize.x,
                        (y / (float)(raycastResolution - 1) - 0.5f) * enemySize.y,
                        (z / (float)(raycastResolution - 1) - 0.5f) * enemySize.z
                    );
                    Vector3 samplePoint = enemyCenter + offset;
                    Vector3 direction = (samplePoint - playerCamera.transform.position).normalized;
                    float distance = Vector3.Distance(playerCamera.transform.position, samplePoint);
                    if (distance <= maxViewDistance)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(playerCamera.transform.position, direction, out hit, distance))
                        {
                            RaycastHit hit;
                        }
                    }
                }

            }
        }
    }*/
    /*IEnumerator FlickerCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(flickerInterval);
            if (isBeingWatched && darknessImage != null)
            {
                flickerCoroutine = StartCoroutine(FlickerEffect());

            }
        }
    }*/
    /*IEnumerator FlickerEffect()
    {
        isFlickering = true;
        float flickerTimer = 0f;

        while (flickerTimer < flickerDuration)
        {
            float alpha = Mathf.PingPong(Time.time / flickerSpeed, 1f);
            canMoveDuringFlicker = alpha > 0.7f;

            Color color = darknessImage.color;
            color.a = alpha * 0.8f;
            darknessImage.color = color;

            flickerTimer += Time.deltaTime;
            yield return null;
        }

        Color finalColor = darknessImage.color;
        finalColor.a = 0f;
        darknessImage.color = finalColor;

        isFlickering = false;
        canMoveDuringFlicker = false;

    }*/
    void VariableSpeed()
    {
        
    }
}
