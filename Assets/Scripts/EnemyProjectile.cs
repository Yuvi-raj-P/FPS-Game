using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage;
    public float lifetime;
    private float spawnTime;

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
        }
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
        }
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Default")) != 0)
        {
            Destroy(gameObject);
        }
    }
}
