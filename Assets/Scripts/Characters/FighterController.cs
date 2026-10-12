
using UnityEngine;

public class FighterController : MonoBehaviour
{
    public enum ControlPreset
    {
        Player1,
        Player2
    }

    [Header("Control Preset")]
    public ControlPreset controlPreset = ControlPreset.Player1;

    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;

    private KeyCode lightPunchKey;
    private KeyCode lightKickKey;
    private KeyCode heavyPunchKey;
    private KeyCode heavyKickKey;

    [Header("Facing Direction")]
    // 1 = face right, -1 = face left
    public int facingDirection = 1;

    [Header("Animation Settings")]
    public Animator animator;
    public string walkingParameter = "IsWalking";

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

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

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.05f;
    public LayerMask wallLayer;

    [Header("Fighter Collision")]
    public LayerMask fighterLayer;
    public float fighterCheckDistance = 0.1f;

    private BoxCollider2D bodyCollider;
    private bool isGrounded;
    private float cooldownRemaining = 0f;
    private bool isAttacking = false;
    private float currentMoveInput;
    private bool isJumping = false;
    private bool canMoveInAir = false;
    private float jumpHorizontalVelocity = 0f;
    private float currentHorizontalVelocity;

    private Rigidbody2D rb;
    public GameObject attackHitbox;
    private SpriteRenderer spriteRenderer;
    private FighterHealth health;
    private Vector3 startingPosition;
    public bool controlsEnabled = true;

    private MatchManager matchManager;
    private ClimateEventManager climateEventManager;

    void OnValidate()
    {
        jumpForce = 12f;
    }
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<BoxCollider2D>();
        matchManager = FindAnyObjectByType<MatchManager>();
        climateEventManager = FindAnyObjectByType<ClimateEventManager>();


        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Set the character's starting direction.
        FaceOpponent();
        ApplyFacingDirection();

        UpdateWalkingAnimation(false);
    }

    void Awake()
    {
        health = GetComponent<FighterHealth>();
        rb = GetComponent<Rigidbody2D>();
        startingPosition = transform.position;
        ApplyControlPreset();
    }

    void ApplyControlPreset()
    {
        if (controlPreset == ControlPreset.Player1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;

            lightPunchKey = KeyCode.U;
            lightKickKey = KeyCode.I;
            heavyPunchKey = KeyCode.J;
            heavyKickKey = KeyCode.K;
        }
        else
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;

            lightPunchKey = KeyCode.Semicolon;
            lightKickKey = KeyCode.Quote;
            heavyPunchKey = KeyCode.Comma;
            heavyKickKey = KeyCode.Period;
        }
    }

    void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(
                0f,
                cooldownRemaining -
                Time.deltaTime / GetAttackTimeMultiplier()
            );
        }

        if (!controlsEnabled)
        {
            currentMoveInput = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            UpdateWalkingAnimation(false);
            return;
        }

        if (matchManager != null && matchManager.MatchEnded)
        {
            currentMoveInput = 0f;
            rb.linearVelocity = Vector2.zero;
            UpdateWalkingAnimation(false);
            SetControlsEnabled(false);
            return;
        }

        FaceOpponent();

        if (health != null && health.IsDefeated)
        {
            currentMoveInput = 0f;
            rb.linearVelocity = Vector2.zero;
            return;
        }



        if (health != null && health.IsInHitstun())
        {
            currentMoveInput = 0f;
            UpdateWalkingAnimation(false);
            return;
        }



        float move = 0f;

        if (Input.GetKey(leftKey))
        {
            move = -1f;
        }

        if (Input.GetKey(rightKey))
        {
            move = 1f;
        }

        currentMoveInput = move;
        if (!isJumping && IsBlockedByFighter(move))
        {
            move = 0f;
        }

        // Don't play the walking animation during a vertical jump.
        UpdateWalkingAnimation(
            move != 0f &&
            !isAttacking &&
            !(isJumping && !canMoveInAir)
        );

        if (!isAttacking && cooldownRemaining <= 0f)
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

                

        float horizontalVelocity;

        if (isJumping)
        {
            // Typhoon pushes airborne characters horizontally,
            // even if they aren't moving left or right.
            float windPush = 0f;

            if (climateEventManager != null &&
                climateEventManager.IsTyphoonActive)
            {
                windPush = climateEventManager.WindDirection
                    * moveSpeed
                    * 0.25f;
            }

            horizontalVelocity =
                (jumpHorizontalVelocity + windPush);
        }
        else if (isAttacking)
        {
            horizontalVelocity = 0f;
        }
        else
        {
            float windMultiplier = climateEventManager != null
                ? climateEventManager.GetMovementSpeedMultiplier(move)
                : 1f;

            horizontalVelocity = move * moveSpeed * windMultiplier;
        }

        // Prevent horizontal movement into a wall.
        bool blockedByWall =
            Mathf.Abs(horizontalVelocity) > 0.01f &&
            IsBlockedByWall(Mathf.Sign(horizontalVelocity));

        bool blockedByFighter =
            !isJumping &&
            Mathf.Abs(horizontalVelocity) > 0.01f &&
            IsBlockedByFighter(Mathf.Sign(horizontalVelocity));

        if (blockedByWall || blockedByFighter)
        {
            horizontalVelocity = 0f;
            currentHorizontalVelocity = 0f;
        }

        if (climateEventManager != null &&
            climateEventManager.IsFloodActive)
        {
            float targetVelocity = horizontalVelocity;

            float rate = Mathf.Approximately(targetVelocity, 0f)
                ? climateEventManager.floodDeceleration
                : climateEventManager.floodAcceleration;

            currentHorizontalVelocity = Mathf.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                rate * Time.deltaTime
            );

            horizontalVelocity = currentHorizontalVelocity;
        }
        else
        {
            currentHorizontalVelocity = horizontalVelocity;
        }

        rb.linearVelocity = new Vector2(
            horizontalVelocity,
            rb.linearVelocity.y
        );




        if (Input.GetKeyDown(jumpKey) &&
            isGrounded &&
            !isAttacking)
        {
            isJumping = true;

            // Lock in the direction held at takeoff.
            jumpHorizontalVelocity = move * moveSpeed;

            // Clear existing vertical velocity before jumping.
            rb.linearVelocity = new Vector2(
                jumpHorizontalVelocity,
                0f
            );

            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );

            StartCoroutine(LogJumpVelocity());
        }
    }

    void ApplyFacingDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDirection < 0;
        }
    }

    void FaceOpponent()
    {
        if (matchManager == null)
            return;

        FighterHealth opponentHealth = null;

        if (matchManager.player1 == health)
            opponentHealth = matchManager.player2;
        else if (matchManager.player2 == health)
            opponentHealth = matchManager.player1;

        if (opponentHealth == null)
            return;

        float difference =
            opponentHealth.transform.position.x - transform.position.x;

        if (Mathf.Abs(difference) < 0.01f)
            return;

        facingDirection = difference > 0 ? 1 : -1;

        ApplyFacingDirection();
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (!enabled)
        {
            StopAllCoroutines();
            currentMoveInput = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            UpdateWalkingAnimation(false);
        }
    }

    public void ResetForRound()
    {
        currentMoveInput = 0f;
        isAttacking = false;
        isJumping = false;
        canMoveInAir = false;
        isGrounded = false;
        jumpHorizontalVelocity = 0f;
        cooldownRemaining = 0f;
        transform.position = startingPosition;
        currentHorizontalVelocity = 0f;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (health != null)
            health.ResetForRound();

        UpdateWalkingAnimation(false);
        FaceOpponentImmediately();
        SetControlsEnabled(false);
    }

    void UpdateWalkingAnimation(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool(
                walkingParameter,
                walking
            );
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
            canMoveInAir = false;
            jumpHorizontalVelocity = 0f;
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

        // Spawn the hitbox toward the opponent.
        attackPosition.x += 1f * facingDirection;

        GameObject hitbox = Instantiate(
            attackHitbox,
            attackPosition,
            Quaternion.identity
        );

        AttackHitbox hitboxScript =
            hitbox.GetComponent<AttackHitbox>();

        if (hitboxScript != null)
        {
            hitboxScript.owner = gameObject;
            hitboxScript.attackData = data;
        }
    }

        float GetAttackTimeMultiplier()
        {
            return climateEventManager != null &&
                climateEventManager.IsSmogActive
                ? 2f
                : 1f;
        }

        System.Collections.IEnumerator PerformAttack(AttackData data)
        {
            isAttacking = true;
            UpdateWalkingAnimation(false);

            // Startup
            yield return StartCoroutine(
                WaitForAttackPhase(data.startupTime)
            );

            if (health != null &&
                (health.IsInHitstun() || health.IsDefeated))
            {
                isAttacking = false;
                yield break;
            }

            Attack(data);

            // Active phase
            yield return StartCoroutine(
                WaitForAttackPhase(data.activeTime)
            );

            // Recovery
            yield return StartCoroutine(
                WaitForAttackPhase(data.recoveryTime)
            );

            // Cooldown begins after recovery.
            cooldownRemaining = data.cooldown;
            isAttacking = false;
        }

        System.Collections.IEnumerator WaitForAttackPhase(
            float baseDuration)
        {
            float elapsed = 0f;

            while (elapsed < baseDuration)
            {
                // Recheck every frame so the event can change mid-phase.
                elapsed += Time.deltaTime / GetAttackTimeMultiplier();

                yield return null;
            }
        }


    public bool ShouldAutoBlock(float attackerX)
    {
        float directionToAttacker =
            Mathf.Sign(attackerX - transform.position.x);

        if (Mathf.Approximately(currentMoveInput, 0f))
        {
            return true;
        }

        return Mathf.Sign(currentMoveInput) ==
               -directionToAttacker;
    }

    bool IsBlockedByWall(float move)
    {
        if (move == 0f || bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        Vector2 origin = new Vector2(
            move > 0 ? bounds.max.x : bounds.min.x,
            bounds.center.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * move,
            wallCheckDistance,
            wallLayer
        );

        return hit.collider != null;
    }

    bool IsBlockedByFighter(float move)
    {
        if (move == 0f || bodyCollider == null)
            return false;

        // Find the opponent.
        FighterController[] fighters =
            FindObjectsByType<FighterController>();

        foreach (FighterController other in fighters)
        {
            if (other == this || other.bodyCollider == null)
                continue;

            Bounds myBounds = bodyCollider.bounds;
            Bounds otherBounds = other.bodyCollider.bounds;

            // Only block if the fighters overlap vertically.
            bool overlapsVertically =
                myBounds.min.y < otherBounds.max.y &&
                myBounds.max.y > otherBounds.min.y;

            if (!overlapsVertically)
                continue;

            // Check whether the opponent is in the direction of movement.
            if (move > 0 &&
                otherBounds.min.x >= myBounds.max.x - 0.02f &&
                otherBounds.min.x - myBounds.max.x <= moveSpeed * Time.deltaTime + 0.1f)
            {
                return true;
            }

            if (move < 0 &&
                otherBounds.max.x <= myBounds.min.x + 0.02f &&
                myBounds.min.x - otherBounds.max.x <= moveSpeed * Time.deltaTime + 0.1f)
            {
                return true;
            }
        }

        return false;
    }

    
    System.Collections.IEnumerator LogJumpVelocity()
    {
        yield return new WaitForFixedUpdate();

        Debug.Log(
            gameObject.name +
            " jump velocity after physics: " +
            rb.linearVelocity.y
        );
    }

    public void FaceOpponentImmediately()
    {
        if (matchManager == null)
            matchManager = FindAnyObjectByType<MatchManager>();

        if (health == null)
            health = GetComponent<FighterHealth>();

        FaceOpponent();
    }

}
