using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float zoomLevel = 5f;

    public Transform player;
    public Tilemap groundTilemap; // get the tilemap

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

        // Set the camera's orthographic size based on the zoom level.
        cam.orthographicSize = zoomLevel;

        // Get camera to follow player
        Vector3 targetPosition = player.position;

        // Clamp camera so it stays inside the tilemap
        float camHeight = cam.orthographicSize;
        float camWidth = cam.aspect * camHeight;

        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        Vector3 smoothPos = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
        transform.position = smoothPos;
    }
}
//using UnityEngine;
//using UnityEngine.Tilemaps;

//public class CameraFollow : MonoBehaviour
//{
//    [Header("Camera Settings")]
//    [SerializeField] private float smoothSpeed = 0.125f;
//    [SerializeField] private float zoomLevel = 5f;

//    public Transform player;
//    public Tilemap groundTilemap;

//    private Camera cam;
//    private Vector3 minBounds;
//    private Vector3 maxBounds;
//    private Animator animator;
//    private Vector3 originalPositionBeforeShake;

//    void Start()
//    {
//        cam = GetComponent<Camera>();
//        animator = GetComponent<Animator>();

//        Bounds tileBounds = groundTilemap.localBounds;
//        minBounds = tileBounds.min;
//        maxBounds = tileBounds.max;
//    }

//    void LateUpdate()
//    {
//        if (player == null) return;

//        cam.orthographicSize = zoomLevel;

//        Vector3 targetPosition = player.position;
//        float camHeight = cam.orthographicSize;
//        float camWidth = cam.aspect * camHeight;

//        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
//        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

//        Vector3 smoothPos = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);

//        // Check if we're currently shaking
//        //bool isShaking = animator != null &&
//        //                animator.GetCurrentAnimatorStateInfo(0).IsName("CameraShake") &&
//        //                animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;

//        //if (isShaking)
//        //{
//        //    // Let the animation control position during shake
//        //    // But store where we should be for after shake
//        //    originalPositionBeforeShake = smoothPos;
//        //}
//        //else
//        //{
//        //    // Force camera back to follow position when not shaking
//        //    transform.position = smoothPos;
//        //}
//    }
//}
