using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using TMPro;

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

    [Header("Crazy Dice Magic")]
    public bool enableCrazyDiceMode = false;
    public int crazyDiceMinCount = 100;
    public int crazyDiceMaxCount = 500;
    public float crazySpawnRadius = 10f;
    public float crazySpawnHeight = 15f;
    public bool useDicePooling = true;
    public int maxPoolSize = 1000;

    private Queue<GameObject> dicePool = new Queue<GameObject>();
    private List<GameObject> activeDice = new List<GameObject>();

    //Effects and Cinematic stuff
    public GameObject eyesOpenImage;
    public GameObject eyesClosedImage;
    public GameObject eyesShockedImage;
    public float diceDestroyDelay = 5f;
    public float shockedStateDuration = 5f;

    [Header("Abilities System")]
    public float minAbilityDuration = 5f;
    public float maxAbilityDuration = 15f;
    public bool enableAbilities = true;
    public float abilityRevealDelay = 2f;

    [Header("Ability UI")]
    public TextMeshProUGUI abilityTitleText;
    public TextMeshProUGUI abilityDescriptionText;
    public GameObject abilityUIPanel;
    public float titleDisplayDuration = 2f;
    public float descriptionDisplayDuration = 3f;

    [Header("Shaking Effect")]
    public float shakeDuration = 1f;
    public float shakeMagnitude = 10f;

    [Header("Slow Motion Effect")]
    public float slowMotionTimeScale = 0.3f;
    public float slowMotionDuration = 3f;

    [Header("Flicker Effect")]
    public Volume globalVolume;
    public float flickerDuration = 1f;
    public float flickerSpeed = 10f;

    [Header("Smooth Transition Settings")]
    public float fogTransitionDuration = 2f;
    public float shatteredRealityTransitionDuration = 1.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);

    //Transition Coroutines
    private Coroutine fogTransitionCoroutine;
    private Coroutine shatteredRealityTransitionCoroutine;

    // Abilities
    private float originalFogDensity = 0.01f;
    private bool fogSettingsStored = false;
    private List<DreamAbility> availableAbilities;
    private DreamAbility currentAbility;
    private bool abilityActive = false;
    private bool abilityQueued = false;

    private Coroutine shatteredRealityCoroutine;
    private Vector3 eyesClosedOriginalPos;
    private Coroutine currentShake;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;
    private MotionBlur motionBlur;



    public enum AbilityType
    {
        FogOfThoughts,
        ShatteredReality
    }
    [System.Serializable]
    public class DreamAbility
    {
        public AbilityType abilityType;
        public string name;
        public string description;
        public float multiplier;

        public DreamAbility(AbilityType type, string abilityName, string desc, float mult)
        {
            abilityType = type;
            name = abilityName;
            description = desc;
            multiplier = mult;
        }
    }

    void Start()
    {
        InitializeAbilities();

        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.saturation.value = 0f;
        }
        else
        {
            Debug.LogWarning("Global Volume or Color Adjustments not found!");
        }
        if (globalVolume != null && globalVolume.profile.TryGet<LensDistortion>(out lensDistortion))
        {
            lensDistortion.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("Lens Distortion not found");
        }
        if (globalVolume != null && globalVolume.profile.TryGet<MotionBlur>(out motionBlur))
        {
            motionBlur.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("Motion Blur not found");
        }
        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
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
        if (enableCrazyDiceMode && useDicePooling)
        {
            InitializeDicePool();
        }
        StartCoroutine(CloseEyesSequence());
        StartCoroutine(StartRollingDice());
        StartCoroutine(RandomShaking());
    }
    void InitializeDicePool()
    {
        for (int i = 0; i < maxPoolSize; i++)
        {
            GameObject pooledDie = Instantiate(dicePrefab);
            pooledDie.SetActive(false);

            Rigidbody rb = pooledDie.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.sleepThreshold = 0.1f;
                rb.maxAngularVelocity = 50f;
            }
            dicePool.Enqueue(pooledDie);
        }
        Debug.Log($"Dice pool initialized with {maxPoolSize} dice.");
    }
    void InitializeAbilities()
    {
        availableAbilities = new List<DreamAbility>
        {
            new DreamAbility(AbilityType.FogOfThoughts, "Fog of Thoughts", "Increased fog density, reduced visibility", 20f),
            new DreamAbility(AbilityType.ShatteredReality, "Shattered Reality", "Distorted reality, increased chaos", 20f)
        };
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
            while (abilityActive)
            {
                yield return new WaitForSeconds(1f);
            }
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
        StartCoroutine(FlickerEffectSequence());
    }
    IEnumerator FlickerEffectSequence()
    {
        StartCoroutine(FlickerEffect());

        yield return new WaitForSeconds(flickerDuration);

        StartCoroutine(SlowMotionEffect());
        StartCoroutine(ShowEyesShocked());

        if (spawnMultipleDice)
        {
            int diceCount;

            if (enableCrazyDiceMode)
            {
                diceCount = Random.Range(crazyDiceMinCount, crazyDiceMaxCount + 1);
                Debug.Log("Crazy dice mode enabled");
                StartCoroutine(SpawnCrazyDice(diceCount));
            }
            else
            {
                diceCount = Random.Range(minDiceCount, maxDiceCount + 1);
                for (int i = 0; i < diceCount; i++)
                {
                    SpawnSingleDie(i);
                }
            }
            Debug.Log($"Total dice spawned: {diceCount}");
        }
        else
        {
            SpawnSingleDie(0);
        }
        if (enableAbilities)
        {
            StartCoroutine(QueueAbility());
        }
    }
    IEnumerator SpawnCrazyDice(int diceCount)
    {
        int diceBatchSize = 20;
        int spawned = 0;

        while (spawned < diceCount)
        {
            int currentBatch = Mathf.Min(diceBatchSize, diceCount - spawned);

            for (int i = 0; i < currentBatch; i++)
            {
                if (useDicePooling)
                {
                    SpawnPooledDie(spawned + i);
                }
                else
                {
                    SpawnCrazySingleDie(spawned + i);
                }
            }
            spawned += currentBatch;

            yield return new WaitForSeconds(0.1f);
        }

    }
    void SpawnPooledDie(int index)
    {
        if (dicePool.Count == 0)
        {
            SpawnCrazySingleDie(index);
            return;
        }
        GameObject die = dicePool.Dequeue();
        die.SetActive(true);
        activeDice.Add(die);
        Vector3 spawnPosition = GetCrazyDiceSpawnPosition();
        die.transform.position = spawnPosition;
        die.transform.rotation = Random.rotation;

        Rigidbody rb = die.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 throwDirection = GetCrazyThrowDirection(spawnPosition);
            rb.AddForce(throwDirection * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
        }
        DiceRotator rotator = die.GetComponent<DiceRotator>();
        if (rotator == null)
        {
            rotator = die.AddComponent<DiceRotator>();
        }
        rotator.Initialize(Random.insideUnitSphere * torqueForce);

        StartCoroutine(ReturnDiceToPool(die, diceDestroyDelay));
    }
    void SpawnCrazySingleDie(int index)
    {
        Vector3 spawnPosition = GetCrazyDiceSpawnPosition();

        GameObject die = Instantiate(dicePrefab, spawnPosition, Random.rotation);
        activeDice.Add(die);

        Rigidbody rb = die.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.sleepThreshold = 0.1f;
            rb.maxAngularVelocity = 50f;
            Vector3 throwDirection = GetCrazyThrowDirection(spawnPosition);
            rb.AddForce(throwDirection * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
        }
        DiceRotator rotator = die.AddComponent<DiceRotator>();
        rotator.Initialize(Random.insideUnitSphere * torqueForce);

        Destroy(die, diceDestroyDelay);
    }
    Vector3 GetCrazyDiceSpawnPosition()
    {
        Vector3 randomOffset = Random.insideUnitSphere * crazySpawnRadius;
        randomOffset.y = Mathf.Abs(randomOffset.y) + crazySpawnHeight;

        return spawnPoint.position + randomOffset;
    }
    Vector3 GetCrazyThrowDirection(Vector3 spawnPosition)
    {
        Vector3 baseDirection = (spawnPoint.forward + Vector3.up * 0.5f).normalized;

        Vector3 randomVariation = new Vector3(Random.Range(-2f, 2f),
        Random.Range(-0.5f, 1f),
        Random.Range(-2f, 2f));

        return (baseDirection + randomVariation).normalized;
    }
    IEnumerator ReturnDiceToPool(GameObject die, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (die != null && activeDice.Contains(die))
        {
            activeDice.Remove(die);
            die.SetActive(false);

            Rigidbody rb = die.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            dicePool.Enqueue(die);
        }
    }
    void SpawnSingleDie(int index)
    {
        Vector3 spawnPosition;
        if (enableCrazyDiceMode)
        {
            spawnPosition = GetCrazyDiceSpawnPosition();
        }
        else if (spawnMultipleDice)
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
            Vector3 throwDirection;
            if (enableCrazyDiceMode)
            {
                throwDirection = GetCrazyThrowDirection(spawnPosition);
            }
            else
            {
                throwDirection = spawnPoint.forward + (Vector3.up * 1f);
                throwDirection += new Vector3(
                Random.Range(-4f, 4f),
                Random.Range(-0.1f, 0.4f),
                Random.Range(-0.9f, 0.9f)
                );
            }
            rb.AddForce(throwDirection.normalized * rollForce * throwForceMultiplier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitCircle * torqueForce, ForceMode.Impulse);
        }
        int diceResult = Random.Range(1, 7);
        if (spawnMultipleDice)
        {
            Debug.Log($"Dice rolled");
        }
        else
        {
            Debug.Log("Dice rolled: " + diceResult);
        }
    }
    IEnumerator QueueAbility()
    {
        if (availableAbilities.Count == 0)
        {
            yield break;
        }
        abilityQueued = true;
        currentAbility = availableAbilities[Random.Range(0, availableAbilities.Count)];

        yield return new WaitForSeconds(abilityRevealDelay);

        StartCoroutine(ShowAbilityUI());

        yield return new WaitForSeconds(1f);

        StartCoroutine(ActiveQueuedAbility());
    }
    IEnumerator ShowAbilityUI()
    {
        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(true);
        }
        if (abilityTitleText != null)
        {
            abilityTitleText.text = "<-fade><+fade><sketchy>" + currentAbility.name + "</+fade><!wait=2></->";
            abilityTitleText.gameObject.SetActive(true);
        }
        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(titleDisplayDuration);
        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.text = "<sketchy>" + currentAbility.description;
            abilityDescriptionText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(descriptionDisplayDuration);

        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
    }
    IEnumerator ActiveQueuedAbility()
    {
        abilityQueued = false;
        abilityActive = true;

        float abilityDuration = Random.Range(minAbilityDuration, maxAbilityDuration);
        Debug.Log($"Ability Activated: {currentAbility.name} for {abilityDuration:F1} seconds");

        ApplyAbilityEffect(currentAbility, true);
        yield return new WaitForSeconds(abilityDuration);

        ApplyAbilityEffect(currentAbility, false);

        Debug.Log($"Ability Expired: {currentAbility.name}");

        abilityActive = false;
        currentAbility = null;
    }

    /*IEnumerator ActivateRandomAbility()
    {
        if (availableAbilities.Count == 0) yield break;
        currentAbility = availableAbilities[Random.Range(0, availableAbilities.Count)];
        float abilityDuration = Random.Range(minAbilityDuration, maxAbilityDuration);

        abilityActive = true;
        Debug.Log($"Ability Activated: {currentAbility.name} for {abilityDuration}");

        ApplyAbilityEffect(currentAbility, true);

        yield return new WaitForSeconds(abilityDuration);

        ApplyAbilityEffect(currentAbility, false);

        Debug.Log($"Ability Ended: {currentAbility.name}");

        abilityActive = false;
        currentAbility = null;
    }*/
    void ApplyAbilityEffect(DreamAbility ability, bool activate)
    {
        switch (ability.abilityType)
        {
            case AbilityType.FogOfThoughts:
                ApplyFogOfThoughts(activate);
                break;
            case AbilityType.ShatteredReality:
                ApplyShatteredReality(activate);
                break;
        }
    }
    void ApplyFogOfThoughts(bool activate)
    {
        if (activate)
        {
            if (!fogSettingsStored)
            {
                originalFogDensity = RenderSettings.fogDensity;
                fogSettingsStored = true;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            if (fogTransitionCoroutine != null)
            {
                StopCoroutine(fogTransitionCoroutine);
            }
            fogTransitionCoroutine = StartCoroutine(SmoothValueTransition(
                startValue: originalFogDensity,
                targetValue: 0.2f,
                duration: fogTransitionDuration,
                onUpdate: (value) => RenderSettings.fogDensity = value,
                onComplete: () =>
                {
                    fogTransitionCoroutine = null;
                    Debug.Log("Fog of Thoughts");
                }
            ));
            Debug.Log($"Fog of Thoughts activated.");
        }
        else
        {
            if (fogTransitionCoroutine != null)
            {
                StopCoroutine(fogTransitionCoroutine);
            }

            if (fogSettingsStored)
            {
                fogTransitionCoroutine = StartCoroutine(SmoothValueTransition(
                    startValue: RenderSettings.fogDensity,
                    targetValue: originalFogDensity,
                    duration: fogTransitionDuration,
                    onUpdate: (value) => RenderSettings.fogDensity = value,
                    onComplete: () =>
                    {
                        fogTransitionCoroutine = null;
                        Debug.Log("Fog of Thoughts ended");
                    }
                ));
            }
        }
    }
    void ApplyShatteredReality(bool activate)
    {
        if (activate)
        {
            if (lensDistortion != null)
            {
                if (shatteredRealityCoroutine != null)
                {
                    StopCoroutine(shatteredRealityTransitionCoroutine);
                }
                shatteredRealityTransitionCoroutine = StartCoroutine(SmoothShatteredRealityTransition(true));
            }
            Debug.Log("Shattered Reality acitivated! Reality");
        }
        else
        {
            if (shatteredRealityTransitionCoroutine != null)
            {
                StopCoroutine(shatteredRealityTransitionCoroutine);
            }
            if (lensDistortion != null)
            {
                shatteredRealityTransitionCoroutine = StartCoroutine(SmoothShatteredRealityTransition(false));
            }
            if (motionBlur != null)
            {
                motionBlur.intensity.value = 0f;
            }
            Debug.Log("Shattered Reality ended");
        }
    }

    void OnDestroy()
    {
        foreach (GameObject die in activeDice)
        {
            if (die != null)
            {
                Destroy(die);
            }
        }
        while (dicePool.Count > 0)
        {
            GameObject die = dicePool.Dequeue();
            if (die != null)
            {
                Destroy(die);
            }
        }
    }
    /*IEnumerator ShatteredRealityEffect()
    {
        float duration = Random.Range(minAbilityDuration, maxAbilityDuration);
        float elapsed = 0f;

        while (elapsed < duration && abilityActive)
        {
            if (lensDistortion != null)
            {

                lensDistortion.intensity.value = 0.76f;
                lensDistortion.xMultiplier.value = 0.742f;
                lensDistortion.yMultiplier.value = 1;
                lensDistortion.center.value = new Vector2(0.57f, 0.47f);
                lensDistortion.scale.value = 0.97f;
            }
            if (motionBlur != null)
            {
                motionBlur.intensity.value = 0.053f;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = 0f;
        }
        if (motionBlur != null)
        {
            motionBlur.intensity.value = 0f;
        }
        shatteredRealityCoroutine = null;
    }*/
    IEnumerator FlickerEffect()
    {
        if (colorAdjustments == null) yield break;

        float elapsed = 0f;
        while (elapsed < flickerDuration)
        {
            float flickerValue = Mathf.Sin(elapsed * flickerSpeed) > 0 ? -100f : 100f;
            colorAdjustments.saturation.value = flickerValue;

            elapsed += Time.deltaTime;
            yield return null;
        }
        colorAdjustments.saturation.value = 0f;
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
        if (currentShake != null)
        {
            StopCoroutine(currentShake);
            currentShake = null;
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.transform.localPosition = eyesClosedOriginalPos;
            eyesClosedImage.SetActive(false);
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
        StartCoroutine(HandleEyesAfterDice());
    }
    IEnumerator HandleEyesAfterDice()
    {
        yield return new WaitForSeconds(diceDestroyDelay);
        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (eyesOpenImage != null)
        {
            eyesOpenImage.SetActive(false);
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
    public bool IsAbilityActive() => abilityActive;
    public DreamAbility GetCurrentAbility() => currentAbility;
    IEnumerator SmoothValueTransition(float startValue, float targetValue, float duration, System.Action<float> onUpdate, System.Action onComplete = null)
    {
        float elasped = 0f;
        while (elasped < duration)
        {
            elasped += Time.deltaTime;
            float progress = elasped / duration;

            float curveValue = transitionCurve.Evaluate(progress);
            float currentValue = Mathf.Lerp(startValue, targetValue, curveValue);
            onUpdate?.Invoke(currentValue);

            yield return null;
        }
        onUpdate?.Invoke(targetValue);
        onComplete?.Invoke();
    }
    IEnumerator SmoothShatteredRealityTransition(bool activate)
    {
        float startIntensity = activate ? 0f : 0.76f;
        float targetIntensity = activate ? 0.76f : 0f;

        float startXMultiplier = activate ? 1f : 0.742f;
        float targetXMultiplier = activate ? 0.742f : 1f;

        float startMotionBlur = activate ? 0f : 0.053f;
        float targetMotionBlur = activate ? 0.053f : 0f;

        Vector2 startCenter = activate ? new Vector2(0.5f, 0.5f) : new Vector2(0.57f, 0.47f);
        Vector2 targetCenter = activate ? new Vector2(0.57f, 0.47f) : new Vector2(0.5f, 0.5f);

        float startScale = activate ? 1f : 0.97f;
        float targetScale = activate ? 0.97f : 1f;

        float elapsed = 0f;
        while (elapsed < shatteredRealityTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shatteredRealityTransitionDuration;

            float curveValue = transitionCurve.Evaluate(progress);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, curveValue);
                lensDistortion.xMultiplier.value = Mathf.Lerp(startXMultiplier, targetXMultiplier, curveValue);
                lensDistortion.center.value = Vector2.Lerp(startCenter, targetCenter, curveValue);
                lensDistortion.scale.value = Mathf.Lerp(startScale, targetScale, curveValue);
            }
            if (motionBlur != null)
            {
                motionBlur.intensity.value = Mathf.Lerp(startMotionBlur, targetMotionBlur, curveValue);
            }
            yield return null;
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = targetIntensity;
            lensDistortion.xMultiplier.value = targetXMultiplier;
            lensDistortion.center.value = targetCenter;
            lensDistortion.scale.value = targetScale;
        }
        if (motionBlur != null)
        {
            motionBlur.intensity.value = targetMotionBlur;
        }
        string direction = activate ? "distorted" : "restored";
        Debug.Log($"Shattered Reality transition complete - reality {direction}");
    
        shatteredRealityTransitionCoroutine = null;
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



/*public class PlayerAbilityManager : MonoBehaviour
{
    private Dreamer dreamer;

    void Start()
    {
        dreamer = GetComponent<Dreamer>();
        if (dreamer == null)
        {
            Debug.LogError("Dreamer component not found on PlayerAbilityManager.");
            return;
        }
        if (dreamer.enableAbilities && dreamer.IsAbilityActive())
        {
            StartCoroutine(dreamer.QueueAbility());
            Debug.Log("PlayerAbilityManager started and Dreamer abilities enabled");
        }
        else
        {
            StartCoroutine(dreamer.ActivateRandomAbility());

        }
        if (dreamer.abilityUIPanel != null)
        {
            dreamer.abilityUIPanel.SetActive(false);
        }
        if (dreamer.eyesOpenImage != null)
        {
            dreamer.eyesOpenImage.SetActive(true);
        }
        if (dreamer.eyesClosedImage != null)
        {
            dreamer.eyesClosedImage.SetActive(false);
        }

    }
    void Update()
    {
        if (dreamer == null) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (dreamer.IsAbilityActive())
            {
                PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
                if (abilityManager != null)
                {
                    abilityManager.StartCoroutine(dreamer.ActivateRandomAbility());
                }
            }
            else if (dreamer.enableAbilities && !dreamer.IsAbilityActive())
            {
                PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
                if (abilityManager != null)
                {
                    abilityManager.StartCoroutine(dreamer.QueueAbility());
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Q) && dreamer.IsAbilityActive())
        {
            PlayerAbilityManager abilityManager = GetComponent<PlayerAbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.StartCoroutine(dreamer.DeactivateAbility());
                Debug.Log("Ability deactivated by PlayerAbilityManager");
            }

        }
        

    }

} 
*/


