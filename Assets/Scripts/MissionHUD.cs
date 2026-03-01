using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a Canvas UI object.
// Wire up the Text references in the Inspector.
// All fields are optional — missing references are silently skipped.

public class MissionHUD : MonoBehaviour
{
    [Header("Top Left")]
    public TextMeshProUGUI phaseText;

    [Header("Top Center")]
    public TextMeshProUGUI timerText;       // shown only during Extraction phase

    [Header("Top Right")]
    public TextMeshProUGUI noiseLevelText;

    [Header("Bottom Left")]
    public TextMeshProUGUI lootCountText;

    [Header("Bottom Right")]
    public TextMeshProUGUI difficultyText;

    [Header("Stamina Bar")]
    public Image staminaBar;     // fill image

    [Header("Slap UI")]
    public TextMeshProUGUI slapPromptText;  // "F: SLAP" or cooldown
    public TextMeshProUGUI slapPopupText;   // "SLAP!" popup

    private PlayerController player;
    private EscapeTimer escapeTimer;
    private LootSpawner lootSpawner;
    private SlapSystem slapSystem;

    private float slapPopupTimer;

    void Start()
    {
        player      = FindObjectOfType<PlayerController>();
        escapeTimer = FindObjectOfType<EscapeTimer>();
        lootSpawner = FindObjectOfType<LootSpawner>();
        slapSystem  = FindObjectOfType<SlapSystem>();

        if (difficultyText != null && DifficultyManager.Instance != null)
            difficultyText.text = DifficultyManager.Instance.current.ToString().ToUpper();

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (slapPopupText != null)
            slapPopupText.gameObject.SetActive(false);

        // Listen for slap events to show popup
        SlapSystem.OnSlapNoise += OnSlapOccurred;
    }

    void OnDestroy()
    {
        SlapSystem.OnSlapNoise -= OnSlapOccurred;
    }

    void Update()
    {
        UpdatePhase();
        UpdateTimer();
        UpdateNoise();
        UpdateLoot();
        UpdateStamina();
        UpdateSlapUI();
    }

    void UpdatePhase()
    {
        if (phaseText == null || GameLoop.Instance == null) return;
        phaseText.text = GameLoop.Instance.CurrentPhase.ToString().ToUpper();
    }

    void UpdateTimer()
    {
        if (timerText == null || escapeTimer == null) return;

        bool extracting = GameLoop.Instance != null &&
                          GameLoop.Instance.CurrentPhase == GameLoop.Phase.Extraction;

        timerText.gameObject.SetActive(extracting);

        if (!extracting) return;

        float t = escapeTimer.TimeRemaining;
        int min = Mathf.FloorToInt(t / 60f);
        int sec = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{min:0}:{sec:00}";
        timerText.color = t <= 30f ? Color.red : Color.white;
    }

    void UpdateNoise()
    {
        if (noiseLevelText == null || player == null) return;

        string label = player.NoiseLevelLabel;
        noiseLevelText.text = $"Noise: {label}";
        noiseLevelText.color = label switch
        {
            "SILENT"  => Color.cyan,
            "LOW"     => Color.green,
            "MEDIUM"  => Color.yellow,
            "HIGH"    => Color.red,
            "STUNNED" => new Color(1f, 0.5f, 0f), // orange
            _         => Color.white
        };
    }

    void UpdateLoot()
    {
        if (lootCountText == null) return;

        int collected = lootSpawner != null ? lootSpawner.CollectedLoot : (player != null ? player.LootCarried : 0);
        int total     = lootSpawner != null ? lootSpawner.TotalLoot : 0;

        lootCountText.text = total > 0
            ? $"Loot: {collected}/{total}"
            : $"Loot: {collected}";
    }

    void UpdateStamina()
    {
        if (staminaBar == null || player == null) return;
        staminaBar.fillAmount = player.Stamina / player.MaxStamina;
        staminaBar.color = player.Stamina < player.MaxStamina * 0.25f ? Color.red : Color.green;
    }

    void UpdateSlapUI()
    {
        if (slapSystem == null) return;

        // Slap prompt
        if (slapPromptText != null)
        {
            if (player.IsRagdolled)
            {
                slapPromptText.text = "STUNNED";
                slapPromptText.color = new Color(1f, 0.5f, 0f);
            }
            else if (slapSystem.IsOnCooldown)
            {
                slapPromptText.text = $"F: {slapSystem.CooldownRemaining:F1}s";
                slapPromptText.color = Color.gray;
            }
            else if (slapSystem.SlapTarget != null)
            {
                slapPromptText.text = "F: SLAP!";
                slapPromptText.color = Color.red;
            }
            else
            {
                slapPromptText.text = "F: Slap";
                slapPromptText.color = new Color(1f, 1f, 1f, 0.3f);
            }
        }

        // Popup fade
        if (slapPopupText != null && slapPopupTimer > 0f)
        {
            slapPopupTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(slapPopupTimer / 0.3f);
            slapPopupText.color = new Color(1f, 0.2f, 0.2f, alpha);

            // Float upward
            slapPopupText.rectTransform.anchoredPosition += Vector2.up * 60f * Time.deltaTime;

            if (slapPopupTimer <= 0f)
                slapPopupText.gameObject.SetActive(false);
        }
    }

    void OnSlapOccurred(Vector3 position, float radius)
    {
        if (slapPopupText == null) return;

        slapPopupText.gameObject.SetActive(true);
        slapPopupText.text = "SLAP!";
        slapPopupText.color = Color.red;
        slapPopupText.rectTransform.anchoredPosition = new Vector2(0f, 50f);
        slapPopupTimer = 0.8f;
    }
}
