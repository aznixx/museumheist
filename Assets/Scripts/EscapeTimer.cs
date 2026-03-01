using UnityEngine;

// Pure logic — no UI. MissionHUD reads TimeRemaining and handles display.

public class EscapeTimer : MonoBehaviour
{
    [Header("Settings")]
    public float escapeDuration = 240f;

    private float timeRemaining;
    private bool isRunning;
    private bool started;

    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public bool IsRunning => isRunning;

    public void StartTimer()
    {
        if (started) return;
        started = true;

        timeRemaining = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.EscapeDuration
            : escapeDuration;

        isRunning = true;
        Debug.Log($"[Timer] Escape timer started: {timeRemaining / 60f:0.0} minutes");
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);

        if (timeRemaining <= 0f)
        {
            isRunning = false;
            GameManager.Instance.TriggerLose("Time's up! You were caught!");
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        isRunning = false;
        started = false;
        timeRemaining = 0f;
    }
}
