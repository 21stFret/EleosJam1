using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    [Header("One-Way Platform Settings")]
    public float fallThroughTime = 0.3f;
    public LayerMask playerLayer = 1;
    
    private Collider2D platformCollider;
    private PlatformEffector2D platformEffector;
    
    void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        
        // Set up the collider for one-way platform
        platformCollider.usedByEffector = true;
        
        // Add PlatformEffector2D if it doesn't exist
        platformEffector = GetComponent<PlatformEffector2D>();
        if (platformEffector == null)
        {
            platformEffector = gameObject.AddComponent<PlatformEffector2D>();
        }
        
        // Configure the platform effector
        platformEffector.useOneWay = true;
        platformEffector.useOneWayGrouping = false;
        platformEffector.surfaceArc = 180f; // Only collide from above
        platformEffector.sideArc = 180f;
    }
    
    void Start()
    {
        // Make sure the platform is on the correct layer
        if (gameObject.layer == 0) // Default layer
        {
            Debug.LogWarning($"OneWayPlatform {gameObject.name} should be on a specific layer for better collision detection!");
        }
    }
    
    public void TemporarilyDisable(float duration)
    {
        if (platformEffector != null)
        {
            StartCoroutine(DisableTemporarily(duration));
        }
    }
    
    private System.Collections.IEnumerator DisableTemporarily(float duration)
    {
        platformEffector.enabled = false;
        yield return new WaitForSeconds(duration);
        platformEffector.enabled = true;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the platform surface direction
        Gizmos.color = Color.green;
        Vector3 surfaceDirection = transform.up;
        Vector3 center = transform.position;
        
        // Draw arrow showing surface normal
        Gizmos.DrawRay(center, surfaceDirection * 2f);
        Gizmos.DrawRay(center + surfaceDirection * 2f, (-surfaceDirection + Vector3.left * 0.3f) * 0.5f);
        Gizmos.DrawRay(center + surfaceDirection * 2f, (-surfaceDirection + Vector3.right * 0.3f) * 0.5f);
    }
}
