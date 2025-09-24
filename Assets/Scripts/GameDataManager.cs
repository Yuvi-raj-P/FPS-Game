using UnityEngine;

public static class GameDataManager
{
    public static int KillCount { get; set; } = 0;
    public static float SurvivalTime { get; set; } = 0f;

    public static void ResetStats()
    {
        KillCount = 0;
        SurvivalTime = 0f;
    }
}