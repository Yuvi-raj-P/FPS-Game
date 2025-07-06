using UnityEngine;

public class Damage : MonoBehaviour
{
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
        Debug.Log("Enemy Died");
        Destroy(this.gameObject);

    }
    public void TakeDamage(float amount)
    {
        health -= amount;
    }
}
