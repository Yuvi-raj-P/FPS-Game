using UnityEngine;
using System.Collections;

public class Dreamer : MonoBehaviour
{
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public float rollForce = 5f;
    public float torqueForce = 5f;
    public float throwForceMultiplier = 1f;
    public float minDiceRollTime;
    public float maxDiceRollTime;

    public bool spawnMultipleDice = false;

    [Header("Multiple Dice Settings")]
    public int minDiceCount = 2;
    public int maxDiceCount = 5;
    public float spawnRadius = 2f;

    //Effects and Cinematic stuff
    public GameObject eyesOpenImage;
    public GameObject eyesClosedImage;
    public GameObject eyesShockedImage;
    public float diceDestroyDelay = 5f;
    public float shockedStateDuration = 2f;

    [Header("Shaking Effect")]
    public float shakeDuration = 1f;
    public float shakeMagnitude = 10f;

    [Header("Slow Motion Effect")]
    public float slowMotionTimeScale = 0.3f;
    public float slowMotionDuration = 3f;

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
            float randomDelay = Random.Range(minDiceRollTime, maxDiceRollTime);
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
        StartCoroutine(SlowMotionEffect());
        StartCoroutine(ShowEyesShocked());

        if(spawnMultipleDice){
            int diceCount = Random.Range(minDiceCount, maxDiceCount + 1);

            for(int i = 0; i < diceCount; i++){
                SpawnSingleDie(i);
            }
            Debug.Log($"Total dice spawned: {diceCount}");
        }
        else{
            SpawnSingleDie(0);
        }
    }
    void SpawnSingleDie(int index)
    {
        Vector3 spawnPosition;
        if (spawnMultipleDice)
        {
            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            randomOffset.y = Mathf.Abs(randomOffset.y);
            spawnPosition = spawnPoint.position + randomOffset;
        }
        else
        {
            spawnPosition = spawnPoint.position;
        }

        GameObject die = Instantiate(dicePrefab, spawnPosition, Random.rotation);
        Destroy(die, diceDestroyDelay);
        Rigidbody rb = die.GetComponent<Rigidbody>();

        DiceRotator rotator = die.AddComponent<DiceRotator>();
        rotator.Initialize(Random.insideUnitCircle * torqueForce);

        if (rb != null)
        {
            Vector3 throwDirection = (spawnPoint.forward + (Vector3.up * 1f));
            throwDirection += new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-0.1f, 0.4f),
                Random.Range(-0.9f, 0.9f)
            );
            rb.AddForce(throwDirection.normalized * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitCircle * torqueForce, ForceMode.Impulse);
        }
        int diceResult = Random.Range(1, 7);
        if (spawnMultipleDice)
        {
            Debug.Log($"Dice {index + 1} rolled: {diceResult}");
        }
        else
        {
            Debug.Log("Dice rolled: " + diceResult);
        }
    }
    IEnumerator SlowMotionEffect()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSeconds(slowMotionDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
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
}

public class DiceRotator : MonoBehaviour
{
    private Vector3 rotationSpeed;

    public void Initialize(Vector3 torque)
    {
        rotationSpeed = torque;
    }
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
