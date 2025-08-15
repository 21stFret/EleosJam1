using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExorcistProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f;
    public bool piercing = false;
    public int maxHits = 1;
    public float knockbackForce = 10f;
    
    [Header("Visual Effects")]
    public GameObject hitEffectPrefab;
    
    [Header("Audio")]
    public AudioClip hitSound;
    
    // Projectile properties
    private Vector2 direction;
    private float speed;
    private float damage;
    private int hitCount = 0;
    
    // Pooling
    private ExorcistCombat combatPool;
    
    // Components
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private AudioSource audioSource;
    
    // Tracking
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    [Header("Explosion Settings")]
    public GameObject explosionEffectPrefab;
    public float explosionRadius = 5f;
    public LayerMask enemyLayer;
    public bool explosive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Create components if they don't exist
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Projectiles typically don't use gravity
        }
        
        if (projectileCollider == null)
        {
            projectileCollider = gameObject.AddComponent<CircleCollider2D>();
            projectileCollider.isTrigger = true;
        }
        
        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        // Start a coroutine to handle lifetime when activated
        StartCoroutine(LifetimeCounter());
    }
    
    IEnumerator LifetimeCounter()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }
    
    void OnEnable()
    {
        // Reset state when object is reactivated from pool
        hitCount = 0;
        hitTargets.Clear();
        
        // Restart lifetime counter
        StopAllCoroutines();
        StartCoroutine(LifetimeCounter());
    }
    
    public void SetPool(ExorcistCombat pool)
    {
        combatPool = pool;
    }
    
    public void Initialize(Vector2 shootDirection, float projectileSpeed, float projectileDamage)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        explosive = combatPool.isExplosive;
        
        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        
        // Rotate projectile to face direction
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the same target twice (for piercing projectiles)
        if (hitTargets.Contains(other)) return;
        
        // Don't hit the player who shot it
        if (other.CompareTag("Player")) return;
        
        // Check if it's an enemy or damageable object
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            HitTarget(other, damageable);
        }
        // Also check for walls/environment to destroy projectile
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                 other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            HitWall(other);
        }
    }

    void HitTarget(Collider2D target, IDamageable damageable)
    {
        // Add to hit targets
        hitTargets.Add(target);
        hitCount++;

        // Deal damage
        damageable.TakeDamage(damage);

        // Add knockback if target has rigidbody
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = direction;
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }

        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Spawn hit effect
        SpawnHitEffect(transform.position);

        // Check if projectile should be destroyed
        if (!piercing || hitCount >= maxHits)
        {
            ReturnToPool();
        }

        // Handle explosion
        if (explosive)
        {
            Explode();
        }
    }
    
    void HitWall(Collider2D wall)
    {
        // Always destroy projectile when hitting walls
        SpawnHitEffect(transform.position);
        
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        ReturnToPool();
    }
    
    void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    private void Explode()
    {
        // Create explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // Deal damage to nearby enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage * combatPool.explosionPercentage);
            }
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        // Stop all movement and effects
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
                
        // Return to pool if we have a reference, otherwise destroy
        if (combatPool != null)
        {
            StopAllCoroutines();
            combatPool.ReturnProjectileToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void DestroyProjectile()
    {
        // Legacy method - redirect to ReturnToPool for compatibility
        ReturnToPool();
    }
    
    void OnBecameInvisible()
    {
        // Return to pool if projectile goes off screen
        ReturnToPool();
    }
}
