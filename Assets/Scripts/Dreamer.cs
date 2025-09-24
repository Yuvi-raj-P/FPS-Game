using UnityEngine;
using UnityEngine.UI;
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

    [Header("Boss Fight UI")]
    public GameObject bossFightUI;
    public Slider bossHealthSlider;

    [Header("Dice Roll Progress")]
    public Slider diceRollProgressSlider;

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

    [Header("Audio Settings")]
    public AudioSource backgroundMusic;
    public float targetPitch = 0.5f;
    public float pitchTransitionDuration = 2f;

    [Header("Smooth Transition Settings")]
    public float fogTransitionDuration = 2f;
    public float shatteredRealityTransitionDuration = 1.5f;
    public float cerebralPainTransitionDuration = 2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);

    private Coroutine fogTransitionCoroutine;
    private Coroutine shatteredRealityTransitionCoroutine;
    private Coroutine cerebralPainTransitionCoroutine;
    private Coroutine pitchTransitionCoroutine;
    private Coroutine neuralTwistVisualCoroutine;
    private float baseHueShift, baseSaturation, baseContrast, baseLensIntensity;

    private Coroutine flickerSequenceRoutine;
    private Coroutine slowMoRoutine;
    private Coroutine eyesShockedRoutine;
    private Coroutine abilityQueueRoutine;
    private Coroutine activeAbilityRoutine;

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
    private Vignette vignette;
    private ChannelMixer channelMixer;
    private DepthOfField depthOfField;
    public float originalMotionBlur = 0.70f;
    private MotionBlur motionBlur;
    private float originalPitch;

    private float originalVignetteIntensity;
    private float originalChannelMixerRed;
    private float originalChannelMixerGreen;
    private float originalChannelMixerBlue;
    private float originalDepthOfFieldRadius;
    private bool originalDepthOfFieldActive;
    private ChromaticAberration chromaticAberration;
    private bool originalVignetteActive;
    private bool originalChannelMixerActive;
    private float originalChromaticAberrationIntensity;
    private bool originalChromaticAberrationActive;


    [Header("Dyslexic Dreamer Settings")]
    public PlayerMotor playerMotor;
    public float neuralTwistTransitionDuration = 1f;

    [Header("Imagination Settings")]
    public float imaginationTransitionDuration = 2f;

    private float currentDiceRollDelay;
    private float diceRollTimer;
    private bool isDiceRollActive = false;

    [Header("Testing System")]
    public bool enableTestingmode = false;
    public AbilityType forcedAbilityType = AbilityType.Starlift;

    public bool overrideAbilityDuration = false;
    public float testAbilityDuration = 5f;

    [Header("Boss Fight System")]
    public bool enableBossFights = true;
    public int wavesBeforeBoss = 5;
    public float bossFightCooldown = 5f;
    public GameObject bossPrefab;
    public float bossSpawnDelay = 10f;
    public AudioClip bossSpawnWarningSound;
    public AudioSource bossWarningAudioSource;

    private int currentWaveCount = 0;
    private bool isBossFightActive = false;
    private bool isBossFightCooldownActive = false;
    private BossController currentBoss;

    [Header("Camera Shake Settings")]
    public CameraShake cameraShake;
    public float diceRollShakeDuration = 1f;
    public float diceRollShakeMagnitude = 0.5f;


    public enum AbilityType
    {
        FogOfThoughts,
        ShatteredReality,
        CerebralPain,
        NeuralTwist,
        Starlift
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

        if (playerMotor == null)
        {
            playerMotor = FindObjectOfType<PlayerMotor>();
            if (playerMotor == null)
            {
                Debug.LogWarning("PlayerMotor NOT FOUND FIX THIS IMMEDIATLY BISH");
            }
        }
        if (cameraShake == null)
        {
            cameraShake = FindObjectOfType<CameraShake>();
            if (cameraShake == null)
            {
                Debug.LogWarning("CAMERA SHAKE NOT AVAILABLE");
            }
        }

        if (backgroundMusic != null)
        {
            originalPitch = backgroundMusic.pitch;
        }


        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.saturation.value = 0f;

            baseHueShift = colorAdjustments.hueShift.value;
            baseSaturation = colorAdjustments.saturation.value;
            baseContrast = colorAdjustments.contrast.value;
        }
        else
        {
            Debug.LogWarning("Global Volume or Color Adjustments not found!");
        }
        if (globalVolume != null && globalVolume.profile.TryGet<LensDistortion>(out lensDistortion))
        {
            lensDistortion.intensity.value = 0f;
            baseLensIntensity = lensDistortion.intensity.value;
        }
        else
        {
            Debug.LogWarning("Lens Distortion not found");
        }
        if (globalVolume != null && globalVolume.profile.TryGet<MotionBlur>(out motionBlur))
        {
            motionBlur.intensity.value = originalFogDensity;
        }
        else
        {
            Debug.LogWarning("Motion Blur not found");
        }

        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            originalVignetteIntensity = vignette.intensity.value;
            originalVignetteActive = vignette.active;
        }
        else
        {
            Debug.LogWarning("Vignette not found");
        }

        if (globalVolume != null && globalVolume.profile.TryGet<ChannelMixer>(out channelMixer))
        {
            originalChannelMixerRed = channelMixer.redOutRedIn.value;
            originalChannelMixerGreen = channelMixer.greenOutGreenIn.value;
            originalChannelMixerBlue = channelMixer.blueOutBlueIn.value;
            originalChannelMixerActive = channelMixer.active;
        }
        else
        {
            Debug.LogWarning("Channel Mixer not found");
        }

        if (globalVolume != null && globalVolume.profile.TryGet<DepthOfField>(out depthOfField))
        {
            originalDepthOfFieldRadius = depthOfField.gaussianMaxRadius.value;
            originalDepthOfFieldActive = depthOfField.active;
        }
        else
        {
            Debug.LogWarning("Depth of Field not found");
        }
        if (globalVolume != null && globalVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            originalChromaticAberrationIntensity = chromaticAberration.intensity.value;
            originalChromaticAberrationActive = chromaticAberration.active;
        }
        else
        {
            Debug.LogWarning("Chromatic Aberration not found");
        }

        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
        if (bossFightUI != null)
        {
            bossFightUI.SetActive(false);
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
        if (diceRollProgressSlider != null)
        {
            diceRollProgressSlider.minValue = 0f;
            diceRollProgressSlider.maxValue = 1f;
            diceRollProgressSlider.value = 0f;
        }

        motionBlur.intensity.value = originalMotionBlur;
        StartCoroutine(CloseEyesSequence());
        StartCoroutine(StartRollingDice());
        StartCoroutine(RandomShaking());
        if (enableTestingmode)
        {
            HandleTestingInput();
        }
    }
    void HandleTestingInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            forcedAbilityType = AbilityType.FogOfThoughts;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            forcedAbilityType = AbilityType.ShatteredReality;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            forcedAbilityType = AbilityType.CerebralPain;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            forcedAbilityType = AbilityType.NeuralTwist;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            forcedAbilityType = AbilityType.Starlift;
        }

        if (Input.GetKeyDown(KeyCode.T) && !abilityActive && !isDiceRollActive)
        {
            StartCoroutine(ForceDiceRoll());
        }
        if (Input.GetKeyDown(KeyCode.B) && !isBossFightActive && !isDiceRollActive)
        {
            StartCoroutine(ForceBossFight());
        }
    }
    IEnumerator ForceBossFight()
    {
        yield return new WaitForSeconds(bossSpawnDelay);

        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
        StartCoroutine(StartBossFight());

    }
    IEnumerator ForceDiceRoll()
    {
        if (eyesClosedImage.activeSelf && currentShake == null)
        {
            currentShake = StartCoroutine(ShakeRoutine(shakeDuration));
            yield return currentShake;
        }
        RollDice();
    }

    void Update()
    {
        if (diceRollProgressSlider != null && isDiceRollActive && !abilityActive)
        {
            float progress = diceRollTimer / currentDiceRollDelay;
            diceRollProgressSlider.value = progress;
        }
        else if (diceRollProgressSlider != null && abilityActive)
        {
            diceRollProgressSlider.value = 0f;
        }
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
            new DreamAbility(AbilityType.ShatteredReality, "Shattered Reality", "Distorted reality, increased chaos", 20f),
            new DreamAbility(AbilityType.CerebralPain, "Cerebral Pain", "Intense mental strain, reduced focus", 20f),
            new DreamAbility(AbilityType.NeuralTwist, "Neural Twist", "Twisted perception, unpredictable effects", 20f),
            new DreamAbility(AbilityType.Starlift, "Starlift", "Mind soars free, truly a transcending experience", 20f)
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
            while (abilityActive || isBossFightActive || isBossFightCooldownActive)
            {
                isDiceRollActive = false;
                yield return new WaitForSeconds(1f);
            }
            if (enableBossFights && currentWaveCount >= wavesBeforeBoss)
            {
                StartCoroutine(StartBossFight());
                yield return new WaitUntil(() => !isBossFightActive && !isBossFightCooldownActive);
                continue;
            }

            currentDiceRollDelay = Random.Range(minDiceRollTime, maxDiceRollTime);
            diceRollTimer = 0f;

            isDiceRollActive = true;
            while (diceRollTimer < currentDiceRollDelay)
            {
                diceRollTimer += Time.deltaTime;
                yield return null;
            }

            isDiceRollActive = false;

            if (eyesClosedImage.activeSelf && currentShake == null)
            {
                currentShake = StartCoroutine(ShakeRoutine(shakeDuration));
                yield return currentShake;
            }
            RollDice();
            currentWaveCount++;
        }
    }
    IEnumerator StartBossFight()
    {
        Debug.Log("Boss Fight Starting!");
        isBossFightActive = true;
        currentWaveCount = 0;

        CancelDiceAndAbilitiesForBoss();

        if (WavesManager.Instance != null)
        {
            WavesManager.Instance.SetBossFightMode(true);
        }
        else
        {
            Debug.LogWarning("BRUHGHGHGH THERE IS NO WAVESmANAGER HERE");
        }
        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
        
        yield return new WaitUntil(() => WavesManager.Instance != null && WavesManager.Instance.IsBossFightActive());

        float halfwayPoint = bossSpawnDelay / 2f;
        yield return new WaitForSeconds(halfwayPoint);

        if (bossSpawnWarningSound != null && bossWarningAudioSource != null)
        {
            bossWarningAudioSource.PlayOneShot(bossSpawnWarningSound);
        }

        yield return new WaitForSeconds(halfwayPoint);
        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }

        if (cameraShake != null)
        {
            StartCoroutine(cameraShake.Shake(1f, 0.4f));
        }
        if (bossPrefab != null && spawnPoint != null)
        {
            Vector3 bossSpawnPosition = spawnPoint.position + Vector3.up * 5f;
            GameObject bossObject = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
            currentBoss = bossObject.GetComponent<BossController>();
            if (bossHealthSlider != null)
            {
                currentBoss.SetHealthSlider(bossHealthSlider);
            }

            if (currentBoss == null)
            {
                Debug.LogError("Boss prefab must have BossController component");
            }

            //StartCoroutine(BossFightVisualEffects());
            //StartCoroutine(BossFightDiceSpawning());

            
        }
    }
    public void OnBossSpawned(BossController boss)
    {
        currentBoss = boss;
        if (currentBoss != null && bossHealthSlider != null)
        {
            currentBoss.SetHealthSlider(bossHealthSlider);
        }
        if (bossFightUI != null)
        {
            bossFightUI.SetActive(true);
        }
    }
    public void OnBossDefeated()
    {
        isBossFightActive = false;
        currentBoss = null;
        StopCoroutine(BossFightVisualEffects());
        StopCoroutine(BossFightDiceSpawning());

        if (WavesManager.Instance != null)
        {
            WavesManager.Instance.SetBossFightMode(false);
        }
        if (bossFightUI != null)
        {
            bossFightUI.SetActive(false);
        }

        if (abilityTitleText != null)
        {
            abilityUIPanel.SetActive(true);
            abilityTitleText.text = "<color=green><size=120%>VICTORY!</size></color>";
            abilityTitleText.gameObject.SetActive(true);

            if (abilityDescriptionText != null)
            {
                abilityDescriptionText.text = "Boss defeated! Catch your breath...";
                abilityDescriptionText.gameObject.SetActive(true);
            }
        }

        StartCoroutine(BossFightCooldown());
    }
    IEnumerator BossFightCooldown()
    {
        isBossFightCooldownActive = true;

        yield return new WaitForSeconds(bossFightCooldown);

        if (abilityUIPanel != null)
        {
            abilityUIPanel.SetActive(false);
        }
        isBossFightCooldownActive = false;
    }
    IEnumerator BossFightVisualEffects()
    {
        float originalSaturation = colorAdjustments?.saturation.value ?? 0f;
        float originalContrast = colorAdjustments?.contrast.value ?? 0f;
        float originalHue = colorAdjustments?.hueShift.value ?? 0f;
        float originalLensIntensity = lensDistortion?.intensity.value ?? 0f;

        while (isBossFightActive)
        {
            float time = Time.time;

            if (colorAdjustments != null)
            {
                float saturationWave = Mathf.Sin(time * 8f) * 50f;
                colorAdjustments.saturation.value = originalSaturation + saturationWave;

                float contrastPulse = Mathf.Sin(time * 6f) * 30f;
                colorAdjustments.contrast.value = originalContrast + contrastPulse;

                float hueShift = Mathf.Sin(time * 4f) * 180f;
                colorAdjustments.hueShift.value = originalHue + hueShift;
            }
            if (lensDistortion != null)
            {
                float distortionPulse = Mathf.Sin(time * 10f) * 0.3f;
                lensDistortion.intensity.value = originalLensIntensity + distortionPulse;
            }
            yield return null;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = originalSaturation;
            colorAdjustments.contrast.value = originalContrast;
            colorAdjustments.hueShift.value = originalHue;
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = originalLensIntensity;
        }
    }
    IEnumerator BossFightDiceSpawning()
    {
        while (isBossFightActive)
        {
            int bossDiceCount = Random.Range(crazyDiceMinCount * 2, crazyDiceMaxCount * 2);

            if (enableCrazyDiceMode && useDicePooling)
            {
                StartCoroutine(SpawnCrazyDice(bossDiceCount));
            }
            else
            {
                for (int i = 0; i < Random.Range(10, 20); i++)
                {
                    SpawnSingleDie(i);
                }
            }
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }


    void RollDice()
    {
        if (dicePrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Die prefab or spawn point is not assigned in the Inspector.");
            return;
        }
        if (isBossFightActive || isBossFightCooldownActive)
        {
            return;
        }
        if (flickerSequenceRoutine != null) StopCoroutine(flickerSequenceRoutine);
        flickerSequenceRoutine = StartCoroutine(FlickerEffectSequence());
    }

    IEnumerator FlickerEffectSequence()
    {

        if (isBossFightActive || isBossFightCooldownActive)
        {
            flickerSequenceRoutine = null;
            yield break;
        }

        StartCoroutine(FlickerEffect());

        yield return new WaitForSeconds(flickerDuration);

        if (slowMoRoutine != null) StopCoroutine(slowMoRoutine);
        slowMoRoutine = StartCoroutine(SlowMotionEffect());

        if (eyesShockedRoutine != null) StopCoroutine(eyesShockedRoutine);
        eyesShockedRoutine = StartCoroutine(ShowEyesShocked());

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
        if (enableAbilities && !isBossFightActive)
        {
            if (abilityQueueRoutine != null) StopCoroutine(abilityQueueRoutine);
            abilityQueueRoutine = StartCoroutine(QueueAbility());
        }
        flickerSequenceRoutine = null;
    }

    IEnumerator QueueAbility()
    {
        if (availableAbilities.Count == 0 || isBossFightActive || isBossFightCooldownActive)
        {
            yield break;
        }

        StartCoroutine(TransitionMusicPitch(targetPitch));

        abilityQueued = true;

        if (enableTestingmode)
        {
            currentAbility = GetAbilityByType(forcedAbilityType);
            if (currentAbility == null)
            {
                currentAbility = availableAbilities[0];
            }
        }
        else
        {
            currentAbility = availableAbilities[Random.Range(0, availableAbilities.Count)];
        }

        //currentAbility = availableAbilities[Random.Range(0, availableAbilities.Count)];

        yield return new WaitForSeconds(abilityRevealDelay);

        StartCoroutine(ShowAbilityUI());

        yield return new WaitForSeconds(1f);

        if (activeAbilityRoutine != null) StopCoroutine(activeAbilityRoutine);
        activeAbilityRoutine = StartCoroutine(ActiveQueuedAbility());
    }

    IEnumerator ActiveQueuedAbility()
    {
        if (isBossFightActive || isBossFightCooldownActive)
        {
            yield break;
        }

        abilityQueued = false;
        abilityActive = true;

        float abilityDuration = (enableTestingmode && overrideAbilityDuration) ? testAbilityDuration : Random.Range(minAbilityDuration, maxAbilityDuration);

        if (enableTestingmode && overrideAbilityDuration)
        {
            abilityDuration = testAbilityDuration;
        }
        else
        {
            abilityDuration = Random.Range(minAbilityDuration, maxAbilityDuration);
        }
        ApplyAbilityEffect(currentAbility, true);

        float elapsed = 0f;
        while (elapsed < abilityDuration && !isBossFightActive && !isBossFightCooldownActive)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyAbilityEffect(currentAbility, false);
        StartCoroutine(TransitionMusicPitch(originalPitch));

        abilityActive = false;
        currentAbility = null;
        activeAbilityRoutine = null;

    }
    DreamAbility GetAbilityByType(AbilityType abilityType)
    {
        foreach (var ability in availableAbilities)
        {
            if (ability.abilityType == abilityType)
            {
                return ability;
            }
        }
        return null;
    }

    IEnumerator TransitionMusicPitch(float targetPitchValue)
    {
        if (backgroundMusic == null)
        {
            yield break;
        }

        if (pitchTransitionCoroutine != null)
        {
            StopCoroutine(pitchTransitionCoroutine);
        }

        float startPitch = backgroundMusic.pitch;
        float elapsed = 0f;

        while (elapsed < pitchTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pitchTransitionDuration;

            float curveValue = transitionCurve.Evaluate(progress);
            backgroundMusic.pitch = Mathf.Lerp(startPitch, targetPitchValue, curveValue);

            yield return null;
        }

        backgroundMusic.pitch = targetPitchValue;
        pitchTransitionCoroutine = null;
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
            case AbilityType.CerebralPain:
                ApplyCerebralPain(activate);
                break;
            case AbilityType.NeuralTwist:
                ApplyNeuralTwist(activate);
                break;
            case AbilityType.Starlift:
                ApplyStarlift(activate);
                break;
        }
    }
    void ApplyStarlift(bool activate)
    {
        if (playerMotor == null)
        {
            Debug.LogError("PLAYER MOTOR IS NULLLLL");
            return;
        }
        if (activate)
        {
            StartCoroutine(SmoothStarlift(true));
        }
        else
        {
            StartCoroutine(SmoothStarlift(false));
        }
    }
    IEnumerator SmoothStarlift(bool activate)
    {
        if (playerMotor == null) yield break;
        bool currentFlyingState = playerMotor.GetFlying();
        bool targetFlyingState = activate;

        if (currentFlyingState == targetFlyingState)
        {
            yield break;
        }
        /*if (activate)
        {
            StartCoroutine(StartliftVisualEffects());
        }*/

        yield return new WaitForSeconds(imaginationTransitionDuration * 0.4f);
        playerMotor.SetFlying(targetFlyingState);

        yield return new WaitForSeconds(imaginationTransitionDuration * 0.6f);
    }
    IEnumerator ImaginationVisualEffects()
    {
        float effectDuration = imaginationTransitionDuration;
        float elapsed = 0f;

        float originalHue = colorAdjustments?.hueShift.value ?? 0f;
        float originalSaturation = colorAdjustments?.saturation.value ?? 0f;
        float originalContrast = colorAdjustments?.contrast.value ?? 0f;
        float originalLensIntensity = lensDistortion?.intensity.value ?? 0f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / effectDuration;
            if (colorAdjustments != null)
            {
                float hueShift = Mathf.Sin(progress * Mathf.PI * 2f) * 30f;
                colorAdjustments.hueShift.value = originalHue + hueShift;

                float saturationBoost = Mathf.Sin(progress * Mathf.PI) * 20f;
                colorAdjustments.saturation.value = originalSaturation + saturationBoost;

                float contrastWave = Mathf.Sin(progress * Mathf.PI * 3f) * 10f;
                colorAdjustments.contrast.value = originalContrast + contrastWave;
            }
            if (lensDistortion != null)
            {
                float distortionWave = Mathf.Sin(progress * Mathf.PI) * 0.1f;
                lensDistortion.intensity.value = originalLensIntensity + distortionWave;
            }
            yield return null;
        }
        if (colorAdjustments != null)
        {
            colorAdjustments.hueShift.value = originalHue;
            colorAdjustments.saturation.value = originalSaturation;
            colorAdjustments.contrast.value = originalContrast;
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = originalLensIntensity;
        }
        if (currentShake == null && eyesClosedImage != null && eyesClosedImage.activeSelf)
        {
            currentShake = StartCoroutine(ShakeRoutine(imaginationTransitionDuration * 0.3f));
        }
    }
    void ApplyNeuralTwist(bool activate)
    {
        if (playerMotor == null)
        {
            Debug.LogError("PLAYER MOTOR IS NULLLLL");
            return;
        }
        if (activate)
        {
            if (neuralTwistVisualCoroutine != null)
            {
                StopCoroutine(neuralTwistVisualCoroutine);

            }
            neuralTwistVisualCoroutine = StartCoroutine(NeuralTwistVisualLoop());

            StartCoroutine(SmoothNeuralTwistTransition(true));

        }
        else
        {
            if (neuralTwistVisualCoroutine != null)
            {
                StopCoroutine(neuralTwistVisualCoroutine);
                neuralTwistVisualCoroutine = null;
            }
            ResetNeuralTwistVisuals();
            StartCoroutine(SmoothNeuralTwistTransition(false));
        }
    }
    IEnumerator NeuralTwistVisualLoop()
    {
        if (colorAdjustments == null && lensDistortion == null) yield break;

        while (abilityActive && currentAbility != null && currentAbility.abilityType == AbilityType.NeuralTwist)
        {
            float elapsed = 0f;
            float effectDuration = neuralTwistTransitionDuration;
            float localBaseHue = colorAdjustments != null ? colorAdjustments.hueShift.value : 0f;
            float localBaseSat = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
            float localBaseCon = colorAdjustments != null ? colorAdjustments.contrast.value : 0f;
            float localBaseLens = lensDistortion != null ? lensDistortion.intensity.value : 0f;

            while (elapsed < effectDuration && abilityActive && currentAbility != null && currentAbility.abilityType == AbilityType.NeuralTwist)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / effectDuration);

                if (colorAdjustments != null)
                {
                    float hueShift = Mathf.Sin(progress * Mathf.PI * 12f) * 360f;
                    colorAdjustments.hueShift.value = hueShift;

                    float saturationPulse = Mathf.Sin(progress * Mathf.PI * 8f) * 100f;
                    colorAdjustments.saturation.value = saturationPulse;

                    float contrastFlash = Mathf.Sin(progress * Mathf.PI * 16f) > 0.5f ? 50f : -20f;
                    colorAdjustments.contrast.value = contrastFlash;
                }
                if (lensDistortion != null)
                {
                    float distortionPulse = Mathf.Sin(progress * Mathf.PI * 3f) * 0.5f;
                    lensDistortion.intensity.value = localBaseLens + distortionPulse;
                }
                yield return null;
            }
            if (colorAdjustments != null)
            {
                colorAdjustments.hueShift.value = localBaseHue;
                colorAdjustments.saturation.value = localBaseSat;
                colorAdjustments.contrast.value = localBaseCon;
            }
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = localBaseLens;
            }
            yield return null;
        }
        ResetNeuralTwistVisuals();
        neuralTwistVisualCoroutine = null;
    }
    void ResetNeuralTwistVisuals()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.hueShift.value = baseHueShift;
            colorAdjustments.saturation.value = baseSaturation;
            colorAdjustments.contrast.value = baseContrast;
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = baseLensIntensity;
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
                motionBlur.intensity.value = originalMotionBlur;
            }
            Debug.Log("Shattered Reality ended");
        }
    }

    void ApplyCerebralPain(bool activate)
    {
        if (activate)
        {
            if (cerebralPainTransitionCoroutine != null)
            {
                StopCoroutine(cerebralPainTransitionCoroutine);
            }
            cerebralPainTransitionCoroutine = StartCoroutine(SmoothCerebralPainTransition(true));
            Debug.Log("Cerebral Pain activated");
        }
        else
        {
            if (cerebralPainTransitionCoroutine != null)
            {
                StopCoroutine(cerebralPainTransitionCoroutine);
            }
            cerebralPainTransitionCoroutine = StartCoroutine(SmoothCerebralPainTransition(false));
            Debug.Log("Cerebral Pain ended");
        }
    }
    IEnumerator SmoothNeuralTwistTransition(bool activate)
    {
        if (playerMotor == null) yield break;

        bool currentInvertedState = playerMotor.GetInvertedControls();
        bool targetInvertedState = activate;

        if (currentInvertedState == targetInvertedState)
        {
            yield break;
        }
        if (activate)
        {
            StartCoroutine(NeuralVisualEffect());
        }

        yield return new WaitForSeconds(neuralTwistTransitionDuration * 0.3f);
        playerMotor.SetInvertedControls(targetInvertedState);

        yield return new WaitForSeconds(neuralTwistTransitionDuration * 0.7f);
    }
    IEnumerator NeuralVisualEffect()
    {
        if (colorAdjustments != null)
        {
            float originalHue = colorAdjustments.hueShift.value;
            float originalSaturation = colorAdjustments.saturation.value;
            float originalContrast = colorAdjustments.contrast.value;
            float elapsed = 0f;
            float effectDuration = neuralTwistTransitionDuration;

            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / effectDuration;

                float hueShift = Mathf.Sin(progress * Mathf.PI * 12f) * 360f;
                colorAdjustments.hueShift.value = hueShift;

                float saturationPulse = Mathf.Sin(progress * Mathf.PI * 8f) * 100f;
                colorAdjustments.saturation.value = saturationPulse;

                float contrastFlash = Mathf.Sin(progress * Mathf.PI * 16f) > 0.5f ? 50f : -20f;
                colorAdjustments.contrast.value = contrastFlash;

                yield return null;
            }

            colorAdjustments.hueShift.value = originalHue;
            colorAdjustments.saturation.value = originalSaturation;
            colorAdjustments.contrast.value = originalContrast;
        }
        if (currentShake == null && eyesClosedImage != null && eyesClosedImage.activeSelf && !isBossFightActive)
        {
            currentShake = StartCoroutine(ShakeRoutine(neuralTwistTransitionDuration * 0.8f));
        }

        if (lensDistortion != null)
        {
            StartCoroutine(NeuralDistortionEffect());
        }
    }
    IEnumerator NeuralDistortionEffect()
    {
        float originalIntensity = lensDistortion.intensity.value;
        float elapsed = 0f;
        float pulseDuration = neuralTwistTransitionDuration * 0.6f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pulseDuration;

            float distorationPulse = Mathf.Sin(progress * Mathf.PI * 3f) * 0.5f;
            lensDistortion.intensity.value = originalIntensity + distorationPulse;

            yield return null;

        }
        lensDistortion.intensity.value = originalIntensity;

    }


    IEnumerator SmoothCerebralPainTransition(bool activate)
    {
        float targetVignetteIntensity = activate ? 0.4f : originalVignetteIntensity;
        float targetChannelMixerRed = activate ? 200f : originalChannelMixerRed;
        float targetChannelMixerGreen = activate ? -121f : originalChannelMixerGreen;
        float targetChannelMixerBlue = activate ? -200f : originalChannelMixerBlue;
        float targetDepthOfFieldRadius = activate ? 1.5f : originalDepthOfFieldRadius;
        float targetChromaticAberrationIntensity = activate ? 0.5f : originalChromaticAberrationIntensity;

        float startVignetteIntensity = vignette != null ? vignette.intensity.value : 0f;
        float startChannelMixerRed = channelMixer != null ? channelMixer.redOutRedIn.value : 100f;
        float startChannelMixerGreen = channelMixer != null ? channelMixer.greenOutGreenIn.value : 100f;
        float startChannelMixerBlue = channelMixer != null ? channelMixer.blueOutBlueIn.value : 100f;
        float startDepthOfFieldRadius = depthOfField != null ? depthOfField.gaussianMaxRadius.value : 1f;
        float startChromaticAberrationIntensity = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;

        if (activate)
        {
            if (vignette != null) vignette.active = true;
            if (channelMixer != null) channelMixer.active = true;
            if (depthOfField != null) depthOfField.active = true;
            if (chromaticAberration != null) chromaticAberration.active = true;
        }

        float elapsed = 0f;
        while (elapsed < cerebralPainTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / cerebralPainTransitionDuration;
            float curveValue = transitionCurve.Evaluate(progress);

            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(startVignetteIntensity, targetVignetteIntensity, curveValue);
            }

            if (channelMixer != null)
            {
                channelMixer.redOutRedIn.value = Mathf.Lerp(startChannelMixerRed, targetChannelMixerRed, curveValue);
                channelMixer.greenOutGreenIn.value = Mathf.Lerp(startChannelMixerGreen, targetChannelMixerGreen, curveValue);
                channelMixer.blueOutBlueIn.value = Mathf.Lerp(startChannelMixerBlue, targetChannelMixerBlue, curveValue);
            }

            if (depthOfField != null)
            {
                depthOfField.gaussianMaxRadius.value = Mathf.Lerp(startDepthOfFieldRadius, targetDepthOfFieldRadius, curveValue);
            }

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(startChromaticAberrationIntensity, targetChromaticAberrationIntensity, curveValue);
            }

            yield return null;
        }

        if (vignette != null)
        {
            vignette.intensity.value = targetVignetteIntensity;
        }

        if (channelMixer != null)
        {
            channelMixer.redOutRedIn.value = targetChannelMixerRed;
            channelMixer.greenOutGreenIn.value = targetChannelMixerGreen;
            channelMixer.blueOutBlueIn.value = targetChannelMixerBlue;
        }

        if (depthOfField != null)
        {
            depthOfField.gaussianMaxRadius.value = targetDepthOfFieldRadius;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = targetChromaticAberrationIntensity;
        }

        if (!activate)
        {
            if (vignette != null) vignette.active = originalVignetteActive;
            if (channelMixer != null) channelMixer.active = originalChannelMixerActive;
            if (depthOfField != null) depthOfField.active = originalDepthOfFieldActive;
            if (chromaticAberration != null) chromaticAberration.active = originalChromaticAberrationActive;
        }

        string direction = activate ? "intensified" : "relieved";
        Debug.Log($"Cerebral Pain transition complete - mental strain {direction}");

        cerebralPainTransitionCoroutine = null;
    }

    IEnumerator SpawnCrazyDice(int diceCount)
    {
        if (isBossFightActive || isBossFightCooldownActive)
        {
            yield break;
        }
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
    private void CancelDiceAndAbilitiesForBoss()
    {
        if (flickerSequenceRoutine != null)
        {
            StopCoroutine(flickerSequenceRoutine);
            flickerSequenceRoutine = null;
        }
        if (slowMoRoutine != null)
        {
            StopCoroutine(slowMoRoutine);
            slowMoRoutine = null;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        if (eyesShockedRoutine != null)
        {
            StopCoroutine(eyesShockedRoutine);
            eyesShockedRoutine = null;
        }
        if (abilityQueueRoutine != null)
        {
            StopCoroutine(abilityQueueRoutine);
            abilityQueueRoutine = null;
        }
        if (abilityActive && currentAbility != null)
        {
            ApplyAbilityEffect(currentAbility, false);
            abilityActive = false;
            currentAbility = null;
        }
        abilityQueued = false;
        isDiceRollActive = false;

        if (neuralTwistVisualCoroutine != null)
        {
            StopCoroutine(neuralTwistVisualCoroutine);
            neuralTwistVisualCoroutine = null;
        }
        ResetNeuralTwistVisuals();

        if (pitchTransitionCoroutine != null)
        {
            StopCoroutine(pitchTransitionCoroutine);
        }
        if (backgroundMusic != null)
        {
            backgroundMusic.pitch = originalPitch;
        }
        if (eyesShockedImage != null)
        {
            eyesShockedImage.SetActive(false);
        }
        if (eyesClosedImage != null)
        {
            eyesClosedImage.SetActive(false);
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
        slowMoRoutine = null;
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
        eyesShockedRoutine = null;
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
        float targetMotionBlur = activate ? 0.53f : originalMotionBlur;

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


