using System;
using UnityEngine;

// Tracks score for one run. Saves per-difficulty high scores.
// Score = (baseScore + timeBonus + lootBonus) x difficultyMultiplier

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Values")]
    public int baseScore  = 1000;
    public int timeBonusPerSecond = 10;   // x seconds remaining
    public int lootBonusPerItem = 100;

    private int lootBonus;
    public int FinalScore { get; private set; }
    public bool IsNewHighScore { get; private set; }

    // Per-difficulty PlayerPrefs keys
    private static string HighScoreKey(DifficultyManager.Difficulty d) => $"HighScore_{d}";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddLootBonus()
    {
        lootBonus += lootBonusPerItem;
    }

    // Kept for backwards compatibility — direct point value version
    public void AddLootBonus(int points)
    {
        lootBonus += points;
    }

    public void CalculateFinalScore(float secondsRemaining)
    {
        secondsRemaining = Mathf.Max(0f, secondsRemaining);

        float diffMult = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.ScoreMultiplier
            : 1f;

        int timePart = Mathf.RoundToInt(secondsRemaining * timeBonusPerSecond);
        int raw = baseScore + timePart + lootBonus;
        FinalScore = Mathf.Max(0, Mathf.RoundToInt(raw * diffMult));

        Debug.Log($"[Score] Base {baseScore} + Time {timePart} + Loot {lootBonus} = {raw} x{diffMult} = {FinalScore}");

        SaveHighScore();
    }

    void SaveHighScore()
    {
        DifficultyManager.Difficulty diff = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.current
            : DifficultyManager.Difficulty.Normal;

        string key = HighScoreKey(diff);
        int best = PlayerPrefs.GetInt(key, 0);

        IsNewHighScore = FinalScore > best;

        if (IsNewHighScore)
            PlayerPrefs.SetInt(key, FinalScore);

        // Save every completed run to leaderboard (not just high scores)
        int slot = GetNextLeaderboardSlot(diff);
        if (FinalScore > 0)
        {
            PlayerPrefs.SetInt($"LB_{diff}_{slot}_Score", FinalScore);
            PlayerPrefs.SetString($"LB_{diff}_{slot}_Date", DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetString($"LB_{diff}_{slot}_Diff", diff.ToString());
            PlayerPrefs.Save();
        }

        Debug.Log(IsNewHighScore
            ? $"[Score] New {diff} high score: {FinalScore}!"
            : $"[Score] {diff} score: {FinalScore} (best: {best})");
    }

    int GetNextLeaderboardSlot(DifficultyManager.Difficulty diff)
    {
        // Find the lowest-score slot in the top 10, or next open slot
        int worstScore = int.MaxValue;
        int worstSlot = 0;

        for (int i = 0; i < 10; i++)
        {
            int s = PlayerPrefs.GetInt($"LB_{diff}_{i}_Score", -1);
            if (s == -1) return i;           // empty slot
            if (s < worstScore) { worstScore = s; worstSlot = i; }
        }

        return worstSlot;  // replace lowest score
    }

    public int GetHighScore(DifficultyManager.Difficulty diff) =>
        PlayerPrefs.GetInt(HighScoreKey(diff), 0);

    public int GetHighScore() =>
        GetHighScore(DifficultyManager.Instance?.current ?? DifficultyManager.Difficulty.Normal);

    public void ResetRun()
    {
        lootBonus = 0;
        FinalScore = 0;
        IsNewHighScore = false;
    }
}
