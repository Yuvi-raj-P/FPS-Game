using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player Stats")]
    public Health playerHealth;
    public Slider healthSlider;
    public Slider armorSlider;

    [Header("Screen Effects")]
    public Image darknessUI;

    public static bool IsBlackoutActive { get; private set; }
    private Coroutine blackoutCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


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
        if (darknessUI != null)
        {
            darknessUI.color = new Color(darknessUI.color.r, darknessUI.color.g, darknessUI.color.b, 0);
        }
    }
    void Update()
    {
        if (playerHealth != null && healthSlider != null && armorSlider != null)
        {
            healthSlider.value = playerHealth.currentHealth;
            armorSlider.value = playerHealth.currentArmor;
        }
    }
    public void TriggerBlackout(float duration)
    {
        if (!IsBlackoutActive && darknessUI != null)
        {
            blackoutCoroutine = StartCoroutine(BlackoutEffect(duration));
        }
    }
    private IEnumerator BlackoutEffect(float duration)
    {
        IsBlackoutActive = true;

        Color color = darknessUI.color;
        color.a = 1f;
        darknessUI.color = color;

        yield return new WaitForSeconds(duration);
        color.a = 0f;
        darknessUI.color = color;
        IsBlackoutActive = false;
        blackoutCoroutine = null;
    }
}
