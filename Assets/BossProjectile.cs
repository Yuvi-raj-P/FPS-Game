using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public int damage;
    public float lifetime;
    private float spawnTime;

    [Header("Collision Filtering")]
    public LayerMask nonDestructibleLayers;
    public string[] nonDestructibleTags = new string[] { "Enemy" };

    [Header("Spin")]
    public Transform visualToSpin;
    public float rotateSpeedY = 360f;

    public void Initialize(int projectileDamage, float projectileLifetime)
    {
        damage = projectileDamage;
        lifetime = projectileLifetime;
        spawnTime = Time.time;
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        (visualToSpin != null ? visualToSpin : transform).Rotate(0f, rotateSpeedY * Time.deltaTime, 0f, Space.Self);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit player for {damage} damage!");
            }
            Destroy(gameObject);
            return;
        }
        if (IsNonDestructible(other))
        {
            return;
        }
        if (IsInLayer(other.gameObject, LayerMask.GetMask("Default")))
        {
            Destroy(gameObject);
        }
    }
    private bool IsNonDestructible(Collider other)
    {
        if (nonDestructibleTags != null)
        {
            for (int i = 0; i < nonDestructibleTags.Length; i++)
            {
                var t = nonDestructibleTags[i];
                if (!string.IsNullOrEmpty(t) && other.CompareTag(t))
                    return true;
            }
        }
        return IsInLayer(other.gameObject, nonDestructibleLayers);
    }

    private bool IsInLayer(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}
