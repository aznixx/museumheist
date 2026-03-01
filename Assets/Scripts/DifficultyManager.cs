using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public enum Difficulty { Easy, Normal, Hard }

    [Header("Current Difficulty")]
    public Difficulty current = Difficulty.Normal;

    // Easy: 5 min / 2 guards / 0.7x sight
    // Normal: 4 min / 3 guards / 1.0x sight
    // Hard: 3 min / 4 guards / 1.3x sight
    private static readonly float[] timerDurations      = { 300f, 240f, 180f };
    private static readonly int[]   maxGuardCounts      = { 2,    3,    4    };
    private static readonly float[] sightMultipliers    = { 0.7f, 1.0f, 1.3f };
    private static readonly float[] scoreMultipliers    = { 0.5f, 1.0f, 2.0f };

    public float EscapeDuration     => timerDurations[(int)current];
    public int   MaxGuards          => maxGuardCounts[(int)current];
    public float SightMultiplier    => sightMultipliers[(int)current];
    public float ScoreMultiplier    => scoreMultipliers[(int)current];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetDifficulty(Difficulty d)
    {
        current = d;
        Debug.Log($"[Difficulty] {d} — Timer: {EscapeDuration / 60f:0} min | Max guards: {MaxGuards} | Sight: {SightMultiplier}x | Score: {ScoreMultiplier}x");
    }
}
