using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public static SaveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveSystem>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("SaveSystem");
                    instance = obj.AddComponent<SaveSystem>();
                }
            }
            return instance;
        }
    }

    [Header("Settings")]
    public int totalLevelsInGame = 10;
    public const int MAX_SAVE_SLOTS = 2;

    private SaveData currentSaveData;
    private int currentSlot = -1; // -1 means no slot selected
    private string saveFolder;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFolder = Application.persistentDataPath + "/Saves/";

            // Create saves folder if it doesn't exist
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Get path for a specific save slot
    private string GetSavePath(int slotNumber)
    {
        return saveFolder + "savefile_" + slotNumber + ".json";
    }

    // Load a specific save slot
    public bool LoadSlot(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= MAX_SAVE_SLOTS)
        {
            Debug.LogError("Invalid slot number!");
            return false;
        }

        string path = GetSavePath(slotNumber);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentSaveData = JsonUtility.FromJson<SaveData>(json);
            currentSlot = slotNumber;
            Debug.Log("Loaded Save Slot " + (slotNumber + 1));
            return true;
        }
        else
        {
            Debug.Log("Save slot " + (slotNumber + 1) + " is empty!");
            return false;
        }
    }

    // Create a new save in a specific slot
    public void CreateNewSave(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= MAX_SAVE_SLOTS)
        {
            Debug.LogError("Invalid slot number!");
            return;
        }

        currentSlot = slotNumber;
        currentSaveData = new SaveData(totalLevelsInGame, slotNumber);
        SaveGame();
        Debug.Log("Created new save in Slot " + (slotNumber + 1));
    }

    // Save current game data
    public void SaveGame()
    {
        if (currentSlot == -1)
        {
            Debug.LogError("No save slot selected!");
            return;
        }

        currentSaveData.saveDate = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(GetSavePath(currentSlot), json);
        Debug.Log("Game Saved to Slot " + (currentSlot + 1));
    }

    // Check if a slot has save data
    public bool DoesSaveExist(int slotNumber)
    {
        return File.Exists(GetSavePath(slotNumber));
    }

    // Get save data for a slot (without loading it)
    public SaveData GetSaveDataPreview(int slotNumber)
    {
        string path = GetSavePath(slotNumber);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        return null;
    }

    // Delete a save slot
    public void DeleteSave(int slotNumber)
    {
        string path = GetSavePath(slotNumber);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted Save Slot " + (slotNumber + 1));
        }
    }

    // Mark a level as completed
    public void CompleteLevel(int levelIndex)
    {
        if (currentSaveData == null)
        {
            Debug.LogError("No save loaded!");
            return;
        }

        if (levelIndex >= 0 && levelIndex < currentSaveData.levelCompleted.Length)
        {
            currentSaveData.levelCompleted[levelIndex] = true;

            if (levelIndex + 1 < totalLevelsInGame && levelIndex + 1 > currentSaveData.highestLevelUnlocked)
            {
                currentSaveData.highestLevelUnlocked = levelIndex + 1;
            }

            SaveGame();
        }
    }

    // Check if a level is completed
    public bool IsLevelCompleted(int levelIndex)
    {
        if (currentSaveData == null) return false;

        if (levelIndex >= 0 && levelIndex < currentSaveData.levelCompleted.Length)
        {
            return currentSaveData.levelCompleted[levelIndex];
        }
        return false;
    }

    // Check if a level is unlocked
    public bool IsLevelUnlocked(int levelIndex)
    {
        if (currentSaveData == null) return false;
        return levelIndex <= currentSaveData.highestLevelUnlocked;
    }

    // Get current level
    public int GetCurrentLevel()
    {
        if (currentSaveData == null) return 1;
        return currentSaveData.currentLevel;
    }

    // Set current level
    public void SetCurrentLevel(int levelIndex)
    {
        if (currentSaveData == null)
        {
            Debug.LogError("No save loaded!");
            return;
        }

        currentSaveData.currentLevel = levelIndex;
        SaveGame();
    }

    // Get highest unlocked level
    public int GetHighestLevelUnlocked()
    {
        if (currentSaveData == null) return 1;
        return currentSaveData.highestLevelUnlocked;
    }

    // Get current slot number
    public int GetCurrentSlot()
    {
        return currentSlot;
    }
}