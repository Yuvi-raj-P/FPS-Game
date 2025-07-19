using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float damage = 10f;
    public float scopedDamage;
    public float scopedDamageMultiplier = 1.3f;
    public float range = 100f;
    public float fireRate = 15f;
    public float reloadTime = 2f;
    public float currentReloadTime { get; private set; }
    public bool isScoped = false;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo;
    public bool IsReloading { get; private set; } = false;

    public enum FireMode { SemiAuto, Auto }
    public FireMode fireMode;

    [Header("References")]
    public Camera fpsCam;
    public GameObject muzzleFlash;
    public GameObject impactEffect;
    public PlayerLook playerLook;

    [Header("Muzzle Flash")]
    public float minMuzzleFlashDuration = 0.05f;
    public float maxMuzzleFlashDuration = 0.15f;

    private float nextTimeToFire = 0f;

    private Animator animator;

    //Dream Mode
    public ParticleSystem dreamModeParticles;
    public AudioSource dreamModeAudio;
    public float dreamModeDuration = 5f;
    public float dreamMode;


    void Awake()
    {
        scopedDamage = damage * scopedDamageMultiplier;
        muzzleFlash.SetActive(false);
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        currentAmmo = magazineSize;
        currentReloadTime = 0f;
    }
    void OnEnable()
    {
        IsReloading = false;
        if (animator != null)
        {
            animator.SetBool("Reloading", false);
        }
    }
    void Update()
    {
        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red);
        if (IsReloading)
        {
            if (isScoped)
            {
                isScoped = false;
                if (playerLook != null) playerLook.SetZoom(false);
                if (animator != null)
                {
                    damage = scopedDamage;
                    animator.SetBool("Scoping", isScoped);
                }
            }
            return;
        }
        bool isAiming = Mouse.current.rightButton.isPressed;
        if (isScoped != isAiming)
        {
            isScoped = isAiming;
            if (playerLook != null)
            {
                playerLook.SetZoom(isScoped);
            }
            if (animator != null)
            {
                animator.SetBool("Scoping", isScoped);
                damage = isScoped ? 20f : 10f;
            }
        }
        
        if (currentAmmo < magazineSize && Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartCoroutine(Reload());
            return;
        }
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        if (currentAmmo > 0)
        {
            bool isShooting = false;
            if (fireMode == FireMode.Auto)
            {
                isShooting = Mouse.current.leftButton.isPressed;
            }
            else
            {
                isShooting = Mouse.current.leftButton.wasPressedThisFrame;
            }

            if (isShooting && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                StartCoroutine(Shoot());
            }
        }
    }
    IEnumerator Reload()
    {
        IsReloading = true;
        Debug.Log("Reloading...");
        if (animator != null)
        {
            animator.SetBool("Reloading", true);
        }

        currentReloadTime = reloadTime;
        while (currentReloadTime > 0f)
        {
            currentReloadTime -= Time.deltaTime;
            yield return null;
        }
        currentReloadTime = 0f;

        if (animator != null)
        {
            animator.SetBool("Reloading", false);
        }
        currentAmmo = magazineSize;
        IsReloading = false;
        Debug.Log("Reloaded.");
    }
    IEnumerator Shoot()
    {
        currentAmmo--;
        muzzleFlash.transform.localRotation = Quaternion.Euler(Random.Range(0f, 360f), -90, 0);
        muzzleFlash.SetActive(true);

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);
            

            Damage damageScript = hit.transform.GetComponent<Damage>();
            if (damageScript != null)
            {
                damageScript.TakeDamage(damage);
            }
            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
        }
        float randomDuration = Random.Range(minMuzzleFlashDuration, maxMuzzleFlashDuration);
        yield return new WaitForSeconds(randomDuration);
        muzzleFlash.SetActive(false);

        
    }
}

