//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------------
// Author		: XXX
// Date 		: 2015-05-12
// Modified By	: YYY
// Modified Date: 2015-05-12
// Description	: Handles broken tile behavior: revealing on flashlight touch
//               and playing sound/alerting enemy only when a specific 'footstep'
//               collider touches it.
//---------------------------------------------------------------------------------
public class TileTrigger : MonoBehaviour
{
     // Define the constant tag name for the player's dedicated footstep collider
     private const string FOOTSTEP_TAG = "FootstepTrigger";

    private SpriteRenderer sprite;
    private bool isVisible = false; // tracked state
    public AudioSource glassSound;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false; // hidden at start
    //glassSound = GetComponent<AudioSource>();
    }

     // Called when flashlight cone touches tile
       public void Reveal()
    {
        if (!isVisible)
        {
            sprite.enabled = true;
            isVisible = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "lighttrigger")
        {
            Reveal();
        }

    // NOW: We compare the tag of the colliding object to the specific FOOTSTEP_TAG.
    // This ensures only the small collider at the player's feet triggers the sound.
        if (other.CompareTag(FOOTSTEP_TAG))
        {
            Debug.Log("Player stepped on broken tile!");

            if (glassSound != null)
            {
                glassSound.Play();
            }

            // Find the enemy and send it to investigate here
            EnemyAI enemy = FindObjectOfType<EnemyAI>();
            if (enemy != null)
            {
                enemy.HearSound(transform.position);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "lighttrigger")
        {
            sprite.enabled = false;
            isVisible = false;
        }
    }
}
