using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseClass : MonoBehaviour, IDamageable
{
    public float health = 100;
    public float speed = 2f;
    public bool isAlive = true;
    public float attackCooldown = 1f;
    protected float lastAttackTime = 0f;
    public float attackRange = 1.5f;
    public float damage = 10f;
    public bool canAttack = true;
    public ParticleSystem deathEffect;
    public RealityType realityType;

    public void TakeDamage(float amount, float stunTime = 0f)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
        if (stunTime > 0f)
        {
            StartCoroutine(StunCoroutine(stunTime));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        canAttack = false;
        float originalSpeed = speed;
        speed = 0f; // Stop movement during stun
        yield return new WaitForSeconds(duration);
        speed = originalSpeed; // Restore movement speed after stun
        canAttack = true;
    }

    protected virtual void Die()
    {
        isAlive = false;

        if (deathEffect != null)
        {
            deathEffect.transform.position = transform.position;
            deathEffect.transform.parent = null; // Detach from parent to avoid being destroyed
            deathEffect.Play();
        }

        // Notify GameManager if it exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonitorEnemy(this);
        }

        gameObject.SetActive(false);

        // Give exp to the player
        ExperienceManager.Instance.SpawnExperienceOrbs(transform.position, realityType);
    }

    public virtual void Update()
    {
        if (!isAlive) return;

        // Implement enemy behavior here
        if (!canAttack)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                canAttack = true;
            }
        }
    }
}
