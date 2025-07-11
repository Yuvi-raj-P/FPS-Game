using UnityEngine;
using System.Collections;

public class Dreamer : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public float rollForce = 5f;
    public float torqueForce = 5f;
    public float throwForceMultiplier = 1f;

    //Effects and Cinematic stuff
    public GameObject eyesOpenImage;
    public GameObject eyesClosedImage;
    public GameObject eyesShockedImage;
    public float diceDestroyDelay = 5f;
    public float shockedStateDuration = 2f;

    [Header("Shaking Effect")]
    public float shakeDuration = 1f;
    public float shakeMagnitude = 10f;

    private Vector3 eyesClosedOriginalPos;
    private Coroutine currentShake;

    void Start()
    {
        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(true);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(false);
            eyesClosedOriginalPos = eyesClosedImage.transform.localPosition;

        }
        if (eyesShockedImage != null)
        {
            eyesShockedImage.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MISSING DREAMER IMAGES BRO FIX THIS ASPAPSADJFIASD!");
        }
        StartCoroutine(CloseEyesSequence());
        StartCoroutine(StartRollingDice());
        StartCoroutine(RandomShaking());
    }
    IEnumerator CloseEyesSequence()
    {
        yield return new WaitForSeconds(4f);
        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(false);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(true);
        }

    }
    IEnumerator StartRollingDice()
    {
        while (true)
        {
            float randomDelay = Random.Range(30f, 60f);
            yield return new WaitForSeconds(randomDelay);

            if (eyesClosedImage.activeSelf && currentShake == null)
            {
                currentShake = StartCoroutine(ShakeRoutine(shakeDuration));
                yield return currentShake;

            }
            RollDice();
        }
    }
    void RollDice()
    {
        if (dicePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Die prefab or spawn point is not assigned in the Inspector.");
            return;
        }
        StartCoroutine(ShowEyesShocked());
        GameObject die1 = Instantiate(dicePrefab, spawnPoint.position, Random.rotation);
        Destroy(die1, diceDestroyDelay);
        Rigidbody rb1 = die1.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            Vector3 throwDirection = (spawnPoint.forward + (Vector3.up * 1f));
            throwDirection += new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-0.1f, 0.4f),
                Random.Range(-0.9f, 0.9f)
            );
            rb1.AddForce(throwDirection.normalized * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb1.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
            
            
        }
        

        int diceResult = Random.Range(1, 7);
        Debug.Log("Dice rolled: " + diceResult);
    }
    IEnumerator ShowEyesShocked()
    {
        if (eyesShockedImage != null)
        {
            StopCoroutine(currentShake);
            currentShake = null;
            eyesClosedImage.transform.localPosition = eyesClosedOriginalPos;
        }
        if (eyesShockedImage != null)
        {
            eyesShockedImage.SetActive(true);
        }

        yield return new WaitForSeconds(shockedStateDuration);
        if (eyesShockedImage != null)
        {
            eyesShockedImage.SetActive(false);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(true);
        }
    }
    IEnumerator RandomShaking()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            if (eyesClosedImage.activeSelf && currentShake == null)
            {
                currentShake = StartCoroutine(ShakeRoutine(shakeDuration));
            }
            
        }
    }
    IEnumerator ShakeRoutine(float duration)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            eyesClosedImage.transform.localPosition = new Vector3(eyesClosedOriginalPos.x + x, eyesClosedOriginalPos.y + y, eyesClosedOriginalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        eyesClosedImage.transform.localPosition = eyesClosedOriginalPos;
        currentShake = null;
    }
    
    void Update()
    {
        
    }
}
