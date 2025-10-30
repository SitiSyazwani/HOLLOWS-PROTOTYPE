using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLose : MonoBehaviour
{
    public GameObject WinPanel;
    public GameObject LosePanel;
    public enum GameState { win, lose, playing };
    public GameState state = GameState.playing;
    public GameObject MessageBox;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemyAI enemyAI;

    private void Update()
    {
        switch (state)
        {
            case GameState.win:
                winCondition();
                break;
            case GameState.lose:
                loseCondition();
                break;
        }
    }

    private void winCondition()
    {
        // DISABLE SCRIPTS, NOT GAMEOBJECTS
        if (playerMovement != null)
            playerMovement.enabled = false;
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            // Force disable VFX when game ends
            if (enemyAI.redOverlay != null)
                enemyAI.redOverlay.SetActive(false);
        }

        if (WinPanel != null)
            WinPanel.SetActive(true);
        if (MessageBox != null)
            MessageBox.SetActive(false);
    }

    private void loseCondition()
    {
        // DISABLE SCRIPTS, NOT GAMEOBJECTS
        if (playerMovement != null)
            playerMovement.enabled = false;
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
            // Force disable VFX when game ends
            if (enemyAI.redOverlay != null)
                enemyAI.redOverlay.SetActive(false);
        }

        if (LosePanel != null)
            LosePanel.SetActive(true);
        if (MessageBox != null)
            MessageBox.SetActive(false);

        ClearInventory();
    }

    private void ClearInventory()
    {
        // Clear the static inventory list
        if (Assets.Scripts.Item.collectedItems != null)
        {
            Assets.Scripts.Item.collectedItems.Clear();
        }

        // Reset the static flags
        Assets.Scripts.Item.keyFound = false;
        Assets.Scripts.Item.batteryCollected = false;

        Debug.Log("?? Inventory cleared! Ready for new game.");
    }

    public void ReplayGame()
    {
        Debug.Log("Replay button clicked!");

        // Double-clear inventory before reloading
        ClearInventory();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}