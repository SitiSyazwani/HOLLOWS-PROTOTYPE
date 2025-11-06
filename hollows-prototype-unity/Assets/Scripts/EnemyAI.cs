using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    //[Header("Animation Shake")]
    //public Animator cameraAnimator;
    //private bool wasShaking = false;
    //[Header("Exclamation Mark UI")]
    //public GameObject exclamationTopLeft;
    //public GameObject exclamationTopRight;
    //public GameObject exclamationBottomLeft;
    //public GameObject exclamationBottomRight;

    //[Header("Proximity Alert")]
    //public float proximityRange = 8f;
    //public float detectionAngle = 45f; // Degrees for corner detection

    public Transform player;
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 30f;
    public float investigateTimeout = 10f; // seconds before giving up if no chase
    public float patrolWaitTime = 10f;      // How long to wait at a patrol point
    public float chaseRange = 20f;         // how close player must be to trigger chase
    public float catchDistance = 0.5f;     // Distance threshold for "caught"
    public Animator animator;

    [Header("Exclamation Marks")]
    public GameObject exclamationTopLeft;
    public GameObject exclamationTopRight;
    public GameObject exclamationBottomLeft;
    public GameObject exclamationBottomRight;
    public float proximityRange = 8f;


    [Header("VFX Settings")]
    public GameObject redOverlay; // Assign your red overlay panel from Canvas in Inspector
    public float pulseSpeed = 2f;
    public float maxIntensity = 0.7f;
    public float minIntensity = 0.3f;
    //private bool hasShaken = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] roamingSounds; // Array of sounds to play randomly while patrolling
    public AudioClip proximitySound; // Sound to play when close to player
    public float roamingSoundMinInterval = 5f;
    public float roamingSoundMaxInterval = 10f;
    public float proximityAudioRange = 8f; // Distance at which proximity sound plays
    
    private float roamingSoundTimer = 0f;
    private float nextRoamingSoundTime;
    private bool isPlayingProximitySound = false;

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
    [SerializeField] private WinLose winlose;

    // VFX tracking
    private bool wasChasing = false;
    private float pulseTimer = 0f;
    private Image overlayImage;

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

        // Get overlay image reference
        if (redOverlay != null)
        {
            overlayImage = redOverlay.GetComponent<Image>();
            if (overlayImage == null)
            {
                Debug.LogWarning("Red Overlay GameObject doesn't have an Image component!");
            }
        }
        // Ensure UI elements are properly positioned
        if (exclamationTopLeft != null)
        {
            RectTransform rect = exclamationTopLeft.GetComponent<RectTransform>();
            if (rect.parent.GetComponent<Canvas>() == null)
            {
                Debug.LogError("Exclamation marks must be children of Canvas!");
            }
        }

        // Initialize audio system
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Set first roaming sound time
        nextRoamingSoundTime = Random.Range(roamingSoundMinInterval, roamingSoundMaxInterval);

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
        UpdateVFX(); // Added VFX update
        UpdateAudio(); // Added audio update
        //UpdateCameraShake();
        UpdateExclamationMark();

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

    private void UpdateAudio()
    {
        if (audioSource == null || player == null) return;

        // Handle proximity sound (plays once when entering Chase state / red screen)
        if (currentState == State.Chase)
        {
            if (!isPlayingProximitySound && proximitySound != null)
            {
                audioSource.PlayOneShot(proximitySound);
                isPlayingProximitySound = true;
                Debug.Log("Playing proximity sound - chase started!");
            }
        }
        else
        {
            // Reset flag when not chasing anymore
            isPlayingProximitySound = false;
        }

        // Handle roaming sounds (only during Patrol state)
        if (currentState == State.Patrol && roamingSounds != null && roamingSounds.Length > 0)
        {
            roamingSoundTimer += Time.deltaTime;

            if (roamingSoundTimer >= nextRoamingSoundTime)
            {
                // Play random roaming sound
                AudioClip randomSound = roamingSounds[Random.Range(0, roamingSounds.Length)];
                audioSource.PlayOneShot(randomSound);
                
                // Reset timer and set next interval
                roamingSoundTimer = 0f;
                nextRoamingSoundTime = Random.Range(roamingSoundMinInterval, roamingSoundMaxInterval);
                
                Debug.Log("Playing roaming sound. Next sound in: " + nextRoamingSoundTime + " seconds");
            }
        }
        else
        {
            // Reset timer when not patrolling
            roamingSoundTimer = 0f;
        }
    }

    //private void UpdateVFX()
    //{
    //    bool isChasing = (currentState == State.Chase);

    //    // Only update if chase state changed
    //    if (isChasing != wasChasing)
    //    {
    //        if (redOverlay != null)
    //        {
    //            redOverlay.SetActive(isChasing);
    //            Debug.Log("VFX: Red overlay " + (isChasing ? "ON" : "OFF"));

    //            // Reset pulse timer when starting chase
    //            if (isChasing)
    //                pulseTimer = 0f;
    //        }
    //        else
    //        {
    //            Debug.LogWarning("Red overlay not assigned in Inspector!");
    //        }

    //        wasChasing = isChasing;
    //    }

    //    // Update VFX effects when chasing
    //    if (isChasing && redOverlay != null && overlayImage != null)
    //    {
    //        UpdateChaseVFX();
    //    }
    //}

    //private void UpdateChaseVFX()
    //{
    //    // Calculate distance-based intensity
    //    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    //    float distanceIntensity = 1f - Mathf.Clamp01(distanceToPlayer / chaseRange);

    //    // Update pulse timer
    //    pulseTimer += Time.deltaTime * pulseSpeed;

    //    // Calculate pulse effect (sin wave between -1 and 1, normalized to 0-1)
    //    float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0 to 1

    //    // Combine distance intensity with pulse effect
    //    float baseAlpha = Mathf.Lerp(minIntensity, maxIntensity, distanceIntensity);
    //    float pulsedAlpha = baseAlpha * (0.8f + 0.2f * pulse); // Pulse modulates alpha by 20%

    //    // Apply to overlay
    //    Color currentColor = overlayImage.color;
    //    currentColor.a = pulsedAlpha;

    //    // Optional: Add subtle color variation based on pulse
    //    float colorPulse = Mathf.Sin(pulseTimer * 0.7f) * 0.1f;
    //    currentColor.r = Mathf.Clamp01(0.8f + colorPulse);
    //    currentColor.g = Mathf.Clamp01(0.2f - colorPulse * 0.5f);
    //    currentColor.b = Mathf.Clamp01(0.2f - colorPulse * 0.5f);

    //    overlayImage.color = currentColor;

    //    // Optional: Add heartbeat-like intense pulses occasionally
    //    if (pulseTimer % 6.28f < 0.1f) // Every ~? seconds
    //    {
    //        currentColor.a = Mathf.Min(currentColor.a + 0.2f, 0.9f);
    //        overlayImage.color = currentColor;
    //    }
    //}
    private void UpdateVFX()
    {
        bool isChasing = (currentState == State.Chase);

        // Only update if chase state changed
        if (isChasing != wasChasing)
        {
            if (redOverlay != null)
            {
                // SIMPLE: Just use SetActive but ensure it doesn't break UI
                redOverlay.SetActive(isChasing);
                Debug.Log("VFX: Red overlay " + (isChasing ? "ON" : "OFF"));

                // Reset pulse timer when starting chase
                if (isChasing)
                    pulseTimer = 0f;
            }
            wasChasing = isChasing;
        }

        // Update VFX effects when chasing
        if (isChasing && redOverlay != null && overlayImage != null)
        {
            UpdateChaseVFX();
        }
    }

    private void UpdateChaseVFX()
    {
        // Calculate distance-based intensity
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceIntensity = 1f - Mathf.Clamp01(distanceToPlayer / chaseRange);

        // Update pulse timer
        pulseTimer += Time.deltaTime * pulseSpeed;

        // Calculate pulse effect
        float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0 to 1

        // Combine distance intensity with pulse effect
        float baseAlpha = Mathf.Lerp(minIntensity, maxIntensity, distanceIntensity);
        float pulsedAlpha = baseAlpha * (0.8f + 0.2f * pulse);

        // Apply to overlay
        Color currentColor = overlayImage.color;
        currentColor.a = pulsedAlpha;

        // Color effects
        float colorPulse = Mathf.Sin(pulseTimer * 0.7f) * 0.1f;
        currentColor.r = Mathf.Clamp01(0.8f + colorPulse);
        currentColor.g = Mathf.Clamp01(0.2f - colorPulse * 0.5f);
        currentColor.b = Mathf.Clamp01(0.2f - colorPulse * 0.5f);

        overlayImage.color = currentColor;
    }

    //private void UpdateCameraShake()
    //{
    //    if (cameraAnimator == null) return;

    //    float distance = Vector3.Distance(transform.position, player.position);
    //    bool isInProximity = (distance < 8f && currentState != State.Chase);

    //    // One-time shake when entering proximity
    //    if (isInProximity && !hasShaken)
    //    {
    //        cameraAnimator.Play("CameraShake", -1, 0f);
    //        hasShaken = true;
    //        Debug.Log("ONE-TIME SHAKE!");
    //    }

    //    // Reset when far away
    //    if (distance > 12f)
    //    {
    //        hasShaken = false;
    //    }
    //}
    private void UpdateExclamationMark()
    {
        if (player == null || Camera.main == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool showExclamation = (distance < proximityRange && currentState != State.Chase);

        // Hide all first
        SetAllExclamationMarks(false);

        if (showExclamation)
        {
            // Show only the correct corner mark
            ShowCorrectCornerMark();
        }
    }

    private void ShowCorrectCornerMark()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0.5f && viewportPos.y < 0.5f)
        {
            // Monster is bottom-left of screen
            if (exclamationBottomLeft != null)
                exclamationBottomLeft.SetActive(true);
        }
        else if (viewportPos.x >= 0.5f && viewportPos.y < 0.5f)
        {
            // Monster is bottom-right of screen
            if (exclamationBottomRight != null)
                exclamationBottomRight.SetActive(true);
        }
        else if (viewportPos.x < 0.5f && viewportPos.y >= 0.5f)
        {
            // Monster is top-left of screen
            if (exclamationTopLeft != null)
                exclamationTopLeft.SetActive(true);
        }
        else
        {
            // Monster is top-right of screen
            if (exclamationTopRight != null)
                exclamationTopRight.SetActive(true);
        }
    }

    private void SetAllExclamationMarks(bool active)
    {
        if (exclamationTopLeft != null) exclamationTopLeft.SetActive(active);
        if (exclamationTopRight != null) exclamationTopRight.SetActive(active);
        if (exclamationBottomLeft != null) exclamationBottomLeft.SetActive(active);
        if (exclamationBottomRight != null) exclamationBottomRight.SetActive(active);
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
        agent.speed = patrolSpeed;

        if (!isWaitingAtPatrolPoint)
        {
            //Debug.Log("(!isWaitingAtPatrolPoint)");
            // 1. Check if we reached the patrol point
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                //Debug.Log("reached patrol point");
                // Reached the destination, start the wait
                isWaitingAtPatrolPoint = true;
                patrolWaitTimer = patrolWaitTime;
                agent.isStopped = true;
                //Debug.Log("Enemy reached patrol point. Starting " + patrolWaitTime + " second wait.");
            }
        }

        // 2. If currently waiting at a patrol point
        if (isWaitingAtPatrolPoint)
        {
            //Debug.Log("(isWaitingAtPatrolPoint)");
            patrolWaitTimer -= Time.deltaTime;

            if (patrolWaitTimer <= 0f)
            {
                // Wait is over, transition back to moving
                currentState = State.Patrol;
                isWaitingAtPatrolPoint = false;
                agent.isStopped = false;
                GoToNextPatrolPoint();
                //Debug.Log("Patrol wait finished, heading to next point.");
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