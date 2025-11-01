using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class CraftingPanelUI : MonoBehaviour
    {
        [Header("Recipe")]
        public OrderedCraftingRecipe currentRecipe;

        [Header("Crafting Slots")]
        public CraftingSlot[] craftingSlots = new CraftingSlot[3]; // 3 slots for the recipe

        [Header("UI")]
        public Button craftButton;
        public TMP_Text recipeInstructionText;
        public GameObject successPanel;

        void Start()
        {
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftButtonClicked);
                craftButton.interactable = false;
            }

            // Initialize the recipe (Level 1: Makeshift Lockpick Set)
            currentRecipe = new OrderedCraftingRecipe(
                "Makeshift Lockpick Set",
                new List<string> { "Metal Bed Frame Piece", "Toothbrush Handle", "Wire" },
                "Makeshift Lockpick Set",
                "Place items in order: 1) Metal Bed Frame, 2) Toothbrush Handle, 3) Wire"
            );

            if (recipeInstructionText != null)
            {
                recipeInstructionText.text = currentRecipe.description;
            }

            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }

        public void CheckRecipe()
        {
            List<string> placedItems = new List<string>();

            // Get items from each slot in order
            foreach (CraftingSlot slot in craftingSlots)
            {
                string itemName = slot.GetItemName();
                if (!string.IsNullOrEmpty(itemName))
                {
                    placedItems.Add(itemName);
                }
            }

            // Check if all slots are filled
            if (placedItems.Count == currentRecipe.orderedItems.Count)
            {
                // Check if order is correct
                if (currentRecipe.CheckOrder(placedItems))
                {
                    // Correct order!
                    if (craftButton != null)
                    {
                        craftButton.interactable = true;
                    }

                    if (feedbackText != null)
                    {
                        feedbackText.text = "Correct! Ready to craft!";
                        feedbackText.color = Color.green;
                    }

                    Debug.Log("Recipe correct! Can craft now.");
                }
                else
                {
                    // Wrong order!
                    if (craftButton != null)
                    {
                        craftButton.interactable = false;
                    }

                    if (feedbackText != null)
                    {
                        feedbackText.text = "Wrong order! Try again.";
                        feedbackText.color = Color.red;
                    }

                    Debug.Log("Recipe incorrect - wrong order!");
                }
            }
            else
            {
                // Not all slots filled
                if (craftButton != null)
                {
                    craftButton.interactable = false;
                }

                if (feedbackText != null)
                {
                    feedbackText.text = "Place all items in the correct slots...";
                    feedbackText.color = Color.white;
                }
            }
        }

        void OnCraftButtonClicked()
        {
            // Remove items from inventory
            foreach (string itemName in currentRecipe.orderedItems)
            {
                Item.collectedItems.Remove(itemName);
                Debug.Log("Removed " + itemName + " from inventory");
            }

            // Add crafted item
            Item.collectedItems.Add(currentRecipe.resultItem);
            Debug.Log("Crafted " + currentRecipe.resultItem + "!");

            // Clear slots
            foreach (CraftingSlot slot in craftingSlots)
            {
                slot.ClearSlot();
            }

            // Show success
            if (successPanel != null)
            {
                successPanel.SetActive(true);
            }

            if (feedbackText != null)
            {
                feedbackText.text = "Crafted " + currentRecipe.resultItem + "!";
                feedbackText.color = Color.green;
            }

            // Refresh inventory
            InventoryUI invUI = FindObjectOfType<InventoryUI>();
            if (invUI != null)
            {
                invUI.RefreshInventory();
            }

            // Close crafting after a delay
            Invoke("CloseCrafting", 2f);
        }

        void CloseCrafting()
        {
            gameObject.SetActive(false);
        }

        public void ResetSlots()
        {
            foreach (CraftingSlot slot in craftingSlots)
            {
                slot.ClearSlot();
            }

            CheckRecipe();
        }
    }
}