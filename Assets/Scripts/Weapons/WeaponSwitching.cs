using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    public float switchDelay = 3f;
    private float nextSwitchTime = 0f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public GameObject reloadTime;
    public bool weaponScoped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SelectWeapon();
        reloadTime.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Gun currentGun = transform.GetChild(selectedWeapon).GetComponent<Gun>();
        weaponScoped = currentGun.isScoped;
        if (ammoText != null && currentGun != null)
        {
            if (currentGun.IsReloading)
            {
                ammoText.text = $"0 | {currentGun.magazineSize}";
                reloadTime.SetActive(true);
                reloadTime.GetComponent<TextMeshProUGUI>().text = $"Reloading in {currentGun.currentReloadTime:0.0}s";
            }
            else
            {
                reloadTime.SetActive(false);
                ammoText.text = $"{currentGun.currentAmmo} | {currentGun.magazineSize}";
            }

        }

        if (currentGun != null && currentGun.IsReloading)
        {
            return;
        }

        if (Time.time < nextSwitchTime)
        {
            return;
        }

        int previousSelectedWeapon = selectedWeapon;
        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (scrollValue > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }

        }
        if (scrollValue < 0f)
        {
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }

        }
        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
            nextSwitchTime = Time.time + switchDelay;
        }
    }
    void SelectWeapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
