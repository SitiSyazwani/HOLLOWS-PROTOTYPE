using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 30f;
    public float investigateTimeout = 10f; // seconds before giving up if no chase
    public float chaseRange = 20f;         // how close player must be to trigger chase
    public float catchDistance = 0.5f;    // Distance threshold for "caught"
    

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;

    private Vector3 lastSoundPos;
    private float investigateTimer = 0f;

    public enum State { Patrol, Investigate, Chase }
    public State currentState = State.Patrol;
    private bool hasReachedInvestigationTarget = false;

    private PlayerMovement playerStatus;
    private bool isHiding;
    [SerializeField] private WinLose winlose;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (player != null)
            playerStatus = player.GetComponent<PlayerMovement>();

        StartPatrol();
        isHiding = FindObjectOfType<PlayerMovement>().isHiding;
    }

    void Update()
    {
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

        // Check for right click input
        if (Input.GetMouseButtonDown(1)) // right click
        {
            lastSoundPos = player.position;
            HearSound(lastSoundPos);
        }

        // Enemy catches player if close enough
        if ((currentState == State.Chase &&
            Vector3.Distance(transform.position, player.position) <= catchDistance &&
            !playerStatus.isHiding))
        {
            winlose.state = WinLose.GameState.lose;
            Debug.Log("Enemy caught the player! Game Over.");
        }

        // if enemy patrolling but player stands too close,, still get chased
        if (Vector3.Distance(transform.position, player.position) <= chaseRange && !playerStatus.isHiding)
        {
            currentState = State.Chase;
            Debug.Log("Enemy spotted player, starting chase!");
        }

    }

    private void PatrolLogic()
    {
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            GoToNextPatrolPoint();
        }
    }

    private void InvestigateLogic()
    {
        agent.speed = chaseSpeed;

        // 1. Check for immediate Chase interruption (player too close)
        if (Vector3.Distance(transform.position, player.position) <= chaseRange && !playerStatus.isHiding)
        {
            currentState = State.Chase;
            hasReachedInvestigationTarget = false; // Reset flag
            Debug.Log("Enemy spotted player, starting chase!");
            return; // Exit early as state changed
        }

        // 2. Check if agent has reached the sound location
        if (!hasReachedInvestigationTarget)
        {
            // If agent is near destination and not calculating a new path, they have arrived.
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                hasReachedInvestigationTarget = true;
                Debug.Log("Enemy reached sound location. Starting investigation countdown.");
            }
        }

        // 3. Handle the investigation timeout once the location is reached
        if (hasReachedInvestigationTarget)
        {
            investigateTimer -= Time.deltaTime;

            // If timer expired or player hiding -> patrol again
            if (investigateTimer <= 0f || playerStatus.isHiding)
            {
                currentState = State.Patrol;
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
            if (Vector3.Distance(transform.position, player.position) > 2 * chaseRange || playerStatus.isHiding)
            {
                currentState = State.Patrol;
                StartPatrol();
                Debug.Log("Enemy lost player, resuming patrol.");
            }
        }
    }

    private void StartPatrol()
    {
        if (patrolPoints.Length == 0) return;

        // Pick the closest patrol point
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

    public void HearSound(Vector3 soundPosition)
    {
        if(currentState != State.Chase)
        {
            currentState = State.Investigate;
            investigateTimer = investigateTimeout;
            lastSoundPos = soundPosition;
            agent.SetDestination(lastSoundPos);
            agent.speed = chaseSpeed; // use same speed for investigate/chase
            hasReachedInvestigationTarget = false;
            Debug.Log("Enemy heard a sound! Investigating " + soundPosition);
        }
        
    }


}
