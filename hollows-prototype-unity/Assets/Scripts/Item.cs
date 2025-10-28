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

        // SIMPLE INVENTORY SYSTEM - No Canvas needed
        public static List<string> collectedItems = new List<string>();
        private static bool showInventory = false;

        // INVENTORY BUTTON
        //private static GameObject inventoryButton;
        //private static bool buttonCreated = false;

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

            // Create inventory button (only once)
            //if (!buttonCreated)
            //{
            //    CreateInventoryButton();
            //    buttonCreated = true;
            //}
        }

        void Update()
        {
            HandleItemCollection();
            // Removed keyboard toggle - using button instead
        }

        // CREATE INVENTORY BUTTON
        //void CreateInventoryButton()
        //{
        //    // Find existing Canvas or create one
        //    Canvas canvas = FindObjectOfType<Canvas>();
        //    if (canvas == null)
        //    {
        //        GameObject canvasObj = new GameObject("InventoryCanvas");
        //        canvas = canvasObj.AddComponent<Canvas>();
        //        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //        canvasObj.AddComponent<CanvasScaler>();
        //        canvasObj.AddComponent<GraphicRaycaster>();
        //    }

        //    // Create button
        //    inventoryButton = new GameObject("InventoryButton");
        //    inventoryButton.transform.SetParent(canvas.transform);

        //    // Add button component
        //    Button button = inventoryButton.AddComponent<Button>();
        //    Image image = inventoryButton.AddComponent<Image>();

        //    // Set button color (semi-transparent gray)
        //    image.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        //    // Set button position (top-right corner)
        //    RectTransform rt = inventoryButton.GetComponent<RectTransform>();
        //    rt.anchorMin = new Vector2(1f, 1f); // Top-right
        //    rt.anchorMax = new Vector2(1f, 1f);
        //    rt.pivot = new Vector2(1f, 1f);
        //    rt.sizeDelta = new Vector2(120f, 40f);
        //    rt.anchoredPosition = new Vector2(-10f, -10f); // 10px from top-right corner

        //    // Create button text
        //    GameObject textObj = new GameObject("ButtonText");
        //    textObj.transform.SetParent(inventoryButton.transform);
        //    Text text = textObj.AddComponent<Text>();
        //    text.text = "INVENTORY";
        //    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //    text.fontSize = 12;
        //    text.color = Color.white;
        //    text.alignment = TextAnchor.MiddleCenter;

        //    // Center text in button
        //    RectTransform textRt = textObj.GetComponent<RectTransform>();
        //    textRt.anchorMin = Vector2.zero;
        //    textRt.anchorMax = Vector2.one;
        //    textRt.sizeDelta = Vector2.zero;
        //    textRt.anchoredPosition = Vector2.zero;

        //    // Add click event
        //    button.onClick.AddListener(ToggleInventory);

        //    Debug.Log("Inventory button created in top-right corner");
        //}
        // SIMPLE INVENTORY BUTTON USING ONGUI
        void OnGUI()
        {
            // Draw inventory button (always visible)
            float buttonWidth = 120f;
            float buttonHeight = 40f;
            float buttonX = Screen.width - buttonWidth - 20f; // 20px from right edge
            float buttonY = 20f; // 20px from top

            // Draw the button
            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "INVENTORY"))
            {
                ToggleInventory();
            }

            // Draw inventory display if open
            if (!showInventory) return;

            // Inventory box below the button
            float boxWidth = 300f;
            float boxHeight = 100f;
            float xPos = (Screen.width - boxWidth) / 2f;
            float yPos = 70f;

            // Draw inventory background
            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "INVENTORY");

            // Draw collected items
            float itemX = xPos + 20f;
            float itemY = yPos + 30f;

            if (collectedItems.Count == 0)
            {
                GUI.Label(new Rect(itemX, itemY, 200f, 30f), "Empty");
            }
            else
            {
                foreach (string itemName in collectedItems)
                {
                    // Set color based on item type
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    if (itemName == "Battery")
                    {
                        style.normal.textColor = Color.yellow;
                    }
                    else if (itemName == "Key")
                    {
                        style.normal.textColor = Color.gray;
                    }
                    else
                    {
                        style.normal.textColor = Color.white;
                    }

                    GUI.Label(new Rect(itemX, itemY, 200f, 30f), "• " + itemName, style);
                    itemY += 25f;
                }
            }

            // Draw instructions
            GUI.Label(new Rect(xPos, yPos + boxHeight - 20f, boxWidth, 20f), "Click button to close");
        }

        // TOGGLE INVENTORY (called by button click)
        public void ToggleInventory()
        {
            showInventory = !showInventory;
            Debug.Log("Inventory " + (showInventory ? "shown" : "hidden") + " - Items: " + collectedItems.Count);
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
    }
}