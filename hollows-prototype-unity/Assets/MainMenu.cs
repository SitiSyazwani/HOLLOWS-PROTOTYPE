using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    public void PlayGame()
    {
        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (fadePanel != null)
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
            yield return new WaitForSeconds(0.2f);
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}