//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;

//---------------------------------------------------------------------------------
// Author		: XXX
// Date  		: 2015-05-12
// Modified By	: YYY
// Modified Date: 2015-05-12
// Description	: This is where you write a summary of what the role of this file.
//---------------------------------------------------------------------------------
public class TileTrigger : MonoBehaviour 
{
    private SpriteRenderer sprite;
    private bool isVisible = false; // tracked state
   
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false; // hidden at start
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
        if(other.gameObject.name == "lighttrigger")
        {
            Reveal();
        }

        if (isVisible && other.CompareTag("Player"))
        {
            Debug.Log("Player stepped on broken tile!");
            // Find the enemy and send it to investigate here
            EnemyAI enemy = FindObjectOfType<EnemyAI>();
            if (enemy != null)
            {
                enemy.InvestigateAt(transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.name == "lighttrigger")
        {
            sprite.enabled = false;
            isVisible = false;
        }
    }
}
