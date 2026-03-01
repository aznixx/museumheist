using UnityEngine;

// Optional bonus loot. Does NOT trigger alarm. Adds 100 pts each.
// Used by LootSpawner — can also be placed manually in the scene.

public class SecondaryLoot : MonoBehaviour
{
    [Header("Settings")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public string lootName = "Golden Vase";

    private bool collected;
    private PlayerController player;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (collected || player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= pickupRange && Input.GetKeyDown(pickupKey))
            Collect();
    }

    void Collect()
    {
        collected = true;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddLootBonus();   // 100 pts via ScoreManager.lootBonusPerItem

        if (player != null)
            player.AddLoot();   // tracks carried count for weight penalty + HUD

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
