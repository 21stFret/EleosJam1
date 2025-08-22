using UnityEngine;

public class BookyProjectile : MonoBehaviour
{
    public float damage = 1f;
    private float lifetime = 5f;
    public TrailRenderer lineRenderer;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player who shot it
        if (!other.CompareTag("Player")) return;

        // Check if it's an enemy or damageable object
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Disable();
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Disable();
        }
    }
    private void Disable()
    {
        lifetime = 5f; // Reset lifetime for potential reuse
        gameObject.SetActive(false);
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
