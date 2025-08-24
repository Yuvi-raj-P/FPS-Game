using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player Stats")]
    public Health playerHealth;
    public Slider healthSlider;
    public Slider armorSlider;

    [Header("Text Display")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;

    [Header("Weapon Display")]
    public WeaponSwitching weaponSwitching;
    public Image gun1Image;
    public Image gun2Image;

    [Header("Kill Counter")]
    public int killCount = 0;
    public TextMeshProUGUI killCountText;

    [Header("Screen Effects")]
    public Image darknessUI;
    public Image damageIndicator;
    public float damageIndicatorDuration = 0.5f;
    public float damageIndicatorFadeSpeed = 3f;
    
    [Header("Damage Status")]
    public bool hasTakenDamage = false;
    
    public static bool IsBlackoutActive { get; private set; }
    private Coroutine blackoutCoroutine;
    private Coroutine damageIndicatorCoroutine;

    private float previousHealth;
    private float previousArmor;
    private int previousSelectedWeapon = 0;

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
        hasTakenDamage = false;

        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;

            armorSlider.maxValue = playerHealth.maxArmor;
            armorSlider.value = playerHealth.currentArmor;

            previousHealth = playerHealth.currentHealth;
            previousArmor = playerHealth.currentArmor;
        }
        else
        {
            Debug.LogWarning("MISSING PLAYER VARIABLES IN UIMANAGER FIX THIS RIGHT NOW!!");
        }
        if (darknessUI != null)
        {
            darknessUI.color = new Color(darknessUI.color.r, darknessUI.color.g, darknessUI.color.b, 0);
        }

        if (damageIndicator != null)
        {
            damageIndicator.color = new Color(damageIndicator.color.r, damageIndicator.color.g, damageIndicator.color.b, 0);
        }
    }

    void Update()
    {
        killCountText.text = killCount.ToString("0");
        healthText.text = playerHealth.currentHealth.ToString("0");
        armorText.text = playerHealth.currentArmor.ToString("0");
        if (playerHealth != null && healthSlider != null && armorSlider != null)
        {
            if (playerHealth.currentHealth < previousHealth || playerHealth.currentArmor < previousArmor)
            {
                hasTakenDamage = true;
                TriggerDamageIndicator();
            }
            healthSlider.value = playerHealth.currentHealth;
            armorSlider.value = playerHealth.currentArmor;

            previousHealth = playerHealth.currentHealth;
            previousArmor = playerHealth.currentArmor;
        }
        if (weaponSwitching != null)
        {
            if (previousSelectedWeapon != weaponSwitching.selectedWeapon)
            {
                UpdateWeaponDisplay();
                previousSelectedWeapon = weaponSwitching.selectedWeapon;
            }
        }
    }
    void UpdateWeaponDisplay()
    {
        if (weaponSwitching == null) return;

        if (gun1Image != null) gun1Image.gameObject.SetActive(false);
        if (gun2Image != null) gun2Image.gameObject.SetActive(false);

        switch (weaponSwitching.selectedWeapon)
        {
            case 0:
                if (gun1Image != null) gun1Image.gameObject.SetActive(true);
                break;
            case 1:
                if (gun2Image != null) gun2Image.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Unknown weapon index: {weaponSwitching.selectedWeapon}");
                break;
        }
    }

    public void ResetDamageFlag()
    {
        hasTakenDamage = false;
    }

    public void TriggerBlackout(float duration)
    {
        if (!IsBlackoutActive && darknessUI != null)
        {
            blackoutCoroutine = StartCoroutine(BlackoutEffect(duration));
        }
    }

    public void TriggerDamageIndicator()
    {
        if (damageIndicator != null)
        {
            if (damageIndicatorCoroutine != null)
            {
                StopCoroutine(damageIndicatorCoroutine);
            }
            damageIndicatorCoroutine = StartCoroutine(DamageIndicatorEffect());
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

    private IEnumerator DamageIndicatorEffect()
    {
        Color color = damageIndicator.color;
        color.a = 1f;
        damageIndicator.color = color;

        yield return new WaitForSeconds(damageIndicatorDuration);

        float fadeTimer = 0f;
        float fadeDuration = 1f / damageIndicatorFadeSpeed;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            damageIndicator.color = color;
            yield return null;
        }

        color.a = 0f;
        damageIndicator.color = color;
        damageIndicatorCoroutine = null;
    }
    private IEnumerator DamageIndicatorEffectDebug()
{
    Debug.Log("DamageIndicatorEffect started");
    
    if (damageIndicator == null)
    {
        Debug.LogError("Damage indicator is null!");
        yield break;
    }
    
    Debug.Log($"Damage indicator active: {damageIndicator.gameObject.activeInHierarchy}");
    Debug.Log($"Damage indicator canvas group: {damageIndicator.GetComponent<CanvasGroup>()}");
    
    Color color = damageIndicator.color;
    Debug.Log($"Original color: {color}");
    
    color.a = 1f;
    damageIndicator.color = color;
    
    Debug.Log($"Set color to: {damageIndicator.color}");
    Debug.Log($"Damage indicator enabled: {damageIndicator.enabled}");

    yield return new WaitForSeconds(damageIndicatorDuration);

    float fadeTimer = 0f;
    float fadeDuration = 1f / damageIndicatorFadeSpeed;

    while (fadeTimer < fadeDuration)
    {
        fadeTimer += Time.deltaTime;
        color.a = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
        damageIndicator.color = color;
        yield return null;
    }

    color.a = 0f;
    damageIndicator.color = color;
    damageIndicatorCoroutine = null;
    
    Debug.Log("DamageIndicatorEffect finished");
}

    public void AddKill(){
        killCount++;
    }
}
