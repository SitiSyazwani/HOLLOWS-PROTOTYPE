//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.Scripts
{

    //---------------------------------------------------------------------------------
    // Author		: SitiSyazwani
    // Date  		: 2025-09-14
    // Modified By	: -
    // Modified Date: -
    // Description	: Handles item collection (Battery, Key) and Exit trigger.
    //---------------------------------------------------------------------------------
    public class Item : MonoBehaviour
    {
        public GameObject popupUI;       // UI popup prompt
        public GameObject message;       // Pickup message
        public GameObject battHealth;    // Battery health UI
        public GameObject item;          // Reference to the collectible item object

        private bool itemFound = false;
        private float delayTime = 3f;

        // Shared inventory flags
        public static bool keyFound = false;
        public static bool batteryCollected = false;

        // Add a private reference to the Flashlight script
        private Flashlight flashlight;

        void Start()
        {
            // Hide popup UI initially
            if (popupUI != null) popupUI.SetActive(false);
            else Debug.LogError("Popup UI not assigned in Inspector on " + gameObject.name);

            // Ensure this collider is trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (message != null) message.SetActive(false);

            // Get a reference to the Flashlight component in the scene
            flashlight = FindObjectOfType<Flashlight>();
            if (flashlight == null)
            {
                Debug.LogError("Flashlight script not found in the scene.");
            }
        }

        void Update()
        {
            Inventory();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                    Debug.Log("Player entered the trigger. Showing UI popup.");
                    popupUI.SetActive(true);
                    itemFound = true;
                
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                popupUI.SetActive(false);
                Debug.Log("Player exited the trigger. Hiding UI popup.");
                itemFound = false;
            }
        }

        void Inventory()
        {
            if (itemFound && Input.GetKeyDown(KeyCode.E))
            {
                if (message != null) message.SetActive(true);

                if (gameObject.name == "Battery")
                {
                    // Check if the flashlight reference is valid before calling the method
                    if (flashlight != null)
                    {
                        flashlight.RechargeBattery();
                    }

                    if (battHealth != null) battHealth.SetActive(true);
                    batteryCollected = true;
                    Debug.Log("Battery collected!");
                    popupUI.SetActive(false);
                }
                else if (gameObject.name == "Key")
                {
                    keyFound = true;
                    Debug.Log("Key collected!");
                    popupUI.SetActive(false);
                }

                else if(gameObject.name == "ExitDoor")
                {
                    WinLose winlose = FindObjectOfType<WinLose>();
                    if (keyFound)    // check shared flag
                    {
                        winlose.state = WinLose.GameState.win;
                        Debug.Log("Key found! Player wins!");
                    }
                }

                if (item != null) item.SetActive(false);
            }

            if (message != null && message.activeSelf)
            {
                StartCoroutine(DisableObjectAfterDelay(delayTime));
            }
        }

        IEnumerator DisableObjectAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (message != null) message.SetActive(false);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
