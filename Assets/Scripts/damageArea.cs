using System.Collections;
using UnityEngine;


public class damageArea : MonoBehaviour
{
    public float damageAmount;
    public float damageInterval = 1f;
    private bool canDamage = true;
    public ParticleSystem damageEffect;
    public bool _enabled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage || !_enabled) return;
        
        if (!other.CompareTag("Player"))
        {
            return;
        }

        other.TryGetComponent(out IDamageable damageable);
        if (damageable != null)
        {
            StartCoroutine(DealDamage(damageable));
            canDamage = false;
        }
    }

    private IEnumerator DealDamage(IDamageable damageable)
    {
        damageable.TakeDamage(damageAmount);
        yield return new WaitForSeconds(damageInterval);
        canDamage = true;
        _enabled = false;
        damageEffect.Stop();
    }

    public void EnableDamage()
    {
        canDamage = true;
        gameObject.SetActive(true);
    }

    public void DisableDamage()
    {
        canDamage = false;
        gameObject.SetActive(false);
    }

}
