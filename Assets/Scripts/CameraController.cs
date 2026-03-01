using UnityEngine;

// Attach to the Camera. Follows the Player tag's transform at eye height.
// No bone following — completely independent of animation.

public class CameraController : MonoBehaviour
{
    [Header("Mouse Look")]
    public float mouseSensitivity = 200f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    [Header("Position")]
    public float eyeHeight = 1.7f;        // standing eye height
    public float crouchEyeHeight = 1.2f;  // crouching eye height
    public float forwardOffset = 0.2f;       // standing forward offset
    public float crouchForwardOffset = 0.4f;  // crouching forward offset
    public float heightSmoothSpeed = 8f;  // how fast the camera moves between heights

    private float pitch = 0f;
    private float currentHeight;
    private PlayerController player;

    // Screen shake
    private float shakeTimer;
    private float shakeIntensity;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();

        transform.SetParent(null);
        currentHeight = eyeHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
    }

    void LateUpdate()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (player != null)
        {
            // Smoothly lerp between standing and crouch height
            float targetHeight = player.IsCrouching ? crouchEyeHeight : eyeHeight;
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightSmoothSpeed * Time.deltaTime);

            Vector3 pos = player.transform.position;
            pos.y += currentHeight;
            float fwd = player.IsCrouching ? crouchForwardOffset : forwardOffset;
            pos += player.transform.forward * fwd;
            // Apply screen shake offset
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float x = Random.Range(-shakeIntensity, shakeIntensity);
                float y = Random.Range(-shakeIntensity, shakeIntensity);
                pos += new Vector3(x, y, 0f);
            }

            transform.position = pos;

            float yaw = player.transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeIntensity = magnitude;
    }
}
