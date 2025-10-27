using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform itemGrid;
    public GameObject itemSlotPrefab;

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in itemGrid) Destroy(child.gameObject);

        foreach (InventoryItem item in InventoryManager.Instance.items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemGrid);
            slot.transform.Find("Icon").GetComponent<Image>().sprite = item.itemIcon;
            slot.transform.Find("Name").GetComponent<Text>().text = item.itemName;
            slot.transform.Find("Qty").GetComponent<Text>().text = item.quantity.ToString();
        }
    }
}


