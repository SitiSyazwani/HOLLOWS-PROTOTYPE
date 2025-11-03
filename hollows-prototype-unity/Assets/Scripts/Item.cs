#define LOG_TRACE_INFO
#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Scripts
{
    //---------------------------------------------------------------------------------
    // Author        : SitiSyazwani
    // Date          : 2025-09-14
    // Modified By   : Rifqah Added Inventory System, Claude Added Lockpick Crafting
    // Description   : Handles item collection, inventory system, and lockpick crafting
    //---------------------------------------------------------------------------------
    public class Item : MonoBehaviour
    {
        public GameObject popupUI;       // UI popup prompt
        public GameObject battHealth;    // Battery health UI
        public GameObject item;          // Reference to the collectible item object

        private bool itemFound = false;

        // Shared inventory flags
        public static bool batteryCollected = false;
        public static bool lockpickCrafted = false; // NEW: Track if lockpick has been crafted

        // Inventory system
        public static List<string> collectedItems = new List<string>();

        // Add a private reference to the Flashlight script
        private Flashlight flashlight;

        // **MODIFIED:** This needs to be static if used by other non-Item scripts.
        // If only used by this instance or other components on the item, keep it public/non-static.
        // Assuming it's used by the Exit item instance only:
        public bool exitFound = false;

        void Start()
        {
            if (item == null)
            {
                item = this.gameObject;
            }
            // Hide popup UI initially
            if (popupUI != null) popupUI.SetActive(false);
            else Debug.LogError("Popup UI not assigned in Inspector on " + gameObject.name);

            // Ensure this collider is trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            // Get a reference to the Flashlight component in the scene
            flashlight = FindObjectOfType<Flashlight>();
            if (flashlight == null)
            {
                Debug.LogError("Flashlight script not found in the scene.");
            }
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

                // **NEW LOGIC:** Set exitFound to true if the item is the Exit Door
                if (IsExitDoor())
                {
                    exitFound = true;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                popupUI.SetActive(false);
                Debug.Log("Player exited the trigger. Hiding UI popup.");
                itemFound = false;

                // **NEW LOGIC:** Set exitFound to false if the item is the Exit Door
                if (IsExitDoor())
                {
                    exitFound = false;
                }
            }
        }

        // **NEW HELPER METHOD**
        /// <summary>
        /// Checks if this specific Item GameObject is the Exit Door, based on Tag or Name.
        /// </summary>
        private bool IsExitDoor()
        {
            return gameObject.CompareTag("ExitDoor") || gameObject.name.Contains("Exit");
        }
        // **END NEW HELPER METHOD**

        void HandleItemCollection()
        {
            if (itemFound && Input.GetKeyDown(KeyCode.E))
            {
                // Determine what type of item this is based on its GameObject name
                if (gameObject.CompareTag("Battery") || gameObject.name.Contains("Battery"))
                {
                    CollectBattery();
                }
                // UPDATED CHECK for new items using full, specific names:
                else if (
                    gameObject.name.Contains("Toothbrush Handle") ||
                    gameObject.name.Contains("Metal Bed Frame Piece") ||
                    gameObject.name.Contains("Wire")
                )
                {
                    CollectGenericItem();
                }
                else if (IsExitDoor()) // Use the helper method here too
                {
                    CheckExitCondition();

                    // Do NOT set exitFound here. It's already handled by OnTriggerEnter/Exit.
                    // Do NOT set item.SetActive(false) here, as the door should remain visible.

                    // Exit condition handles its own message timing
                    return; // Exit early for exit door
                }
                else
                {
                    // Fallback for any other collectible item
                    CollectGenericItem();
                }

                // Only hide UI and item for collectibles, not the exit door
                if (!IsExitDoor())
                {
                    // Hide popup
                    popupUI.SetActive(false);

                    // Hide the collected item object
                    if (item != null)
                    {
                        item.SetActive(false);
                    }
                }
            }
        }

        void CollectBattery()
        {
            Debug.Log("Collecting Battery...");

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

                // Refresh inventory UI (assuming it exists)
                InventoryUI invUI = FindObjectOfType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshInventory();
                }
            }
        }

        void CollectGenericItem()
        {
            string itemName = gameObject.name;

            // Normalize the name to the exact string we want in the inventory
            if (itemName.Contains("Toothbrush Handle")) itemName = "Toothbrush Handle";
            else if (itemName.Contains("Metal Bed Frame Piece")) itemName = "Metal Bed Frame Piece";
            else if (itemName.Contains("Wire")) itemName = "Wire";

            Debug.Log("Collecting item: " + itemName);

            if (!collectedItems.Contains(itemName))
            {
                collectedItems.Add(itemName);
                Debug.Log(itemName + " added to inventory. Total items: " + collectedItems.Count);

                InventoryUI invUI = FindObjectOfType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshInventory();
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

            // NEW WINNING CONDITION: Player must have the crafted lockpick
            // Check for "Makeshift Lockpick Set" which is created by the CraftingPanelUI
            if (collectedItems.Contains("Makeshift Lockpick Set"))
            {
                lockpickCrafted = true; // Set flag for consistency
                winlose.state = WinLose.GameState.win;
                Debug.Log("Makeshift Lockpick Set used! Player escapes and wins!");
            }
            else
            {
                // Check what components they have
                bool hasToothbrushHandle = collectedItems.Contains("Toothbrush Handle");
                bool hasBedFramePiece = collectedItems.Contains("Metal Bed Frame Piece");
                bool hasWire = collectedItems.Contains("Wire");

                if (hasToothbrushHandle && hasBedFramePiece && hasWire)
                {
                    Debug.Log("You have all the components! Open the crafting menu to create the Makeshift Lockpick Set.");
                }
                else
                {
                    Debug.Log("Need to craft a Makeshift Lockpick Set to exit! Collect: Metal Bed Frame Piece, Toothbrush Handle, and Wire, then use the crafting menu.");
                }
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
            batteryCollected = false;
            lockpickCrafted = false; // NEW: Reset lockpick status
            Debug.Log("Inventory cleared");
        }
    }
}