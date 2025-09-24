using System.Reflection.Emit;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public float currentHealth;
    public int healthRegenRate = 1;
    public float maxArmor = 50;
    public float currentArmor;
    public int armorRegenRate = 1;
    public AudioSource healthSound;
    public AudioSource damageSound;
    public AudioSource deathSound;

    public GameObject gameManager;

    private bool isDying = false;
    public LevelLoader levelLoader;
    public string gameOverSceneName = "GameOver";

    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;

        if (levelLoader == null)
        {
            Debug.LogWarning("LevelLoader reference is missing in Health script.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDying)
        {
            RegenerateHealth();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
    }
    public void TakeDamage(float damage)
    {
        if (isDying) return;
        damageSound.PlayOneShot(damageSound.clip);
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
        if (isDying) return;
        isDying = true;

        if (deathSound != null)
        {
            deathSound.Play();
        }

        Destroy(gameManager);
        if (levelLoader != null)
        {
            DisablePlayerControls();
            levelLoader.LoadSpecificLevel(gameOverSceneName);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    void DisablePlayerControls()
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        var weapons = GetComponentsInChildren<Gun>(true);
        foreach (var weapon in weapons)
        {
            weapon.enabled = false;
        }
        
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
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
    public void Heal(int healAmount)
{
    float healRemaining = healAmount;
    
    if (currentHealth < maxHealth)
    {
        float healthToHeal = Mathf.Min(healRemaining, maxHealth - currentHealth);
        currentHealth += healthToHeal;
        healRemaining -= healthToHeal;
    }
    
    if (healRemaining > 0 && currentArmor < maxArmor)
    {
        currentArmor += healRemaining;
        currentArmor = Mathf.Min(currentArmor, maxArmor);
    }
    
    healthSound.Play();
}
    
}
