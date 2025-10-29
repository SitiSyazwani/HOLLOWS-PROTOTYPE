// IMPORTANT: This script requires the Player, Walls, Doors, and Furniture
// to all be on the SAME Sorting Layer (e.g., "Wall") in the Unity Inspector.

using UnityEngine;

//---------------------------------------------------------------------------------
// Description	: Dynamically controls the SpriteRenderer's sorting order 
//               based on the player's vertical (Y) position in the world, 
//               with an offset to ensure the player is always drawn over the ground.
//---------------------------------------------------------------------------------
public class DynamicSorting : MonoBehaviour
{
    // Offset added to the calculated order. If the Ground Tilemap is on the 
    // SAME Sorting Layer as the Player and is set to Order 0, a positive offset
    // ensures the Player is always drawn on top of the Ground.
    [SerializeField] private int groundOffset = 100;

    private SpriteRenderer playerRenderer;

    void Start()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer == null)
        {
            Debug.LogError("DynamicSorting script requires a SpriteRenderer component on the same GameObject!");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        // 1. Calculate the dynamic depth: (Y position * -100)
        // Y = 5.0 -> Order = -500 (Behind Wall)
        // Y = -5.0 -> Order = 500 (In Front of Wall)
        int dynamicDepth = (int)(transform.position.y * -100);

        // 2. Add the ground offset to ensure the player is always visually over the floor.
        // This ensures the minimum sorting order is high enough to clear the ground.
        playerRenderer.sortingOrder = dynamicDepth + groundOffset;
    }
}
