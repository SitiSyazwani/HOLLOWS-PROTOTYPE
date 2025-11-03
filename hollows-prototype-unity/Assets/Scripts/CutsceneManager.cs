using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    
    [Header("Scene Management")]
    public string menuSceneName = "Menu"; // Change to your menu scene name
    
    [Header("Skip Settings")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    void Start()
    {
        // Find video player if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            // Subscribe to video finished event
            videoPlayer.loopPointReached += OnVideoFinished;
            
            Debug.Log("Cutscene started. Press " + skipKey + " to skip.");
        }
        else
        {
            Debug.LogError("No Video Player found! Please assign it in the inspector.");
        }
    }

    void Update()
    {
        // Allow skipping the cutscene
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            SkipCutscene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Cutscene finished. Loading menu scene: " + menuSceneName);
        LoadMenuScene();
    }

    void SkipCutscene()
    {
        Debug.Log("Cutscene skipped by player.");
        
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        LoadMenuScene();
    }

    void LoadMenuScene()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}