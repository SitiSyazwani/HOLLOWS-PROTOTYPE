using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject inventoryPanel;
        public ItemSlot[] itemSlots = new ItemSlot[6]; // Fixed 6 slots
        public TMP_Text titleText;

        private bool isOpen = false;

        void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            if (titleText != null)
            {
                titleText.text = "INVENTORY - Click item to use";
            }

            // Clear all slots initially
            ClearAllSlots();
        }

        void Update()
        {
            // Toggle with Tab key
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }

            // Also check if game is paused - don't open inventory if paused
            if (PauseMenu.GameIsPaused && isOpen)
            {
                HideInventory();
            }
        }

        public void ToggleInventory()
        {
            // Don't open if game is paused
            if (PauseMenu.GameIsPaused)
            {
                return;
            }

            isOpen = !isOpen;

            if (isOpen)
            {
                ShowInventory();
            }
            else
            {
                HideInventory();
            }
        }

        void ShowInventory()
        {
            inventoryPanel.SetActive(true);
            RefreshInventory();
            Debug.Log("Inventory opened");
        }

        void HideInventory()
        {
            inventoryPanel.SetActive(false);
            Debug.Log("Inventory closed");
        }

        public void RefreshInventory()
        {
            // Clear all slots first
            ClearAllSlots();

            // Get collected items
            List<string> items = Item.GetInventory();

            // Fill slots with items (max 6)
            for (int i = 0; i < items.Count && i < 6; i++)
            {
                if (itemSlots[i] != null)
                {
                    itemSlots[i].SetItem(items[i]);
                }
            }
        }

        void ClearAllSlots()
        {
            foreach (ItemSlot slot in itemSlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
        }
    }
}