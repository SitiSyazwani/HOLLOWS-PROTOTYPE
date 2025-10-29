using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;      // Your main buttons (Start, Settings, Quit)
    public GameObject saveSlotPanel;      // The save slot menu
    public Image fadePanel;

    [Header("Settings")]
    public float fadeDuration = 1f;

    void Start()
    {
        // Make sure save slot panel is hidden at start
        if (saveSlotPanel != null)
        {
            saveSlotPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Call this from your Start button
    public void OpenSaveSlotMenu()
    {
        mainMenuPanel.SetActive(false);
        saveSlotPanel.SetActive(true);
    }

    // Call this to go back to main menu from save slots
    public void BackToMainMenu()
    {
        saveSlotPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
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