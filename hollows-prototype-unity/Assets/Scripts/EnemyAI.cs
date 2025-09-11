//#define LOG_TRACE_INFO
//#define LOG_EXTRA_INFO

using UnityEngine;
using System.Collections;
using UnityEngine.AI;

//---------------------------------------------------------------------------------
// Author		: Siti Syazwani
// Date  		: 2025-9-12
// Modified By	: -
// Modified Date: -
// Description	: This is Enemy Movement script.
//---------------------------------------------------------------------------------
public class EnemyAI : MonoBehaviour 
{
	#region Variables
	//===================
	// Public Variables
	//===================
	public Transform player; // Reference to the player's Transform
	public Transform[] patrolPoints; // Array of waypoints for patrolling
	public float patrolSpeed = 2f;
	public float chaseSpeed = 5f;

    //===================
    // Private Variables
    //===================

    private NavMeshAgent agent;
	private bool isChasing = false;
	private int currentPatrolIndex = 0;


    #endregion

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Ensure the agent is not affecting Z-axis rotation in 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        StartPatrol();
    }

    void Update()
    {
        // Toggle states with a button press
        if (Input.GetKeyDown(KeyCode.Space)) // Use any key you want
        {
            ToggleChase();
        }

        if (isChasing)
        {
            // Chase logic using NavMeshAgent
            if (player != null)
            {
                agent.SetDestination(player.position);
                agent.speed = chaseSpeed;
            }
        }
        else
        {
            // Patrol logic
            if (agent.remainingDistance < 0.5f && !agent.pathPending)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    private void ToggleChase()
    {
        isChasing = !isChasing;
        if (isChasing)
        {
            Debug.Log("Triggered: Starting Chase!");
        }
        else
        {
            Debug.Log("Stopped: Resuming Patrol.");
            // When stopping the chase, find the closest patrol point to resume the patrol path
            StartPatrol();
        }
    }

    private void StartPatrol()
    {
        // Find the closest patrol point to start from
        float minDistance = float.MaxValue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                currentPatrolIndex = i;
            }
        }
        GoToNextPatrolPoint();
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        agent.speed = patrolSpeed;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}
