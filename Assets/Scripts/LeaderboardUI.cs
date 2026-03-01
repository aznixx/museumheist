using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a UI panel. Call Show() from the Result screen.
// Displays top 10 local scores per difficulty.

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Transform rowContainer;     // parent object for rows
    public GameObject rowPrefab;       // prefab with 4 TMP children: Rank, Score, Difficulty, Date
    public Button closeButton;
    public TextMeshProUGUI titleText;

    private DifficultyManager.Difficulty shownDifficulty;

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(DifficultyManager.Difficulty difficulty)
    {
        shownDifficulty = difficulty;
        panel?.SetActive(true);

        if (titleText != null)
            titleText.text = $"TOP 10 — {difficulty.ToString().ToUpper()}";

        PopulateRows(difficulty);
    }

    public void Hide()
    {
        panel?.SetActive(false);
    }

    void PopulateRows(DifficultyManager.Difficulty diff)
    {
        if (rowContainer == null || rowPrefab == null) return;

        // Clear old rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        // Collect all 10 slots
        var entries = new System.Collections.Generic.List<(int score, string date, string difficulty)>();
        for (int i = 0; i < 10; i++)
        {
            int score = PlayerPrefs.GetInt($"LB_{diff}_{i}_Score", -1);
            if (score < 0) continue;

            string date = PlayerPrefs.GetString($"LB_{diff}_{i}_Date", "---");
            string d    = PlayerPrefs.GetString($"LB_{diff}_{i}_Diff", diff.ToString());
            entries.Add((score, date, d));
        }

        // Sort descending by score
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Populate with data
        for (int i = 0; i < entries.Count; i++)
        {
            GameObject row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 4)
            {
                texts[0].text = $"#{i + 1}";
                texts[1].text = entries[i].score.ToString("N0");
                texts[2].text = entries[i].difficulty;
                texts[3].text = entries[i].date;

                // Highlight top 3
                if (i == 0)      SetRowColor(texts, new Color(1f, 0.84f, 0f));     // gold
                else if (i == 1) SetRowColor(texts, new Color(0.75f, 0.75f, 0.8f)); // silver
                else if (i == 2) SetRowColor(texts, new Color(0.8f, 0.5f, 0.2f));   // bronze
            }
        }

        // Fill empty rows
        for (int i = entries.Count; i < 10; i++)
        {
            GameObject row = Instantiate(rowPrefab, rowContainer);
            row.SetActive(true);
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 4)
            {
                texts[0].text = $"#{i + 1}";
                texts[1].text = "---";
                texts[2].text = "---";
                texts[3].text = "---";
                SetRowColor(texts, new Color(0.4f, 0.4f, 0.4f));
            }
        }
    }

    void SetRowColor(TextMeshProUGUI[] texts, Color color)
    {
        foreach (var t in texts)
            t.color = color;
    }
}
