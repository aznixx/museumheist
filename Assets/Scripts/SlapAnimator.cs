using UnityEngine;

/// <summary>
/// Plays a slap animation clip via the Animator.
/// Attach to the character model (same object as the Animator).
///
/// Setup:
/// 1. Add your slap AnimationClip to the Animator Controller
/// 2. Create a trigger parameter called "Slap"
/// 3. Add a transition from Any State → your slap state using the "Slap" trigger
/// 4. On that transition: uncheck Has Exit Time, set Transition Duration to 0.05
/// 5. The slap state should transition back to your locomotion blend tree when done
/// </summary>
public class SlapAnimator : MonoBehaviour
{
    [Header("Animation")]
    public AnimationClip slapClip;               // drag your slap animation here
    public string slapTriggerName = "Slap";       // Animator trigger parameter name
    public float crossfadeDuration = 0.05f;       // blend into slap animation

    private Animator animator;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlaySlap()
    {
        if (isPlaying || animator == null) return;

        animator.SetTrigger(slapTriggerName);
        isPlaying = true;

        // Auto-reset isPlaying after the clip finishes
        float clipLength = slapClip != null ? slapClip.length : 0.5f;
        Invoke(nameof(ResetPlaying), clipLength);
    }

    void ResetPlaying()
    {
        isPlaying = false;
    }
}
