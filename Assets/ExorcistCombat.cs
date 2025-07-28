using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExorcistCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float meleeCooldown = 0.8f;
    public float rangedCooldown = 1.2f;
    
    [Header("Melee Attack (Reality 1)")]
    public float meleeRange = 1.5f;
    public float meleeWidth = 1f;
    public float meleeHeight = 1f;
    public float meleeDamage = 50f;
    public LayerMask enemyLayerMask = 1;
    public float meleeAttackDuration = 0.2f;
    
    [Header("Ranged Attack (Reality 0)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float projectileDamage = 30f;
    public int projectilePoolSize = 10;
    
    [Header("Visual Effects")]
    public GameObject meleeEffectPrefab;
    public GameObject muzzleFlashPrefab;
    
    [Header("Audio")]
    public AudioClip meleeAttackSound;
    public AudioClip rangedAttackSound;
    
    // Components and references
    private RealityManager realityManager;
    private AudioSource audioSource;
    private ExorsistController playerController;
    
    // Input System
    public InputActionAsset inputActions;
    private InputAction attackAction;
    
    // Combat state
    private float lastMeleeAttackTime;
    private float lastRangedAttackTime;
    private bool isAttacking;
    
    // Melee attack optimization - persistent hitbox
    public GameObject meleeHitbox;
    private BoxCollider2D meleeCollider;
    
    // Projectile pooling
    private Queue<GameObject> projectilePool = new Queue<GameObject>();
    private Transform projectilePoolParent;
    
    public void Init()
    {
        // Get components
        realityManager = FindObjectOfType<RealityManager>();
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<ExorsistController>();
        
        // Create audio source if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set up input actions
        inputActions = GameManager.Instance?.playerInput.actions;
        if (inputActions != null)
        {
            attackAction = inputActions.FindAction("Attack");
            if (attackAction != null)
            attackAction.performed += OnAttack;
        }
        
        // Create fire point if it doesn't exist
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            firePoint = firePointObj.transform;
        }
        
        // Initialize projectile pool
        InitializeProjectilePool();
    }
    
    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            
            if (attackAction != null)
                attackAction.performed += OnAttack;
        }
    }
    
    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            
            if (attackAction != null)
                attackAction.performed -= OnAttack;
        }
    }
    
    void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        if (isAttacking) return;
        
        if (realityManager != null)
        {
            int currentReality = realityManager.currentReality;
            
            if (currentReality == 1)
            {
                TryMeleeAttack();
            }
            else if (currentReality == 0)
            {
                TryRangedAttack();
            }
        }
        else
        {
            // Fallback if no reality manager - default to melee
            TryMeleeAttack();
        }
    }
    
    void TryMeleeAttack()
    {
        if (Time.time - lastMeleeAttackTime < meleeCooldown) return;
        
        StartCoroutine(PerformMeleeAttack());
    }
    
    void TryRangedAttack()
    {
        if (Time.time - lastRangedAttackTime < rangedCooldown) return;
        
        PerformRangedAttack();
    }
    
    IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        lastMeleeAttackTime = Time.time;
        
        // Play sound
        if (meleeAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(meleeAttackSound);
        }
        
        // Position and enable melee hitbox
        UpdateMeleeHitboxPosition();
        EnableMeleeHitbox(true);
        
        // Spawn visual effect
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (meleeEffectPrefab != null)
        {
            Vector3 effectPosition = transform.position + (Vector3)attackDirection * meleeRange * 0.5f;
            GameObject effect = Instantiate(meleeEffectPrefab, effectPosition, Quaternion.identity);
            
            // Flip effect based on player direction
            if (transform.localScale.x < 0)
            {
                effect.transform.localScale = new Vector3(-1, 1, 1);
            }
            
            Destroy(effect, 1f);
        }
        
        // Wait for attack duration
        yield return new WaitForSeconds(meleeAttackDuration);
        
        // Disable melee hitbox
        EnableMeleeHitbox(false);
        
        isAttacking = false;
    }
    
    void PerformRangedAttack()
    {
        lastRangedAttackTime = Time.time;
        
        // Play sound
        if (rangedAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rangedAttackSound);
        }
        
        // Spawn muzzle flash
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, 0.2f);
        }
        
        // Get projectile from pool
        GameObject projectile = GetPooledProjectile();
        if (projectile != null && firePoint != null)
        {
            // Position and activate projectile
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            projectile.transform.SetParent(null); // Detach from pool parent
            projectile.SetActive(true);
            
            // Set projectile direction based on player facing
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            
            // Configure projectile
            ExorcistProjectile projectileScript = projectile.GetComponent<ExorcistProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(shootDirection, projectileSpeed, projectileDamage);
            }
        }
    }
   
    void UpdateMeleeHitboxPosition()
    {
        if (meleeHitbox == null) return;

        // Update collider size in case settings changed
        if (meleeCollider != null)
        {
            meleeCollider.size = new Vector2(meleeRange, meleeHeight);
        }
    }
    
    void EnableMeleeHitbox(bool enable)
    {
        if (meleeHitbox != null)
        {
            UpdateMeleeHitboxPosition(); // Update position before enabling
            meleeHitbox.SetActive(enable);
        }
    }
    
    void InitializeProjectilePool()
    {
        if (projectilePrefab == null) return;
        
        // Create pool parent
        projectilePoolParent = new GameObject("ProjectilePool").transform;
        projectilePoolParent.SetParent(transform);
        
        // Create pooled projectiles
        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject pooledProjectile = Instantiate(projectilePrefab, projectilePoolParent);
            pooledProjectile.SetActive(false);
            
            // Make sure the projectile knows about the pool for returning
            ExorcistProjectile projectileScript = pooledProjectile.GetComponent<ExorcistProjectile>();
            if (projectileScript != null)
            {
                projectileScript.SetPool(this);
            }
            
            projectilePool.Enqueue(pooledProjectile);
        }
    }
    
    public GameObject GetPooledProjectile()
    {
        if (projectilePool.Count > 0)
        {
            return projectilePool.Dequeue();
        }
        
        // If pool is empty, create a new one (shouldn't happen often)
        Debug.LogWarning("Projectile pool exhausted, creating new projectile");
        GameObject newProjectile = Instantiate(projectilePrefab, projectilePoolParent);
        ExorcistProjectile projectileScript = newProjectile.GetComponent<ExorcistProjectile>();
        if (projectileScript != null)
        {
            projectileScript.SetPool(this);
        }
        return newProjectile;
    }
    
    public void ReturnProjectileToPool(GameObject projectile)
    {
        if (projectile != null)
        {
            projectile.SetActive(false);
            projectile.transform.SetParent(projectilePoolParent);
            projectilePool.Enqueue(projectile);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw melee attack range
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * (meleeRange * 0.5f);
        Vector3 boxSize = new Vector3(meleeRange, meleeHeight, 0.1f);
        Gizmos.DrawWireCube(attackPosition, boxSize);
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            
            // Draw firing direction
            Vector2 shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Gizmos.DrawRay(firePoint.position, shootDirection * 2f);
        }
    }
}

// Interface for objects that can take damage
public interface IDamageable
{
    void TakeDamage(float damage);
}
