using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the text is fully displayed for the current line
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            // If the text is still typing, skip to the end of the current line
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        // Corrected: use ToCharArray() method instead of the property toCharArray
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            // Corrected: use WaitForSeconds instead of WaitForSecondd
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            // Corrected: use StartCoroutine instead of StartCoroution
            StartCoroutine(TypeLine());
        }
        else
        {
            // Corrected: set the active state using .SetActive(bool) method
            gameObject.SetActive(false);
        }
    }
}