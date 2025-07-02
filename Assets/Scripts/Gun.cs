using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;
    public GameObject muzzleFlash;
    public float muzzleFlashDuration = 0.05f;
    public float minMuzzleFlashDuration = 0.05f;
    public float maxMuzzleFlashDuration = 0.15f;
    public GameObject impactEffect;


    //Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 2f;

    //Burst
    public int bulletsPerBurst = 3;
    public int currentBurst;

    public float spreadIntensity;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }
    public ShootingMode currentShootingMode;

    void Awake()
    {
        muzzleFlash.SetActive(false);
        readyToShoot = true;
        currentBurst = bulletsPerBurst;
    }
    void Update()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (readyToShoot && isShooting)
        {
            currentBurst = bulletsPerBurst;
            ShootGun();
        }
    }
    IEnumerator Shoot()
    {

        readyToShoot = false;
        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

         muzzleFlash.transform.localRotation = Quaternion.Euler(Random.Range(0f, 360f), -90, 0);
        muzzleFlash.SetActive(true);

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform);
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            
        }

        float randomDuration = Random.Range(minMuzzleFlashDuration, maxMuzzleFlashDuration);
        yield return new WaitForSeconds(randomDuration);
        muzzleFlash.SetActive(false);
    }
}

