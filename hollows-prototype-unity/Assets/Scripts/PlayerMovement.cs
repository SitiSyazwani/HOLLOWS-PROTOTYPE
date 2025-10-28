using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float exhaustedMultiplier = 0.5f;

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 5f;
    [SerializeField] private float energyDrainRate = 1f;
    [SerializeField] private float energyRegenRate = 0.5f;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite sideSprite;

    [Header("Arm Settings")]
    // NEW: Separate GameObjects for left and right arms
    [Tooltip("The arm GameObject used when facing Left (localScale.x > 0)")]
    [SerializeField] private GameObject armGameObjectL;
    [Tooltip("The arm GameObject used when facing Right (localScale.x < 0)")]
    [SerializeField] private GameObject armGameObjectR;

    // armLocalXOffset and armLocalPositionBase are now obsolete and removed.

    [Header("References")]
    public GameObject player;
    public Transform energyBar;
    public GameObject flashlight; // This is the 'directFlashlight' for vertical movement

    // NEW: Variable to track the last flashlight object that was active
    private GameObject lastActiveFlashlight = null;

    // Vertical Flashlight Constants (Based on user request)
    private const float FLASH_X_OFFSET_UP = 0.42f;
    private const float FLASH_X_OFFSET_DOWN = -0.42f;
    private const float FLASH_Z_ROT_UP = 0f;
    private const float FLASH_Z_ROT_DOWN = 180f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movementDirection;

    private float currentEnergy;
    private bool isSprinting;
    public bool isTriggering = false;
    public bool isHiding;
    public AudioSource sprint;
    private EnemyAI enemyAI;

    private bool wasSprintingLastFrame = false;

    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        spriteRenderer = player.GetComponent<SpriteRenderer>();

        // --- NEW: Initialize the state of the separate flashlight components ---
        if (armGameObjectL != null) armGameObjectL.SetActive(false);
        if (armGameObjectR != null) armGameObjectR.SetActive(false);
        if (flashlight != null) flashlight.SetActive(false);

        // Default to the left arm being active at the start (Player facing left/forward)
        if (armGameObjectL != null)
        {
            armGameObjectL.SetActive(true);
            lastActiveFlashlight = armGameObjectL;
        }
        else if (flashlight != null)
        {
            lastActiveFlashlight = flashlight;
        }
        // --- END NEW INIT ---

        currentEnergy = maxEnergy;
        if (sprint != null)
        {
            sprint.Stop();
        }
        enemyAI = FindObjectOfType<EnemyAI>();
    }

    void Update()
    {
        movementDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentEnergy > 0f && movementDirection.magnitude > 0;

        if (isSprinting)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;
            currentEnergy = Mathf.Max(currentEnergy, 0f);
        }
        else
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }

        if (energyBar != null)
        {
            float energyPercent = currentEnergy / maxEnergy;
            // The existing energy bar logic seems to be for a vertical bar. If it's horizontal, you might want to use:
            // energyBar.localScale = new Vector3(energyPercent, energyBar.localScale.y, energyBar.localScale.z);
            // Sticking to original code's y-axis scaling here:
            energyBar.localScale = new Vector3(energyBar.localScale.x, energyPercent, energyBar.localScale.z);
        }

        // --- Sprite/Flashlight State Logic ---
        UpdateSprite();

        if (isSprinting && !wasSprintingLastFrame)
        {
            if (sprint != null)
            {
                sprint.Play();
            }
            if (enemyAI != null)
            {
                enemyAI.HearSound(transform.position);
            }
        }
        else if (!isSprinting && wasSprintingLastFrame)
        {
            if (sprint != null)
            {
                sprint.Stop();
            }
        }

        wasSprintingLastFrame = isSprinting;
    }

    void FixedUpdate()
    {
        float currentSpeed = movementSpeed;

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }
        else if (currentEnergy <= 0f)
        {
            currentSpeed *= exhaustedMultiplier;
        }

        rb.velocity = movementDirection * currentSpeed;
    }

    /// <summary>
    /// Handles changing the sprite and enabling/disabling the correct arm or vertical flashlight.
    /// This is the sole authority for setting armGameObjectL, armGameObjectR, and flashlight active states.
    /// </summary>
    void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float horizontalInput = movementDirection.x;
        float verticalInput = movementDirection.y;
        float threshold = 0.01f;

        // CRITICAL: If the player is idle, restore the LAST active state
        if (Mathf.Abs(horizontalInput) < threshold && Mathf.Abs(verticalInput) < threshold)
        {
            // Only re-activate if it's not already active (optimisation)
            if (lastActiveFlashlight != null && !lastActiveFlashlight.activeSelf)
            {
                lastActiveFlashlight.SetActive(true);

                // Ensure the *other* objects are off
                if (armGameObjectL != null && armGameObjectL != lastActiveFlashlight) armGameObjectL.SetActive(false);
                if (armGameObjectR != null && armGameObjectR != lastActiveFlashlight) armGameObjectR.SetActive(false);
                if (flashlight != null && flashlight != lastActiveFlashlight) flashlight.SetActive(false);
            }
            // If the player is idle, and we have a lastActiveFlashlight, stop all other logic.
            return;
        }

        // Check horizontal movement (side sprites)
        // If horizontal movement is dominant, or we are only moving horizontally
        if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput) && Mathf.Abs(horizontalInput) > threshold)
        {
            // HORIZONTAL MOVEMENT
            if (flashlight != null) flashlight.SetActive(false); // Deactivate the vertical flashlight
            spriteRenderer.sprite = sideSprite;

            // Handle Side-Facing Movement
            if (horizontalInput > 0) // Moving Right
            {
                spriteRenderer.flipX = true;
                // FIX: Right Arm should be active when facing/moving Right (flipX=true)
                if (armGameObjectL != null) armGameObjectL.SetActive(false); // Deactivate Left Arm
                if (armGameObjectR != null) armGameObjectR.SetActive(true); // Activate Right Arm
                lastActiveFlashlight = armGameObjectR; // FIX: Store the ACTIVATED arm
            }
            else // Moving Left
            {
                spriteRenderer.flipX = false;
                // FIX: Left Arm should be active when facing/moving Left (flipX=false)
                if (armGameObjectR != null) armGameObjectR.SetActive(false); // Deactivate Right Arm
                if (armGameObjectL != null) armGameObjectL.SetActive(true); // Activate Left Arm
                lastActiveFlashlight = armGameObjectL; // FIX: Store the ACTIVATED arm
            }
        }
        // Check vertical movement (front/back sprites)
        // If vertical movement is dominant, or we are only moving vertically
        else if (Mathf.Abs(verticalInput) >= Mathf.Abs(horizontalInput) && Mathf.Abs(verticalInput) > threshold)
        {
            // VERTICAL MOVEMENT
            spriteRenderer.flipX = false;

            // Deactivate both side arms
            if (armGameObjectL != null) armGameObjectL.SetActive(false);
            if (armGameObjectR != null) armGameObjectR.SetActive(false);

            if (flashlight != null)
            {
                flashlight.SetActive(true); // Activate the vertical flashlight
                lastActiveFlashlight = flashlight; // Store as last active
            }

            if (verticalInput > 0) // Moving Up
            {
                spriteRenderer.sprite = backSprite;
                // NEW: Flip flashlight for facing UP
                FlipFlashlightVertical(true);
            }
            else // Moving Down
            {
                spriteRenderer.sprite = frontSprite;
                // NEW: Flip flashlight for facing DOWN
                FlipFlashlightVertical(false);
            }
        }
    }

    /// <summary>
    /// Sets the local position and rotation of the vertical flashlight based on the player's facing direction (Up/Down).
    /// </summary>
    /// <param name="isFacingUp">True if the player is facing the back sprite (moving up), False if front (moving down).</param>
    void FlipFlashlightVertical(bool isFacingUp)
    {
        if (flashlight == null)
        {
            return;
        }

        Vector3 currentLocalPosition = flashlight.transform.localPosition;

        if (isFacingUp) // Facing Back (Moving Up) -> X: 0.42, Z-Rot: 0
        {
            flashlight.transform.localPosition = new Vector3(FLASH_X_OFFSET_UP, currentLocalPosition.y, currentLocalPosition.z);
            // This is handled by the Flashlight.cs script for aiming, but we set the base rotation here
            flashlight.transform.localRotation = Quaternion.Euler(0, 0, FLASH_Z_ROT_UP);
        }
        else // Facing Front (Moving Down) -> X: -0.42, Z-Rot: 180
        {
            flashlight.transform.localPosition = new Vector3(FLASH_X_OFFSET_DOWN, currentLocalPosition.y, currentLocalPosition.z);
            // This is handled by the Flashlight.cs script for aiming, but we set the base rotation here
            flashlight.transform.localRotation = Quaternion.Euler(0, 0, FLASH_Z_ROT_DOWN);
        }
    }
}