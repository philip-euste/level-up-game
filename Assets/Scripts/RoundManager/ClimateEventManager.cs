
using UnityEngine;

public class ClimateEventManager : MonoBehaviour
{
    public enum ClimateEventType
    {
        Typhoon,
        Smog,
        Flood,
        Wildfire
    }

    [Header("Event Timing")]
    public float eventDuration = 30f;
    public float inactiveDuration = 30f;

    [Header("Typhoon")]
    public float windDirectionChangeInterval = 15f;
    public float withWindSpeedMultiplier = 1.5f;
    public float againstWindSpeedMultiplier = 0.5f;

    [Header("Flood")]
    public float floodAcceleration = 8f;
    public float floodDeceleration = 2f;

    public bool IsEventActive { get; private set; }

    public bool IsTyphoonActive =>
        IsEventActive &&
        activeEvent == ClimateEventType.Typhoon;

    public bool IsSmogActive =>
        IsEventActive &&
        activeEvent == ClimateEventType.Smog;

    public bool IsFloodActive =>
        IsEventActive &&
        activeEvent == ClimateEventType.Flood;

    public ClimateEventType ActiveEvent => activeEvent;

    // -1 = left, 1 = right.
    public int WindDirection { get; private set; } = 1;

    private ClimateEventType activeEvent;
    private ClimateEventType previousEvent;
    private bool hasPreviousEvent;

    private float nextEventStartTime;
    private float nextWindChangeTime;
    private float eventEndTime;

    void Start()
    {
        // Start with 30 seconds of free time.
        IsEventActive = false;
        nextEventStartTime = Time.time + inactiveDuration;

        Debug.Log("Free time started.");
    }

    void Update()
    {
        if (IsEventActive)
        {
            if (Time.time >= eventEndTime)
            {
                EndEvent();
                return;
            }

            if (IsTyphoonActive &&
                Time.time >= nextWindChangeTime)
            {
                ChangeWindDirection();
            }

            return;
        }

        if (Time.time >= nextEventStartTime)
        {
            StartRandomEvent();
        }
    }


    void StartRandomEvent()
    {
        ClimateEventType[] availableEvents =
        {
            ClimateEventType.Typhoon,
            ClimateEventType.Smog,
            ClimateEventType.Flood,
            ClimateEventType.Wildfire
        };

        // Exclude the previous event to prevent repeats.
        int choiceCount = hasPreviousEvent
            ? availableEvents.Length - 1
            : availableEvents.Length;

        int choice = Random.Range(0, choiceCount);

        if (hasPreviousEvent &&
            availableEvents[choice] == previousEvent)
        {
            choice = availableEvents.Length - 1;
        }

        activeEvent = availableEvents[choice];

        previousEvent = activeEvent;
        hasPreviousEvent = true;

        IsEventActive = true;
        eventEndTime = Time.time + eventDuration;

        WindDirection = Random.value < 0.5f ? -1 : 1;
        nextWindChangeTime =
            Time.time + windDirectionChangeInterval;

        Debug.Log("Climate event started: " + activeEvent);
    }


    void ChangeWindDirection()
    {
        WindDirection *= -1;

        nextWindChangeTime =
            Time.time + windDirectionChangeInterval;

        Debug.Log("Typhoon wind direction: " + WindDirection);
    }

    void EndEvent()
    {
        Debug.Log("Climate event ended: " + activeEvent);

        IsEventActive = false;
        nextEventStartTime = Time.time + inactiveDuration;

        Debug.Log("Free time started.");
    }

    public float GetMovementSpeedMultiplier(float movementDirection)
    {
        if (!IsTyphoonActive ||
            Mathf.Approximately(movementDirection, 0f))
        {
            return 1f;
        }

        return Mathf.Sign(movementDirection) == WindDirection
            ? withWindSpeedMultiplier
            : againstWindSpeedMultiplier;
    }
}
