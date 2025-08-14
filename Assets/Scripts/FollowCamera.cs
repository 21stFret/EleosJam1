using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 2, -10);
    public float followSpeed = 2f;
    public bool smoothDampPosition = false;
    
    [Header("Smooth Damping (Alternative to Lerp)")]
    public float smoothTime = 0.3f;
    
    [Header("Camera Bounds (Optional)")]
    public bool useBounds = false;
    public Vector2 minBounds = Vector2.zero;
    public Vector2 maxBounds = Vector2.zero;
    
    // Private variables for smooth damping
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        // If no target is assigned, try to find the player
        if (target == null)
        {
            ExorcistController player = FindFirstObjectByType<ExorcistController>();
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Set initial position if target exists
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Apply camera movement based on selected method
        Vector3 newPosition;
        
        if (smoothDampPosition)
        {
            // Use SmoothDamp for more natural movement
            newPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
        else
        {
            // Use Lerp for consistent speed
            newPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
        
        // Apply bounds if enabled
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
            newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
        }
        
        transform.position = newPosition;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw camera bounds if enabled
        if (useBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw offset visualization
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(target.position, target.position + offset);
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);
        }
    }
}
