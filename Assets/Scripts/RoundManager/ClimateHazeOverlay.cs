
using UnityEngine;
using UnityEngine.UI;

public class ClimateHazeOverlay : MonoBehaviour
{
    [Header("References")]
    public ClimateEventManager climateEventManager;
    public Image hazeImage;

    [Header("Haze Settings")]
    [Range(0f, 0.4f)]
    public float hazeOpacity = 0.4f;

    public float fadeSpeed = 1f;

    private float targetOpacity = 0f;

    void Start()
    {
        if (climateEventManager == null)
        {
            climateEventManager =
                FindAnyObjectByType<ClimateEventManager>();
        }

        if (hazeImage == null)
        {
            hazeImage = GetComponent<Image>();
        }

        if (hazeImage != null)
        {
            hazeImage.color = new Color(0f, 0f, 0f, 0f);
        }
    }

    void Update()
    {
        if (climateEventManager == null || hazeImage == null)
            return;

        Color tint = GetClimateColor();

        targetOpacity = climateEventManager.IsEventActive
            ? hazeOpacity
            : 0f;

        Color currentColor = hazeImage.color;

        float newAlpha = Mathf.MoveTowards(
            currentColor.a,
            targetOpacity,
            fadeSpeed * Time.deltaTime
        );

        hazeImage.color = new Color(
            tint.r,
            tint.g,
            tint.b,
            newAlpha
        );
    }

    Color GetClimateColor()
    {
        if (!climateEventManager.IsEventActive)
            return Color.clear;

        switch (climateEventManager.ActiveEvent)
        {
            case ClimateEventManager.ClimateEventType.Smog:
                return new Color(0.25f, 0.65f, 0.20f);

            case ClimateEventManager.ClimateEventType.Typhoon:
                return new Color(0.45f, 0.48f, 0.52f);

            case ClimateEventManager.ClimateEventType.Flood:
                return new Color(0.10f, 0.45f, 0.90f);

            case ClimateEventManager.ClimateEventType.Wildfire:
                return new Color(0.90f, 0.16f, 0.08f);

            default:
                return Color.clear;
        }
    }
}
