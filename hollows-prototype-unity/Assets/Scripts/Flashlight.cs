using UnityEngine;
using UnityEngine.Rendering.Universal;

//---------------------------------------------------------------------------------
// Author        : SitiSyazwani
// Date          : 13/9/2025
// Modified By   : Gemini
// Modified Date : 27/10/2025 (updated rotation fixes)
// Description   : Controls the flashlight's aiming, battery drain, and light obstruction.
//---------------------------------------------------------------------------------
public class Flashlight : MonoBehaviour
{
    #region Variables
    [Header("Flashlight Settings")]
    public float maxRadius = 5f;
    public float angle = 45f;
    public LayerMask obstructionMask;

    [Header("Battery Settings")]
    public float maxBatteryLife = 3.0f; // Represents 3 bars
    public float drainRate = 0.1f;
    public GameObject[] batteryBars; // UI squares for battery display

    [Header("Aim Constraint Settings")]
    public Transform playerBody;
    [Tooltip("The arm GameObject used when facing Left (Side View - PARENT)")]
    public Transform armObjectLeft;
    [Tooltip("The arm GameObject used when facing Right (Side View - PARENT)")]
    public Transform armObjectRight;
    public Transform directFlashlight; // The light/object for front/back views (Vertical Light - CHILD)

    [Tooltip("Max rotation angle upwards (positive relative to base).")]
    public float upRotationLimit = 60f;
    [Tooltip("Max rotation angle downwards (positive relative to base).")]
    public float downRotationLimit = 30f;

    public float verticalRotationLimit = 90f;
    public float rotationSpeed = 10f;

    [Header("Vertical Flashlight Constants")]
    public float flashBwdZRotation = 0f;
    public float flashFwdZRotation = 180f;

    [SerializeField] private Light2D verticalLight;
    [SerializeField] private Collider2D verticalFlashlightCollider;

    private bool flashOn = true;
    private AudioSource flashSound;
    private float currentBatteryLife;

    private Transform currentArm;
    private Light2D currentHorizontalLight;
    private Collider2D currentHorizontalFlashlightCollider;

    // --- store each arm's initial world Z rotation so clamps are relative to them ---
    private float armLeftBaseZ = 0f;
    private float armRightBaseZ = 0f;

    public float flashUpMinZ = -30f;   // When facing upward
    public float flashUpMaxZ = 30f;

    public float flashDownMinZ = 150f; // When facing downward (rotated 180°)
    public float flashDownMaxZ = 210f;

    #endregion

    #region Own Methods
    void Start()
    {
        flashSound = GetComponent<AudioSource>();
        currentBatteryLife = maxBatteryLife;

        if (armObjectLeft != null)
            armLeftBaseZ = armObjectLeft.rotation.eulerAngles.z;
        if (armObjectRight != null)
            armRightBaseZ = armObjectRight.rotation.eulerAngles.z;
    }

    void Update()
    {
        // Toggle Flashlight (Mouse Right Click)
        if (Input.GetMouseButtonDown(1) && currentBatteryLife > 0)
        {
            flashOn = !flashOn;
            if (flashSound != null) flashSound.Play();
        }

        // Drain battery
        if (flashOn)
        {
            currentBatteryLife -= drainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(currentBatteryLife, 0f);
        }

        UpdateBatteryUI();

        bool isArmActive = (armObjectLeft != null && armObjectLeft.gameObject.activeSelf) || (armObjectRight != null && armObjectRight.gameObject.activeSelf);
        bool isDirectFlashlightActive = directFlashlight != null && directFlashlight.gameObject.activeSelf;

        currentArm = null;
        currentHorizontalLight = null;
        currentHorizontalFlashlightCollider = null;

        if (armObjectLeft != null && armObjectLeft.gameObject.activeSelf)
            currentArm = armObjectLeft;
        else if (armObjectRight != null && armObjectRight.gameObject.activeSelf)
            currentArm = armObjectRight;

        if (currentArm != null)
        {
            currentHorizontalLight = currentArm.GetComponentInChildren<Light2D>();
            currentHorizontalFlashlightCollider = currentArm.GetComponentInChildren<Collider2D>();
        }

        if (currentHorizontalLight != null) currentHorizontalLight.enabled = flashOn && isArmActive;
        if (currentHorizontalFlashlightCollider != null) currentHorizontalFlashlightCollider.enabled = flashOn && isArmActive;
        if (verticalLight != null) verticalLight.enabled = flashOn && isDirectFlashlightActive;
        if (verticalFlashlightCollider != null) verticalFlashlightCollider.enabled = flashOn && isDirectFlashlightActive;

        Vector3 playerPos = playerBody.position;

        // ===========================
        // SIMPLE: Clamp arm rotation only
        // ===========================
        if (isArmActive && currentArm != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 aimDirection = (mousePos - playerPos).normalized;

            // Compute the aiming angle
            float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            //  FIX: Invert the angle for the left arm (it faces opposite direction)
            if (currentArm == armObjectLeft)
                targetAngle += 180f;

            // Smoothly rotate toward the target
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            currentArm.rotation = Quaternion.Slerp(
                currentArm.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Clamp rotation to [-60, +30] relative to base facing
            float z = currentArm.eulerAngles.z;
            if (z > 180f) z -= 360f; // normalize to -180 ~ +180
            z = Mathf.Clamp(z, -60f, 30f);
            currentArm.rotation = Quaternion.Euler(0f, 0f, z);

            // Optional: handle light obstruction
            if (currentHorizontalLight != null)
                HandleLightObstruction(playerPos, currentArm.transform.right, currentHorizontalLight);
        }
        else if (isDirectFlashlightActive && directFlashlight != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetDir = (mousePos - playerBody.position).normalized;

            // ----------------------
            // Determine Facing Direction
            // ----------------------
            bool facingUp = playerBody.localScale.y >= 0f; // adjust if your facing is on x-axis instead

            // Ignore mouse on "wrong side"
            if (facingUp && mousePos.y < playerBody.position.y) return;
            if (!facingUp && mousePos.y > playerBody.position.y) return;

            // ----------------------
            // Base rotation and limits
            // ----------------------
            float minLimit, maxLimit;
            if (facingUp)
            {
                minLimit = flashUpMinZ;   // e.g., -30
                maxLimit = flashUpMaxZ;   // e.g., 30
            }
            else
            {
                minLimit = flashDownMinZ; // e.g., -30 relative to downward
                maxLimit = flashDownMaxZ; // e.g., 30 relative to downward
            }

            // ----------------------
            // Compute angle relative to the flashlight's local "up"
            // ----------------------
            Vector2 baseForward = directFlashlight.up; // use the actual transform up
            float angleDiff = Vector2.SignedAngle(baseForward, targetDir);

            // Clamp angle difference
            float clampedAngleDiff = Mathf.Clamp(angleDiff, minLimit, maxLimit);

            // Final rotation
            Quaternion targetRot = Quaternion.AngleAxis(clampedAngleDiff, Vector3.forward) * directFlashlight.rotation;

            // Smooth rotation
            directFlashlight.rotation = Quaternion.Slerp(
                directFlashlight.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            // ----------------------
            // Light obstruction
            // ----------------------
            HandleLightObstruction(playerBody.position, directFlashlight.up, verticalLight);
        }



    }

    private void UpdateBatteryUI()
    {
        if (batteryBars.Length >= 3)
        {
            if (currentBatteryLife <= 2.0f && batteryBars[2] != null && batteryBars[2].activeSelf) batteryBars[2].SetActive(false);
            if (currentBatteryLife <= 1.0f && batteryBars[1] != null && batteryBars[1].activeSelf) batteryBars[1].SetActive(false);

            if (currentBatteryLife <= 0.0f)
            {
                if (batteryBars[0] != null && batteryBars[0].activeSelf) batteryBars[0].SetActive(false);

                if (flashOn)
                {
                    flashOn = false;
                    if (armObjectLeft != null) armObjectLeft.gameObject.SetActive(false);
                    if (armObjectRight != null) armObjectRight.gameObject.SetActive(false);
                    if (directFlashlight != null) directFlashlight.gameObject.SetActive(false);
                    Debug.Log("Battery dead! Flashlight cannot be turned on.");
                }
            }
        }
    }

    private void HandleLightObstruction(Vector3 playerPos, Vector2 rayDirection, Light2D activeLight)
    {
        if (activeLight != null && flashOn)
        {
            RaycastHit2D hit = Physics2D.Raycast(playerPos, rayDirection, maxRadius, obstructionMask);

            if (hit.collider != null)
            {
                activeLight.pointLightOuterRadius = Vector2.Distance(playerPos, hit.point);
            }
            else
            {
                activeLight.pointLightOuterRadius = maxRadius;
            }
        }
    }

    public void RechargeBattery()
    {
        currentBatteryLife = maxBatteryLife;
        for (int i = 0; i < batteryBars.Length; i++)
        {
            if (batteryBars[i] != null)
                batteryBars[i].SetActive(true);
        }
    }
    #endregion
}
