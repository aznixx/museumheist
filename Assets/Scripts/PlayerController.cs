using UnityEngine;

// Attach to the Player GameObject.
// Requires a CharacterController component on the same object.

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float gravity = -15f;

    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 200f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;   // per second while running
    public float staminaRegenRate = 10f;   // per second while not running

    [Header("Noise Radii")]
    public float walkNoiseRadius = 0f;
    public float runNoiseRadius = 10f;
    public float crouchNoiseRadius = 0f;

    [Header("Footstep Audio")]
    public AudioSource audioSource;
    public AudioClip footstepTile;
    public AudioClip footstepCarpet;
    public float footstepIntervalWalk = 0.5f;
    public float footstepIntervalRun = 0.28f;

    private CharacterController cc;
    private float yaw = 0f;
    private Vector3 velocity;

    [Header("Dummy Mode")]
    public bool isDummy = false;  // check this for slap test dummies

    private bool isRunning;
    private bool isCrouching;
    private bool isCaught;
    private bool hasArtifact;

    private float stamina;
    private bool staminaDepleted;

    private float footstepTimer;
    private int lootCarried;

    // Ragdoll state
    private bool isRagdolled;
    private float ragdollTimer;
    private Rigidbody[] boneRigidbodies;
    private Collider[] boneColliders;
    private Animator animator;

    public bool HasArtifact => hasArtifact;
    public bool IsCaught => isCaught;
    public bool IsCrouching => isCrouching;
    public bool IsRunning => isRunning;
    public bool IsRagdolled => isRagdolled;
    public bool IsMoving => !isRagdolled && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public int LootCarried => lootCarried;

    public float CurrentNoiseRadius
    {
        get
        {
            if (isRagdolled) return 0f;
            if (isCrouching) return crouchNoiseRadius;
            if (isRunning)   return runNoiseRadius;
            return walkNoiseRadius;
        }
    }

    public string NoiseLevelLabel
    {
        get
        {
            if (isRagdolled)                  return "STUNNED";
            if (isCrouching)                  return "SILENT";
            if (!isRunning)                   return "LOW";
            if (stamina > maxStamina * 0.3f)  return "HIGH";
            return "MEDIUM";
        }
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        stamina = maxStamina;
        cc.height = standHeight;

        // Cache all bone rigidbodies/colliders created by the Ragdoll Wizard
        // These are on child bones, not on this root GameObject
        var allRbs = GetComponentsInChildren<Rigidbody>();
        var allCols = GetComponentsInChildren<Collider>();

        // Filter out anything on the root player object itself
        var rbList = new System.Collections.Generic.List<Rigidbody>();
        var colList = new System.Collections.Generic.List<Collider>();

        foreach (var rb in allRbs)
            if (rb.gameObject != gameObject)
                rbList.Add(rb);

        foreach (var col in allCols)
            if (col.gameObject != gameObject)
                colList.Add(col);

        boneRigidbodies = rbList.ToArray();
        boneColliders = colList.ToArray();

        // Start with ragdoll OFF — bones kinematic, colliders disabled
        SetBoneRagdoll(false);

        Debug.Log($"[Ragdoll] Found {boneRigidbodies.Length} bone rigidbodies, {boneColliders.Length} bone colliders");
    }

    void Update()
    {
        if (isCaught) return;

        // Handle ragdoll recovery
        if (isRagdolled)
        {
            ragdollTimer -= Time.deltaTime;
            if (ragdollTimer <= 0f)
                EndRagdoll();
            return; // no input during ragdoll
        }

        // Dummies just stand there — no input processing
        if (isDummy) return;

        HandleLook();
        HandleCrouch();
        HandleMovement();
        HandleStamina();
        HandleFootsteps();
    }

    // -------------------------------------------------------
    //  Look
    // -------------------------------------------------------

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yaw += mouseX;
        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    // -------------------------------------------------------
    //  Crouch
    // -------------------------------------------------------

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
            isCrouching = !isCrouching;

        cc.height = Mathf.Lerp(cc.height, isCrouching ? crouchHeight : standHeight, Time.deltaTime * 10f);
    }

    // -------------------------------------------------------
    //  Movement
    // -------------------------------------------------------

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool moving = h != 0 || v != 0;

        // Can't run while crouching or stamina depleted
        isRunning = Input.GetKey(KeyCode.LeftShift) && moving && !isCrouching && !staminaDepleted;

        float speed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Slow down slightly when carrying lots of loot (5+ items = 10% penalty)
        if (lootCarried >= 5) speed *= 0.9f;

        Vector3 move = transform.right * h + transform.forward * v;
        move = Vector3.ClampMagnitude(move, 1f);

        cc.Move(move * speed * Time.deltaTime);

        // Gravity
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    // -------------------------------------------------------
    //  Stamina
    // -------------------------------------------------------

    void HandleStamina()
    {
        if (isRunning)
        {
            stamina = Mathf.Max(0f, stamina - staminaDrainRate * Time.deltaTime);
            if (stamina <= 0f)
                staminaDepleted = true;
        }
        else
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
            // Re-enable running once stamina is at least 20% restored
            if (staminaDepleted && stamina >= maxStamina * 0.2f)
                staminaDepleted = false;
        }
    }

    // -------------------------------------------------------
    //  Footsteps
    // -------------------------------------------------------

    void HandleFootsteps()
    {
        if (audioSource == null) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool moving = (h != 0 || v != 0) && cc.isGrounded;

        if (!moving) { footstepTimer = 0f; return; }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0f) return;

        footstepTimer = isRunning ? footstepIntervalRun : footstepIntervalWalk;

        AudioClip clip = footstepTile; // swap to footstepCarpet on carpet surfaces
        if (clip == null) return;

        float volume = isCrouching ? 0.1f : (isRunning ? 0.8f : 0.3f);
        audioSource.PlayOneShot(clip, volume);
    }

    // -------------------------------------------------------
    //  Ragdoll (Slap System)
    // -------------------------------------------------------

    public void StartRagdoll(float duration, Vector3 force)
    {
        if (isRagdolled || isCaught) return;

        isRagdolled = true;
        ragdollTimer = duration;

        // Disable CharacterController and Animator
        cc.enabled = false;
        if (animator != null)
            animator.enabled = false;

        // Enable bone physics
        SetBoneRagdoll(true);

        // Apply slap force to the hips (root bone) for a visible tumble
        if (boneRigidbodies.Length > 0)
        {
            // Find the hips rigidbody (usually the first one) and apply force
            Rigidbody hipsRb = boneRigidbodies[0];
            hipsRb.AddForce(force, ForceMode.Impulse);
        }

        Debug.Log($"[Ragdoll] {gameObject.name} got slapped! Ragdolled for {duration}s");
    }

    void SetBoneRagdoll(bool enabled)
    {
        foreach (Rigidbody rb in boneRigidbodies)
        {
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (Collider col in boneColliders)
        {
            // Skip the root GameObject's own colliders (CharacterController)
            if (col.gameObject == gameObject) continue;
            col.enabled = enabled;
        }
    }

    void EndRagdoll()
    {
        isRagdolled = false;

        // Get the hips position to know where the body ended up
        Vector3 hipsPos = transform.position;
        if (boneRigidbodies.Length > 0)
            hipsPos = boneRigidbodies[0].transform.position;

        // Disable bone physics
        SetBoneRagdoll(false);

        // Move the player root to where the hips landed
        transform.position = new Vector3(hipsPos.x, hipsPos.y, hipsPos.z);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        // Re-enable CharacterController and Animator
        cc.enabled = true;
        if (animator != null)
            animator.enabled = true;

        Debug.Log($"[Ragdoll] {gameObject.name} recovered.");
    }

    // -------------------------------------------------------
    //  Public API
    // -------------------------------------------------------

    public void AddLoot()
    {
        lootCarried++;
    }

    public void PickUpArtifact()
    {
        hasArtifact = true;
        GameManager.Instance.StartEscapeTimer();
    }

    public void GetCaught()
    {
        isCaught = true;

        // End ragdoll if caught while ragdolled
        if (isRagdolled)
            EndRagdoll();

        GameManager.Instance.TriggerLose("Caught by a guard!");
    }
}
