using UnityEngine;
using Assets.Scripts;

public class DialogueTrigger : MonoBehaviour
{
    // Assign your Dialogue Panel GameObject here in the Inspector
    public Dialogue dialogueManager;

    // **NEW: Master toggle to disable all dialogue**
    [Header("Dialogue Settings")]
    [Tooltip("Uncheck this to completely disable all dialogue")]
    public bool dialogueEnabled = false;
    // --------------------------------------------

    // **FIX 1: NEW PUBLIC REFERENCE**
    [Header("Exit Reference")]
    [Tooltip("Drag the Item script component from the Exit Door GameObject here.")]
    public Assets.Scripts.Item exitItemReference;
    // ----------------------------

    // --- Define your custom dialogue lines here ---
    [Header("Start Dialogue (No Items)")]
    public string[] startLines = new string[] {
    "Where am I.. its pitch black here"
  };

    [Header("Default Dialogue (No Items)")]
    public string[] defaultLines = new string[] {
    "I need to find materials to some tools to open this door."
  };

    [Header("Battery Found Dialogue")]
    public string[] batteryFoundLines = new string[] {
    "Battery! I can use this for my flashlight."
  };

    // UPDATED DIALOGUE ARRAYS
    [Header("Toothbrush Handle Found Dialogue")]
    public string[] toothbrushHandleFoundLines = new string[] {
    "A toothbrush handle... useless for hygiene, but the handle might be useful.",
  };

    [Header("Metal Bed Frame Piece Found Dialogue")]
    public string[] metalBedFramePieceFoundLines = new string[] {
    "A piece of the metal bed frame. Sturdy enough to be a handle or base.",
  };

    [Header("Wire Found Dialogue")]
    public string[] wireFoundLines = new string[] {
    "A piece of wire. I can use to open something.",
  };

    [Header("All Escape Components Found (Combined)")]
    public string[] allItemsLines = new string[] {
    "I have wire, toothbrush and this metal.. maybe i can craft a lock pick",
  };

    // **FIX 2: Removed buggy 'infrontexit' and 'item' declarations.**

    //---------------------------------------------------------

    void Start()
    {
        // **FIX 3: Removed crashing line that tried to access an unassigned 'item'.**

        // Basic check to prevent NullReferenceErrors
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<Dialogue>();
            if (dialogueManager == null)
            {
                Debug.LogError("Dialogue Manager not found! Assign it in the Inspector or check the scene.");
            }
        }
        // Added a warning for the new reference
        if (exitItemReference == null)
        {
            Debug.LogWarning("Exit Item Reference is not assigned in the Inspector. Dialogue logic for the exit door will not work.");
        }
        HandleGlobalDialogueTrigger(GetStartDialogue());
    }

    void Update()
    {
        // Check for 'E' key press every frame, globally.
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGlobalDialogueTrigger(GetCurrentDialogue());
        }
    }

    private void HandleGlobalDialogueTrigger(string[] dialogue)
    {
        // **NEW: Early exit if dialogue is disabled**
        if (!dialogueEnabled) return;
        
        // 1. Get the appropriate lines based on current inventory status
        string[] linesToShow = GetCurrentDialogue();

        // 2. Start the dialogue if lines are available
        if (linesToShow != null && linesToShow.Length > 0 && dialogueManager != null)
        {
            dialogueManager.StartCustomDialogue(linesToShow);
        }
    }

    private string[] GetStartDialogue()
    {
        return startLines;
    }


    private string[] GetCurrentDialogue()
    {
        bool hasBattery = Assets.Scripts.Item.HasItem("Battery");

        // CHECKING FOR EXACT ITEM NAMES:
        bool hasToothbrushHandle = Assets.Scripts.Item.HasItem("Toothbrush Handle");
        bool hasBedFramePiece = Assets.Scripts.Item.HasItem("Metal Bed Frame Piece");
        bool hasWire = Assets.Scripts.Item.HasItem("Wire");

        // **FIX 4: Dynamic check for 'infrontexit'**
        bool isInFrontOfExit = false;
        if (exitItemReference != null)
        {
            // This correctly accesses the public non-static variable from the assigned instance
            isInFrontOfExit = exitItemReference.exitFound;
        }

        // Final condition check: All 3 components needed for the tool.
        if (hasToothbrushHandle && hasBedFramePiece && hasWire)
        {
            return allItemsLines;
        }

        // Individual item checks (Only show if *only* one of the new items is collected)
        if (hasToothbrushHandle && !hasBedFramePiece && !hasWire)
        {
            return toothbrushHandleFoundLines;
        }
        else if (hasBedFramePiece && !hasToothbrushHandle && !hasWire)
        {
            return metalBedFramePieceFoundLines;
        }
        else if (hasWire && !hasToothbrushHandle && !hasBedFramePiece)
        {
            return wireFoundLines;
        }

        // Battery check
        else if (hasBattery)
        {
            return batteryFoundLines;
        }
        // **FIX 5: Logic updated to use the correct variable (isInFrontOfExit) and operator (&&)**
        else if (!hasToothbrushHandle && !hasBedFramePiece && !hasWire && isInFrontOfExit)
        {
            return defaultLines;
        }
        else
        {
            return startLines;
        }
    }
}