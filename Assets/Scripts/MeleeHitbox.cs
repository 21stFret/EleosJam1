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
    public float scaleEffector;

    public void Initialize(ExorcistCombat combat)
    {
        combatController = combat;
        damage = combat.meleeDamage;
        enemyLayerMask = combat.enemyLayerMask;
        stunTime = combat.stunTime;
        knockbackForce = combat.meleeKnockbackForce;
        hitboxCollider = combat.meleeCollider;
        hitboxCollider.size = new Vector2(combat.meleeRange, combat.meleeRange);
        sR.transform.localScale = new Vector3(combat.meleeRange * scaleEffector, combat.meleeRange * scaleEffector, 1);
        sR.transform.localPosition = new Vector3(0, hitboxCollider.offset.y, 0);
    }

    void OnEnable()
    {
        // Clear hit enemies when hitbox is enabled (new attack)
        hitEnemies.Clear();
        Vector2 size = new Vector2(hitboxCollider.size.x, hitboxCollider.size.y);
        float angle = transform.rotation.eulerAngles.z;
        Vector2 boxPhysCenter = transform.position + transform.up * hitboxCollider.offset.y;
        var hitColliders = Physics2D.OverlapBoxAll(boxPhysCenter, size, angle, enemyLayerMask);

        // Debug draw the actual overlap box area with rotation
        Vector3 boxCenter = boxPhysCenter;

        // Calculate rotated corners
        float angleRad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        
        // Local corner positions (relative to center)
        Vector2[] localCorners = new Vector2[]
        {
            new Vector2(-size.x/2, -size.y/2), // Bottom Left
            new Vector2(size.x/2, -size.y/2),  // Bottom Right
            new Vector2(size.x/2, size.y/2),   // Top Right
            new Vector2(-size.x/2, size.y/2)   // Top Left
        };
        
        // Rotate and translate corners to world space
        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 local = localCorners[i];
            Vector2 rotated = new Vector2(
                local.x * cos - local.y * sin,
                local.x * sin + local.y * cos
            );
            worldCorners[i] = boxCenter + (Vector3)rotated;
        }
        
        // Draw the rotated box outline
        Debug.DrawLine(worldCorners[0], worldCorners[1], Color.red, 2f); // Bottom
        Debug.DrawLine(worldCorners[1], worldCorners[2], Color.red, 2f); // Right
        Debug.DrawLine(worldCorners[2], worldCorners[3], Color.red, 2f); // Top
        Debug.DrawLine(worldCorners[3], worldCorners[0], Color.red, 2f); // Left

        foreach (var collider in hitColliders)
        {
            if (IsEnemy(collider))
            {
                hitEnemies.Add(collider);

                // Deal damage to enemy
                IDamageable damageable = collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage, stunTime, knockbackForce);
                }
            }
        }
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
