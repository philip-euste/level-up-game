using UnityEngine;

public class FighterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    private bool isGrounded;
    private int facingDirection = 1;

    private Rigidbody2D rb;
    public GameObject attackHitbox;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = 0f;
        if (Input.GetKeyDown(KeyCode.U))
        {
            Attack();
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            move = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            move = 1f;
        }

        if (move != 0)
        {
            facingDirection = (int)Mathf.Sign(move);
            GetComponent<SpriteRenderer>().flipX = facingDirection == -1;
        }

        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)        
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

    void Attack()
    {
        Vector3 attackPosition = transform.position;

        attackPosition.x += 0.75f * facingDirection;

        GameObject hitbox = Instantiate(
            attackHitbox,
            attackPosition,
            Quaternion.identity
        );

        hitbox.transform.localScale = new Vector3(
            facingDirection,
            1,
            1
        );
    }
}