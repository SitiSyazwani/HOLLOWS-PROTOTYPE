using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SaveSlotManager : MonoBehaviour
{
    public SaveSlotUI[] saveSlots;

    [Header("Fade Settings")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public string scene;

    public void LoadGame(int slotNumber)
    {
        if (SaveSystem.Instance.LoadSlot(slotNumber))
        {
            int currentLevel = SaveSystem.Instance.GetCurrentLevel();
            StartCoroutine(FadeAndLoadScene(scene));
        }
    }

    public void NewGame(int slotNumber)
    {
        SaveSystem.Instance.CreateNewSave(slotNumber);
        StartCoroutine(FadeAndLoadScene(scene));
    }

    public void DeleteSave(int slotNumber)
    {
        SaveSystem.Instance.DeleteSave(slotNumber);

        if (saveSlots[slotNumber] != null)
        {
            saveSlots[slotNumber].UpdateSlotDisplay();
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
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

        SceneManager.LoadScene(sceneName);
    }
}