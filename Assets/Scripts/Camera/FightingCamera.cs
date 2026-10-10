
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FightingCamera : MonoBehaviour
{
    public Transform fighter1;
    public Transform fighter2;

    [Header("Camera Movement")]
    public float followSpeed = 5f;

    [Header("Dynamic Zoom")]
    public float closeZoom = 4f;
    public float farZoom = 6f;
    public float zoomSpeed = 3f;
    public float distanceForMaxZoom = 10f;

    [Header("Stage Boundaries")]
    public float leftWall = -10f;
    public float rightWall = 10f;
    public float groundY = -4.5f;

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

        // Midpoint between fighters
        Vector3 midpoint =
            (fighter1.position + fighter2.position) / 2f;

        // Calculate desired zoom from fighter distance
        float distance = Vector2.Distance(
            fighter1.position,
            fighter2.position
        );

        float zoomAmount = Mathf.Clamp01(
            distance / distanceForMaxZoom
        );

        float desiredZoom = Mathf.Lerp(
            closeZoom,
            farZoom,
            zoomAmount
        );

        // Never zoom out farther than the stage width allows
        float maxZoomForWidth =
            (rightWall - leftWall) / (2f * cam.aspect);

        desiredZoom = Mathf.Min(
            desiredZoom,
            maxZoomForWidth
        );

        // Smooth zoom
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            desiredZoom,
            zoomSpeed * Time.deltaTime
        );

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Keep camera inside the side walls
        float minX = leftWall + halfWidth;
        float maxX = rightWall - halfWidth;

        float targetX;

        if (minX > maxX)
        {
            targetX = (leftWall + rightWall) / 2f;
        }
        else
        {
            targetX = Mathf.Clamp(
                midpoint.x,
                minX,
                maxX
            );
        }

        // Keep the bottom of the screen at the ground
        float targetY = groundY + halfHeight;

        // Smooth horizontal movement
        float newX = Mathf.Lerp(
            transform.position.x,
            targetX,
            followSpeed * Time.deltaTime
        );

        // Apply camera position
        transform.position = new Vector3(
            newX,
            targetY,
            -10f
        );
    }
}
