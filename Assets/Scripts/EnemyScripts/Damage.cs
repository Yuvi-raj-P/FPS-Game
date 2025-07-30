using UnityEngine;

public class Damage : MonoBehaviour
{
    [Header("Damage")]
    public float health = 100f;

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            Die();
        }
    }
    void Die()
    {
        Destroy(this.gameObject);

    }
    public void TakeDamage(float amount)
    {
        health -= amount;
    }
}
