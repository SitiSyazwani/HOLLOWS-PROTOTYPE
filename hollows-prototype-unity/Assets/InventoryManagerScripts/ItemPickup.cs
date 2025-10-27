using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemPickup : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon;
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            InventoryManager.Instance.AddItem(itemName, itemIcon);
            gameObject.SetActive(false); // or Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}

