using System.Collections.Generic;
using UnityEngine;

// Procedurally places guards at random spawn points each run.
// Attach to any GameObject. Assign guardPrefab and spawnPoints in the Inspector.

public class GuardSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject guardPrefab;
    public Transform[] spawnPoints;

    [Header("Guard Count")]
    public int baseGuardCount = 3;

    [Header("Randomization")]
    [Tooltip("Guards spawn within this radius of their base spawn point")]
    public float spawnOffsetRadius = 3f;

    private List<GameObject> spawnedGuards = new List<GameObject>();

    void Start()
    {
        SpawnGuards();
    }

    public void SpawnGuards()
    {
        DespawnAll();

        int count = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.MaxGuards
            : baseGuardCount;

        count = Mathf.Clamp(count, 1, spawnPoints.Length);

        // Shuffle spawn points so a different subset is chosen each run
        List<Transform> shuffled = Shuffle(new List<Transform>(spawnPoints));

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = RandomOffset(shuffled[i].position);
            GameObject guard = Instantiate(guardPrefab, spawnPos, shuffled[i].rotation);

            // Apply difficulty sight range multiplier
            GuardAI ai = guard.GetComponent<GuardAI>();
            if (ai != null && DifficultyManager.Instance != null)
            {
                float mult = DifficultyManager.Instance.SightMultiplier;
                ai.sightRange   *= mult;
                ai.hearingRange *= mult;
            }

            // Shuffle this guard's patrol waypoints for a different route each run
            if (ai != null && ai.patrolPoints.Length > 1)
                ai.patrolPoints = Shuffle(new List<Transform>(ai.patrolPoints)).ToArray();

            spawnedGuards.Add(guard);
        }

        Debug.Log($"[GuardSpawner] Spawned {count} guards");
    }

    Vector3 RandomOffset(Vector3 basePos)
    {
        Vector2 circle = Random.insideUnitCircle * spawnOffsetRadius;
        return basePos + new Vector3(circle.x, 0f, circle.y);
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
        return list;
    }

    void DespawnAll()
    {
        foreach (var g in spawnedGuards)
            if (g != null) Destroy(g);
        spawnedGuards.Clear();
    }
}
