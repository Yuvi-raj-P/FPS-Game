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

    [Header("Audio")]
    public AudioSource gunShotAudioSource;
    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;
    [Range(0.8f, 1.2f)]
    public float minVolume = 0.9f;
    [Range(0.8f, 1.2f)]
    public float maxVolume = 1.0f;


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

    [Header("Camera Effects")]
    public CameraShake cameraShake;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.4f;

    private float nextTimeToFire = 0f;

    private Animator animator;
    private float originalPitch;
    private float originalVolume;

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

        if (gunShotAudioSource != null)
        {
            originalPitch = gunShotAudioSource.pitch;
            originalVolume = gunShotAudioSource.volume;
        }
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
        StartCoroutine(cameraShake.Shake(shakeDuration, shakeMagnitude));

        if (gunShotAudioSource != null)
        {
            float randomPitch = Random.Range(minPitch, maxPitch);
            float randomVolume = Random.Range(minVolume, maxVolume);

            gunShotAudioSource.pitch = randomPitch;
            gunShotAudioSource.PlayOneShot(gunShotAudioSource.clip, randomVolume);

            StartCoroutine(ResetAudioAfterShot());
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);


            Damage damageScript = hit.transform.GetComponent<Damage>();
            BossController bossScript = hit.transform.GetComponent<BossController>();
            if (damageScript != null)
            {
                damageScript.TakeDamage(damage);
            }
            else if (bossScript != null)
            {
                bossScript.TakeDamage(damage);
            }
            else
            {
                Debug.Log("No Damage since there is no damage script to the object");
            }
            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
        }
        float randomDuration = Random.Range(minMuzzleFlashDuration, maxMuzzleFlashDuration);
        yield return new WaitForSeconds(randomDuration);
        muzzleFlash.SetActive(false);


    }
    IEnumerator ResetAudioAfterShot()
    {
        yield return new WaitForSeconds(0.1f);
        if (gunShotAudioSource != null)
        {
            gunShotAudioSource.pitch = originalPitch;
        }
    }
}

