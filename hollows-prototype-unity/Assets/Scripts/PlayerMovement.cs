using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float exhaustedMultiplier = 0.5f; // slow speed when energy is 0

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 5f;      // seconds of sprint
    [SerializeField] private float energyDrainRate = 1f; // energy per second
    [SerializeField] private float energyRegenRate = 0.5f; // regen per second when not sprinting

    [Header("References")]
    public GameObject player;
    public Transform energyBar; // the rectangle sprite (scale x will shrink)

    private Rigidbody2D rb;
    private Vector2 movementDirection;

    private float currentEnergy;
    private bool isSprinting;
    public bool isTriggering = false;
    public bool isHiding;
    // Remember to assign your sprint audio clip to this AudioSource in the Inspector.
    public AudioSource sprint;
    private EnemyAI enemyAI; // Reference to the EnemyAI script

    private bool wasSprintingLastFrame = false; // New variable to track the previous sprinting state

    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        currentEnergy = maxEnergy;
        // Make sure the audio source is stopped initially
        if (sprint != null)
        {
            sprint.Stop();
        }
        enemyAI = FindObjectOfType<EnemyAI>();
    }

    void Update()
    {
        // Get input direction
        movementDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Sprint input and energy check
        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentEnergy > 0f && movementDirection.magnitude > 0;

        // Energy logic
        if (isSprinting)
        {
            currentEnergy -= energyDrainRate * Time.deltaTime;
            currentEnergy = Mathf.Max(currentEnergy, 0f);
        }
        else
        {
            // Regenerate when not sprinting
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        }

        // Update energy bar UI (scale on X axis)
        if (energyBar != null)
        {
            float energyPercent = currentEnergy / maxEnergy;
            energyBar.localScale = new Vector3(1f, energyPercent, 1f);
        }

        // --- New Audio Logic ---
        // Check if the player is starting to sprint
        if (isSprinting && !wasSprintingLastFrame)
        {
            if (sprint != null)
            {
                sprint.Play();
            }
            enemyAI.HearSound(transform.position);
        }
        // Check if the player has stopped sprinting
        else if (!isSprinting && wasSprintingLastFrame)
        {
            if (sprint != null)
            {
                sprint.Stop();
            }
        }

        // Update the state for the next frame
        wasSprintingLastFrame = isSprinting;

    }

    void FixedUpdate()
    {
        float currentSpeed = movementSpeed;

        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }
        else if (currentEnergy <= 0f)
        {
            // Slower when exhausted
            currentSpeed *= exhaustedMultiplier;
        }

        rb.velocity = movementDirection * currentSpeed;
    }
}
