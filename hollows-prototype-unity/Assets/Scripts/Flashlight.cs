//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

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
    public float maxRadius = 5f;
    public float angle = 45f;
    public LayerMask obstructionMask;

    //===================
    // Private Variables
    //===================
    private Light2D flashlightLight;
    private bool flashOn = true;
    private AudioSource flashSound;
    private Collider2D flashCollider;
    #endregion

    #region Own Methods
    void Start()
    {
        flashlightLight = GetComponent<Light2D>();
        flashlightLight.lightType = Light2D.LightType.Point;
        flashSound = GetComponent<AudioSource>();
        flashCollider = GetComponentInChildren<Collider2D>();
    }

    void Update()
    {
        //Toggle Flashlight
        if (Input.GetMouseButtonDown(1))
        {
            if (flashOn)
            {
                //Debug.Log("Flash toggled OFF!");
                if (flashSound != null) Debug.Log("Flash toggled OFF!"); flashSound.Play();
                flashlightLight.enabled = false;
                flashOn = false;
                flashCollider.enabled = false;

                EnemyAI enemy = FindObjectOfType<EnemyAI>();
                if (enemy != null)
                {
                    enemy.HearSound(transform.position);
                }

            }
            else
            {
                //Debug.Log("Flash toggled ON!");
                if (flashSound != null) Debug.Log("Flash toggled ON!"); flashSound.Play();
                flashlightLight.enabled = true;
                flashOn = true;
                flashCollider.enabled = true;

                EnemyAI enemy = FindObjectOfType<EnemyAI>();
                if (enemy != null)
                {
                    enemy.HearSound(transform.position);
                }

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
    #endregion
}
