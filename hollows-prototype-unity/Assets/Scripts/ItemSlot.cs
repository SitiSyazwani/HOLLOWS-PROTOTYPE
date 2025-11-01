using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public TMP_Text itemNameText;
        public Image itemIcon;
        public GameObject emptySlotIndicator;

        [Header("Item Icons")]
        public Sprite batteryIcon;
        public Sprite keyIcon;
        public Sprite metalBedFrameIcon;  // NEW
        public Sprite toothbrushIcon;      // NEW
        public Sprite wireIcon;            // NEW
        public Sprite lockpickIcon;        // NEW
        public Sprite defaultIcon;

        [Header("Tooltip")]
        public GameObject tooltipPanel;
        public TMP_Text tooltipText;

        private string itemName = "";
        private bool hasItem = false;
        private Sprite currentIcon;
        private bool isDragging = false;
        private GameObject draggableItem;
        private bool isInCraftingSlot = false;

        void Start()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public void SetItem(string item)
        {
            itemName = item;
            hasItem = true;
            isInCraftingSlot = false; // Important - reset this flag

            // Set the text
            if (itemNameText != null)
            {
                itemNameText.text = item;

                // Set text color based on item type
                if (item == "Battery")
                {
                    itemNameText.color = Color.yellow;
                }
                else if (item == "Key")
                {
                    itemNameText.color = Color.gray;
                }
                else
                {
                    itemNameText.color = Color.white;
                }
            }

            // Set the icon
            if (itemIcon != null)
            {
                itemIcon.enabled = true; // Important - show the icon

                if (item == "Battery" && batteryIcon != null)
                {
                    itemIcon.sprite = batteryIcon;
                    currentIcon = batteryIcon;
                }
                else if (item == "Key" && keyIcon != null)
                {
                    itemIcon.sprite = keyIcon;
                    currentIcon = keyIcon;
                }
                else if (item == "Metal Bed Frame Piece" && metalBedFrameIcon != null)
                {
                    itemIcon.sprite = metalBedFrameIcon;
                    currentIcon = metalBedFrameIcon;
                }
                else if (item == "Toothbrush Handle" && toothbrushIcon != null)
                {
                    itemIcon.sprite = toothbrushIcon;
                    currentIcon = toothbrushIcon;
                }
                else if (item == "Wire" && wireIcon != null)
                {
                    itemIcon.sprite = wireIcon;
                    currentIcon = wireIcon;
                }
                else if (item == "Makeshift Lockpick Set" && lockpickIcon != null)
                {
                    itemIcon.sprite = lockpickIcon;
                    currentIcon = lockpickIcon;
                }
                else if (defaultIcon != null)
                {
                    itemIcon.sprite = defaultIcon;
                    currentIcon = defaultIcon;
                }
            }

            // Hide empty slot indicator
            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(false);
            }
        }

        public void ClearSlot()
        {
            itemName = "";
            hasItem = false;
            currentIcon = null;

            if (itemNameText != null)
            {
                itemNameText.text = "";
            }

            if (itemIcon != null)
            {
                itemIcon.enabled = false;
                itemIcon.sprite = null;
            }

            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(true);
            }

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isDragging && hasItem)
            {
                EquipItem();
            }
        }

        void EquipItem()
        {
            Debug.Log("Equipping item: " + itemName);

            if (EquippedItemUI.Instance != null)
            {
                EquippedItemUI.Instance.EquipItem(itemName, currentIcon);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!hasItem) return;

            isDragging = true;
            CreateDraggableItem();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggableItem != null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                    return;

                RectTransform canvasRect = canvas.transform as RectTransform;
                RectTransform draggableRect = draggableItem.transform as RectTransform;

                Vector2 localPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, eventData.position, canvas.worldCamera, out localPos))
                {
                    draggableRect.anchoredPosition = localPos;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggableItem != null)
            {
                CraftingSlot targetSlot = null;

                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                foreach (var result in results)
                {
                    CraftingSlot slot = result.gameObject.GetComponent<CraftingSlot>();
                    if (slot != null && slot.IsEmpty())
                    {
                        targetSlot = slot;
                        break;
                    }
                }

                if (targetSlot != null)
                {
                    draggableItem.transform.SetParent(targetSlot.transform);
                    draggableItem.transform.position = targetSlot.transform.position;

                    CanvasGroup cg = draggableItem.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 1f;
                        cg.blocksRaycasts = true;
                    }

                    targetSlot.PlaceItem(itemName, draggableItem, this); // Pass reference to this slot

                    // HIDE the item from inventory (but don't remove from list)
                    if (itemIcon != null)
                    {
                        itemIcon.enabled = false;
                    }
                    if (itemNameText != null)
                    {
                        itemNameText.text = "";
                    }
                    isInCraftingSlot = true; // Mark as used

                    Debug.Log("Placed " + itemName + " in slot " + targetSlot.slotNumber);
                }
                else
                {
                    Destroy(draggableItem);
                    Debug.Log("Item not placed - destroyed");
                }
            }

            draggableItem = null;
            isDragging = false;
        }



        void CreateDraggableItem()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found!");
                return;
            }

            draggableItem = new GameObject(itemName + "_Draggable");
            draggableItem.transform.SetParent(canvas.transform, false);

            RectTransform rect = draggableItem.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            rect.anchoredPosition = ((RectTransform)transform).anchoredPosition;

            Image img = draggableItem.AddComponent<Image>();
            if (currentIcon != null)
            {
                img.sprite = currentIcon;
            }
            else
            {
                img.color = Color.white;
            }
            img.raycastTarget = false;

            CanvasGroup cg = draggableItem.AddComponent<CanvasGroup>();
            cg.alpha = 0.7f;
            cg.blocksRaycasts = false;

            // Always appear on top of UI
            Canvas dragCanvas = draggableItem.AddComponent<Canvas>();
            dragCanvas.overrideSorting = true;
            dragCanvas.sortingOrder = 999;

            Debug.Log("Created draggable: " + itemName);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hasItem && !isDragging)
            {
                ShowTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        void ShowTooltip()
        {
            if (tooltipPanel != null && tooltipText != null)
            {
                ItemData data = ItemDatabase.GetItemData(itemName);
                tooltipText.text = data.description;
                tooltipPanel.SetActive(true);
            }
        }

        public void RestoreItem()
        {
            isInCraftingSlot = false;

            if (itemIcon != null && currentIcon != null)
            {
                itemIcon.enabled = true;
                itemIcon.sprite = currentIcon;
            }

            if (itemNameText != null)
            {
                itemNameText.text = itemName;
            }
        }

        void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public string GetItemName()
        {
            return itemName;
        }

        public bool HasItem()
        {
            return hasItem;
        }
    }
}
