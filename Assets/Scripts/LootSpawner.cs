using System.Collections.Generic;
using UnityEngine;

// Randomly spawns secondary loot items each run from a pool of spawn points.
// Attach to any GameObject. Assign lootPrefab and all possible spawnPoints in the Inspector.

public class LootSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject lootPrefab;
    public Transform[] spawnPoints;

    [Header("Count per Run")]
    public int minLoot = 3;
    public int maxLoot = 8;

    [Header("Loot Names (picked randomly)")]
    public string[] lootNames = { "Gold Coins", "Ancient Scroll", "Jeweled Crown", "Painting", "Canopic Jar", "Amulet", "Scarab", "Papyrus" };

    private List<GameObject> spawnedLoot = new List<GameObject>();

    public int TotalLoot { get; private set; }
    public int CollectedLoot => TotalLoot - CountActive();

    void Start()
    {
        SpawnLoot();
    }

    public void SpawnLoot()
    {
        DespawnAll();

        int count = Random.Range(minLoot, maxLoot + 1);
        count = Mathf.Min(count, spawnPoints.Length);
        TotalLoot = count;

        List<Transform> shuffled = Shuffle(new List<Transform>(spawnPoints));

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(lootPrefab, shuffled[i].position, shuffled[i].rotation);

            SecondaryLoot loot = obj.GetComponent<SecondaryLoot>();
            if (loot != null && lootNames.Length > 0)
                loot.lootName = lootNames[Random.Range(0, lootNames.Length)];

            spawnedLoot.Add(obj);
        }

        Debug.Log($"[LootSpawner] Spawned {count} loot items");
    }

    int CountActive()
    {
        int active = 0;
        foreach (var l in spawnedLoot)
            if (l != null && l.activeSelf) active++;
        return active;
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
        foreach (var l in spawnedLoot)
            if (l != null) Destroy(l);
        spawnedLoot.Clear();
        TotalLoot = 0;
    }
}
