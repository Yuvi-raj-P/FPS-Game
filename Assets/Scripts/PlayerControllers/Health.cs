using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public float currentHealth;
    public int healthRegenRate = 1;
    public float maxArmor = 50;
    public float currentArmor;
    public int armorRegenRate = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;
    }

    // Update is called once per frame
    void Update()
    {
        RegenerateHealth();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void TakeDamage(float damage)
    {
        float damageAbsorbedByArmor = Mathf.Min(damage, currentArmor);
        currentArmor -= damageAbsorbedByArmor;

        float remainingDamage = damage - damageAbsorbedByArmor;
        if (remainingDamage > 0)
        {
            currentHealth -= remainingDamage;
        }
        Debug.Log("Player took " + damage + " damage. Armor: " + currentArmor + ", Health: " + currentHealth);
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
        else if (currentHealth >= maxHealth && currentArmor < maxArmor)
        {
            currentArmor += armorRegenRate * Time.deltaTime;
            currentArmor = Mathf.Min(currentArmor, maxArmor);
        }

    }
    
    /*void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            int damage = other.GetComponent<Enemy>().attackDamage;
            TakeDamage(damage);
        }
    }*/

}
