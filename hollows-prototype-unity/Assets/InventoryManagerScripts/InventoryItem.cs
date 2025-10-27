using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public Sprite itemIcon;
    public int quantity;

    public InventoryItem(string name, Sprite icon, int qty = 1)
    {
        itemName = name;
        itemIcon = icon;
        quantity = qty;
    }
}

