using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotPanel;
    public Image fadePanel;

    [Header("Settings")]
    public float fadeDuration = 1f;

    void Start()
    {
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
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

    public void BackToMainMenu()
    {
        saveSlotPanel.SetActive(false);
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