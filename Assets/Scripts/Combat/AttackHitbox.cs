using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public GameObject owner;
    public AttackData attackData;

    void Start()
    {
        Destroy(gameObject, attackData.activeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        FighterHealth target =
            other.GetComponentInParent<FighterHealth>();

        if (target == null || target.gameObject == owner)
            return;

        int direction =
            owner.transform.position.x < target.transform.position.x
            ? 1 : -1;

        target.TakeDamage(attackData.damage);
        target.ApplyKnockback(direction, attackData.knockbackForce,
                              attackData.upwardForce,
                              attackData.hitstunDuration);

        Destroy(gameObject);
    }
}