//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//---------------------------------------------------------------------------------
// Author		: SitiSyazwani
// Date  		: 2025-09-14
// Modified By	: -
// Modified Date: -
// Description	: This is file for collision.
//---------------------------------------------------------------------------------
public class Battery : MonoBehaviour 
{
    // The UI element to be shown/hidden.
    // Assign this in the Inspector.
    public GameObject popupUI;
    public GameObject message;
    public GameObject battHealth; 

    private bool batteryFound = false;
    private float delayTime = 3f;
    // Use this tag to identify the player object.
    // Make sure your player GameObject has this tag.

    void Start()
    {
        // Initially, the UI popup should be hidden.
        if (popupUI != null)
        {
            popupUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Popup UI GameObject is not assigned in the Inspector on " + gameObject.name);
        }

        // Ensure the collider is a trigger.
        // This is a good practice to prevent accidental errors.
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        
    }

     void Update()
    {
        Inventory();
        
    }

    // Called when another collider enters this trigger.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the entering collider is the player.
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger. Showing UI popup.");
            // Show the UI popup.
            popupUI.SetActive(true);
            batteryFound = true;
                
            
        }
    }

    // Called when another collider exits this trigger.
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if the exiting collider is the player.
        if (other.gameObject.CompareTag("Player"))
        {
            // Hide the UI popup.
            popupUI.SetActive(false);
            Debug.Log("Player exited the trigger. Hiding UI popup.");
        }
    }

    void Inventory()
    {
        if (batteryFound)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                message.SetActive(true);
                battHealth.SetActive(true);
            }
        }

        if(message.activeSelf== true)
        {
            StartCoroutine(DisableObjectAfterDelay(delayTime));
        }
    }

    IEnumerator DisableObjectAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        message.SetActive(false); // Disables the entire GameObject
    }
}
