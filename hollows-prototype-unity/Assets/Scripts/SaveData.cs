using System;

[Serializable]
public class SaveData
{
    public int slotNumber;                  // Which slot is this
    public int currentLevel = 1;
    public int highestLevelUnlocked = 1;
    public bool[] levelCompleted;
    public string saveDate;                 // When was this saved
    public float totalPlayTime;             // Optional: track play time

    public SaveData(int totalLevels, int slot)
    {
        slotNumber = slot;
        levelCompleted = new bool[totalLevels];
        saveDate = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm");
    }
}