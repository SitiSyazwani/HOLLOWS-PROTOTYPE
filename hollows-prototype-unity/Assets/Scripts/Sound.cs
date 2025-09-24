using UnityEngine;
using System.Collections;

public class Sound : MonoBehaviour 
{
    public AudioSource heartBeatAudio;
    public float fadeSpeed = 1.0f;

    // Reference to the EnemyAI script
    private EnemyAI enemyAI;
    private WinLose winLose;

    //private bool isChasing = false;

    void Start()
    {
        if (heartBeatAudio != null)
        {
            heartBeatAudio.volume = 0;
            heartBeatAudio.Play();
        }
        enemyAI = FindObjectOfType<EnemyAI>();
        winLose = FindObjectOfType<WinLose>();
    }

    void Update()
    {
        if (enemyAI.currentState == EnemyAI.State.Chase && 
            winLose.state == WinLose.GameState.playing)
        {
           
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        else if (enemyAI.currentState != EnemyAI.State.Chase ||
            winLose.state != WinLose.GameState.playing)
        {
           
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

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
