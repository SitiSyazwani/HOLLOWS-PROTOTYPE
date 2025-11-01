using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Assets.Scripts
{
    public class CraftingSlot : MonoBehaviour, IDropHandler
    {
        public int slotNumber;
        public TMP_Text slotNumberText;

        private string currentItem = "";
        private GameObject currentDraggableItem;

        void Start()
        {
            if (slotNumberText != null)
            {
                slotNumberText.text = slotNumber.ToString();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Drop handled in ItemSlot
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(currentItem);
        }

        public void PlaceItem(string itemName, GameObject draggable)
        {
            currentItem = itemName;
            currentDraggableItem = draggable;

            CraftingPanelUI craftingPanel = GetComponentInParent<CraftingPanelUI>();
            if (craftingPanel != null)
            {
                craftingPanel.CheckRecipe();
            }
        }

        public void ClearSlot()
        {
            if (currentDraggableItem != null)
            {
                Destroy(currentDraggableItem);
            }

            currentItem = "";
            currentDraggableItem = null;
        }

        public string GetItemName()
        {
            return currentItem;
        }
    }
}