using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotPanel;
    public GameObject howToPlayPanel;
    public GameObject quitConfirmationPanel;  // New confirmation panel
    public Image fadePanel;

    [Header("Settings")]
    public float fadeDuration = 1f;

    void Start()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void OpenSaveSlotMenu()
    {
        mainMenuPanel.SetActive(false);
        saveSlotPanel.SetActive(true);
    }

    public void OpenHowToPlay()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        saveSlotPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // Reset all button states in the main menu
        StartCoroutine(ResetMainMenuButtons());
    }

    private IEnumerator ResetMainMenuButtons()
    {
        // Wait one frame for the panel to be fully active
        yield return null;

        // Find all ButtonScript components in the main menu panel
        ButtonScript[] buttons = mainMenuPanel.GetComponentsInChildren<ButtonScript>();
        foreach (ButtonScript btn in buttons)
        {
            btn.ResetButton();
        }
    }

    public void QuitGame()
    {
        // Show confirmation panel WITHOUT hiding main menu
        if (quitConfirmationPanel != null)
        {
            // Don't deactivate mainMenuPanel here!
            quitConfirmationPanel.SetActive(true);
            quitConfirmationPanel.transform.SetAsLastSibling(); // Brings to front
        }
        else
        {
            // Fallback if no confirmation panel is set
            ConfirmQuit();
        }
    }


    public void ConfirmQuit()
    {
        if (fadePanel != null)
        {
            StartCoroutine(FadeAndQuit());
        }
        else
        {
            Debug.Log("QUIT!");
            Application.Quit();
        }
    }

    public void CancelQuit()
    {
        quitConfirmationPanel.SetActive(false);
    }

    private IEnumerator FadeAndQuit()
    {
        Color startColor = fadePanel.color;
        Color targetColor = new Color(0f, 0f, 0f, 1f);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.color = targetColor;
        Application.Quit();
    }
}