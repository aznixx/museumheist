using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    public enum GuardState { Patrol, Investigate, Chase }

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;
    public float patrolSpeed = 4f;

    [Header("Detection")]
    public float sightRange = 20f;
    public float sightAngle = 120f;   // full cone width in degrees
    public float hearingRange = 15f;
    public LayerMask obstacleMask;

    [Header("Chase")]
    public float chaseSpeed = 6f;
    public float catchDistance = 1.5f;

    [Header("Alert Communication")]
    public float alertRadius = 30f;   // radius within which nearby guards are alerted

    [Header("Investigate")]
    public float investigateDuration = 10f;  // seconds at destination before returning to patrol

    // Static event: any guard that spots the player broadcasts position to all others
    public static event System.Action<Vector3> OnAlertBroadcast;

    private NavMeshAgent agent;
    private PlayerController player;
    private GuardState state = GuardState.Patrol;
    private int patrolIndex;
    private float waitTimer;
    private Vector3 investigatePosition;
    private float investigateTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();
    }

    void OnEnable()
    {
        OnAlertBroadcast += OnReceiveAlert;
        SlapSystem.OnSlapNoise += OnHearSlap;
    }

    void OnDisable()
    {
        OnAlertBroadcast -= OnReceiveAlert;
        SlapSystem.OnSlapNoise -= OnHearSlap;
    }

    void Update()
    {
        if (player == null || player.IsCaught) return;

        switch (state)
        {
            case GuardState.Patrol:      DoPatrol();      break;
            case GuardState.Investigate: DoInvestigate(); break;
            case GuardState.Chase:       DoChase();       break;
        }

        CheckSight();
        CheckHearing();
    }

    // -------------------------------------------------------
    //  States
    // -------------------------------------------------------

    void DoPatrol()
    {
        agent.speed = patrolSpeed;
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
                waitTimer = patrolWaitTime;
            }
        }
    }

    void DoInvestigate()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(investigatePosition);

        // Count down once we've arrived
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            investigateTimer -= Time.deltaTime;
            if (investigateTimer <= 0f)
                SetState(GuardState.Patrol);
        }
    }

    void DoChase()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.transform.position);

        if (Vector3.Distance(transform.position, player.transform.position) <= catchDistance)
        {
            player.GetCaught();
            return;
        }

        if (!CanSeePlayer())
        {
            StartInvestigate(player.transform.position);
        }
    }

    // -------------------------------------------------------
    //  Detection
    // -------------------------------------------------------

    void CheckSight()
    {
        if (state == GuardState.Chase) return;

        if (CanSeePlayer())
        {
            // If player is ragdolled and guard sees them — instant chase
            // "Easy prey" — ragdolled player can't run
            BroadcastAlert(player.transform.position);
            SetState(GuardState.Chase);
        }
    }

    void CheckHearing()
    {
        if (state == GuardState.Chase) return;

        float noise = player.CurrentNoiseRadius;
        if (noise <= 0f) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= noise && dist <= hearingRange)
            StartInvestigate(player.transform.position);
    }

    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.transform.position - transform.position;

        if (toPlayer.magnitude > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > sightAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, toPlayer.magnitude, obstacleMask))
            return false;

        return true;
    }

    // -------------------------------------------------------
    //  Slap noise reaction
    // -------------------------------------------------------

    void OnHearSlap(Vector3 slapPosition, float noiseRadius)
    {
        float dist = Vector3.Distance(transform.position, slapPosition);
        if (dist > noiseRadius) return;

        // Slaps are LOUD — guards react aggressively
        // Close guards: chase toward the noise
        // Far guards: investigate
        if (dist <= noiseRadius * 0.4f)
        {
            // Very close — go straight to chase if we can see the area
            BroadcastAlert(slapPosition);
            StartInvestigate(slapPosition);
            investigateTimer = investigateDuration * 1.5f; // search longer
        }
        else
        {
            StartInvestigate(slapPosition);
        }

        Debug.Log($"[Guard] {gameObject.name} heard a SLAP at {dist:F1}m away!");
    }

    // -------------------------------------------------------
    //  Alert communication
    // -------------------------------------------------------

    void BroadcastAlert(Vector3 position)
    {
        OnAlertBroadcast?.Invoke(position);
    }

    void OnReceiveAlert(Vector3 alertPosition)
    {
        if (state == GuardState.Chase) return;

        float dist = Vector3.Distance(transform.position, alertPosition);
        if (dist <= alertRadius)
            StartInvestigate(alertPosition);
    }

    // -------------------------------------------------------
    //  Helpers
    // -------------------------------------------------------

    void StartInvestigate(Vector3 position)
    {
        investigatePosition = position;
        investigateTimer = investigateDuration;
        SetState(GuardState.Investigate);
    }

    void SetState(GuardState newState)
    {
        state = newState;
    }

    void OnDrawGizmosSelected()
    {
        // Sight cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Hearing range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Alert radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}
