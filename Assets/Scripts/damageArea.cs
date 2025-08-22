using System.Collections;
using UnityEngine;


public class damageArea : MonoBehaviour
{
    public float damageAmount;
    public float damageInterval = 1f;
    public float damageDuration = 2f;
    private bool canDamage = true;
    public ParticleSystem damageEffect;
    private bool _enabled;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_enabled) return;

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
        _enabled = true;
        canDamage = true;
        gameObject.SetActive(true);
        StartCoroutine(DisableDamageAfterDelay());
        damageEffect.Play();
    }

    public void DisableDamage()
    {
        _enabled = false;
        canDamage = false;
        gameObject.SetActive(false);
        damageEffect.Stop();
    }

    private IEnumerator DisableDamageAfterDelay()
    {
        yield return new WaitForSeconds(damageDuration);
        DisableDamage();
    }

}
