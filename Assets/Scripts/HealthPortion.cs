using UnityEngine;

public class HealthPortion : MonoBehaviour
{
    public int healthHealAmount = 30;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healthHealAmount);
                Destroy(this.gameObject);
            }
        }
    }
}
