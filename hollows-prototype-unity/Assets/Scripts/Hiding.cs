using UnityEngine;

public class Hiding : MonoBehaviour
{
    public GameObject uiPrompt;   // Assign a "Press H to Hide" UI (disable by default)

    private bool playerInside = false;
    //public bool isHiding;
    private GameObject player;    // reference to player object
    private PlayerMovement playerMovement;

    void Start()
    {
        if (uiPrompt != null)
            uiPrompt.SetActive(false);

        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            ToggleHide();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            player = other.gameObject; // store reference to player
            if (uiPrompt != null) uiPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (player.activeSelf == false) {
                playerInside = true;
            }
            else {
                playerInside = false;
            }
            
            if (uiPrompt != null) uiPrompt.SetActive(false);
        }
    }

    private void ToggleHide()
    {

        //if (!isHiding)
        //{
        //    // Hide player
        //    player.SetActive(false);
        //    isHiding = true;
        //}
        //else
        //{
        //    // Unhide player
        //    player.SetActive(true);
        //    isHiding = false;
        //}
        if (!playerMovement.isHiding)
        {
            // Hide player
            player.SetActive(false);
            playerMovement.isHiding = true;
        }
        else
        {
            // Unhide player
            player.SetActive(true);
            playerMovement.isHiding = false;
        }
    }
}
