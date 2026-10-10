using UnityEngine;

public class FighterController : MonoBehaviour
{
    [Header("Movement Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;

    [Header("Attack Controls")]
    public KeyCode lightPunchKey = KeyCode.U;
    public KeyCode lightKickKey = KeyCode.I;
    public KeyCode heavyPunchKey = KeyCode.J;
    public KeyCode heavyKickKey = KeyCode.K;

    public AttackData lightAttack = new AttackData
    {
        attackName = "Light",
        damage = 5f,
        knockbackForce = 3f,
        upwardForce = 0.5f,
        hitstunDuration = 0.2f,
        cooldown = 0.25f,
        startupTime = 0.05f,
        activeTime = 0.1f,
        recoveryTime = 0.1f,
    };

    public AttackData heavyAttack = new AttackData
    {
        attackName = "Heavy",
        damage = 15f,
        knockbackForce = 8f,
        upwardForce = 2f,
        hitstunDuration = 0.6f,
        cooldown = 0.8f,
        startupTime = 0.25f,
        activeTime = 0.15f,
        recoveryTime = 0.4f,
    };

    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private bool isGrounded;
    private int facingDirection = 1;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private float currentMoveInput;

    private Rigidbody2D rb;
    public GameObject attackHitbox;
    private SpriteRenderer spriteRenderer;

    private FighterHealth health;

    
    void Start()
    {
        health = GetComponent<FighterHealth>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        if (health != null && health.IsInHitstun())
        {
            currentMoveInput = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float move = 0f;

        if (Input.GetKey(leftKey))
            move = -1f;

        if (Input.GetKey(rightKey))
            move = 1f;

        currentMoveInput = move;

        if (move != 0f)
        {
            facingDirection = (int)Mathf.Sign(move);
            spriteRenderer.flipX = facingDirection == -1;
        }

        if (!isAttacking && Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(lightPunchKey) ||
                Input.GetKeyDown(lightKickKey))
            {
                StartCoroutine(PerformAttack(lightAttack));
            }
            else if (Input.GetKeyDown(heavyPunchKey) ||
                    Input.GetKeyDown(heavyKickKey))
            {
                StartCoroutine(PerformAttack(heavyAttack));
            }
        }

        rb.linearVelocity = new Vector2(
            isAttacking ? 0f : move * moveSpeed,
            rb.linearVelocity.y
        );

        if (Input.GetKeyDown(jumpKey) && isGrounded && !isAttacking)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
    

    // FOR JUMPING AND GROUND CHECKING:
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision) 
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void Attack(AttackData data)
    {
        Vector3 attackPosition = transform.position;
        attackPosition.x += 1f * facingDirection;

        GameObject hitbox = Instantiate(
            attackHitbox,
            attackPosition,
            Quaternion.identity
        );

        AttackHitbox hitboxScript = hitbox.GetComponent<AttackHitbox>();

        if (hitboxScript != null)
        {
            hitboxScript.owner = gameObject;
            hitboxScript.attackData = data;
        }
    }

    System.Collections.IEnumerator PerformAttack(AttackData data)
    {
        isAttacking = true;

        // Startup
        yield return new WaitForSeconds(data.startupTime);

        // Cancel if interrupted during startup
        if (health != null && health.IsInHitstun())
        {
            isAttacking = false;
            yield break;
        }

        // Active
        Attack(data);
        yield return new WaitForSeconds(data.activeTime);

        // Recovery
        yield return new WaitForSeconds(data.recoveryTime);

        nextAttackTime = Time.time + data.cooldown;
        isAttacking = false;
    }

    public bool ShouldAutoBlock(float attackerX)
    {
        float directionToAttacker =
            Mathf.Sign(attackerX - transform.position.x);

        // Standing still: autoblock.
        if (Mathf.Approximately(currentMoveInput, 0f))
            return true;

        // Moving away from the attacker: autoblock.
        return Mathf.Sign(currentMoveInput) == -directionToAttacker;
    }
}