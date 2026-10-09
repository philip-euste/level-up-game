using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    public void ApplyKnockback(
    int direction,
    float force,
    float upwardForce,
    float hitstunDuration)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(
            direction * force,
            upwardForce
        );

        // Extend hitstun to at least this attack's duration.
        hitstunEndTime = Mathf.Max(hitstunEndTime, Time.time + hitstunDuration);

    }

    public float maxHealth = 100f;
    public float currentHealth;

    public float hitstunDuration = 0.3f;
    private float hitstunEndTime = 0f;

    public float knockbackForce = 5f;
    public float knockbackUpwardForce = 1.5f;

    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        Debug.Log(
            gameObject.name + " HP: " +
            currentHealth + "/" + maxHealth
        );

        if (currentHealth <= 0f)
        {
            Debug.Log(gameObject.name + " is defeated!");
        }
        hitstunEndTime = Time.time + hitstunDuration;
    }

    public bool IsInHitstun()
    {
        return Time.time < hitstunEndTime;
    }
}