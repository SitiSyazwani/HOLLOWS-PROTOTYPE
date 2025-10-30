using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class ItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public TMP_Text itemNameText;
        public Image itemIcon;
        public GameObject emptySlotIndicator;

        [Header("Item Icons")]
        public Sprite batteryIcon;
        public Sprite keyIcon;
        public Sprite defaultIcon;

        [Header("Tooltip")]
        public GameObject tooltipPanel;
        public TMP_Text tooltipText;

        private string itemName = "";
        private bool hasItem = false;
        private Sprite currentIcon;

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

            if (itemNameText != null)
            {
                itemNameText.text = item;

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

            if (itemIcon != null)
            {
                itemIcon.enabled = true;

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
                else if (defaultIcon != null)
                {
                    itemIcon.sprite = defaultIcon;
                    currentIcon = defaultIcon;
                }
            }

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
            if (hasItem)
            {
                EquipItem();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hasItem)
            {
                ShowTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        void EquipItem()
        {
            Debug.Log("Equipping item: " + itemName);

            if (EquippedItemUI.Instance != null)
            {
                EquippedItemUI.Instance.EquipItem(itemName, currentIcon);
            }
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