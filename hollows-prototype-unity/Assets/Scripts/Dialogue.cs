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

    public float textSpeed = 0.05f; // Keep this public for inspector tuning
    private int index;
    private bool dialogueActive = false;

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
        // Assuming PauseMenu is correctly implemented.
        if (!dialogueActive || (FindObjectOfType<PauseMenu>() != null && PauseMenu.GameIsPaused))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == currentLines[index])
            {
                NextLine();
            }
            else
            {
                // Skip the typing animation to show the full line immediately
                StopAllCoroutines();
                textComponent.text = currentLines[index];
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
    }

    void NextLine()
    {
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