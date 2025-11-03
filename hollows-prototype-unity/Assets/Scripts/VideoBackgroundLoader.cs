using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoBackgroundLoader : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;
    public GameObject menuElements; // The canvas or parent object containing your buttons
    
    [Header("Settings")]
    public bool hideMenuUntilVideoReady = true;
    public float maxWaitTime = 3f; // Maximum time to wait before showing menu anyway

    private bool videoReady = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("No Video Player assigned!");
            ShowMenu();
            return;
        }

        // Hide menu initially if option is enabled
        if (hideMenuUntilVideoReady && menuElements != null)
        {
            menuElements.SetActive(false);
        }

        // Subscribe to the prepared event
        videoPlayer.prepareCompleted += OnVideoPrepared;
        
        // Make sure the video is set to prepare
        videoPlayer.Prepare();

        // Start a timeout coroutine in case video takes too long
        StartCoroutine(VideoLoadTimeout());

        Debug.Log("Preparing video background...");
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared and ready!");
        videoReady = true;
        
        // Start playing the video
        vp.Play();
        
        // Show the menu
        ShowMenu();
    }

    IEnumerator VideoLoadTimeout()
    {
        yield return new WaitForSeconds(maxWaitTime);
        
        if (!videoReady)
        {
            Debug.LogWarning("Video took too long to load. Showing menu anyway.");
            ShowMenu();
            
            // Try to play the video anyway
            if (videoPlayer != null)
            {
                videoPlayer.Play();
            }
        }
    }

    void ShowMenu()
    {
        if (menuElements != null)
        {
            menuElements.SetActive(true);
            Debug.Log("Menu displayed!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}