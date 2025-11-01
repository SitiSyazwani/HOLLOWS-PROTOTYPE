using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    // Assign your TextMeshPro component here in the Inspector
    public TextMeshProUGUI textComponent;

    // The lines array is now private and receives data from the trigger
    private string[] currentLines;

    // --- NEW PUBLIC VARIABLES ---
    public float textSpeed = 0.05f; // Keep this public for inspector tuning
    public float lineDuration = 4.0f; // NEW: Time in seconds before auto-advancing

    private int index;
    private bool dialogueActive = false;
    private Coroutine autoAdvanceCoroutine; // NEW: Reference to the running timer coroutine

    void Start()
    {
        // 1. Hide the dialogue panel instantly on Start
        gameObject.SetActive(false);

        // 2. Ensure the text component starts empty
        if (textComponent != null)
        {
            textComponent.text = string.Empty;
        }
    }

    void Update()
    {
        // Check if dialogue is active and game is NOT paused
        if (!dialogueActive || (FindObjectOfType<PauseMenu>() != null && PauseMenu.GameIsPaused))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // NEW: Stop the automatic timer immediately on a mouse click
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }

            if (textComponent.text == currentLines[index])
            {
                NextLine();
            }
            else
            {
                // Skip the typing animation to show the full line immediately
                StopAllCoroutines();
                textComponent.text = currentLines[index];

                // NEW: Since typing finished instantly, restart the auto-advance timer
                if (dialogueActive) // Check if dialogue didn't just end from the skip
                {
                    autoAdvanceCoroutine = StartCoroutine(AutoAdvanceTimer());
                }
            }
        }
    }

    /// <summary>
    /// PUBLIC METHOD: Called by DialogueTrigger to start the dialogue.
    /// </summary>
    /// <param name="dialogueLines">The array of string lines to display.</param>
    public void StartCustomDialogue(string[] dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue lines are empty, cannot start dialogue.");
            return;
        }

        // Stop any previous coroutine
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        currentLines = dialogueLines;
        gameObject.SetActive(true); // SHOW the dialogue panel
        dialogueActive = true;
        index = 0;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        // Safety check
        if (currentLines == null || index >= currentLines.Length) yield break;

        foreach (char c in currentLines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        // NEW: Typing is finished. Start the auto-advance timer.
        // We only start the timer if the line successfully finished typing.
        if (textComponent.text == currentLines[index])
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceTimer());
        }
    }

    // NEW COROUTINE: Waits for the duration, then calls NextLine
    IEnumerator AutoAdvanceTimer()
    {
        yield return new WaitForSeconds(lineDuration);

        // Only advance if the dialogue is still active (wasn't manually closed)
        if (dialogueActive)
        {
            NextLine();
        }
    }

    void NextLine()
    {
        // NEW: Stop the timer when advancing to prevent an immediate double-advance
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        if (index < currentLines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue
            gameObject.SetActive(false); // HIDE dialogue panel
            dialogueActive = false;
            currentLines = null; // Clean up
        }
    }
}