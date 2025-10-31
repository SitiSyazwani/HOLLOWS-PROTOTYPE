using UnityEngine;
using Assets.Scripts; // Crucial to access the static HasItem method in your Item class

public class DialogueTrigger : MonoBehaviour
{
    // Assign your Dialogue GameObject (which has the Dialogue script) here
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
            // Try to find it if not assigned (only recommended for single-instance manager scripts)
            dialogueManager = FindObjectOfType<Dialogue>();
            if (dialogueManager == null)
            {
                Debug.LogError("Dialogue Manager not found! Assign it in the Inspector or check the scene.");
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the Player is in the trigger zone and presses 'E'
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            string[] linesToShow = GetCurrentDialogue();
            
            if (linesToShow != null && linesToShow.Length > 0)
            {
                // Start the dialogue with the dynamically chosen lines
                dialogueManager.StartCustomDialogue(linesToShow);
            }
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
            // Highest priority: Both items found
            return allItemsLines;
        }
        else if (hasKey)
        {
            // Second priority: Only the key
            return keyFoundLines;
        }
        else if (hasBattery)
        {
            // Third priority: Only the battery
            return batteryFoundLines;
        }
        else
        {
            // Default message
            return defaultLines;
        }
    }
}