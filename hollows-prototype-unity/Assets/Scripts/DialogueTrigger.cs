using UnityEngine;
using Assets.Scripts;

public class DialogueTrigger : MonoBehaviour
{
    // Assign your Dialogue Panel GameObject here in the Inspector
    public Dialogue dialogueManager;

    // --- Define your custom dialogue lines here ---

    [Header("Default Dialogue (No Items)")]
    public string[] defaultLines = new string[] {
        "Hello there, adventurer. I'm afraid I can't help you yet.",
        "You should look for something useful."
    };

    [Header("Battery Found Dialogue")]
    public string[] batteryFoundLines = new string[] {
        "Oh, you found the Battery! That's essential for the generator.",
        "You should take it to the next room."
    };

    [Header("Key Found Dialogue")]
    public string[] keyFoundLines = new string[] {
        "Wait, is that a Key? Great! You can finally open the exit door.",
        "Hurry, before it's too late!"
    };

    [Header("All Items Found Dialogue (Combined)")]
    public string[] allItemsLines = new string[] {
        "You have all the necessary items! Go, save yourself!",
    };

    //---------------------------------------------------------

    void Start()
    {
        // Basic check to prevent NullReferenceErrors
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Dialogue>();
            if (dialogueManager == null)
            {
                Debug.LogError("Dialogue Manager not found! Assign it in the Inspector or check the scene.");
            }
        }
    }

    void Update()
    {
        // **CRITICAL CHANGE**: Check for 'E' key press every frame, globally.
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGlobalDialogueTrigger();
        }
    }

    // Removed all OnTriggerEnter2D, OnTriggerStay2D, and OnTriggerExit2D methods.

    private void HandleGlobalDialogueTrigger()
    {
        // 1. Get the appropriate lines based on current inventory status
        string[] linesToShow = GetCurrentDialogue();

        // 2. Start the dialogue if lines are available
        if (linesToShow != null && linesToShow.Length > 0 && dialogueManager != null)
        {
            dialogueManager.StartCustomDialogue(linesToShow);
        }
    }

    /// <summary>
    /// Checks the Inventory (using Item.HasItem) and returns the appropriate dialogue.
    /// </summary>
    private string[] GetCurrentDialogue()
    {
        bool hasBattery = Assets.Scripts.Item.HasItem("Battery");
        bool hasKey = Assets.Scripts.Item.HasItem("Key");

        if (hasBattery && hasKey)
        {
            return allItemsLines;
        }
        else if (hasKey)
        {
            return keyFoundLines;
        }
        else if (hasBattery)
        {
            return batteryFoundLines;
        }
        else
        {
            return defaultLines;
        }
    }
}