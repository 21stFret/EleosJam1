using System.Collections.Generic;
using UnityEngine;

public class Rope2D : MonoBehaviour
{
    [Header("Rope Configuration")]
    public int segmentCount = 20;
    public float segmentLength = 0.5f;
    public float ropeWidth = 0.1f;
    
    [Header("Physics Settings")]
    public float segmentMass = 0.1f;
    public float jointDamping = 0.5f;
    public float jointFrequency = 1f;
    public bool useGravity = true;
    
    [Header("Rope Endpoints")]
    public Transform startPoint;
    public Transform endPoint;
    public bool attachStartPoint = true;
    public bool attachEndPoint = false;
    
    [Header("Visual Settings")]
    public Material ropeMaterial;
    public LineRenderer lineRenderer;
    public bool useLineRenderer = true;
    public float lineWidth = 0.1f;
    
    [Header("Collision")]
    public bool enableCollision = true;
    public LayerMask collisionLayer = -1;
    
    // Private variables
    public List<GameObject> ropeSegments = new List<GameObject>();
    private List<DistanceJoint2D> ropeJoints = new List<DistanceJoint2D>();
    private bool isInitialized = false;
    
    void Start()
    {
        if (ropeSegments.Count > 0)
        {
            
            CreateJoints();
                    
            // Attach endpoints
            AttachEndpoints();
            
            isInitialized = true;
            return;
        }
        CreateRope();
    }
    
    void Update()
    {

    }
    
    public void CreateRope()
    {
        if (isInitialized)
        {
            DestroyRope();
        }
        
        // Validate endpoints
        if (startPoint == null)
        {
            Debug.LogError("Start point is not assigned!");
            return;
        }
        
        // Calculate rope direction and segment positions
        Vector3 startPos = startPoint.position;
        Vector3 endPos = endPoint != null ? endPoint.position : startPos + Vector3.down * (segmentCount * segmentLength);
        Vector3 ropeDirection = (endPos - startPos).normalized;
        float totalDistance = Vector3.Distance(startPos, endPos);
        
        // Adjust segment length if needed
        float actualSegmentLength = totalDistance / segmentCount;
        
        // Create rope segments
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 segmentPosition = startPos + ropeDirection * (actualSegmentLength * i);
            GameObject segment = CreateRopeSegment(segmentPosition, i);
            ropeSegments.Add(segment);
        }
        
        // Create joints between segments
        CreateJoints();
        
        // Attach endpoints
        AttachEndpoints();
        
        isInitialized = true;
    }
    
    GameObject CreateRopeSegment(Vector3 position, int index)
    {
        GameObject segment = new GameObject($"RopeSegment_{index}");
        segment.transform.position = position;
        segment.transform.parent = transform;
        
        // Add Rigidbody2D
        Rigidbody2D rb = segment.AddComponent<Rigidbody2D>();
        rb.mass = segmentMass;
        rb.gravityScale = useGravity ? 1f : 0f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        
        // Add Collider if enabled
        if (enableCollision)
        {
            CircleCollider2D collider = segment.AddComponent<CircleCollider2D>();
            collider.radius = ropeWidth * 0.5f;
            
            // Set collision layer
            segment.layer = Mathf.RoundToInt(Mathf.Log(collisionLayer.value, 2));
        }
        
        return segment;
    }
    
    void CreateJoints()
    {
        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            DistanceJoint2D joint = ropeSegments[i].AddComponent<DistanceJoint2D>();
            joint.connectedBody = ropeSegments[i + 1].GetComponent<Rigidbody2D>();
            joint.enableCollision = false;
            
            ropeJoints.Add(joint);
        }
    }
    
    void AttachEndpoints()
    {
        if (attachStartPoint && startPoint != null && ropeSegments.Count > 0)
        {
            // Attach first segment to start point
            DistanceJoint2D startJoint = ropeSegments[0].AddComponent<DistanceJoint2D>();

            // If start point has a Rigidbody2D, connect to it
            Rigidbody2D startRb = startPoint.GetComponent<Rigidbody2D>();
            if (startRb != null)
            {
                startJoint.connectedBody = startRb;
            }
            else
            {
                // Create a static anchor
                startJoint.connectedAnchor = startPoint.position;
            }
            

            startJoint.enableCollision = false;
        }
        
        if (attachEndPoint && endPoint != null && ropeSegments.Count > 0)
        {
            // Attach last segment to end point
            DistanceJoint2D endJoint = ropeSegments[ropeSegments.Count - 1].AddComponent<DistanceJoint2D>();

            Rigidbody2D endRb = endPoint.GetComponent<Rigidbody2D>();
            if (endRb != null)
            {
                endJoint.connectedBody = endRb;
            }
            else
            {
                endJoint.connectedAnchor = endPoint.position;
            }
            

            endJoint.enableCollision = false;
        }
    }
    
    public void DestroyRope()
    {
        // Destroy all rope segments
        foreach (GameObject segment in ropeSegments)
        {
            if (segment != null)
            {
                DestroyImmediate(segment);
            }
        }
        
        ropeSegments.Clear();
        ropeJoints.Clear();
        isInitialized = false;
    }
    
    // Public methods for runtime control
    public void SetRopeLength(int newSegmentCount)
    {
        segmentCount = newSegmentCount;
        CreateRope();
    }
    
    public void SetEndPoint(Transform newEndPoint)
    {
        endPoint = newEndPoint;
        CreateRope();
    }
    
    public Vector3 GetRopeEndPosition()
    {
        if (ropeSegments.Count > 0 && ropeSegments[ropeSegments.Count - 1] != null)
        {
            return ropeSegments[ropeSegments.Count - 1].transform.position;
        }
        return Vector3.zero;
    }
    
    public List<GameObject> GetRopeSegments()
    {
        return new List<GameObject>(ropeSegments);
    }
    
    public void AddForceToRope(Vector2 force, int segmentIndex = -1)
    {
        if (segmentIndex == -1)
        {
            // Apply force to all segments
            foreach (GameObject segment in ropeSegments)
            {
                Rigidbody2D rb = segment.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.AddForce(force);
                }
            }
        }
        else if (segmentIndex >= 0 && segmentIndex < ropeSegments.Count)
        {
            // Apply force to specific segment
            Rigidbody2D rb = ropeSegments[segmentIndex].GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(force);
            }
        }
    }
    
    // Context menu for editor testing
    [ContextMenu("Recreate Rope")]
    void RecreateRope()
    {
        CreateRope();
    }
    
    [ContextMenu("Destroy Rope")]
    void DestroyRopeMenu()
    {
        DestroyRope();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw rope configuration in editor
        if (startPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.2f);
            
            if (endPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(endPoint.position, 0.2f);
                
                // Draw line between start and end points
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(startPoint.position, endPoint.position);
            }
            else
            {
                // Draw predicted rope length
                Vector3 predictedEnd = startPoint.position + Vector3.down * (segmentCount * segmentLength);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(startPoint.position, predictedEnd);
                Gizmos.DrawWireSphere(predictedEnd, 0.2f);
            }
        }
    }
}
