//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//---------------------------------------------------------------------------------
// Author		: XXX
// Date  		: 2015-05-12
// Modified By	: YYY
// Modified Date: 2015-05-12
// Description	: This is where you write a summary of what the role of this file.
//---------------------------------------------------------------------------------
public class WinLose : MonoBehaviour
{
    #region Variables
    //===================
    // Public Variables
    //===================
    public GameObject WinPanel;
    public GameObject LosePanel;
    public enum GameState { win, lose, playing };
    public GameState state = GameState.playing;

    //===================
    // Private Variables
    //===================

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemyAI enemyAI;
    #endregion

    private void Update()
    {
        switch (state)
        {
            case GameState.win:
                Debug.Log("Win");
                winCondition();
                break;
            case GameState.lose:
                Debug.Log("Lose");
                loseCondition();
                break;
            case GameState.playing:
                break;
        }
    }

    #region Own Methods
    private void winCondition()
    {
        playerMovement.player.SetActive(false);
        enemyAI.gameObject.SetActive(false);
        WinPanel.SetActive(true);
    }

    private void loseCondition()
    {
        playerMovement.player.SetActive(false);
        enemyAI.gameObject.SetActive(false);
        LosePanel.SetActive(true);
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
