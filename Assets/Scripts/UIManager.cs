using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Health playerHealth;
    public Slider healthSlider;
    public Slider armorSlider;
    

    void Start()
    {
        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;

            armorSlider.maxValue = playerHealth.maxArmor;
            armorSlider.value = playerHealth.currentArmor;
        }
        else
        {
            Debug.LogWarning("MISSING PLAYER VARIABLES IN UIMANAGER FIX THIS RIGHT NOW!!");
        }
    }
    void Update() {
        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.value = playerHealth.currentHealth;
            armorSlider.value = playerHealth.currentArmor;
        }
    }
}
