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

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider == null)
        {
            Debug.LogError("ExperienceObject requires a Collider2D component.");
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
            if (!targetTransform.CompareTag("Player"))
            {
                if (Vector2.Distance(transform.position, targetTransform.position) > transform.localScale.x)
                {
                    transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, Time.deltaTime * speed);
                }
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, Time.deltaTime * speed);
            if (Vector2.Distance(transform.position, targetTransform.position) <  transform.localScale.x)
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
            //transform.DOPunchScale(Vector3.one * 1.5f, pulseTime).SetEase(Ease.InOutSine);
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
    }
}
