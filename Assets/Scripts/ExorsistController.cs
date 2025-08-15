using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExorcistController : MonoBehaviour, IDamageable
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
    public ParticleSystem hitEffect;
    public Material sharedPlayerMaterial;
    private bool isHitFlashing;

    // Components
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    
    // Input System
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction minimapAction;
    
    // Movement variables
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool wasGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isTouchingWall;
    private bool isWallSliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        moveAction = inputActions.FindAction("Move");
        jumpAction = inputActions.FindAction("Jump");
        minimapAction = inputActions.FindAction("Minimap");
    }

    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();

            if (jumpAction != null)
                jumpAction.performed += OnJump;

            if (minimapAction != null)
                minimapAction.performed += OnMinimap;
        }
    }
    
    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();

            if (jumpAction != null)
                jumpAction.performed -= OnJump;
            if (minimapAction != null)
                minimapAction.performed -= OnMinimap;
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
        
        // If we just landed, reset any jump states
        if (!wasGrounded && isGrounded)
        {
            // Player just landed
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
            float rayHeight = i * (wallCheckHeight / wallCheckRayCount);
            Vector2 rayOrigin = (Vector2)groundCheck.position + Vector2.up * rayHeight;
            
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, wallCheckDistance, wallLayerMask);
            if (hit.collider != null)
            {
                if(hit.collider.GetComponent<OneWayPlatform>() != null)
                {
                    // Ignore one-way platforms
                    continue;
                }

                hitWall = true;
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
        if (!context.performed)
            return;

        // Check if we want to drop through a one-way platform
        if (isGrounded && IsOnOneWayPlatform() && verticalInput < 0)
        {
            DropThroughPlatform();
            return;
        }

        jumpBufferCounter = jumpBufferTime;
        
        // Can jump immediately if grounded or within coyote time
        if (coyoteTimeCounter > 0f || isGrounded)
        {
            PerformJump();
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
    }
    
    bool IsOnOneWayPlatform()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, oneWayPlatformMask);
    }
    
    void DropThroughPlatform()
    {
        // Find all one-way platforms we're standing on
        Collider2D[] platforms = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, oneWayPlatformMask);
        
        foreach (Collider2D platform in platforms)
        {
            // Check if platform has PlatformEffector2D
            PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
            if (effector != null)
            {
                // Temporarily disable the effector
                // This didn't work as expected, so we will ignore collision instead
                // StartCoroutine(DisableEffectorTemporarily(effector, jumpDownDuration));
                StartCoroutine(IgnoreCollisionTemporarily(platform, jumpDownDuration));
            }
            else
            {
                // Fallback: temporarily ignore collision
                StartCoroutine(IgnoreCollisionTemporarily(platform, jumpDownDuration));
            }
        }
        
        // Add small downward velocity to help drop through
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
    }
    
    IEnumerator IgnoreCollisionTemporarily(Collider2D platform, float duration)
    {
        Physics2D.IgnoreCollision(playerCollider, platform, true);
        yield return new WaitForSeconds(duration);
        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, false);
        }
    }

    public void OnMinimap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Toggle minimap visibility
            GameManager.Instance.ToggleMinimap();
        }
    }

    public void TakeDamage(float amount, float stunTime = 0f)
    {
        // Implement damage logic here
        Debug.Log($"Exorsist took {amount} damage!");
        currentHealth -= amount;
        if (currentHealth < 0)
            currentHealth = 0;
        GameManager.Instance?.gameUI.UpdateHealthBar(currentHealth / maxHealth);
        StartCoroutine(PlayHitEffect());
        if (currentHealth == 0)
        {
            Die();
        }
    }

    private IEnumerator PlayHitEffect()
    {
        if (!isHitFlashing)
        {
            isHitFlashing = true;
            sharedPlayerMaterial.SetFloat("_StrongTintFade", 1);
            hitEffect.Play();
            yield return new WaitForSeconds(0.1f);
            sharedPlayerMaterial.SetFloat("_StrongTintFade", 0);
            isHitFlashing = false;
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
                float rayHeight = i * (wallCheckHeight / wallCheckRayCount);
                Vector2 rayOrigin = (Vector2)groundCheck.position + Vector2.up * rayHeight;
                Gizmos.DrawRay(rayOrigin, direction * wallCheckDistance);
            }
        }
    }
}
