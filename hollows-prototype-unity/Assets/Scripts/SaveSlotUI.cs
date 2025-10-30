using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("References")]
    public int slotNumber; // Set to 0, 1, or 2 in Inspector
    public TMP_Text slotInfoText;
    public Button loadButton;
    public Button newGameButton;
    public Button deleteButton;

    [Header("Manager")]
    public SaveSlotManager slotManager;

    void Start()
    {
        UpdateSlotDisplay();
    }

    public void UpdateSlotDisplay()
    {
        if (SaveSystem.Instance.DoesSaveExist(slotNumber))
        {
            // Slot has save data
            SaveData data = SaveSystem.Instance.GetSaveDataPreview(slotNumber);
            slotInfoText.text = "Save File " + (slotNumber + 1) + "\n" +
                              "Level: " + data.currentLevel + "\n" +
                              "Date: " + data.saveDate;

            loadButton.gameObject.SetActive(true);
            newGameButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(true);
        }
        else
        {
            // Empty slot
            slotInfoText.text = "Save File " + (slotNumber + 1) + "\n" +
                              "Empty Slot";

            loadButton.gameObject.SetActive(false);
            newGameButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(false);
        }
    }

    public void OnLoadClicked()
    {
        slotManager.LoadGame(slotNumber);
    }

    public void OnNewGameClicked()
    {
        slotManager.NewGame(slotNumber);
    }

    public void OnDeleteClicked()
    {
        slotManager.DeleteSave(slotNumber);
    }
}