using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public GameObject owner;
    public AttackData attackData;

    void Start()
    {
        if (attackData == null)
        {
            Debug.LogError("AttackHitbox has no AttackData!", this);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject, attackData.activeTime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        FighterHealth ownerHealth =
            owner != null ? owner.GetComponent<FighterHealth>() : null;

        if (ownerHealth != null && ownerHealth.IsDefeated)
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

        int direction =
            owner.transform.position.x < target.transform.position.x
            ? 1 : -1;

        float damage = attackData.damage;

        FighterController defender =
            target.GetComponent<FighterController>();

        if (defender != null &&
            defender.ShouldAutoBlock(owner.transform.position.x))
        {
            damage *= 0.1f; // Autoblock reduces damage by 90%.
        }

        target.TakeDamage(damage);

        target.ApplyKnockback(direction, attackData.knockbackForce,
                              attackData.upwardForce,
                              attackData.hitstunDuration);

        Destroy(gameObject);
    }
}