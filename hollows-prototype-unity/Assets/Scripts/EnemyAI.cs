using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 30f;
    public float investigateTimeout = 10f; // seconds before giving up if no chase
    public float patrolWaitTime = 10f;      // How long to wait at a patrol point
    public float chaseRange = 20f;         // how close player must be to trigger chase
    public float catchDistance = 0.5f;     // Distance threshold for "caught"
    public Animator animator;

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;

    private Vector3 lastSoundPos;
    private float investigateTimer = 0f;

    // Variables for Patrol Wait functionality
    private bool isWaitingAtPatrolPoint = false;
    private float patrolWaitTimer = 0f;

    public enum State { Patrol, Investigate, Chase }
    public State currentState = State.Patrol;
    private bool hasReachedInvestigationTarget = false;

    private PlayerMovement playerStatus;
    // Removed private isHiding variable, relying on playerStatus.isHiding
    [SerializeField] private WinLose winlose;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Essential for non-3D movement, prevents NavMeshAgent from controlling character rotation/up-axis
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (player != null)
            playerStatus = player.GetComponent<PlayerMovement>();

        if (winlose == null)
            winlose = FindObjectOfType<WinLose>();

        StartPatrol();
    }

    void Update()
    {
        // Global check for Chase state (always prioritized)
        if (Vector3.Distance(transform.position, player.position) <= chaseRange &&
            (playerStatus != null && !playerStatus.isHiding))
        {
            currentState = State.Chase;
            Debug.Log("Enemy spotted player, starting chase!");
        }

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;

            case State.Investigate:
                InvestigateLogic();
                break;

            case State.Chase:
                ChaseLogic();
                break;
        }

        UpdateAnimation();

        // Check for right click input (simulating player making noise)
        if (Input.GetMouseButtonDown(1))
        {
            // Only trigger investigation if not already chasing
            if (currentState != State.Chase)
            {
                lastSoundPos = player.position;
                HearSound(lastSoundPos);
            }
        }

        // Enemy catches player if close enough
        if (currentState == State.Chase &&
            Vector3.Distance(transform.position, player.position) <= catchDistance &&
            (playerStatus != null && !playerStatus.isHiding) &&
            winlose != null)
        {
            winlose.state = WinLose.GameState.lose;
            Debug.Log("Enemy caught the player! Game Over.");
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        bool isMoving = agent.velocity.sqrMagnitude > 0.01f;

        bool shouldBeIdle = true;

        if (currentState == State.Investigate && hasReachedInvestigationTarget)
        {
            // Idle while waiting for investigation timer
            shouldBeIdle = true;
        }
        else if (currentState == State.Patrol && isWaitingAtPatrolPoint)
        {
            // Idle while waiting for patrol timer
            shouldBeIdle = true;
        }
        else if (currentState == State.Patrol && !isMoving && !agent.pathPending)
        {
            // Fallback idle if stopped for any other reason in Patrol state
            shouldBeIdle = true;
        }
        // If chasing and moving, shouldBeIdle will be false.
        animator.SetBool("isIdle", shouldBeIdle);
    }

    private void PatrolLogic()
    {
        //Debug.Log("Paying patrol point");
        agent.speed = patrolSpeed;

        if (!isWaitingAtPatrolPoint)
        {
            Debug.Log("(!isWaitingAtPatrolPoint)");
            // 1. Check if we reached the patrol point
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("reached patrol point");
                // Reached the destination, start the wait
                isWaitingAtPatrolPoint = true;
                patrolWaitTimer = patrolWaitTime;
                agent.isStopped = true; 
                Debug.Log("Enemy reached patrol point. Starting " + patrolWaitTime + " second wait.");

            }

        }
        // 2. If currently waiting at a patrol point
        if (isWaitingAtPatrolPoint)
        {
            Debug.Log("(isWaitingAtPatrolPoint)");
            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0f)
            {
                // Wait is over, transition back to moving
                currentState = State.Patrol;
                isWaitingAtPatrolPoint = false;
                agent.isStopped = false;
                GoToNextPatrolPoint();
                Debug.Log("Patrol wait finished, heading to next point.");
            }
            return; // Stay in the waiting state
        }
        
    }

    private void InvestigateLogic()
    {
        agent.speed = chaseSpeed;

        // 1. Check if agent has reached the sound location
        if (!hasReachedInvestigationTarget)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                hasReachedInvestigationTarget = true;
                investigateTimer = investigateTimeout; // Start countdown upon arrival
                agent.isStopped = true; // Stop agent to look around
                Debug.Log("Enemy reached sound location. Starting investigation countdown.");
            }
        }

        // 2. Handle the investigation timeout once the location is reached
        if (hasReachedInvestigationTarget)
        {
            investigateTimer -= Time.deltaTime;

            // If timer expired or player hiding -> patrol again
            if (investigateTimer <= 0f || (playerStatus != null && playerStatus.isHiding))
            {
                currentState = State.Patrol;
                agent.isStopped = false; // Resume movement
                StartPatrol();
                hasReachedInvestigationTarget = false; // Reset flag
                Debug.Log("Enemy gave up investigation, resuming patrol.");
            }
        }
    }


    private void ChaseLogic()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
            agent.speed = chaseSpeed;

            // If player gets too far, give up
            if (Vector3.Distance(transform.position, player.position) > 2 * chaseRange || (playerStatus != null && playerStatus.isHiding))
            {
                currentState = State.Patrol;
                agent.isStopped = false; // Resume movement
                StartPatrol();
                Debug.Log("Enemy lost player, resuming patrol.");
            }
        }
    }

    private void StartPatrol()
    {
        if (patrolPoints.Length == 0) return;
        PatrolLogic();
        // Ensure agent is ready to move
        agent.isStopped = false;

        // Find the closest patrol point to start with
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

        // Set the initial destination
        GoToNextPatrolPoint();
    }

    private void GoToNextPatrolPoint()
    {
        PatrolLogic();
        if (patrolPoints.Length == 0) return;

        // Ensure movement is enabled before setting destination
        agent.isStopped = false;

        // Set the destination to the current index
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        agent.speed = patrolSpeed;

        // Increment the index for the NEXT call
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void HearSound(Vector3 soundPosition)
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (currentState != State.Chase)
        {
            currentState = State.Investigate;

            // Resume movement immediately to start investigating
            agent.isStopped = false;

            // Reset investigation state
            investigateTimer = investigateTimeout;
            hasReachedInvestigationTarget = false;

            // Set new destination
            lastSoundPos = soundPosition;
            agent.SetDestination(lastSoundPos);
            agent.speed = chaseSpeed;

            Debug.Log("Enemy heard a sound! Investigating " + soundPosition);
        }
    }
}
