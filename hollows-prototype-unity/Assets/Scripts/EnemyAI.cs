using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 10f;
    public float investigateTimeout = 5f; // seconds before giving up if no chase
    public float chaseRange = 5f;         // how close player must be to trigger chase
    public float catchDistance = 0.5f;    // Distance threshold for "caught"
    

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;

    private Vector3 lastClickPosition;
    private float investigateTimer = 0f;

    private enum State { Patrol, Investigate, Chase }
    private State currentState = State.Patrol;

    private PlayerMovement playerStatus; // <-- script that holds isHiding flag
    [SerializeField] private WinLose winlose;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        if (player != null)
            playerStatus = player.GetComponent<PlayerMovement>();

        StartPatrol();
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
            lastClickPosition = player.position;
            currentState = State.Investigate;
            investigateTimer = investigateTimeout;
            agent.SetDestination(lastClickPosition);
            agent.speed = patrolSpeed;
            Debug.Log("Enemy is investigating last click position!");
        }

        // Enemy catches player if close enough
        if ((currentState == State.Chase &&
            Vector3.Distance(transform.position, player.position) <= catchDistance))
        {
            winlose.state = WinLose.GameState.lose;
            Debug.Log("Enemy caught the player! Game Over.");
        }

        if(Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            winlose.state = WinLose.GameState.lose;
            Debug.Log("Enemy caught the player! Game Over.");
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
        investigateTimer -= Time.deltaTime;

        // If player is close AND not hiding -> chase
        if (Vector3.Distance(transform.position, player.position) <= chaseRange && !playerStatus.isHiding)
        {
            currentState = State.Chase;
            Debug.Log("Enemy spotted player, starting chase!");
        }
        // If timer expired or player hiding -> patrol again
        else if (investigateTimer <= 0f)
        {
            currentState = State.Patrol;
            StartPatrol();
            Debug.Log("Enemy gave up investigation, resuming patrol.");
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

    public void InvestigateAt(Vector3 position)
    {
        currentState = State.Investigate;
        investigateTimer = investigateTimeout;
        lastClickPosition = position;
        agent.SetDestination(lastClickPosition);
        agent.speed = patrolSpeed;
        Debug.Log("Enemy is investigating forced position: " + position);
    }

    public void HearSound(Vector3 soundPosition)
    {
        currentState = State.Investigate;
        investigateTimer = investigateTimeout;
        lastClickPosition = soundPosition;
        agent.SetDestination(lastClickPosition);
        agent.speed = chaseSpeed; // use same speed for investigate/chase
        Debug.Log("Enemy heard a sound! Investigating " + soundPosition);
    }


}
