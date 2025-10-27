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
    // Modified By	: Added Inventory System
    // Modified Date: 2024-01-20
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

        // INVENTORY SYSTEM - Add these variables
        public static List<string> collectedItems = new List<string>();
        private static bool showInventory = false;
        private static GameObject inventoryDisplay;
        private static bool inventoryCreated = false;

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

            // Create inventory display (only once)
            if (!inventoryCreated)
            {
                CreateInventoryDisplay();
                inventoryCreated = true;
            }
        }

        void Update()
        {
            HandleItemCollection();
            HandleInventoryToggle();
        }

        // INVENTORY SYSTEM METHODS
        void CreateInventoryDisplay()
        {
            Debug.Log("Creating inventory display...");

            // Find existing Canvas or create one
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("InventoryCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; // FIXED: Use ScreenSpaceOverlay
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("Created new Canvas");
            }

            // Create inventory background panel
            inventoryDisplay = new GameObject("InventoryDisplay");
            inventoryDisplay.transform.SetParent(canvas.transform);

            // Add Image component for background
            Image bg = inventoryDisplay.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark semi-transparent

            // Set position and size (top of screen)
            RectTransform rt = inventoryDisplay.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(400, 80);
            rt.anchoredPosition = new Vector2(0, -10);

            // Add layout group for automatic positioning
            HorizontalLayoutGroup layout = inventoryDisplay.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 20f;
            layout.padding = new RectOffset(10, 10, 5, 5);

            // Add title text
            GameObject titleObj = new GameObject("InventoryTitle");
            titleObj.transform.SetParent(inventoryDisplay.transform);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "INVENTORY:";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 16;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;

            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.sizeDelta = new Vector2(100, 30);

            // Initially hide inventory
            inventoryDisplay.SetActive(false);
            Debug.Log("Inventory display created successfully");
        }

        void UpdateInventoryDisplay()
        {
            if (inventoryDisplay == null) return;

            // Remove old item displays (keep the title)
            for (int i = inventoryDisplay.transform.childCount - 1; i >= 1; i--)
            {
                Transform child = inventoryDisplay.transform.GetChild(i);
                // Only remove item displays, not the title
                if (child.name != "InventoryTitle")
                {
                    Destroy(child.gameObject);
                }
            }

            // Create item slots for collected items
            foreach (string itemName in collectedItems)
            {
                GameObject itemSlot = new GameObject(itemName + "Slot");
                itemSlot.transform.SetParent(inventoryDisplay.transform);

                // Add vertical layout for icon + text
                VerticalLayoutGroup slotLayout = itemSlot.AddComponent<VerticalLayoutGroup>();
                slotLayout.childAlignment = TextAnchor.MiddleCenter;
                slotLayout.spacing = 2f;

                RectTransform slotRt = itemSlot.GetComponent<RectTransform>();
                slotRt.sizeDelta = new Vector2(70, 60);

                // Create icon background
                GameObject iconBg = new GameObject("IconBackground");
                iconBg.transform.SetParent(itemSlot.transform);
                Image iconImg = iconBg.AddComponent<Image>();

                // Set icon color based on item type
                if (itemName == "Battery")
                {
                    iconImg.color = Color.yellow;
                }
                else if (itemName == "Key")
                {
                    iconImg.color = new Color(0.7f, 0.7f, 0.7f); // Silver color
                }
                else
                {
                    iconImg.color = Color.green; // Default color for other items
                }

                // Add a simple border
                iconImg.color = new Color(iconImg.color.r, iconImg.color.g, iconImg.color.b, 0.8f);

                RectTransform iconRt = iconBg.GetComponent<RectTransform>();
                iconRt.sizeDelta = new Vector2(50, 40);

                // Create item name text
                GameObject textObj = new GameObject("ItemText");
                textObj.transform.SetParent(itemSlot.transform);
                Text text = textObj.AddComponent<Text>();
                text.text = itemName;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 12;
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;

                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.sizeDelta = new Vector2(70, 20);
            }

            // If no items, show "Empty"
            if (collectedItems.Count == 0)
            {
                GameObject emptySlot = new GameObject("EmptySlot");
                emptySlot.transform.SetParent(inventoryDisplay.transform);

                GameObject emptyText = new GameObject("EmptyText");
                emptyText.transform.SetParent(emptySlot.transform);
                Text text = emptyText.AddComponent<Text>();
                text.text = "Empty";
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 14;
                text.color = Color.gray;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontStyle = FontStyle.Italic;

                RectTransform textRt = emptyText.GetComponent<RectTransform>();
                textRt.sizeDelta = new Vector2(100, 30);
            }
        }

        void HandleInventoryToggle()
        {
            // Toggle inventory with I key
            if (Input.GetKeyDown(KeyCode.I))
            {
                showInventory = !showInventory;
                if (inventoryDisplay != null)
                {
                    inventoryDisplay.SetActive(showInventory);
                    if (showInventory)
                    {
                        UpdateInventoryDisplay();
                        Debug.Log("Inventory shown with " + collectedItems.Count + " items");
                    }
                    else
                    {
                        Debug.Log("Inventory hidden");
                    }
                }
            }
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

                // Update inventory display if it's open
                if (showInventory)
                {
                    UpdateInventoryDisplay();
                }

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
            Debug.Log("Inventory cleared");
        }

        // Helper method to force show inventory (for testing)
        public static void ShowInventory()
        {
            showInventory = true;
            if (inventoryDisplay != null)
            {
                inventoryDisplay.SetActive(true);
                // Note: UpdateInventoryDisplay would need to be called from an instance
            }
        }

        // Helper method to force hide inventory
        public static void HideInventory()
        {
            showInventory = false;
            if (inventoryDisplay != null)
            {
                inventoryDisplay.SetActive(false);
            }
        }
    }
}