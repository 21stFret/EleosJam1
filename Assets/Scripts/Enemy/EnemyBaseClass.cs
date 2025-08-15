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
    private ExorcistController playerController;
    protected bool isStunned = false;
    private bool isImmune = false;

    public void StartingImmunity()
    {
        isImmune = true;
        StartCoroutine(RemoveImmunity());
    }

    private IEnumerator RemoveImmunity()
    {
        yield return new WaitForSeconds(0.5f);
        isImmune = false;
    }

    public void TakeDamage(float amount, float stunTime = 0f, float knockbackForce = 0f)
    {
        if (isImmune) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
        if (stunTime > 0f)
        {
            StartCoroutine(StunCoroutine(stunTime));
        }
        if (knockbackForce > 0f)
        {
            // Apply knockback force
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (playerController == null)
                {
                    playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<ExorcistController>();
                }
                Vector2 knockbackDirection = (transform.position - playerController.transform.position).normalized;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        canAttack = false;
        isStunned = true;
        yield return new WaitForSeconds(duration);
        canAttack = true;
        isStunned = false;
    }

    public virtual void Die()
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
