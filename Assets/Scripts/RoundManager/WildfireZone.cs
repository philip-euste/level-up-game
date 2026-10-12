
using UnityEngine;

public class WildfireZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 2f;
    [SerializeField] private float damageInterval = 1f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer zoneRenderer;

    private ClimateEventManager climateEventManager;
    private float nextDamageTime;
    private bool isWildfireActive;


    private void Start()
    {
        climateEventManager =
            FindAnyObjectByType<ClimateEventManager>();

        if (zoneRenderer == null)
            zoneRenderer = GetComponent<SpriteRenderer>();

        // Hide the orange visual immediately.
        if (zoneRenderer != null)
            zoneRenderer.enabled = false;

        isWildfireActive = false;

        UpdateWildfireState();
    }


    private void Update()
    {
        UpdateWildfireState();
    }

    private void UpdateWildfireState()
    {
        bool shouldBeActive =
            climateEventManager != null &&
            climateEventManager.IsEventActive &&
            climateEventManager.ActiveEvent ==
                ClimateEventManager.ClimateEventType.Wildfire;

        if (shouldBeActive == isWildfireActive)
            return;

        isWildfireActive = shouldBeActive;

        if (zoneRenderer != null)
            zoneRenderer.enabled = isWildfireActive;

        nextDamageTime = 0f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isWildfireActive || Time.time < nextDamageTime)
            return;

        FighterHealth fighter =
            other.GetComponentInParent<FighterHealth>();

        if (fighter == null || fighter.IsDefeated)
            return;

        fighter.TakeDamage(damagePerSecond);
        nextDamageTime = Time.time + damageInterval;
    }
}
