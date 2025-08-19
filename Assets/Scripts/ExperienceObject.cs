using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class ExperienceObject : MonoBehaviour
{
    private Collider2D _collider;
    public int experienceAmount = 10;
    public RealityType realityType;
    public bool _active = false;
    public Transform targetTransform;
    public float pulseTime = 0.5f;
    private float pulseTimer = 0f;
    public float speed = 1f;
    public ParticleSystem pickupEffectLiving, pickupEffectSpirit;
    public bool forceSpawn = false;
    public float pickupDistance = 1f;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogError("ExperienceObject requires a Collider2D component.");
        }
        if (forceSpawn)
        {
            ToggleActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            targetTransform = collider.transform;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            targetTransform = null;
        }
    }

    void Update()
    {
        if (_active && targetTransform != null)
        {
            float stoppingDistance = transform.localScale.x * 1.5f;
            if (!targetTransform.CompareTag("Player"))
            {
                if (Vector2.Distance(transform.position, targetTransform.position) > stoppingDistance)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, Time.deltaTime * speed);
                }
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, Time.deltaTime * speed);
            if (Vector2.Distance(transform.position, targetTransform.position) < pickupDistance)
            {
                ExperienceManager.Instance.GiveExperienceToPlayer(experienceAmount, realityType);
                ToggleActive(false);
            }
            return;
        }


        pulseTimer += Time.deltaTime;
        if (pulseTimer >= pulseTime)
        {
            pulseTimer = 0f;
            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, 5f))
            {
                if (collider.GetComponent<ExperienceObject>() != null && collider.transform != transform)
                {
                    targetTransform = collider.transform;
                    break;
                }
            }
        }
    }

    public void ToggleActive(bool isActive)
    {
        gameObject.SetActive(isActive);
        _collider.enabled = isActive;
        _active = isActive;
        if (isActive)
        {
            SwitchParticleSystems();
        }
        else
        {
            pickupEffectLiving.Stop();
            pickupEffectSpirit.Stop();
        }
    }

    private void SwitchParticleSystems()
    {
        if(realityType == RealityType.Living)
        {
            pickupEffectSpirit.Stop();
            pickupEffectLiving.Play();
        }
        else if(realityType == RealityType.Spirit)
        {
            pickupEffectLiving.Stop();
            pickupEffectSpirit.Play();
        }
    }
}
