
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public GameObject owner;
    public AttackData attackData;

    private ClimateEventManager climateEventManager;
    private float elapsedTime;

    void Start()
    {
        if (attackData == null)
        {
            Debug.LogError("AttackHitbox has no AttackData!", this);
            Destroy(gameObject);
            return;
        }

        climateEventManager =
            FindAnyObjectByType<ClimateEventManager>();
    }

    void Update()
    {
        if (attackData == null)
            return;

        float timeMultiplier =
            climateEventManager != null &&
            climateEventManager.IsSmogActive
                ? 2f
                : 1f;

        elapsedTime += Time.deltaTime / timeMultiplier;

        if (elapsedTime >= attackData.activeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null || attackData == null)
        {
            Destroy(gameObject);
            return;
        }

        FighterHealth ownerHealth =
            owner.GetComponent<FighterHealth>();

        if (ownerHealth != null && ownerHealth.IsDefeated)
        {
            Destroy(gameObject);
            return;
        }

        FighterController ownerController =
            owner.GetComponent<FighterController>();

        if (ownerController != null &&
            !ownerController.controlsEnabled)
        {
            Destroy(gameObject);
            return;
        }

        FighterHealth target =
            other.GetComponentInParent<FighterHealth>();

        if (target == null ||
            target.gameObject == owner ||
            target.IsDefeated)
        {
            return;
        }

        FighterController targetController =
            target.GetComponent<FighterController>();

        if (targetController != null &&
            !targetController.controlsEnabled)
        {
            Destroy(gameObject);
            return;
        }

        int direction =
            owner.transform.position.x < target.transform.position.x
                ? 1
                : -1;

        float damage = attackData.damage;

        FighterController defender =
            target.GetComponent<FighterController>();

        if (defender != null &&
            defender.ShouldAutoBlock(owner.transform.position.x))
        {
            damage *= 0.1f;
        }

        target.TakeDamage(damage);

        target.ApplyKnockback(
            direction,
            attackData.knockbackForce,
            attackData.upwardForce,
            attackData.hitstunDuration
        );

        Destroy(gameObject);
    }
}
