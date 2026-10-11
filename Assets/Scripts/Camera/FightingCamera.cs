
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FightingCamera : MonoBehaviour
{
    public Transform fighter1;
    public Transform fighter2;

    [Header("Camera Movement")]
    public float followSpeed = 5f;
    public float verticalFollowSpeed = 5f;

    [Header("Dynamic Zoom")]
    public float closeZoom = 4f;
    public float farZoom = 6f;
    public float zoomInSpeed = 3f;
    public float zoomOutSpeed = 12f;
    public float distanceForMaxZoom = 10f;

    [Header("Fighter Visibility")]
    public float verticalPadding = 2.5f;
    public float horizontalPadding = 2f;

    [Header("Stage Boundaries")]
    public float leftWall = -20f;
    public float rightWall = 20f;
    public float groundY = -4.5f;

    [Header("Ground Visibility")]
    public float groundMargin = 1f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (fighter1 == null || fighter2 == null)
            return;

        // Find the midpoint and horizontal separation.
        Vector3 midpoint =
            (fighter1.position + fighter2.position) / 2f;

        float distanceX = Mathf.Abs(
            fighter1.position.x - fighter2.position.x
        );

        // Normal zoom based on distance between fighters.
        float zoomAmount = Mathf.Clamp01(
            distanceX / distanceForMaxZoom
        );

        float normalZoom = Mathf.Lerp(
            closeZoom,
            farZoom,
            zoomAmount
        );

        // Find the highest visible point of either fighter.
        float fighterTop = Mathf.Max(
            GetFighterTop(fighter1),
            GetFighterTop(fighter2)
        );

        // The camera must fit from the ground area to the fighters.
        float bottomY = groundY - groundMargin;
        float topY = fighterTop + verticalPadding;

        float requiredVerticalSize = (topY - bottomY) / 2f;

        // Never let the camera view extend beyond the stage walls.
        float stageWidth = rightWall - leftWall;
        float maxZoomForStage = stageWidth / (2f * cam.aspect);

        // Limit horizontal zoom to the stage width.
        // Determine the zoom needed to fit both fighters horizontally.
        float requiredHorizontalSize =
            (distanceX / 2f + horizontalPadding) / cam.aspect;

        // Keep both fighters visible horizontally and vertically,
        // while limiting the camera to the stage width.
        float desiredZoom = Mathf.Min(
            Mathf.Max(
                normalZoom,
                requiredVerticalSize,
                requiredHorizontalSize
            ),
            maxZoomForStage
        );

        if (requiredHorizontalSize > maxZoomForStage)
        {
            Debug.LogWarning(
                "Fighters exceed the available camera width."
            );
        }

        // Zoom out quickly to avoid clipping during jumps.
        // Zoom in gradually to avoid sudden camera changes.
        float currentZoomSpeed =
            desiredZoom > cam.orthographicSize
            ? zoomOutSpeed
            : zoomInSpeed;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            desiredZoom,
            currentZoomSpeed * Time.deltaTime
        );

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Keep the bottom edge below the ground line.
        // Zooming out expands the view upward without losing the floor.
        float targetY = bottomY + halfHeight;

        // Keep the camera within the stage horizontally when possible.
        // Center the camera between both fighters.
        float targetX = (fighter1.position.x + fighter2.position.x) / 2f;

        // Keep the camera within the stage when possible.
        float minX = leftWall + halfWidth;
        float maxX = rightWall - halfWidth;

        if (minX <= maxX)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }
        else
        {
            targetX = (leftWall + rightWall) / 2f;
        }

        // Smooth movement.
        float newX = Mathf.Lerp(
            transform.position.x,
            targetX,
            followSpeed * Time.deltaTime
        );

        float newY = Mathf.Lerp(
            transform.position.y,
            targetY,
            verticalFollowSpeed * Time.deltaTime
        );

        transform.position = new Vector3(newX, newY, -20f);
    }

    float GetFighterTop(Transform fighter)
    {
        Renderer[] renderers =
            fighter.GetComponentsInChildren<Renderer>();

        float top = fighter.position.y;

        foreach (Renderer r in renderers)
        {
            if (r.enabled)
                top = Mathf.Max(top, r.bounds.max.y);
        }

        return top;
    }
}
