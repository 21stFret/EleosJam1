using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private ExorcistCombat combatController;
    private float damage;
    private LayerMask enemyLayerMask;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    
    public void Initialize(ExorcistCombat combat, float meleeDamage, LayerMask enemyMask)
    {
        combatController = combat;
        damage = meleeDamage;
        enemyLayerMask = enemyMask;
    }
    
    void OnEnable()
    {
        // Clear hit enemies when hitbox is enabled (new attack)
        hitEnemies.Clear();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's an enemy and we haven't hit it this attack
        if (IsEnemy(other) && !hitEnemies.Contains(other))
        {
            hitEnemies.Add(other);
            
            // Deal damage to enemy
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // Add knockback if enemy has rigidbody
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
            if (enemyRb != null && combatController != null)
            {
                Vector2 knockbackDirection = (other.transform.position - combatController.transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * 500f, ForceMode2D.Impulse);
            }
        }
    }
    
    bool IsEnemy(Collider2D collider)
    {
        return (enemyLayerMask.value & (1 << collider.gameObject.layer)) > 0;
    }
}
