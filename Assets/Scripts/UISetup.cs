using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to your Canvas. On Awake, creates BriefingPanel, ResultPanel,
/// and LeaderboardPanel with all required children, then wires them
/// to GameLoop and LeaderboardUI automatically.
///
/// Run once — panels persist for the session. Safe to leave attached.
/// </summary>
public class UISetup : MonoBehaviour
{
    [Header("Colors")]
    public Color panelColor       = new Color(0.05f, 0.05f, 0.1f, 0.92f);
    public Color headerColor      = new Color(1f, 0.84f, 0f);       // gold
    public Color bodyColor        = Color.white;
    public Color accentColor      = new Color(0.2f, 0.8f, 1f);      // cyan
    public Color buttonColor      = new Color(0.15f, 0.15f, 0.25f);
    public Color buttonTextColor  = Color.white;
    public Color dangerColor      = new Color(1f, 0.3f, 0.3f);

    [Header("Font Sizes")]
    public float titleSize   = 42f;
    public float headerSize  = 28f;
    public float bodySize    = 22f;
    public float buttonSize  = 20f;
    public float smallSize   = 18f;

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[UISetup] Must be attached to a Canvas!");
            return;
        }

        BuildBriefingPanel();
        BuildResultPanel();
        BuildLeaderboardPanel();
        BuildSlapHUD();

        Debug.Log("[UISetup] All UI panels created and wired.");
    }

    // =============================================================
    //  BRIEFING PANEL
    // =============================================================

    void BuildBriefingPanel()
    {
        GameLoop gl = FindObjectOfType<GameLoop>();
        if (gl == null) { Debug.LogWarning("[UISetup] GameLoop not found — skipping BriefingPanel."); return; }

        // Root panel — full screen dark overlay
        GameObject panel = CreatePanel("BriefingPanel", transform);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        StretchFill(panelRT);

        // Content container — centered column
        GameObject content = CreateVerticalGroup("BriefingContent", panel.transform, 20f,
            new Vector2(700, 500), TextAnchor.MiddleCenter);

        // -- Museum name (title)
        TextMeshProUGUI museumText = CreateTMP("BriefingMuseumText", content.transform,
            "Cairo National Museum", titleSize, headerColor, TextAlignmentOptions.Center);

        // -- Divider line
        CreateDivider(content.transform, headerColor);

        // -- Artifact
        TextMeshProUGUI artifactText = CreateTMP("BriefingArtifactText", content.transform,
            "Target: Eye of Ra", headerSize, bodyColor, TextAlignmentOptions.Center);

        // -- Guard count
        TextMeshProUGUI guardCountText = CreateTMP("BriefingGuardCountText", content.transform,
            "3 guards on patrol", bodySize, bodyColor, TextAlignmentOptions.Center);

        // -- Difficulty
        TextMeshProUGUI difficultyText = CreateTMP("BriefingDifficultyText", content.transform,
            "Difficulty: NORMAL", bodySize, accentColor, TextAlignmentOptions.Center);

        // -- Timer info
        TextMeshProUGUI timerText = CreateTMP("BriefingTimerText", content.transform,
            "Starting in 120s  [CONTINUE]", bodySize, bodyColor, TextAlignmentOptions.Center);

        // -- Spacer
        CreateSpacer(content.transform, 20f);

        // -- Continue button
        Button continueBtn = CreateButton("BriefingContinueBtn", content.transform,
            "BEGIN INFILTRATION", 260f, 50f, accentColor);

        // Wire to GameLoop
        gl.briefingPanel          = panel;
        gl.briefingMuseumText     = museumText;
        gl.briefingArtifactText   = artifactText;
        gl.briefingGuardCountText = guardCountText;
        gl.briefingDifficultyText = difficultyText;
        gl.briefingTimerText      = timerText;
        gl.briefingContinueButton = continueBtn;

        panel.SetActive(false);
    }

    // =============================================================
    //  RESULT PANEL
    // =============================================================

    void BuildResultPanel()
    {
        GameLoop gl = FindObjectOfType<GameLoop>();
        if (gl == null) { Debug.LogWarning("[UISetup] GameLoop not found — skipping ResultPanel."); return; }

        // Root panel
        GameObject panel = CreatePanel("ResultPanel", transform);
        StretchFill(panel.GetComponent<RectTransform>());

        // Content container
        GameObject content = CreateVerticalGroup("ResultContent", panel.transform, 16f,
            new Vector2(700, 550), TextAnchor.MiddleCenter);

        // -- Title
        TextMeshProUGUI titleText = CreateTMP("ResultTitleText", content.transform,
            "MISSION COMPLETE!", titleSize, headerColor, TextAlignmentOptions.Center);

        CreateDivider(content.transform, headerColor);

        // -- Fail reason (hidden on win)
        TextMeshProUGUI failReasonText = CreateTMP("ResultFailReasonText", content.transform,
            "", headerSize, dangerColor, TextAlignmentOptions.Center);

        // -- Score
        TextMeshProUGUI scoreText = CreateTMP("ResultScoreText", content.transform,
            "Score: 0", headerSize, bodyColor, TextAlignmentOptions.Center);

        // -- Score breakdown
        TextMeshProUGUI breakdownText = CreateTMP("ResultBreakdownText", content.transform,
            "", smallSize, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center);

        // -- High score flag
        TextMeshProUGUI highScoreText = CreateTMP("ResultHighScoreText", content.transform,
            "", headerSize, headerColor, TextAlignmentOptions.Center);

        // -- Spacer
        CreateSpacer(content.transform, 24f);

        // -- Button row
        GameObject buttonRow = CreateHorizontalGroup("ResultButtons", content.transform, 16f);

        Button replayBtn    = CreateButton("ReplayBtn", buttonRow.transform, "REPLAY", 180f, 48f, accentColor);
        Button diffBtn      = CreateButton("ChangeDifficultyBtn", buttonRow.transform, "DIFFICULTY", 180f, 48f, buttonColor);
        Button leaderBtn    = CreateButton("LeaderboardBtn", buttonRow.transform, "LEADERBOARD", 200f, 48f, buttonColor);

        // Wire to GameLoop
        gl.resultPanel          = panel;
        gl.resultTitleText      = titleText;
        gl.resultScoreText      = scoreText;
        gl.resultBreakdownText  = breakdownText;
        gl.resultHighScoreText  = highScoreText;
        gl.resultFailReasonText = failReasonText;
        gl.replayButton         = replayBtn;
        gl.changeDifficultyButton = diffBtn;
        gl.leaderboardButton    = leaderBtn;

        panel.SetActive(false);
    }

    // =============================================================
    //  LEADERBOARD PANEL
    // =============================================================

    void BuildLeaderboardPanel()
    {
        LeaderboardUI lbUI = FindObjectOfType<LeaderboardUI>();
        if (lbUI == null) { Debug.LogWarning("[UISetup] LeaderboardUI not found — skipping LeaderboardPanel."); return; }

        // Root panel
        GameObject panel = CreatePanel("LeaderboardPanel", transform);
        StretchFill(panel.GetComponent<RectTransform>());

        // Content container
        GameObject content = CreateVerticalGroup("LeaderboardContent", panel.transform, 12f,
            new Vector2(700, 600), TextAnchor.UpperCenter);

        // -- Title
        TextMeshProUGUI titleText = CreateTMP("LeaderboardTitleText", content.transform,
            "TOP 10 — NORMAL", titleSize, headerColor, TextAlignmentOptions.Center);

        CreateDivider(content.transform, headerColor);

        // -- Column headers
        GameObject headerRow = CreateRowObject("HeaderRow", content.transform);
        CreateTMP("H_Rank", headerRow.transform, "RANK", smallSize, accentColor, TextAlignmentOptions.Center);
        CreateTMP("H_Score", headerRow.transform, "SCORE", smallSize, accentColor, TextAlignmentOptions.Center);
        CreateTMP("H_Diff", headerRow.transform, "DIFFICULTY", smallSize, accentColor, TextAlignmentOptions.Center);
        CreateTMP("H_Date", headerRow.transform, "DATE", smallSize, accentColor, TextAlignmentOptions.Center);

        // -- Scroll area for rows
        GameObject rowContainer = new GameObject("RowContainer");
        rowContainer.transform.SetParent(content.transform, false);
        RectTransform rcRT = rowContainer.AddComponent<RectTransform>();
        rcRT.sizeDelta = new Vector2(660, 360);
        VerticalLayoutGroup rcVLG = rowContainer.AddComponent<VerticalLayoutGroup>();
        rcVLG.spacing = 4f;
        rcVLG.childForceExpandWidth = true;
        rcVLG.childForceExpandHeight = false;
        rcVLG.childControlWidth = true;
        rcVLG.childControlHeight = true;
        ContentSizeFitter rcCSF = rowContainer.AddComponent<ContentSizeFitter>();
        rcCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // -- Spacer
        CreateSpacer(content.transform, 12f);

        // -- Close button
        Button closeBtn = CreateButton("LeaderboardCloseBtn", content.transform,
            "CLOSE", 180f, 44f, dangerColor);

        // -- Create row prefab (not in scene hierarchy — instantiated as needed)
        GameObject rowPrefab = CreateRowPrefab();

        // Wire to LeaderboardUI
        lbUI.panel         = panel;
        lbUI.titleText     = titleText;
        lbUI.rowContainer  = rowContainer.transform;
        lbUI.rowPrefab     = rowPrefab;
        lbUI.closeButton   = closeBtn;

        panel.SetActive(false);
    }

    GameObject CreateRowPrefab()
    {
        GameObject row = CreateRowObject("LeaderboardRow", null);
        row.SetActive(false); // deactivate — it's a template, not a live object

        // Parent it somewhere hidden so it doesn't get destroyed
        row.transform.SetParent(transform, false);

        return row;
    }

    GameObject CreateRowObject(string name, Transform parent)
    {
        GameObject row = new GameObject(name);
        if (parent != null) row.transform.SetParent(parent, false);

        RectTransform rt = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(660, 32);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.padding = new RectOffset(8, 8, 2, 2);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 32f;

        // 4 columns: Rank, Score, Difficulty, Date
        CreateTMP("Rank", row.transform, "#1", smallSize, bodyColor, TextAlignmentOptions.Center);
        CreateTMP("Score", row.transform, "---", smallSize, bodyColor, TextAlignmentOptions.Center);
        CreateTMP("Difficulty", row.transform, "---", smallSize, bodyColor, TextAlignmentOptions.Center);
        CreateTMP("Date", row.transform, "---", smallSize, bodyColor, TextAlignmentOptions.Center);

        return row;
    }

    // =============================================================
    //  SLAP HUD
    // =============================================================

    void BuildSlapHUD()
    {
        MissionHUD hud = FindObjectOfType<MissionHUD>();
        if (hud == null) { Debug.LogWarning("[UISetup] MissionHUD not found — skipping SlapHUD."); return; }

        // Slap prompt — bottom center
        GameObject promptGo = new GameObject("SlapPromptText");
        promptGo.transform.SetParent(transform, false);
        RectTransform promptRT = promptGo.AddComponent<RectTransform>();
        promptRT.anchorMin = new Vector2(0.5f, 0f);
        promptRT.anchorMax = new Vector2(0.5f, 0f);
        promptRT.pivot = new Vector2(0.5f, 0f);
        promptRT.anchoredPosition = new Vector2(0f, 60f);
        promptRT.sizeDelta = new Vector2(200f, 40f);

        TextMeshProUGUI promptTMP = promptGo.AddComponent<TextMeshProUGUI>();
        promptTMP.text = "F: Slap";
        promptTMP.fontSize = bodySize;
        promptTMP.color = new Color(1f, 1f, 1f, 0.3f);
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.raycastTarget = false;

        // Slap popup — center screen
        GameObject popupGo = new GameObject("SlapPopupText");
        popupGo.transform.SetParent(transform, false);
        RectTransform popupRT = popupGo.AddComponent<RectTransform>();
        popupRT.anchorMin = new Vector2(0.5f, 0.5f);
        popupRT.anchorMax = new Vector2(0.5f, 0.5f);
        popupRT.pivot = new Vector2(0.5f, 0.5f);
        popupRT.anchoredPosition = new Vector2(0f, 50f);
        popupRT.sizeDelta = new Vector2(300f, 60f);

        TextMeshProUGUI popupTMP = popupGo.AddComponent<TextMeshProUGUI>();
        popupTMP.text = "SLAP!";
        popupTMP.fontSize = titleSize;
        popupTMP.color = Color.red;
        popupTMP.alignment = TextAlignmentOptions.Center;
        popupTMP.fontStyle = FontStyles.Bold;
        popupTMP.raycastTarget = false;
        popupGo.SetActive(false);

        // Wire to HUD
        hud.slapPromptText = promptTMP;
        hud.slapPopupText = popupTMP;
    }

    // =============================================================
    //  UI FACTORY HELPERS
    // =============================================================

    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color = panelColor;
        img.raycastTarget = true; // blocks clicks behind panel

        return go;
    }

    void StretchFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    GameObject CreateVerticalGroup(string name, Transform parent, float spacing,
        Vector2 size, TextAnchor childAlign)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        // center in parent
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = spacing;
        vlg.childAlignment = childAlign;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return go;
    }

    GameObject CreateHorizontalGroup(string name, Transform parent, float spacing)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 54f;

        return go;
    }

    TextMeshProUGUI CreateTMP(string name, Transform parent, string text,
        float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize + 14f;

        return tmp;
    }

    Button CreateButton(string name, Transform parent, string label,
        float width, float height, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;

        // Button label
        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);

        RectTransform labelRT = labelGo.AddComponent<RectTransform>();
        StretchFill(labelRT);

        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = buttonSize;
        tmp.color = buttonTextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;

        return btn;
    }

    void CreateDivider(Transform parent, Color color)
    {
        GameObject go = new GameObject("Divider");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        Image img = go.AddComponent<Image>();
        img.color = new Color(color.r, color.g, color.b, 0.4f);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 2f;
    }

    void CreateSpacer(Transform parent, float height)
    {
        GameObject go = new GameObject("Spacer");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }
}
