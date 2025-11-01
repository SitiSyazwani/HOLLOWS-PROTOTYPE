using UnityEngine;
using System.Collections;

public class AppQuit : MonoBehaviour
{
    private void Update()
    {
        
    }

    public void ExitGame()
    {
        
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
