using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Tilemap groundTilemap; // get the tilemap
    public float smoothSpeed = 0.125f;

    private Camera cam;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Get world-space bounds of the tilemap
        Bounds tileBounds = groundTilemap.localBounds;
        minBounds = tileBounds.min;
        maxBounds = tileBounds.max;
    }

    void LateUpdate()
    {
        if (player == null) return;

        //get camera to follow player
        Vector3 targetPosition = player.position;

        float camHeight = cam.orthographicSize;
        float camWidth = cam.aspect * camHeight;

        // Clamp camera so it stays inside the tilemap
        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        Vector3 smoothPos = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
        transform.position = smoothPos;
    }
}
