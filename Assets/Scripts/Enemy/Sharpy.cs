using System.Collections;
using UnityEngine;

public class Sharpy : EnemyBaseClass
{
    [Header("Sharpy Movement")]
    public float edgeRayDistance = 1f;
    public float jumpForce = 8f;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float jumpCooldown = 1f;
    private float lastJumpTime = -10f;
    public float jumpHeight = 2f;
    public bool isJumping = false;
    private Rigidbody2D rb;
    public bool movingRight = true;

    private bool jumpDelay = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastJumpTime = Time.time;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!isAlive) return;
        if (jumpDelay) return;
        CheckGrounded();
        Jump();
        CheckForWalls();
    }

    void CheckForWalls()
    {
        // Raycast to check for walls
        float rayLength = edgeRayDistance / 2;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * (movingRight ? 0.5f : -0.5f);
        Vector2 rayDirection = Vector2.right * (movingRight ? 1f : -1f); // Use unit direction vector
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, wallLayer);
        Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.blue);
        if (hit.collider != null)
        {
            // Wall detected, turn around
            movingRight = !movingRight;
            rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y); // Reverse horizontal velocity
        }
    }

    void CheckGrounded()
    {
        // Raycast downwards to check for ground
        float rayLength = edgeRayDistance / 2;
        Vector2 rayOrigin = (Vector2)transform.position - Vector2.up * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red);
        if (hit.collider != null)
        {
            // Ground detected
            isJumping = false;
        }
    }

    void Jump()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return; // Prevent jumping too soon
        if (isJumping) return; // Prevent jumping if already in the air

        Vector2 jumpDirection = movingRight ? Vector2.right : Vector2.left;
        rb.linearVelocity = new Vector2(jumpDirection.x * speed * 2, jumpForce);
        movingRight = jumpDirection.x > 0;
        lastJumpTime = Time.time;
        isJumping = true;
        jumpDelay = true;
        StartCoroutine(JumpDelayCoroutine()); // Delay before allowing next jump
    }

    private IEnumerator JumpDelayCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        jumpDelay = false;
    }

    public override void Die()
    {
        base.Die();
        GameManager.Instance.SpawnSpecificEnemy(GameManager.Instance.GetEnemyPrefab(EnemyType.Sulker), transform);
    }
}
