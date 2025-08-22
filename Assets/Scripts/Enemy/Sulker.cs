using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sulker : EnemyBaseClass
{
    [Header("Movement Settings")]
    public float fleeDistance = 5f;
    public float acceleration = 15f;
    private Transform playerTransform;
    private Rigidbody2D rb;
    public damageArea attackPrefab;

    [Header("Dodge Settings")]
    public float dodgeDistance = 3f;
    public float dodgeCooldown = 2f;
    public float dodgeRadius = 1f;
    public float dodgePulseTimer = 0.5f;
    public float attackTimer = 5f;
    private Vector3 lastPos;

    [Header("Wander Settings")]
    public float wanderPulseTimer = 1f;
    public float wanderRadius = 10f;
    private float wanderCooldown = 0f;
    private Vector3 wanderTarget;
    private bool isfleeing = false;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        
        // Setup Rigidbody2D if not already configured
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
                
        lastPos = transform.position;
        SetNewWanderTarget();
    }

    public override void Update()
    {
        base.Update();
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer < fleeDistance)
        {
            FleePlayer();
        }
        else
        {
            Wander();
        }

        // Check for attack
        attackCooldown -= Time.deltaTime;
        if (Vector3.Distance(transform.position, lastPos) > attackRange && attackCooldown <= 0)
        {
            AttackPlayer();
        }
    }

    void FleePlayer()
    {
        isfleeing = true;
        Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;
        
        // Use physics force for fleeing
        Vector2 fleeForce = fleeDirection * acceleration * 2f; // Flee faster than normal movement
        rb.AddForce(fleeForce, ForceMode2D.Force);
        
        // Limit speed
        if (rb.linearVelocity.magnitude > speed * 1.2f) // Allow faster fleeing
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed * 1.2f;
        }
        
        // Set new wander target in flee direction
        Vector3 newWander = fleeDirection * 8f;
        wanderTarget = transform.position + newWander + (Vector3)Random.insideUnitCircle * 3f;
        wanderTarget.z = 0;
    }

    void AttackPlayer()
    {
        if (attackPrefab != null)
        {
            attackPrefab.transform.position = transform.position;
            attackPrefab.transform.SetParent(null);
            attackPrefab.EnableDamage();
        }
        attackCooldown = attackTimer;
        lastPos = transform.position;
    }

    void Wander()
    {
        isfleeing = false;
        wanderCooldown -= Time.deltaTime;
        
        // Check if we've reached the wander target or need a new one
        float distanceToTarget = Vector3.Distance(transform.position, wanderTarget);
        
        if (wanderCooldown <= 0 || distanceToTarget < 1f)
        {
            SetNewWanderTarget();
            wanderCooldown = wanderPulseTimer;
        }
        
        // Move towards wander target using physics
        Vector3 direction = (wanderTarget - transform.position).normalized;
        Vector2 wanderForce = direction * acceleration;
        rb.AddForce(wanderForce, ForceMode2D.Force);
        
        // Limit speed during wandering
        if (rb.linearVelocity.magnitude > speed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    void SetNewWanderTarget()
    {
        // Generate a random wander target
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = transform.position + (Vector3)randomDirection;
        wanderTarget.z = 0;

        // Keep wander target in bounds
        if (wanderTarget.y < 0)
        {
            wanderTarget.y = 0;
        }
        if (wanderTarget.x < -50)
        {
            wanderTarget.x = 0;
        }
        if (wanderTarget.x > 50)
        {
            wanderTarget.x = 50;
        }
        if (wanderTarget.y > 45)
        {
            wanderTarget.y = 45;
        }

    }
}
