using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExorsistController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.1f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayerMask = 1;
    public float groundCheckRadius = 0.2f;
    
    [Header("One-Way Platform")]
    public LayerMask oneWayPlatformMask = 1;
    public float jumpDownDuration = 0.3f;
    
    [Header("Wall Slide Prevention")]
    public LayerMask wallLayerMask = 1;
    public float wallCheckDistance = 0.6f;
    public float wallSlideSpeed = 2f;
    public int wallCheckRayCount = 3;
    public float wallCheckHeight;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool isAlive = true;

    // Components
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    
    // Input System
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    
    // Movement variables
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumpingDown;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool verticalJumpTriggered;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        inputActions = GameManager.Instance?.playerInput.actions;

        // Set up input actions
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Move");
            jumpAction = inputActions.FindAction("Jump");
        }
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            
            if (jumpAction != null)
                jumpAction.performed += OnJump;
        }
    }
    
    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            
            if (jumpAction != null)
                jumpAction.performed -= OnJump;
        }
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        CheckWallSliding();
        HandleCoyoteTime();
        HandleJumpBuffer();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleInput()
    {
        // Get horizontal input
        if (moveAction != null)
        {
            horizontalInput = moveAction.ReadValue<Vector2>().x;
            verticalInput = moveAction.ReadValue<Vector2>().y;
        }
    }
    
    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        // If we just landed, reset jump down state
        if (!wasGrounded && isGrounded)
        {
            isJumpingDown = false;
        }
    }
    
    void CheckWallSliding()
    {
        // Only check for walls if player is giving input
        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            isTouchingWall = false;
            isWallSliding = false;
            return;
        }
        
        // Determine direction to check
        Vector2 direction = horizontalInput > 0 ? Vector2.right : Vector2.left;
        
        // Check for walls with multiple raycasts from bottom to top
        bool hitWall = false;
        for (int i = 0; i < wallCheckRayCount; i++)
        {
            float rayHeight = (wallCheckHeight / (wallCheckRayCount - 1)) * i - (wallCheckHeight * 0.5f);
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * rayHeight;
            
            if (Physics2D.Raycast(rayOrigin, direction, wallCheckDistance, wallLayerMask))
            {
                hitWall = true;
                break; // Found a wall, no need to check more rays
            }
        }
        
        isTouchingWall = hitWall;

        // Wall sliding conditions:
        // 1. Not grounded (in air)
        // 2. Touching a wall in the direction of movement
        // 3. Falling (negative Y velocity)
        isWallSliding = !isGrounded && isTouchingWall;
    }
    
    void HandleCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    
    void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
            
            // If we have a buffered jump and we just became grounded, perform the jump
            if (isGrounded && jumpBufferCounter > 0)
            {
                PerformJump();
            }
        }
    }
    
    void HandleMovement()
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // Wall slide prevention - limit falling speed when wall sliding
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(0, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        
        // Flip character sprite based on movement direction
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed || isJumpingDown)
            return; // Ignore if already jumping down or jump action not performed

        // Check if we're on a one-way platform
        if (isGrounded && IsOnOneWayPlatform() && verticalInput < 0)
        {
            StartCoroutine(JumpDownThroughPlatform());
            return; // Exit early to prevent jump execution
        }

        jumpBufferCounter = jumpBufferTime;
        
        // Can jump immediately if grounded or within coyote time
        if (coyoteTimeCounter > 0f || isGrounded)
        {
            PerformJump();
        }
        // If not grounded, the jump will be buffered and executed when we land
        // (handled in HandleJumpBuffer)
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumpingDown = false;
    }
    
    bool IsOnOneWayPlatform()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, oneWayPlatformMask);
    }
    
    IEnumerator JumpDownThroughPlatform()
    {
        isJumpingDown = true;
        
        // Temporarily ignore collision with one-way platforms
        Collider2D[] platforms = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, oneWayPlatformMask);
        
        foreach (Collider2D platform in platforms)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, true);
        }
        
        // Add slight downward force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        
        // Wait for the specified duration
        yield return new WaitForSeconds(jumpDownDuration);
        
        // Re-enable collision with platforms
        foreach (Collider2D platform in platforms)
        {
            if (platform != null)
            {
                Physics2D.IgnoreCollision(playerCollider, platform, false);
            }
        }
        
        isJumpingDown = false;
    }

    public void TriggerVerticalJump()
    {
        if (verticalJumpTriggered)
            return; // Prevent multiple triggers
        verticalJumpTriggered = true;
        StartCoroutine(PerformVerticalJump());
    }

    private IEnumerator PerformVerticalJump()
    {
        // Temporarily ignore collision with one-way platforms
        Collider2D[] platforms = Physics2D.OverlapCircleAll(transform.position + transform.up * 1.5f, groundCheckRadius, oneWayPlatformMask);

        foreach (Collider2D platform in platforms)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, true);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(jumpDownDuration);

        // Re-enable collision with platforms
        foreach (Collider2D platform in platforms)
        {
            if (platform != null)
            {
                Physics2D.IgnoreCollision(playerCollider, platform, false);
            }
        }
        verticalJumpTriggered = false;
    }

    public void TakeDamage(float amount)
    {
        // Implement damage logic here
        Debug.Log($"Exorsist took {amount} damage!");
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;
        GameManager.Instance?.gameUI.UpdateHealthBar(currentHealth / maxHealth);
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isAlive = false;
        Debug.Log("Exorsist died!");
        GameManager.Instance?.OnGameWin?.Invoke();
        GameManager.Instance?.EndGame();

    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check circle
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Draw wall check rays only in the direction of input
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Gizmos.color = isWallSliding ? Color.yellow : Color.blue;
            Vector2 direction = horizontalInput > 0 ? Vector2.right : Vector2.left;
            
            for (int i = 0; i < wallCheckRayCount; i++)
            {
                float rayHeight = (wallCheckHeight / (wallCheckRayCount - 1)) * i - (wallCheckHeight * 0.5f);
                Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * rayHeight;
                Gizmos.DrawRay(rayOrigin, direction * wallCheckDistance);
            }
        }
    }
}
