using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string name, Sprite icon, int qty = 1)
    {
        foreach (InventoryItem item in items)
        {
            if (item.itemName == name)
            {
                item.quantity += qty;
                return;
            }
        }
        items.Add(new InventoryItem(name, icon, qty));
    }

    public void RemoveItem(string name, int qty = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == name)
            {
                items[i].quantity -= qty;
                if (items[i].quantity <= 0) items.RemoveAt(i);
                return;
            }
        }
    }
}
