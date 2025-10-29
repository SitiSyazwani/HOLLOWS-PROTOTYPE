using UnityEngine;
using System.Collections;

public class AppQuit : MonoBehaviour
{
    private void Update()
    {
        
    }

    void ExitGame()
    {
        
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
