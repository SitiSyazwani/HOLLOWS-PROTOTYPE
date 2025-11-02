using UnityEngine;
using System.Collections;

public class Sound : MonoBehaviour
{
    [Header("Chase Audio")]
    public AudioSource heartBeatAudio;
    public float fadeSpeed = 1.0f;

    // NEW: Running Audio Source
    [Header("Player Audio")]
    public AudioSource runningAudio;

    // Reference to the EnemyAI script
    private EnemyAI enemyAI;
    private WinLose winLose;

    void Start()
    {
        // 1. Initialize Heartbeat Audio (Start played, but volume 0)
        if (heartBeatAudio != null)
        {
            heartBeatAudio.volume = 0;
            heartBeatAudio.Play();
        }

        // 2. Initialize Running Audio (Stop if playing, ready to be controlled)
        if (runningAudio != null)
        {
            runningAudio.Stop();
        }

        enemyAI = FindObjectOfType<EnemyAI>();
        winLose = FindObjectOfType<WinLose>();
    }

    void Update()
    {
        // Safety Check: Only run if essential references are found
        if (enemyAI == null || winLose == null) return;

        bool isChasing = enemyAI.currentState == EnemyAI.State.Chase;
        bool isGamePlaying = winLose.state == WinLose.GameState.playing;

        // ===================================
        // 1. HEARTBEAT AUDIO (Fade In/Out on Chase)
        // ===================================
        if (isChasing && isGamePlaying)
        {
            // StartCoroutine will handle the fade, no need for the running audio check here
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        else // Not chasing OR game is not playing
        {
            // This includes Win/Lose states, ensuring heartbeat fades out
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        // ===================================
        // 2. RUNNING AUDIO (Instant On/Off based on Game State)
        // ===================================
        // Assuming your PlayerMovement script controls when the running audio should play
        // (e.g., setting runningAudio.Play() when moving).

        if (!isGamePlaying)
        {
            // If the game is paused, won, or lost, stop the running sound immediately.
            if (runningAudio != null && runningAudio.isPlaying)
            {
                runningAudio.Stop();
            }
        }
        // If the game IS playing, the PlayerMovement script is responsible for starting/stopping
        // the running audio based on player input/movement velocity. We just ensure it's
        // forcibly stopped if the game state is anything but 'playing'.
    }

    private IEnumerator FadeIn()
    {
        while (heartBeatAudio.volume < 1)
        {
            heartBeatAudio.volume += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while (heartBeatAudio.volume > 0)
        {
            heartBeatAudio.volume -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}