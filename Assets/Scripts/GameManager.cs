using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public EscapeTimer escapeTimer;
    public ExtractionPoint extractionPoint;

    private bool gameOver;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartEscapeTimer()
    {
        if (escapeTimer != null)
            escapeTimer.StartTimer();
        // Log is handled inside EscapeTimer.StartTimer()
    }

    public void TriggerWin()
    {
        if (gameOver) return;
        gameOver = true;

        float timeLeft = escapeTimer != null ? escapeTimer.TimeRemaining : 0f;
        escapeTimer?.StopTimer();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.CalculateFinalScore(timeLeft);

        if (GameLoop.Instance != null)
            GameLoop.Instance.OnMissionComplete(won: true);

        Debug.Log("[GameManager] Mission complete!");
    }

    public void TriggerLose(string reason)
    {
        if (gameOver) return;
        gameOver = true;

        escapeTimer?.StopTimer();

        if (GameLoop.Instance != null)
            GameLoop.Instance.OnMissionComplete(won: false, reason: reason);

        Debug.Log($"[GameManager] Mission failed: {reason}");
    }

    public void RestartMission()
    {
        Time.timeScale = 1f; // unpause (ResultScreen sets timeScale to 0)

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetRun();

        if (escapeTimer != null)
            escapeTimer.ResetTimer();

        gameOver = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
