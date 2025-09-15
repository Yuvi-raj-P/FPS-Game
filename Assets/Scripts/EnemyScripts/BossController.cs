using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Boss Events")]
    public UnityEngine.Events.UnityEvent OnBossSpawned;
    public UnityEngine.Events.UnityEvent OnBossDeath;

    [Header("Visual Effects")]
    public GameObject deathEffect;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private Dreamer dreamerScript;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();

        dreamerScript = FindObjectOfType<Dreamer>();

        if (dreamerScript == null)
        {
            Debug.LogError("Dreamer script not found in the scene.");
        }

        OnBossSpawned?.Invoke();
        if (dreamerScript != null)
        {
            dreamerScript.OnBossSpawned(this);
        }

    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    void Die()
    {
        if (isDead) return;

        isDead = true;
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        OnBossDeath?.Invoke();

        if (dreamerScript != null)
        {
            dreamerScript.OnBossDefeated();
        }
        StartCoroutine(DestroyBossAfterDelay(2f));
    }
    IEnumerator DestroyBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public bool IsDead() => isDead;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    void Update()
    {
        /* Testing Purposes
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(20f);
        }*/
    }

}