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
        private ItemSlot sourceSlot; // NEW - track where item came from

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

        public void PlaceItem(string itemName, GameObject draggable, ItemSlot source)
        {
            currentItem = itemName;
            currentDraggableItem = draggable;
            sourceSlot = source; // Remember source

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

            // Restore item to inventory slot
            if (sourceSlot != null)
            {
                sourceSlot.RestoreItem();
            }

            currentItem = "";
            currentDraggableItem = null;
            sourceSlot = null;
        }

        public string GetItemName()
        {
            return currentItem;
        }
    }
}