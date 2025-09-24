using System.Collections;
using NUnit.Framework;
using TMPEffects.TMPAnimations.Animations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneManager : MonoBehaviour
{
    [Header("Screen Objects")]
    public GameObject[] screenObjects;
    [Header("Transition Settings")]
    public float fadeTransitionTime = 0.5f;

    [Header("Next Scene")]
    public string nextSceneName = "MainMenu";
    public LevelLoader levelLoader;
    public Canvas canvas;
    [Header("Audio Settings")]
    public AudioClip finalCutsceneMusic;
    public float musicVolume = 1.0f;
    public float musicDelay = 2.0f;

    [Header("Game Data")]
    public bool resetStatsAfterCutscene = true;

    private int currentScreenIndex = -1;
    private bool isTransitioning = false;
    private CanvasGroup[] screenCanvasGroups;
    public AudioSource audioSource;


    void Start()
    {
        
        screenCanvasGroups = new CanvasGroup[screenObjects.Length];
        for (int i = 0; i < screenObjects.Length; i++)
        {
            if (screenObjects[i] != null)
            {
                CanvasGroup canvasGroup = screenObjects[i].GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = screenObjects[i].AddComponent<CanvasGroup>();
                }
                screenCanvasGroups[i] = canvasGroup;
                canvasGroup.alpha = 0;
                screenObjects[i].SetActive(false);
            }
        }
        SetupClickDetection();

        ShowNextScreen();
    }
    void SetupClickDetection()
    {
        if (canvas != null)
        {
            GameObject clickDetector = new GameObject("ClickDetector");
            clickDetector.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = clickDetector.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            rectTransform.sizeDelta = Vector2.zero;

            Image image = clickDetector.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);

            Button button = clickDetector.AddComponent<Button>();
            button.onClick.AddListener(OnCanvasClick);
        }
    }
    public void OnCanvasClick()
    {
        if (!isTransitioning)
        {
            ShowNextScreen();
        }
    }
    private void ShowNextScreen()
    {
        if (isTransitioning) return;
        int nextIndex = currentScreenIndex + 1;

        if (nextIndex >= screenObjects.Length)
        {
            if (levelLoader != null)
            {
                levelLoader.LoadSpecificLevel(nextSceneName);
            }
            else
            {
                Debug.LogWarning("LevelLoader is missing. Cannot transition to next scene.");
            }
            return;
        }
        if (nextIndex == screenObjects.Length - 1 && finalCutsceneMusic != null)
        {
            ResetGameStats();
            StartCoroutine(PlayDelayedMusic());
        }
        TransitionToScreen(nextIndex);
    }
    private IEnumerator PlayDelayedMusic()
    {
        yield return new WaitForSeconds(musicDelay);

        if (audioSource != null && finalCutsceneMusic != null)
        {
            audioSource.PlayOneShot(finalCutsceneMusic, musicVolume);
        }
    }
    private void TransitionToScreen(int screenIndex)
    {
        isTransitioning = true;
        StartCoroutine(FadeOutCurrentScreen(() =>
        {
            currentScreenIndex = screenIndex;
            StartCoroutine(FadeInCurrentScreen(() =>
            {
                isTransitioning = false;
            }));
        }));
    }
    private IEnumerator FadeOutCurrentScreen(System.Action onComplete)
    {
        if (currentScreenIndex >= 0 && currentScreenIndex < screenCanvasGroups.Length)
        {
            CanvasGroup canvasGroup = screenCanvasGroups[currentScreenIndex];
            GameObject screen = screenObjects[currentScreenIndex];

            if (canvasGroup != null && screen.activeSelf)
            {
                float timer = 0;
                float startAlpha = canvasGroup.alpha;
                while (timer < fadeTransitionTime)
                {
                    timer += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, timer / fadeTransitionTime);
                    yield return null;
                }
                canvasGroup.alpha = 0;
                screen.SetActive(false);
            }
        }
        onComplete?.Invoke();
    }
    private void ResetGameStats()
    {
        PlayerPrefs.SetInt("KillCount", 0);
        PlayerPrefs.SetFloat("SurvivalTime", 0f);

        PlayerPrefs.Save();
    }
    private IEnumerator FadeInCurrentScreen(System.Action onComplete)
    {
        if (currentScreenIndex >= 0 && currentScreenIndex < screenCanvasGroups.Length)
        {
            CanvasGroup canvasGroup = screenCanvasGroups[currentScreenIndex];
            GameObject screen = screenObjects[currentScreenIndex];

            if (canvasGroup != null)
            {
                screen.SetActive(true);
                canvasGroup.alpha = 0;
                float timer = 0;

                while (timer < fadeTransitionTime)
                {
                    timer += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeTransitionTime);
                    yield return null;
                }
                canvasGroup.alpha = 1;
            }
        }
        onComplete?.Invoke();
        
    }

}