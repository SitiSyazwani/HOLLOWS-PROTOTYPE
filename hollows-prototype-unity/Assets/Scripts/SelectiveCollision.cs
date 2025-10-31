using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectiveCollision : MonoBehaviour
{
    // Assign the specific colliders you want to interact with in the Inspector.
    // An array (Collider2D[]) works just like a list for Inspector drag-and-drop.
    public Collider2D[] targetColliders; // Changed from List<Collider2D> to Collider2D[]
    public Collider2D thisCollider;

    void Start()
    {
        // 1. Get the collider attached to THIS GameObject
        //Collider2D thisCollider = GetComponent<Collider2D>();

        if (thisCollider == null)
        {
            Debug.LogError("SelectiveCollisionArray requires a Collider2D component on this GameObject.");
            return;
        }

        if (targetColliders == null || targetColliders.Length == 0)
        {
            Debug.LogWarning("Target Colliders array is empty or null. All collisions will be ignored except for the current object itself.");
        }

        // 2. Ignore collisions with all other colliders initially
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        // Use a set for quick lookup of targets for better performance inside the loop
        // The array is converted to a HashSet for fast O(1) lookups.
        HashSet<Collider2D> targetSet = new HashSet<Collider2D>(targetColliders);

        foreach (Collider2D otherCollider in allColliders)
        {
            // Skip if the other collider is THIS collider
            if (otherCollider == thisCollider)
            {
                continue;
            }

            // Check if the other collider is one of the assigned targets
            if (targetSet.Contains(otherCollider))
            {
                // If it's a target, ensure collision is NOT ignored
                Physics2D.IgnoreCollision(thisCollider, otherCollider, false);
                Debug.Log($"Collision **enabled** between {gameObject.name} and {otherCollider.gameObject.name}.");
            }
            else
            {
                // If it's NOT a target, ignore collision
                Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
            }
        }

        if (targetColliders != null && targetColliders.Length > 0)
        {
            Debug.Log($"Collision setup complete for {gameObject.name}. {targetColliders.Length} targets enabled. All others ignored.");
        }
    }
}