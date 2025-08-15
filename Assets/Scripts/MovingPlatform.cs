using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2f;
    public float waitTime = 1f;
    public bool loop = true;
    
    [Header("Passenger Detection")]
    public LayerMask passengerLayerMask = 1;
    public float passengerCheckDistance = 0.5f;
    
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private float waitTimer;
    public bool _active = true;
    
    // Passenger tracking
    private List<Transform> passengers = new List<Transform>();
    private RealityObject realityObject;

    // Start is called before the first frame update
    void Start()
    {
        endPoint.SetParent(null); // Ensure endPoint is not a child of this object
        startPoint.SetParent(null); // Ensure startPoint is not a child of this object
        targetPosition = endPoint.position;
        lastPosition = transform.position;
        realityObject = GetComponent<RealityObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_active) return;
        
        // Store position before moving
        lastPosition = transform.position;
        
        // Move the platform
        MovePlatform();
        
        // Move passengers if platform moved
        if (Vector3.Distance(lastPosition, transform.position) > 0.001f)
        {
            MovePassengers();
        }
    }

    void MovePlatform()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                if (loop)
                    targetPosition = (targetPosition == endPoint.position) ? startPoint.position : endPoint.position;
                else
                    _active = false; // Stop moving if not looping
            }
        }
    }
    
    void MovePassengers()
    {
        if (!realityObject.isActive)
        {
            // If reality object is inactive, do not move passengers
            return;
        }
        Vector3 deltaMovement = transform.position - lastPosition;
        
        // Update passenger list
        UpdatePassengerList();
        
        // Move all passengers
        foreach (Transform passenger in passengers)
        {
            if (passenger != null)
            {
                passenger.position += deltaMovement;
            }
        }
    }
    
    void UpdatePassengerList()
    {
        passengers.Clear();
        
        // Get the bounds of the platform
        Collider2D platformCollider = GetComponent<Collider2D>();
        if (platformCollider == null) return;
        
        // Check for objects above the platform
        Vector2 checkPosition = new Vector2(transform.position.x, platformCollider.bounds.max.y);
        Vector2 checkSize = new Vector2(platformCollider.bounds.size.x, passengerCheckDistance);
        
        Collider2D[] objectsAbove = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f, passengerLayerMask);
        
        foreach (Collider2D obj in objectsAbove)
        {
            if (obj.transform != transform) // Don't include the platform itself
            {
                passengers.Add(obj.transform);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw passenger detection area
        Collider2D platformCollider = GetComponent<Collider2D>();
        if (platformCollider != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 checkPosition = new Vector3(transform.position.x, platformCollider.bounds.max.y, transform.position.z);
            Vector3 checkSize = new Vector3(platformCollider.bounds.size.x, passengerCheckDistance, 0);
            Gizmos.DrawWireCube(checkPosition, checkSize);
        }
        
        // Draw movement path
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawWireSphere(startPoint.position, 0.3f);
            Gizmos.DrawWireSphere(endPoint.position, 0.3f);
        }
    }
}
