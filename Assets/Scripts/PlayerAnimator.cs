using UnityEngine;

/// <summary>
/// Bridges PlayerController movement state to the Animator.
/// Attach to the character model (the GameObject with the Animator).
/// Player must have the "Player" tag.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Drift Fix")]
    public Transform hipBone;       // assign mixamorig_Hips

    [Header("Head Hide")]
    public Transform headBone;      // assign mixamorig_Head

    [Header("Animation Tuning")]
    [Tooltip("How fast the Speed parameter blends toward the target value")]
    public float speedDampTime = 0.1f;

    [Tooltip("Scale walk animation playback to match movement feel")]
    public float walkAnimSpeed = 1.0f;

    [Tooltip("Scale run animation playback to match movement feel")]
    public float runAnimSpeed = 1.0f;

    [Tooltip("Scale crouch animation playback to match movement feel")]
    public float crouchAnimSpeed = 0.8f;

    private Animator animator;
    private PlayerController player;
    private Vector3 hipLocalPosStart;

    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsCrouchHash   = Animator.StringToHash("IsCrouching");
    private static readonly int AnimSpeedHash  = Animator.StringToHash("AnimSpeed");

    void Start()
    {
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();

        if (hipBone != null)
            hipLocalPosStart = hipBone.localPosition;

        if (headBone != null)
            headBone.localScale = Vector3.zero;
    }

    void Update()
    {
        if (animator == null || player == null) return;

        // Calculate a normalized speed: 0 = idle, 0.5 = walk, 1.0 = run
        float targetSpeed = 0f;
        float animSpeed = 1f;

        if (player.IsMoving)
        {
            if (player.IsCrouching)
            {
                targetSpeed = 1f;
                animSpeed = crouchAnimSpeed;
            }
            else if (player.IsRunning)
            {
                targetSpeed = 1f;
                animSpeed = runAnimSpeed;
            }
            else
            {
                targetSpeed = 0.5f;
                animSpeed = walkAnimSpeed;
            }
        }

        // Smooth damp the speed parameter — prevents abrupt transitions
        animator.SetFloat(SpeedHash, targetSpeed, speedDampTime, Time.deltaTime);
        animator.SetBool(IsCrouchHash, player.IsCrouching);
        animator.SetFloat(AnimSpeedHash, animSpeed);
    }

    void LateUpdate()
    {
        if (hipBone != null)
        {
            Vector3 pos = hipBone.localPosition;
            pos.x = hipLocalPosStart.x;
            pos.z = hipLocalPosStart.z;
            hipBone.localPosition = pos;
        }
    }
}
