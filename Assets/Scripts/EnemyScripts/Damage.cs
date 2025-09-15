using UnityEngine;

public class Damage : MonoBehaviour
{
    [Header("Damage")]
    public float health = 100f;
    public AudioClip deathSound;
    public float pitchForEffect;
    public float volume;
    public GameObject damageIndicatorPrefab;

    void Update()
    {
        if (health <= 0f)
        {
            Die();
        }
    }
    void Die()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayOneShot(deathSound, pitchForEffect, volume);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddKill();
        }
        else
        {
            Debug.Log("UIManager fudging dont exists");
        }
        Destroy(this.gameObject);
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Instatiating the number");
        DamageIndicator indicator = Instantiate(damageIndicatorPrefab, transform.position, Quaternion.identity).GetComponent<DamageIndicator>();
        indicator.SetDamageText(amount);
    }
}