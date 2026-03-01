using UnityEngine;
using System.Collections;

/// <summary>
/// The Slap Mechanic — press F to slap a nearby player.
/// Creates loud noise (alerts guards), ragdolls the victim for 2 seconds.
/// Attach to the Player GameObject (same as PlayerController).
/// </summary>
public class SlapSystem : MonoBehaviour
{
    [Header("Slap Settings")]
    public float slapRange = 2.5f;
    public float slapCooldown = 6f;
    public float ragdollDuration = 2f;
    public float slapForce = 25f;          // impulse force on ragdoll — sends them flying

    [Header("Impact Timing")]
    public float impactDelay = 0.3f;       // seconds after F press before impact hits (sync with animation)

    [Header("Noise")]
    public float slapNoiseRadius = 20f;   // guards within this range hear it

    [Header("Audio")]
    public AudioClip slapSound;
    public AudioClip gruntSound;
    public float slapVolume = 0.8f;

    [Header("Screen Shake")]
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.08f;

    // Static event: guards subscribe to this to hear slaps
    public static event System.Action<Vector3, float> OnSlapNoise;

    // State
    private float cooldownTimer;
    private PlayerController myPlayer;
    private AudioSource audioSource;

    // Slap target detection
    public PlayerController SlapTarget { get; private set; }
    public bool CanSlap => cooldownTimer <= 0f && SlapTarget != null && !myPlayer.IsRagdolled && !myPlayer.IsCaught;
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
    public bool IsOnCooldown => cooldownTimer > 0f;

    void Start()
    {
        myPlayer = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (myPlayer == null || myPlayer.IsCaught) return;

        // Tick cooldown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Find closest slappable target
        SlapTarget = FindSlapTarget();

        // Input
        if (Input.GetKeyDown(KeyCode.F) && CanSlap)
            ExecuteSlap(SlapTarget);
    }

    PlayerController FindSlapTarget()
    {
        // Find all players in range (for multiplayer — in single player this finds nothing)
        PlayerController[] allPlayers = FindObjectsOfType<PlayerController>();
        PlayerController closest = null;
        float closestDist = slapRange;

        foreach (var p in allPlayers)
        {
            if (p == myPlayer) continue;            // can't slap yourself
            if (p.IsCaught || p.IsRagdolled) continue;  // can't slap caught/ragdolled players

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p;
            }
        }

        return closest;
    }

    void ExecuteSlap(PlayerController victim)
    {
        // Start cooldown immediately (prevent spam)
        cooldownTimer = slapCooldown;

        // Play slap arm animation on the slapper right away
        SlapAnimator slapAnim = GetComponentInChildren<SlapAnimator>();
        if (slapAnim != null)
            slapAnim.PlaySlap();

        // Delay the impact to sync with the animation
        StartCoroutine(DelayedImpact(victim));
    }

    IEnumerator DelayedImpact(PlayerController victim)
    {
        yield return new WaitForSeconds(impactDelay);

        // Victim might have been caught or destroyed during the windup
        if (victim == null || victim.IsCaught) yield break;

        // Direction from slapper to victim (recalculate — they may have moved)
        Vector3 slapDir = (victim.transform.position - transform.position).normalized;

        // Ragdoll the victim
        Vector3 force = (slapDir + Vector3.up * 0.5f).normalized * slapForce;
        victim.StartRagdoll(ragdollDuration, force);

        // Play slap sound at the victim's position
        if (slapSound != null)
            AudioSource.PlayClipAtPoint(slapSound, victim.transform.position, slapVolume);

        // Play grunt on the victim
        if (gruntSound != null)
            AudioSource.PlayClipAtPoint(gruntSound, victim.transform.position, 0.6f);

        // Broadcast noise to guards
        OnSlapNoise?.Invoke(victim.transform.position, slapNoiseRadius);

        // Screen shake on the slapper
        CameraController cam = FindObjectOfType<CameraController>();
        if (cam != null)
            cam.TriggerShake(shakeDuration, shakeMagnitude);

        Debug.Log($"[Slap] {gameObject.name} slapped {victim.gameObject.name}!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, slapRange);
    }
}
