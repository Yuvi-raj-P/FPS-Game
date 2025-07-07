using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public float currentHealth;
    public int healthRegenRate = 1;
    public float armor = 50;
    public int armorRegenRate = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void TakeDamage(int damage)
    {
        float effectiveDamage = Mathf.Max(damage - armor, 0);
        currentHealth -= effectiveDamage;
        Debug.Log("Player took damage: " + effectiveDamage + ", Current Health: " + currentHealth);
    }
    void Die()
    {
        Debug.Log("Player has died.");
        this.gameObject.SetActive(false);
    }
    void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        }
        if (armor < 50 && currentHealth > maxHealth)
        {
            armor += armorRegenRate * Time.deltaTime;
            armor = Mathf.Min(armor, 50);
        }

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            int damage = other.GetComponent<EnemyController>().attackDamage;
            TakeDamage(damage);
        }
    }
    
}
