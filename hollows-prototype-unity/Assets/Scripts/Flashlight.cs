using UnityEngine;
using UnityEngine.Rendering.Universal;

//---------------------------------------------------------------------------------
// Author		: SitiSyazwani
// Date  		: 13/9/2025
// Modified By	: -
// Modified Date: -
// Description	: This is the code for the flashlight effects and control.
//---------------------------------------------------------------------------------
public class Flashlight : MonoBehaviour
{
    #region Variables
    //===================
    // Public Variables
    //===================
    [Header("Flashlight Settings")]
    public float maxRadius = 5f;
    public float angle = 45f;
    public LayerMask obstructionMask;

    [Header("Battery Settings")]
    public float maxBatteryLife = 3.0f; // Represents 3 bars
    public float drainRate = 0.1f; // How fast the battery drains per second
    public GameObject[] batteryBars; // An array to hold the 3 UI squares

    //===================
    // Private Variables
    //===================
    private Light2D flashlightLight;
    private bool flashOn = true;
    private AudioSource flashSound;
    private Collider2D flashCollider;
    private float currentBatteryLife;
    #endregion

    #region Own Methods
    void Start()
    {
        flashlightLight = GetComponent<Light2D>();
        flashlightLight.lightType = Light2D.LightType.Point;
        flashSound = GetComponent<AudioSource>();
        flashCollider = GetComponentInChildren<Collider2D>();
        currentBatteryLife = maxBatteryLife;
    }

    void Update()
    {
        // Toggle Flashlight
        // Only allow toggling if there is still battery life
        if (Input.GetMouseButtonDown(1) && currentBatteryLife > 0)
        {
            if (flashOn)
            {
                Debug.Log("Flash toggled OFF!");
                if (flashSound != null) flashSound.Play();
                flashlightLight.enabled = false;
                flashOn = false;
                flashCollider.enabled = false;
            }
            else
            {
                Debug.Log("Flash toggled ON!");
                if (flashSound != null) flashSound.Play();
                flashlightLight.enabled = true;
                flashOn = true;
                flashCollider.enabled = true;
            }
        }

        // Drain battery if the flashlight is on
        if (flashOn)
        {
            currentBatteryLife -= drainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(currentBatteryLife, 0f); // Ensure battery doesn't go below 0
        }

        // Update the UI based on battery life
        if (batteryBars.Length >= 3)
        {
            // Bar 3
            if (currentBatteryLife <= 2.0f && batteryBars[2].activeSelf)
            {
                batteryBars[2].SetActive(false);
            }
            // Bar 2
            if (currentBatteryLife <= 1.0f && batteryBars[1].activeSelf)
            {
                batteryBars[1].SetActive(false);
            }
            // Bar 1
            if (currentBatteryLife <= 0.0f && batteryBars[0].activeSelf)
            {
                batteryBars[0].SetActive(false);
                // Turn off the flashlight when battery is completely dead
                flashlightLight.enabled = false;
                flashOn = false;
                flashCollider.enabled = false;
                Debug.Log("Battery dead! Flashlight cannot be turned on.");
            }
        }


        // Get the player's position (our parent's position)
        Vector3 playerPos = transform.parent.position;

        // Get the direction the player is aiming at (e.g., from mouse position)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - playerPos).normalized;

        // Rotate the flashlight to face the mouse position
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ - 90);


        // Cast a single ray to check for obstructions
        RaycastHit2D hit = Physics2D.Raycast(playerPos, direction, maxRadius, obstructionMask);
        if (hit.collider != null)
        {
            // If the ray hits an obstruction, shorten the light's radius
            flashlightLight.pointLightOuterRadius = Vector2.Distance(playerPos, hit.point);
        }
    }

    // Public method to be called by other scripts
    public void RechargeBattery()
    {
        // Increase the battery life to full
        currentBatteryLife = maxBatteryLife;
        // Turn all the battery bars back on
        for (int i = 0; i < batteryBars.Length; i++)
        {
            if (batteryBars[i] != null)
            {
                batteryBars[i].SetActive(true);
            }
        }
    }
    #endregion
}
