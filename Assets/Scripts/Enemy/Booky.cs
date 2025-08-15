using UnityEngine;

public class Booky : EnemyBaseClass
{
    [Header("Booky Movement")]
    public float edgeRayDistance = 1f;
    public float jumpForce = 8f;
    public LayerMask platformLayer;
    public LayerMask wallLayer;
    public float platformSearchRadius = 3f;
    public float jumpCooldown = 1f;
    public float jumpHeight = 2f;
    private bool isJumping = false;

    private Rigidbody2D rb;
    public bool movingRight = true;
    private float lastJumpTime = -10f;
    private GameObject currentPlatform;

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
        if (!isAlive || isStunned) return;
        MoveAlongPlatform();
        CheckEdgeAndJump();
        CheckForWalls();
    }

    void MoveAlongPlatform()
    {
        float direction = movingRight ? 1f : -1f;
        // Flip sprite
        if (direction > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        if (currentPlatform == null) return;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    void CheckEdgeAndJump()
    {
        // Raycast down in front
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * (movingRight ? 0.4f : -0.4f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, edgeRayDistance, platformLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * edgeRayDistance, Color.red);
        if (hit.collider == null)
        {
            if (isJumping) return; // Prevent jumping if already in the air
            if (Time.time - lastJumpTime > jumpCooldown)
            {
                // At edge, look for next platform
                Collider2D nearestPlatform = FindNearestPlatform();
                if (nearestPlatform != null)
                {
                    JumpToPlatform(nearestPlatform);
                }
                else
                {
                    // No platform found, turn around
                    movingRight = !movingRight;
                }
            }
            else
            {
                // No platform found, turn around
                movingRight = !movingRight;
            }
        }
        if (hit.collider != null)
        {
            // On platform, update current platform reference
            currentPlatform = hit.collider.gameObject;
            isJumping = false; // Reset jumping state when on platform
        }
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
        }
    }

    Collider2D FindNearestPlatform()
    {
        Collider2D[] platforms = Physics2D.OverlapCircleAll(transform.position, platformSearchRadius, platformLayer);
        Collider2D nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var platform in platforms)
        {
            if (platform.gameObject == currentPlatform) continue; // Skip current platform
            float dist = Vector2.Distance(transform.position, platform.transform.position);
            if (dist < minDist && platform.transform.position.y < transform.position.y + jumpHeight && platform.transform.position.x != currentPlatform.transform.position.x)
            {
                minDist = dist;
                nearest = platform;
            }
        }
        return nearest;
    }

    void JumpToPlatform(Collider2D platform)
    {
        Vector2 jumpDirection = (platform.transform.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(jumpDirection.x * speed *2, jumpForce);
        lastJumpTime = Time.time;
        // Face jump direction
        movingRight = jumpDirection.x > 0;
        isJumping = true;
        currentPlatform = null;
    }

    public override void Die()
    {
        base.Die();
        GameManager.Instance.SpawnSpecificEnemy(GameManager.Instance.GetEnemyPrefab(EnemyType.Wisp), transform);
    }

    void OnDrawGizmosSelected()
    {
        // Draw platform search radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, platformSearchRadius);
    }
}
