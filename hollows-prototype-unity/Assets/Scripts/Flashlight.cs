using UnityEngine;
using UnityEngine.Rendering.Universal;

//---------------------------------------------------------------------------------
// Author        : SitiSyazwani
// Date          : 13/9/2025
// Modified By   : Gemini
// Modified Date : 02/11/2025 (CRITICAL DEBUG LOGGING ADDED FOR MISSING COMPONENTS)
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
    public float maxBatteryLife = 3.0f;
    public float drainRate = 0.1f;
    public GameObject[] batteryBars;

    [Header("Aim Constraint Settings (Pivots)")]
    // These must remain public for aiming logic.
    public Transform playerBody;
    [Tooltip("The arm GameObject used when facing Left (arm_pivotL)")]
    public Transform armObjectLeft;
    [Tooltip("The arm GameObject used when facing Right (arm_pivotR)")]
    public Transform armObjectRight;
    public Transform directFlashlight; // The pivot for front/back views (lightV parent)

    // --- Private references (Automatically fetched in Start) ---
    private Light2D horizontalLightL;
    private Collider2D horizontalColliderL;
    private GameObject horizontalGOL; // GameObject of the LightL component

    private Light2D horizontalLightR;
    private Collider2D horizontalColliderR;
    private GameObject horizontalGOR; // GameObject of the LightR component

    private Light2D verticalLight;
    private Collider2D verticalFlashlightCollider;
    private GameObject verticalGO; // GameObject of the LightV component
                                   // ------------------------------------------------------------

    [Tooltip("Max rotation angle upwards (positive relative to base).")]
    public float upRotationLimit = 60f;
    [Tooltip("Max rotation angle downwards (positive relative to base).")]
    public float downRotationLimit = 30f;

    public float verticalRotationLimit = 90f;
    public float rotationSpeed = 10f;

    [Header("Vertical Flashlight Constants")]
    public float flashBwdZRotation = 0f;
    public float flashFwdZRotation = 180f;

    private bool flashOn = true;
    public AudioSource flashSound;
    private float currentBatteryLife;

    private float armLeftBaseZ = 0f;
    private float armRightBaseZ = 0f;

    public float flashUpMinZ = -30f;
    public float flashUpMaxZ = 30f;
    public float flashDownMinZ = 150f;
    public float flashDownMaxZ = 210f;

    #endregion

    #region Own Methods
    void Start()
    {
        currentBatteryLife = maxBatteryLife;

        if (armObjectLeft != null)
        {
            // Try to find the Light2D component anywhere in the children/grandchildren
            horizontalLightL = armObjectLeft.GetComponentInChildren<Light2D>(true);

            // This is the robust check based on your hierarchy image:
            // Try getting the 'arm' child first, then finding Light2D under that.
            if (horizontalLightL == null)
            {
                Transform armChild = armObjectLeft.Find("arm");
                if (armChild != null)
                {
                    horizontalLightL = armChild.GetComponentInChildren<Light2D>(true);
                }
            }

            if (horizontalLightL != null)
            {
                horizontalGOL = horizontalLightL.gameObject;
                horizontalColliderL = horizontalGOL.GetComponent<Collider2D>();
                Debug.Log("Flashlight: Found Left Horizontal Light.");
            }
            else
            {
                Debug.LogError("Flashlight Error: Failed to find Light2D for Left Arm. Check your 'arm_pivotL' assignment.");
            }
        }

        // 2. HORIZONTAL RIGHT (Apply the same check)
        if (armObjectRight != null)
        {
            horizontalLightR = armObjectRight.GetComponentInChildren<Light2D>(true);

            if (horizontalLightR == null)
            {
                Transform armChild = armObjectRight.Find("arm");
                if (armChild != null)
                {
                    horizontalLightR = armChild.GetComponentInChildren<Light2D>(true);
                }
            }

            if (horizontalLightR != null)
            {
                horizontalGOR = horizontalLightR.gameObject;
                horizontalColliderR = horizontalGOR.GetComponent<Collider2D>();
                Debug.Log("Flashlight: Found Right Horizontal Light.");
            }
            else
            {
                Debug.LogError("Flashlight Error: Failed to find Light2D for Right Arm. Check your 'arm_pivotR' assignment.");
            }
        }

        // 3. VERTICAL (Still simple as lightV is directly under the assigned pivot)
        if (directFlashlight != null)
        {
            // In your hierarchy, lightV seems to BE the object you assigned, 
            // so we check on the object itself first.
            verticalLight = directFlashlight.GetComponent<Light2D>();

            if (verticalLight == null)
            {
                // Fallback: check children if lightV is a parent pivot
                verticalLight = directFlashlight.GetComponentInChildren<Light2D>(true);
            }

            if (verticalLight != null)
            {
                verticalGO = verticalLight.gameObject;
                verticalFlashlightCollider = verticalGO.GetComponent<Collider2D>();
                Debug.Log("Flashlight: Found Vertical Light.");
            }
            else
            {
                Debug.LogError("Flashlight Error: Failed to find Light2D for Vertical Light. Check your 'directFlashlight' assignment.");
            }
        }

        // Initial state check
        SetAllLightsState(flashOn);
    }

    void Update()
    {
        // Toggle Flashlight (Mouse Right Click)
        if (Input.GetMouseButtonDown(1) && currentBatteryLife > 0)
        {
            flashOn = !flashOn;
            if (flashSound != null) flashSound.Play();
            SetAllLightsState(flashOn);
        }

        // Drain battery
        if (flashOn && currentBatteryLife > 0)
        {
            currentBatteryLife -= drainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(currentBatteryLife, 0f);
        }

        // Check for battery death
        if (currentBatteryLife <= 0.0f && flashOn)
        {
            flashOn = false;
            Debug.Log("Battery dead! Flashlight cannot be turned on.");
            SetAllLightsState(false);
        }

        UpdateBatteryUI();

        // CRITICAL: Call every frame to synchronize with player movement/arm activity
        // This is the logic that FIXES the original bug.
        SetAllLightsState(flashOn);

        // The rest of the aiming logic...
        if (flashOn)
        {
            Transform currentArm = null;
            Vector3 playerPos = playerBody.position;
            Light2D activeLight = null;

            // 1. HORIZONTAL AIMING - LEFT
            if (armObjectLeft != null && armObjectLeft.gameObject.activeSelf)
            {
                currentArm = armObjectLeft;
                activeLight = horizontalLightL;

                if (activeLight == null) return; // Exit if light wasn't found in Start()

                // ... (rest of aiming logic for left arm) ...
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 aimDirection = (mousePos - playerPos).normalized;

                float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                targetAngle += 180f;

                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
                currentArm.rotation = Quaternion.Slerp(currentArm.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                float z = currentArm.eulerAngles.z;
                if (z > 180f) z -= 360f;
                z = Mathf.Clamp(z, -60f, 30f);
                currentArm.rotation = Quaternion.Euler(0f, 0f, z);

                HandleLightObstruction(playerPos, currentArm.transform.right, activeLight);
            }
            // 1. HORIZONTAL AIMING - RIGHT
            else if (armObjectRight != null && armObjectRight.gameObject.activeSelf)
            {
                currentArm = armObjectRight;
                activeLight = horizontalLightR;

                if (activeLight == null) return; // Exit if light wasn't found in Start()

                // ... (rest of aiming logic for right arm) ...
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 aimDirection = (mousePos - playerPos).normalized;

                float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
                currentArm.rotation = Quaternion.Slerp(currentArm.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                float z = currentArm.eulerAngles.z;
                if (z > 180f) z -= 360f;
                z = Mathf.Clamp(z, -60f, 30f);
                currentArm.rotation = Quaternion.Euler(0f, 0f, z);

                HandleLightObstruction(playerPos, currentArm.transform.right, activeLight);
            }
            // 2. VERTICAL AIMING
            else if (directFlashlight != null && directFlashlight.gameObject.activeSelf)
            {
                activeLight = verticalLight;

                if (activeLight == null) return; // Exit if light wasn't found in Start()

                // ... (rest of aiming logic for vertical light) ...
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 targetDir = (mousePos - playerBody.position).normalized;

                bool facingUp = playerBody.localScale.y >= 0f;

                float minLimit = facingUp ? flashUpMinZ : flashDownMinZ;
                float maxLimit = facingUp ? flashUpMaxZ : flashDownMaxZ;

                float angleDiff = Vector2.SignedAngle(directFlashlight.up, targetDir);
                float clampedAngleDiff = Mathf.Clamp(angleDiff, minLimit, maxLimit);

                Quaternion targetRot = Quaternion.AngleAxis(clampedAngleDiff, Vector3.forward) * directFlashlight.rotation;

                directFlashlight.rotation = Quaternion.Slerp(directFlashlight.rotation, targetRot, rotationSpeed * Time.deltaTime);

                HandleLightObstruction(playerBody.position, directFlashlight.up, verticalLight);
            }
        }
    }

    /// <summary>
    /// CRITICAL FIX: Central function to control the light GameObject's Active State.
    /// This forces the physical light objects OFF if the global flashOn state is false.
    /// </summary>
    private void SetAllLightsState(bool userToggleState)
    {
        // Combined state: Must be turned on by the user AND have battery
        bool finalState = userToggleState && (currentBatteryLife > 0f);

        // Horizontal Left
        if (horizontalGOL != null)
        {
            // Only active if finalState is true AND the parent arm pivot is active
            horizontalGOL.SetActive(finalState && armObjectLeft.gameObject.activeSelf);
        }

        // Horizontal Right
        if (horizontalGOR != null)
        {
            // Only active if finalState is true AND the parent arm pivot is active
            horizontalGOR.SetActive(finalState && armObjectRight.gameObject.activeSelf);
        }

        // Vertical
        if (verticalGO != null)
        {
            // Only active if finalState is true AND the parent pivot is active
            verticalGO.SetActive(finalState && directFlashlight.gameObject.activeSelf);
        }

        // Ensure component enabled state is synchronized for obstruction checks
        if (horizontalLightL != null) horizontalLightL.enabled = horizontalGOL != null && horizontalGOL.activeSelf;
        if (horizontalColliderL != null) horizontalColliderL.enabled = horizontalGOL != null && horizontalGOL.activeSelf;
        if (horizontalLightR != null) horizontalLightR.enabled = horizontalGOR != null && horizontalGOR.activeSelf;
        if (horizontalColliderR != null) horizontalColliderR.enabled = horizontalGOR != null && horizontalGOR.activeSelf;
        if (verticalLight != null) verticalLight.enabled = verticalGO != null && verticalGO.activeSelf;
        if (verticalFlashlightCollider != null) verticalFlashlightCollider.enabled = verticalGO != null && verticalGO.activeSelf;
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
            }
        }
    }

    private void HandleLightObstruction(Vector3 playerPos, Vector2 rayDirection, Light2D activeLight)
    {
        if (activeLight != null && activeLight.enabled)
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
        SetAllLightsState(flashOn);
    }
    #endregion
}