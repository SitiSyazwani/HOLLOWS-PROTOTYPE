using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Toggle toggle;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        inventoryPanel.SetActive(isOn);
    }
}
