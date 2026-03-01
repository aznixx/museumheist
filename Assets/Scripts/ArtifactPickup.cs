using UnityEngine;

public class ArtifactPickup : MonoBehaviour
{
    [Header("Interaction")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;

    private bool isPickedUp;
    private Transform player;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isPickedUp || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= pickupRange && Input.GetKeyDown(pickupKey))
            Pickup();
    }

    void Pickup()
    {
        isPickedUp = true;

        if (GameManager.Instance != null)
            GameManager.Instance.StartEscapeTimer();

        if (GameLoop.Instance != null)
            GameLoop.Instance.OnArtifactStolen();

        gameObject.SetActive(false);
        Debug.Log("Artifact stolen! Get to the exit!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
