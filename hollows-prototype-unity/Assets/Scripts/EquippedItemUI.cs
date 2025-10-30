using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts
{
    public class EquippedItemUI : MonoBehaviour
    {
        public static EquippedItemUI Instance { get; private set; }

        [Header("UI References")]
        public GameObject equippedPanel;
        public Image equippedIcon;
        public TMP_Text equippedNameText;
        public TMP_Text instructionText;

        private string currentEquippedItem = "";

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            ClearEquippedItem();
        }

        void Update()
        {
            // Use equipped item with Right Click or U key
            if (!string.IsNullOrEmpty(currentEquippedItem))
            {
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.U))
                {
                    UseEquippedItem();
                }
            }
        }

        public void EquipItem(string itemName, Sprite icon)
        {
            currentEquippedItem = itemName;

            if (equippedPanel != null)
            {
                equippedPanel.SetActive(true);
            }

            if (equippedIcon != null && icon != null)
            {
                equippedIcon.sprite = icon;
                equippedIcon.enabled = true;
            }

            if (equippedNameText != null)
            {
                equippedNameText.text = itemName;
            }

            if (instructionText != null)
            {
                instructionText.text = "Right Click or [U] to use";
            }

            Debug.Log("Equipped: " + itemName);
        }

        public void ClearEquippedItem()
        {
            currentEquippedItem = "";

            if (equippedPanel != null)
            {
                equippedPanel.SetActive(false);
            }

            if (equippedIcon != null)
            {
                equippedIcon.enabled = false;
                equippedIcon.sprite = null;
            }

            if (equippedNameText != null)
            {
                equippedNameText.text = "";
            }

            if (instructionText != null)
            {
                instructionText.text = "";
            }
        }

        void UseEquippedItem()
        {
            Debug.Log("Using equipped item: " + currentEquippedItem);

            switch (currentEquippedItem)
            {
                case "Battery":
                    UseBattery();
                    break;

                case "Key":
                    Debug.Log("Go to the exit door to use the key!");
                    break;

                default:
                    Debug.Log(currentEquippedItem + " cannot be used.");
                    break;
            }
        }

        void UseBattery()
        {
            Flashlight flashlight = FindObjectOfType<Flashlight>();
            if (flashlight != null)
            {
                flashlight.RechargeBattery();
                Debug.Log("Battery used! Flashlight recharged.");

                // Remove from inventory
                Item.collectedItems.Remove("Battery");

                // Clear equipped item
                ClearEquippedItem();

                // Refresh inventory
                InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshInventory();
                }
            }
        }

        public string GetEquippedItem()
        {
            return currentEquippedItem;
        }
    }
}