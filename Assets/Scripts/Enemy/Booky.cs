using UnityEngine;

public class Booky : EnemyBaseClass
{
    [Header("Booky Movement")]
    public float edgeRayDistance = 1f;
    public float jumpForce = 8f;
    public LayerMask platformLayer;
    public float platformSearchRadius = 3f;
    public float jumpCooldown = 1f;
    public float jumpHeight = 2f;

    private Rigidbody2D rb;
    private bool movingRight = true;
    private float lastJumpTime = -10f;
    private GameObject currentPlatform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (!isAlive) return;
        MoveAlongPlatform();
        CheckEdgeAndJump();
    }

    void MoveAlongPlatform()
    {
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        // Flip sprite
        if (direction > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void CheckEdgeAndJump()
    {
        // Raycast down in front
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * (movingRight ? 0.5f : -0.5f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, edgeRayDistance, platformLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * edgeRayDistance, Color.red);
        if (hit.collider == null && Time.time - lastJumpTime > jumpCooldown)
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
        else if (hit.collider != null)
        {
            // On platform, update current platform reference
            currentPlatform = hit.collider.gameObject;
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
            if (dist < minDist && platform.transform.position.y < transform.position.y + jumpHeight)
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
        rb.linearVelocity = new Vector2(jumpDirection.x * speed, jumpForce);
        lastJumpTime = Time.time;
        // Face jump direction
        movingRight = jumpDirection.x > 0;
    }

    public override void Die()
    {
        base.Die();
        GameManager.Instance.SpawnSpecificEnemy(GameManager.Instance.GetEnemyPrefab(EnemyType.Wisp), transform);
    }

    void OnDrawGizmosSelected()
    {
        // Draw edge ray
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.right * (movingRight ? 0.5f : -0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * edgeRayDistance);
        // Draw platform search radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, platformSearchRadius);
    }
}
