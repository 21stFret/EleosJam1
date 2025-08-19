using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExorcistCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float meleeCooldown = 0.8f;
    public float rangedCooldown = 1.2f;
    public float stunTime = 0.1f;
    
    [Header("Melee Attack (Reality 1)")]
    public float meleeRange = 1.5f;
    public float meleeHeight = 1f;
    public float meleeDamage = 50f;
    public LayerMask enemyLayerMask = 1;
    public float meleeAttackDuration = 0.2f;
    public float meleeKnockbackForce = 5f;
    public float meleeOffset = 0.5f;

    [Header("Ranged Attack (Reality 0)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float projectileDamage = 30f;
    public int projectilePoolSize = 10;
    public float projectileAmount = 1f;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionPercentage = 0.5f;
    public bool isExplosive = false;


    [Header("Visual Effects")]
    public GameObject meleeEffectPrefab;
    public GameObject muzzleFlashPrefab;
    
    [Header("Audio")]
    public AudioClip meleeAttackSound;
    public AudioClip rangedAttackSound;
    
    // Components and references
    private RealityManager realityManager;
    private AudioSource audioSource;
    private ExorcistController playerController;
    
    // Input System
    public InputActionAsset inputActions;
    private InputAction attackAction;
    
    // Combat state
    private float lastMeleeAttackTime;
    private float lastRangedAttackTime;
    private bool isAttacking;
    
    // Melee attack optimization - persistent hitbox
    public MeleeHitbox meleeHitbox;
    public BoxCollider2D meleeCollider;
    
    // Projectile pooling
    private Queue<GameObject> projectilePool = new Queue<GameObject>();
    private Transform projectilePoolParent;
    private bool isInitialized;

    public void Init()
    {
        if (isInitialized) return;

        isInitialized = true;

        // Get components
        realityManager = RealityManager.Instance;
        audioSource = GetComponent<AudioSource>();
        playerController = GetComponent<ExorcistController>();

        // Create audio source if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set up input actions
        //inputActions = GameManager.Instance?.playerInput.actions;
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

        meleeCollider = meleeHitbox.GetComponent<BoxCollider2D>();
        meleeHitbox.Initialize(this);
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
            int currentReality = realityManager.currentRealityIndex;
            
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
        UpdateMeleeCollider();
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
        EnableMeleeHitbox(true);
        
        // Spawn visual effect
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        if (meleeEffectPrefab != null)
        {
            // Flip effect based on player direction
            //meleeEffectPrefab.transform.localScale = attackDirection;
            meleeEffectPrefab.GetComponent<Animator>().SetTrigger("Attack");
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

        // Set projectile direction based on mouse position
        Vector2 shootDirection = (Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - firePoint.position).normalized;
        
        for (int i = 0; i < projectileAmount; i++)
        {
            if (projectileAmount == 1)
            {
                SpawnProjectile(shootDirection);
                return;
            }
            Vector2 spreadDirection = Quaternion.Euler(0, 0, -10f + i * (20f / (projectileAmount - 1))) * shootDirection;
            SpawnProjectile(spreadDirection);
        }

    }

    void SpawnProjectile(Vector2 direction)
    {
        GameObject projectile = GetPooledProjectile();
        if (projectile != null && firePoint != null)
        {
            // Position and activate projectile
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            projectile.transform.SetParent(null); // Detach from pool parent
            projectile.SetActive(true);

            // Configure projectile
            ExorcistProjectile projectileScript = projectile.GetComponent<ExorcistProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, projectileSpeed, projectileDamage);
            }
        }
    }

    public void UpdateMeleeCollider()
    {
        if (meleeCollider != null)
        {
            meleeCollider.size = new Vector2(meleeHeight, meleeRange);
            meleeCollider.offset = new Vector2(0, (meleeRange - 1) * 0.5f);
        }
        meleeHitbox.Initialize(this);
    }

    void EnableMeleeHitbox(bool enable)
    {
        if (meleeHitbox != null)
        {
            Vector2 meleeDirection = (Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position).normalized;
            meleeHitbox.gameObject.transform.position = transform.position + (Vector3)meleeDirection.normalized  * meleeOffset;
            meleeHitbox.gameObject.transform.rotation = Quaternion.LookRotation(meleeHitbox.gameObject.transform.forward, meleeDirection);
            meleeHitbox.gameObject.SetActive(enable);
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
}

// Interface for objects that can take damage
public interface IDamageable
{
    void TakeDamage(float damage, float stunTime = 0f, float knockbackForce = 0f);
}
