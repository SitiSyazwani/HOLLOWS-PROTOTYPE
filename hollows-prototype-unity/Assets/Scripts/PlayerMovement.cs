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

    [Header("Animation Settings")]
    [Tooltip("The Animator component on the player GameObject.")]
    [SerializeField] private Animator animator;

    // Animator Parameter Names
    private const string ANIM_IS_WALKING_SIDE = "IsWalkingSide";
    private const string ANIM_IS_WALKING_UP = "IsWalkingUp";
    private const string ANIM_IS_WALKING_DOWN = "IsWalkingDown";
    private const string ANIM_SPEED_MULTIPLIER = "SpeedMultiplier";

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
    private Sprite lastFacingSide;

    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (animator == null)
        {
            animator = player.GetComponent<Animator>();
        }

        // --- CRITICAL: Force ALL flashlights OFF first ---
        if (armGameObjectL != null) armGameObjectL.SetActive(false);
        if (armGameObjectR != null) armGameObjectR.SetActive(false);
        if (flashlight != null) flashlight.SetActive(false);

        // Disable animator initially so it doesn't override our setup
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Set initial sprite to SIDE (facing left)
        if (spriteRenderer != null && sideSprite != null)
        {
            spriteRenderer.sprite = sideSprite;
            spriteRenderer.flipX = false; // false = facing LEFT
            lastFacingSide = sideSprite;
        }

        // NOW activate ONLY the LEFT arm flashlight
        if (armGameObjectL != null)
        {
            armGameObjectL.SetActive(true);
            lastActiveFlashlight = armGameObjectL;
        }

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

        // Check if all three conditions for sprinting are met
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
            energyBar.localScale = new Vector3(energyBar.localScale.x, energyPercent, energyBar.localScale.z);
        }

        // --- Sprite/Flashlight/Animation State Logic ---
        UpdateSprite();

        if (isSprinting && !wasSprintingLastFrame)
        {
            if (sprint != null)
            {
                sprint.Play();
            }
            if (enemyAI != null)
            {
                // This call might need to be adjusted based on the EnemyAI script's implementation
                // enemyAI.HearSound(transform.position); 
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

        // DEBUG: Check if isSprinting is being set correctly
        // if (isSprinting) Debug.Log("Sprinting: True, Energy: " + currentEnergy);
    }

    void FixedUpdate()
    {
        float currentSpeed = movementSpeed;
        float animationSpeedMultiplier = 1f; // Default animation speed is 1.0 (I changed it from 2f)

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
            animationSpeedMultiplier = sprintMultiplier + 10.0f; // make animation faster

            // --- DEBUG LOG: Check calculated speed when sprinting ---
            // Debug.Log($"SPRINTING. Base Speed: {movementSpeed}, Multiplier: {sprintMultiplier}, Final Speed: {currentSpeed}");
        }
        else if (currentEnergy <= 0f)
        {
            currentSpeed *= exhaustedMultiplier;
            animationSpeedMultiplier = exhaustedMultiplier; // slower animation when tired

            // --- DEBUG LOG: Check calculated speed when exhausted ---
            // Debug.Log($"EXHAUSTED. Base Speed: {movementSpeed}, Final Speed: {currentSpeed}");
        }
        // else {
        //     // --- DEBUG LOG: Check calculated speed when walking normally ---
        //     // Debug.Log($"WALKING. Final Speed: {currentSpeed}");
        // }

        Vector2 newPos = rb.position + movementDirection * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Only set animation speed if the animator is enabled
        if (animator != null && animator.enabled)
        {
            animator.SetFloat(ANIM_SPEED_MULTIPLIER, animationSpeedMultiplier);
        }
    }


    /// <summary>
    /// Handles changing the sprite, enabling/disabling the correct flashlight, and setting animation states.
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

        bool isMoving = movementDirection.magnitude > threshold;

        // ** THE CORE FIX **
        // If the player is not moving, disable the Animator to stop it from overriding the manual sprite.
        if (animator != null)
        {
            animator.enabled = isMoving;
        }

        // ----------------------------------------------------------------------------------
        // IDLE STATE (Animator is OFF, manual sprite/flashlight takes over)
        // ----------------------------------------------------------------------------------
        if (!isMoving)
        {
            // Explicitly set the sprite to the last known facing sprite (e.g., frontSprite, backSprite, or sideSprite)
            spriteRenderer.sprite = lastFacingSide;

            // Only re-activate the last flashlight (optimisation)
            if (lastActiveFlashlight != null && !lastActiveFlashlight.activeSelf)
            {
                lastActiveFlashlight.SetActive(true);

                // Ensure the *other* objects are off
                if (armGameObjectL != null && armGameObjectL != lastActiveFlashlight) armGameObjectL.SetActive(false);
                if (armGameObjectR != null && armGameObjectR != lastActiveFlashlight) armGameObjectR.SetActive(false);
                if (flashlight != null && flashlight != lastActiveFlashlight) flashlight.SetActive(false);
            }
            // If the player is idle, we are done.
            return;
        }

        // ----------------------------------------------------------------------------------
        // MOVING STATE (Animator is ON, manual sprite/flashlight update runs)
        // ----------------------------------------------------------------------------------

        // Reset all directional animation booleans when moving (Animator is enabled here)
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_WALKING_SIDE, false);
            animator.SetBool(ANIM_IS_WALKING_UP, false);
            animator.SetBool(ANIM_IS_WALKING_DOWN, false);
        }


        // Check horizontal movement (side sprites)
        // If horizontal movement is dominant, or we are only moving horizontally
        if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput) && Mathf.Abs(horizontalInput) > threshold)
        {
            // HORIZONTAL MOVEMENT
            if (flashlight != null) flashlight.SetActive(false); // Deactivate the vertical flashlight
            lastFacingSide = sideSprite;

            // Set SIDE animation true
            if (animator != null) animator.SetBool(ANIM_IS_WALKING_SIDE, true);

            // Handle Side-Facing Movement
            if (horizontalInput > 0) // Moving Right
            {
                spriteRenderer.flipX = true;
                if (armGameObjectL != null) armGameObjectL.SetActive(false); // Deactivate Left Arm
                if (armGameObjectR != null) armGameObjectR.SetActive(true); // Activate Right Arm
                lastActiveFlashlight = armGameObjectR; // Store the ACTIVATED arm
            }
            else // Moving Left
            {
                spriteRenderer.flipX = false;
                if (armGameObjectR != null) armGameObjectR.SetActive(false); // Deactivate Right Arm
                if (armGameObjectL != null) armGameObjectL.SetActive(true); // Activate Left Arm
                lastActiveFlashlight = armGameObjectL; // Store the ACTIVATED arm
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

            if (verticalInput > 0) // Moving Up (Back Sprite)
            {
                lastFacingSide = backSprite;
                FlipFlashlightVertical(true);
                // Set UP animation true
                if (animator != null) animator.SetBool(ANIM_IS_WALKING_UP, true);
            }
            else // Moving Down (Front Sprite)
            {
                lastFacingSide = frontSprite;
                FlipFlashlightVertical(false);
                // Set DOWN animation true
                if (animator != null) animator.SetBool(ANIM_IS_WALKING_DOWN, true);
            }
        }

        // This line is now only hit when moving, and the animator will take over the sprite
        // but we still update lastFacingSide for when the player stops.
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
