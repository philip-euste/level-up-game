using System.Collections.Generic;
using UnityEngine;

public class WildfireZone : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 2f;
    [SerializeField] private float damageInterval = 1f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer zoneRenderer;

    private ClimateEventManager climateEventManager;
    private bool isWildfireActive;

    private readonly Dictionary<FighterHealth, float> nextDamageTimes =
        new Dictionary<FighterHealth, float>();

    private void Start()
    {
        climateEventManager =
            FindAnyObjectByType<ClimateEventManager>();

        if (zoneRenderer == null)
            zoneRenderer = GetComponent<SpriteRenderer>();

        if (zoneRenderer != null)
            zoneRenderer.enabled = false;

        UpdateWildfireState();
    }

    private void Update()
    {
        UpdateWildfireState();

        if (!isWildfireActive)
            return;

        float interval = Mathf.Max(damageInterval, 0.01f);

        List<FighterHealth> fighters =
            new List<FighterHealth>(nextDamageTimes.Keys);

        foreach (FighterHealth fighter in fighters)
        {
            if (fighter == null || fighter.IsDefeated)
            {
                nextDamageTimes.Remove(fighter);
                continue;
            }

            if (Time.time >= nextDamageTimes[fighter])
            {
                fighter.TakeDamage(damagePerSecond);
                nextDamageTimes[fighter] = Time.time + interval;
            }
        }
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

        nextDamageTimes.Clear();

        if (isWildfireActive)
            RegisterFightersInsideZone();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FighterHealth fighter =
            other.GetComponentInParent<FighterHealth>();

        if (fighter == null || fighter.IsDefeated)
            return;

        if (!nextDamageTimes.ContainsKey(fighter))
            nextDamageTimes[fighter] = Time.time;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        FighterHealth fighter =
            other.GetComponentInParent<FighterHealth>();

        if (fighter != null)
            nextDamageTimes.Remove(fighter);
    }

    private void RegisterFightersInsideZone()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();

        if (zoneCollider == null)
            return;

        FighterHealth[] fighters =
            FindObjectsByType<FighterHealth>(FindObjectsSortMode.None);

        foreach (FighterHealth fighter in fighters)
        {
            if (fighter == null || fighter.IsDefeated)
                continue;

            Collider2D[] colliders =
                fighter.GetComponentsInChildren<Collider2D>();

            foreach (Collider2D fighterCollider in colliders)
            {
                if (fighterCollider == zoneCollider ||
                    fighterCollider.isTrigger)
                    continue;

                if (zoneCollider.bounds.Intersects(fighterCollider.bounds))
                {
                    if (!nextDamageTimes.ContainsKey(fighter))
                        nextDamageTimes[fighter] = Time.time;

                    break;
                }
            }
        }
    }
}