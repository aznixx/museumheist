using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages the 6-phase gameplay loop.
// Attach to the same GameObject as GameManager.

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance { get; private set; }

    public enum Phase
    {
        Briefing,       // 1. Show artifact location, guards, map
        Planning,       // 2. Team talks strategy
        Infiltration,   // 3. Sneak through museum
        Theft,          // 4. Grab artifact, alarms trigger
        Extraction,     // 5. Race to exit
        Result          // 6. Success or restart screen
    }

    public Phase CurrentPhase { get; private set; } = Phase.Briefing;

    [Header("Phase Durations (seconds)")]
    public float briefingDuration = 120f;
    public float planningDuration = 240f;

    [Header("Briefing UI")]
    public GameObject briefingPanel;
    public TextMeshProUGUI briefingMuseumText;
    public TextMeshProUGUI briefingArtifactText;
    public TextMeshProUGUI briefingGuardCountText;
    public TextMeshProUGUI briefingDifficultyText;
    public TextMeshProUGUI briefingTimerText;
    public Button briefingContinueButton;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultBreakdownText;
    public TextMeshProUGUI resultHighScoreText;
    public TextMeshProUGUI resultFailReasonText;
    public Button replayButton;
    public Button changeDifficultyButton;
    public Button leaderboardButton;

    [Header("Mission Info")]
    public string museumName = "Cairo National Museum of Antiquities";
    public string artifactName = "Eye of Ra — Egyptian Wing, Room 7";
    public int guardCount = 3;

    private float phaseTimer;
    private bool phaseTimerActive;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Wire buttons — UISetup assigns these before Start runs (Awake vs Start ordering)
        if (briefingContinueButton != null)
            briefingContinueButton.onClick.AddListener(SkipToInfiltration);

        if (replayButton != null)
            replayButton.onClick.AddListener(() => GameManager.Instance.RestartMission());

        if (changeDifficultyButton != null)
            changeDifficultyButton.onClick.AddListener(CycleDifficulty);

        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(ShowLeaderboard);

        briefingPanel?.SetActive(false);
        resultPanel?.SetActive(false);

        EnterPhase(Phase.Briefing);
    }

    void Update()
    {
        if (!phaseTimerActive) return;

        phaseTimer -= Time.deltaTime;

        // Update briefing countdown
        if (CurrentPhase == Phase.Briefing && briefingTimerText != null)
        {
            int sec = Mathf.CeilToInt(Mathf.Max(0f, phaseTimer));
            briefingTimerText.text = $"Starting in {sec}s  [CONTINUE]";
        }

        if (phaseTimer <= 0f)
            OnPhaseTimerExpired();
    }

    // -------------------------------------------------------
    //  Phase transitions
    // -------------------------------------------------------

    public void EnterPhase(Phase phase)
    {
        CurrentPhase = phase;
        phaseTimerActive = false;

        switch (phase)
        {
            case Phase.Briefing:
                ShowBriefing();
                StartPhaseTimer(briefingDuration);
                break;

            case Phase.Planning:
                briefingPanel?.SetActive(false);
                Debug.Log("[GameLoop] Phase 2: Planning — team coordinates strategy.");
                StartPhaseTimer(planningDuration);
                break;

            case Phase.Infiltration:
                briefingPanel?.SetActive(false);
                LockCursor(true);
                Debug.Log("[GameLoop] Phase 3: Infiltration — sneak through the museum.");
                break;

            case Phase.Theft:
                Debug.Log("[GameLoop] Phase 4: Theft — artifact grabbed, alarm triggered!");
                GameManager.Instance.StartEscapeTimer();
                AdvanceTo(Phase.Extraction);
                break;

            case Phase.Extraction:
                Debug.Log("[GameLoop] Phase 5: Extraction — race to the exit!");
                break;

            case Phase.Result:
                ShowResultScreen();
                break;
        }
    }

    // -------------------------------------------------------
    //  Briefing
    // -------------------------------------------------------

    void ShowBriefing()
    {
        LockCursor(false);
        briefingPanel?.SetActive(true);

        string diff = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.current.ToString()
            : "Normal";

        float mins = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.EscapeDuration / 60f
            : 4f;

        int guards = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.MaxGuards
            : guardCount;

        if (briefingMuseumText    != null) briefingMuseumText.text    = museumName;
        if (briefingArtifactText  != null) briefingArtifactText.text  = $"Target: {artifactName}";
        if (briefingGuardCountText != null) briefingGuardCountText.text = $"{guards} guards on patrol";
        if (briefingDifficultyText != null) briefingDifficultyText.text = $"Difficulty: {diff.ToUpper()}";
        if (briefingTimerText     != null) briefingTimerText.text     = $"Escape within: {mins:0} minutes";

        Debug.Log("[GameLoop] Phase 1: Briefing");
    }

    // -------------------------------------------------------
    //  Result Screen
    // -------------------------------------------------------

    public void ShowResultScreen(bool won = false, string failReason = "")
    {
        CurrentPhase = Phase.Result;
        phaseTimerActive = false;

        LockCursor(false);
        Time.timeScale = 0f; // pause game behind the panel

        resultPanel?.SetActive(true);

        if (won)
        {
            if (resultTitleText != null)
                resultTitleText.text = "MISSION COMPLETE!";

            if (resultFailReasonText != null)
                resultFailReasonText.gameObject.SetActive(false);

            if (resultScoreText != null && ScoreManager.Instance != null)
            {
                int score = ScoreManager.Instance.FinalScore;
                resultScoreText.text = $"Score: {score:N0}";
            }

            // Score breakdown
            if (resultBreakdownText != null && ScoreManager.Instance != null)
            {
                var sm = ScoreManager.Instance;
                float diffMult = DifficultyManager.Instance != null
                    ? DifficultyManager.Instance.ScoreMultiplier : 1f;
                resultBreakdownText.gameObject.SetActive(true);
                resultBreakdownText.text = $"Base: {sm.baseScore}  +  Time bonus  +  Loot bonus  x{diffMult:0.0}";
            }

            if (resultHighScoreText != null && ScoreManager.Instance != null)
            {
                resultHighScoreText.gameObject.SetActive(true);
                resultHighScoreText.text = ScoreManager.Instance.IsNewHighScore
                    ? "★ NEW HIGH SCORE! ★"
                    : $"Best: {ScoreManager.Instance.GetHighScore():N0}";
            }
        }
        else
        {
            if (resultTitleText != null)
                resultTitleText.text = "MISSION FAILED";

            if (resultFailReasonText != null)
            {
                resultFailReasonText.gameObject.SetActive(true);
                resultFailReasonText.text = string.IsNullOrEmpty(failReason) ? "Unknown cause" : failReason;
            }

            if (resultScoreText != null)
                resultScoreText.text = "";

            if (resultBreakdownText != null)
                resultBreakdownText.gameObject.SetActive(false);

            if (resultHighScoreText != null)
                resultHighScoreText.gameObject.SetActive(false);
        }

        // Update replay button label based on outcome
        if (replayButton != null)
        {
            var label = replayButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = won ? "REPLAY" : "RETRY";
        }

        Debug.Log($"[GameLoop] Phase 6: Result — {(won ? "WON" : "LOST: " + failReason)}");
    }

    // -------------------------------------------------------
    //  Button callbacks
    // -------------------------------------------------------

    void CycleDifficulty()
    {
        if (DifficultyManager.Instance == null) return;

        var d = DifficultyManager.Instance.current;
        var next = (DifficultyManager.Difficulty)(((int)d + 1) % 3);
        DifficultyManager.Instance.SetDifficulty(next);

        // Update button label to show new difficulty
        if (changeDifficultyButton != null)
        {
            var label = changeDifficultyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = next.ToString().ToUpper();
        }

        Debug.Log($"[GameLoop] Difficulty changed to {next}");
    }

    void ShowLeaderboard()
    {
        LeaderboardUI lb = FindObjectOfType<LeaderboardUI>();
        if (lb == null) return;

        var diff = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.current
            : DifficultyManager.Difficulty.Normal;

        lb.Show(diff);
    }

    // -------------------------------------------------------
    //  Helpers
    // -------------------------------------------------------

    void OnPhaseTimerExpired()
    {
        phaseTimerActive = false;
        switch (CurrentPhase)
        {
            case Phase.Briefing:  AdvanceTo(Phase.Planning);    break;
            case Phase.Planning:  AdvanceTo(Phase.Infiltration); break;
        }
    }

    public void OnArtifactStolen() => EnterPhase(Phase.Theft);

    public void OnMissionComplete(bool won = false, string reason = "") =>
        ShowResultScreen(won, reason);

    void AdvanceTo(Phase next) => EnterPhase(next);

    void StartPhaseTimer(float duration)
    {
        phaseTimer = duration;
        phaseTimerActive = true;
    }

    public void SkipToInfiltration() => EnterPhase(Phase.Infiltration);

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
