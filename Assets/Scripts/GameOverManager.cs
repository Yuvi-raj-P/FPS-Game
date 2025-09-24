using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI survivalTimeText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Scene Names")]
    public string mainGameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    public LevelLoader levelLoader;

    void Start()
    {
        if (levelLoader == null)
        {
            Debug.LogWarning("LEVEL LOADER NOT FOUND");
        }
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        DisplayGameStats();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void DisplayGameStats()
    {
        int killCount = PlayerPrefs.GetInt("KillCount", 0);
        float time = PlayerPrefs.GetFloat("SurvivalTime", 0f);
        if (killCountText != null)
        {
            killCountText.text = killCount.ToString();
        }
        if (survivalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60);
            survivalTimeText.text = minutes + "m " + seconds + "s ";
        }
        
    }
    void RestartGame()
    {
        GameDataManager.ResetStats();
        if (levelLoader != null)
        {
            levelLoader.LoadSpecificLevel(mainGameSceneName);
        }
    }
    void ReturnToMainMenu()
    {
        if (levelLoader != null)
        {
            levelLoader.LoadSpecificLevel(mainMenuSceneName);
        }
    }
}