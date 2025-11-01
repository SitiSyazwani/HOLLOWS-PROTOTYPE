#define LOG_TRACE_INFO
#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Scripts
{
    //---------------------------------------------------------------------------------
    // Author		: SitiSyazwani
    // Date  		: 2025-09-14
    // Modified By	: Rifqah Added Inventory System
    // Description	: Handles item collection and inventory system
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

        // Inventory system
        public static List<string> collectedItems = new List<string>();

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

            // ====== TEMPORARY DEBUG CODE - ADD TEST ITEMS ======
            #if UNITY_EDITOR
                if (collectedItems.Count == 0) // Only add if inventory is empty
                {
                    collectedItems.Add("Metal Bed Frame Piece");
                    collectedItems.Add("Toothbrush Handle");
                    collectedItems.Add("Wire");
                    Debug.Log(" TEST: Added crafting items to inventory");
        
                    // Refresh inventory UI if it exists
                    InventoryUI invUI = FindObjectOfType<InventoryUI>();
                    if (invUI != null)
                    {
                        invUI.RefreshInventory();
                    }
                }
            #endif
                        // ====== END DEBUG CODE ======
        }

        void Update()
        {
            HandleItemCollection();
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

        void HandleItemCollection()
        {
            if (itemFound && Input.GetKeyDown(KeyCode.E))
            {
                if (message != null) message.SetActive(true);

                // Determine what type of item this is
                if (gameObject.CompareTag("Battery") || gameObject.name.Contains("Battery"))
                {
                    CollectBattery();
                }
                else if (gameObject.CompareTag("Key") || gameObject.name.Contains("Key"))
                {
                    CollectKey();
                }
                else if (gameObject.CompareTag("ExitDoor") || gameObject.name.Contains("Exit"))
                {
                    CheckExitCondition();
                }
                else
                {
                    // Generic item collection
                    CollectGenericItem();
                }

                // Hide the collected item
                if (item != null)
                {
                    item.SetActive(false);
                    Debug.Log("Item hidden: " + gameObject.name);
                }

                // Hide popup after collection
                popupUI.SetActive(false);

                // Disable message after delay
                if (message != null && message.activeSelf)
                {
                    StartCoroutine(DisableObjectAfterDelay(delayTime));
                }
            }
        }

        void CollectBattery()
        {
            Debug.Log("Collecting Battery...");

            // Check if the flashlight reference is valid before calling the method
            if (flashlight != null)
            {
                flashlight.RechargeBattery();
                Debug.Log("Flashlight recharged");
            }

            if (battHealth != null)
            {
                battHealth.SetActive(true);
                Debug.Log("Battery health UI shown");
            }

            batteryCollected = true;

            // Add to inventory
            if (!collectedItems.Contains("Battery"))
            {
                collectedItems.Add("Battery");
                Debug.Log("Battery added to inventory. Total items: " + collectedItems.Count);

                // Refresh inventory UI
                InventoryUI invUI = FindObjectOfType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshInventory();
                }
            }

            if (message != null)
            {
                Text messageText = message.GetComponent<Text>();
                if (messageText != null)
                {
                    messageText.text = "Battery Collected!";
                }
            }
        }

        void CollectKey()
        {
            Debug.Log("Collecting Key...");

            keyFound = true;

            // Add to inventory
            if (!collectedItems.Contains("Key"))
            {
                collectedItems.Add("Key");
                Debug.Log("Key added to inventory. Total items: " + collectedItems.Count);

                // Refresh inventory UI
                InventoryUI invUI = FindObjectOfType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshInventory();
                }
            }

            if (message != null)
            {
                Text messageText = message.GetComponent<Text>();
                if (messageText != null)
                {
                    messageText.text = "Key Collected!";
                }
            }
        }

        void CollectGenericItem()
        {
            string itemName = gameObject.name;
            Debug.Log("Collecting generic item: " + itemName);

            // Add to inventory with the object's name
            if (!collectedItems.Contains(itemName))
            {
                collectedItems.Add(itemName);
                Debug.Log(itemName + " added to inventory. Total items: " + collectedItems.Count);

                // Refresh inventory UI
                InventoryUI invUI = FindObjectOfType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshInventory();
                }
            }

            if (message != null)
            {
                Text messageText = message.GetComponent<Text>();
                if (messageText != null)
                {
                    messageText.text = itemName + " Collected!";
                }
            }
        }

        void CheckExitCondition()
        {
            WinLose winlose = FindObjectOfType<WinLose>();
            if (winlose == null)
            {
                Debug.LogError("WinLose script not found in scene!");
                return;
            }

            if (keyFound || collectedItems.Contains("Key"))
            {
                winlose.state = WinLose.GameState.win;
                Debug.Log("Key found! Player wins!");

                if (message != null)
                {
                    Text messageText = message.GetComponent<Text>();
                    if (messageText != null)
                    {
                        messageText.text = "You Escaped! You Win!";
                    }
                    message.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Need key to exit!");

                if (message != null)
                {
                    Text messageText = message.GetComponent<Text>();
                    if (messageText != null)
                    {
                        messageText.text = "You need a Key to escape!";
                    }
                    message.SetActive(true);
                    StartCoroutine(DisableObjectAfterDelay(2f));
                }
            }
        }

        IEnumerator DisableObjectAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (message != null)
            {
                message.SetActive(false);
                Debug.Log("Message hidden after delay");
            }
        }

        // Static method to check if player has item (can be called from other scripts)
        public static bool HasItem(string itemName)
        {
            return collectedItems.Contains(itemName);
        }

        // Static method to get all collected items
        public static List<string> GetInventory()
        {
            return new List<string>(collectedItems);
        }

        // Static method to clear inventory (for resetting game)
        public static void ClearInventory()
        {
            collectedItems.Clear();
            keyFound = false;
            batteryCollected = false;
            Debug.Log("Inventory cleared");
        }
    }
}