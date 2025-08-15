using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private ExorcistCombat combatController;
    private float damage;
    private float stunTime;
    private LayerMask enemyLayerMask;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();
    public BoxCollider2D hitboxCollider;
    private float knockbackForce = 5f;
    public SpriteRenderer sR;

    public void Initialize(ExorcistCombat combat)
    {
        combatController = combat;
        damage = combat.meleeDamage;
        enemyLayerMask = combat.enemyLayerMask;
        stunTime = combat.stunTime;
        knockbackForce = combat.meleeKnockbackForce;
        hitboxCollider = combat.meleeCollider;
        sR.transform.localScale = new Vector3(sR.transform.localScale.x, combat.meleeRange, 1);
        sR.transform.localPosition = new Vector3(hitboxCollider.offset.x, hitboxCollider.offset.y, 0);
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
                damageable.TakeDamage(damage, stunTime, knockbackForce);
            }
        
        }
    }
    
    bool IsEnemy(Collider2D collider)
    {
        return collider.CompareTag("Enemy");
    }
}
