using UnityEngine;

public class manualsort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    // Multiplier to give fine-grained sorting. Higher number = more depth levels.
    private const int SORT_RESOLUTION = 100; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("ManualYSort requires a SpriteRenderer component.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        // Calculate the new sorting order.
        // We multiply Y position by -SORT_RESOLUTION.
        // Lower Y (further down the screen) results in a HIGHER Order in Layer (draw on top).
        int newOrder = Mathf.RoundToInt(transform.position.y * -SORT_RESOLUTION);
        
        // Apply the new order to the Sprite Renderer.
        spriteRenderer.sortingOrder = newOrder;
    }
}