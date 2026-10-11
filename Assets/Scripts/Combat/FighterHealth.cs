
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float hitstunDuration = 0.3f;

    private float hitstunEndTime = 0f;

    public bool IsDefeated { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        IsDefeated = false;
    }

    public void TakeDamage(float damage)
    {
        if (IsDefeated || damage <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        Debug.Log(
            gameObject.name + " HP: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0f)
        {
            Defeat();
            return;
        }

        hitstunEndTime = Mathf.Max(
            hitstunEndTime,
            Time.time + hitstunDuration
        );
    }

    void Defeat()
    {
        IsDefeated = true;
        hitstunEndTime = 0f;

        Debug.Log(gameObject.name + " is defeated!");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public bool IsInHitstun()
    {
        return !IsDefeated && Time.time < hitstunEndTime;
    }

    public void ApplyKnockback(
        int direction,
        float force,
        float upwardForce,
        float attackHitstunDuration)
    {
        if (IsDefeated)
            return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            direction * force,
            upwardForce
        );

        hitstunEndTime = Mathf.Max(
            hitstunEndTime,
            Time.time + attackHitstunDuration
        );
    }
}
