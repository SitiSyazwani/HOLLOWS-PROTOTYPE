using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectiveCollision : MonoBehaviour
{
    // Assign the specific collider you want to interact with in the Inspector.
    // This MUST be a Collider2D.
    public Collider2D targetCollider;

    void Start()
    {
        // 1. Get the collider attached to THIS GameObject (the one the script is on)
        Collider2D thisCollider = GetComponent<Collider2D>();

        if (thisCollider == null)
        {
            Debug.LogError("SelectiveCollision requires a Collider2D component on this GameObject.");
            return;
        }

        if (targetCollider == null)
        {
            Debug.LogWarning("Target Collider is not assigned. No selective collision will be set up.");
            // If no target is set, we still proceed to ignore everything else.
        }

        // 2. Ignore collisions with all other colliders initially
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        // This approach can be computationally expensive on 'Start' if you have hundreds of colliders.
        // For performance, using Unity's Layer-Based Collision is the recommended method.
        foreach (Collider2D otherCollider in allColliders)
        {
            // Skip the check if the other collider is THIS collider
            if (otherCollider == thisCollider)
            {
                continue;
            }

            // If the other collider is the assigned target, skip ignoring it for now.
            if (otherCollider == targetCollider)
            {
                continue;
            }

            // Ignore collision between this object and all other colliders
            Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
        }

        // 3. Ensure collision with the specific target collider is ENABLED
        if (targetCollider != null)
        {
            // Explicitly ensure collision is NOT ignored with the target
            Physics2D.IgnoreCollision(thisCollider, targetCollider, false);
            Debug.Log($"Collision enabled between {gameObject.name} and {targetCollider.gameObject.name}. All others ignored.");
        }
    }
}