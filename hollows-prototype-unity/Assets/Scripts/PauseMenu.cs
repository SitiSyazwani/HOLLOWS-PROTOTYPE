using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;
    public Image fadePanel;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public float fadeDuration = 1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenuUI != null && settingsMenuUI.activeSelf)
            {
                CloseSettings();
            }
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        if (fadePanel != null)
        {
            StartCoroutine(FadeAndLoadMainMenu());
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private IEnumerator FadeAndLoadMainMenu()
    {
        Color startColor = fadePanel.color;
        Color targetColor = new Color(0f, 0f, 0f, 1f);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.color = targetColor;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}