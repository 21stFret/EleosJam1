using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RealityObject : MonoBehaviour
{
    [Header("Object Settings")]
    public bool disableWhenInactive = false;
    public bool disableCollidersWhenInactive = true;
    
    // Component references
    private Collider2D[] colliders;
    private Tilemap[] tilemaps;

    public SpriteRenderer minimap;

    
    // Original values
    public Color originalColor;
    private bool[] originalColliderStates;
    
    // Current state
    private bool isActive = true;
    private Coroutine transitionCoroutine;
    
    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        // Get all colliders
        colliders = GetComponentsInChildren<Collider2D>();
        originalColliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            originalColliderStates[i] = colliders[i].enabled;
        }

        // Get all Tilemaps (including children)
        tilemaps = GetComponentsInChildren<Tilemap>();

        /*
        // Get all behaviors (scripts that might need to be disabled)
        behaviors = GetComponentsInChildren<MonoBehaviour>();
        originalBehaviorStates = new bool[behaviors.Length];
        for (int i = 0; i < behaviors.Length; i++)
        {
            // Don't disable this script or essential Unity components
            if (behaviors[i] != this && 
                !(behaviors[i] is Transform) && 
                !(behaviors[i] is SpriteRenderer) && 
                !(behaviors[i] is Collider2D))
            {
                originalBehaviorStates[i] = behaviors[i].enabled;
            }
            else
            {
                originalBehaviorStates[i] = true;
            }
        }
        */
    }
    
    public void SetState(bool active, float inactiveOpacity, float transitionSpeed)
    {
        if (isActive == active) return;
        
        isActive = active;
        
        // Stop any existing transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        // Start new transition
        transitionCoroutine = StartCoroutine(TransitionToState(active, inactiveOpacity, transitionSpeed));
    }
    
    private IEnumerator TransitionToState(bool active, float inactiveOpacity, float transitionSpeed)
    {
        float targetAlpha = active ? 1f : inactiveOpacity;
        float currentAlpha = tilemaps.Length > 0 ? tilemaps[0].color.a : 1f;

        // Handle colliders and behaviors immediately
        SetCollidersState(active);
        if (disableWhenInactive)
        {
            //SetBehaviorsState(active);
        }
        
        // Animate sprite transparency
        float elapsedTime = 0f;
        float duration = Mathf.Abs(targetAlpha - currentAlpha) / transitionSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float alpha = Mathf.Lerp(currentAlpha, targetAlpha, progress);
            
            SetSpriteAlpha(alpha);
            
            yield return null;
        }
        
        // Ensure final values are set
        SetSpriteAlpha(targetAlpha);
        
        transitionCoroutine = null;
    }
    
    private void SetSpriteAlpha(float alpha)
    {
        for (int i = 0; i < tilemaps.Length; i++)
        {
            if (tilemaps[i] != null)
            {
                Color color = originalColor;
                color.a = alpha;
                tilemaps[i].color = color;
            }
        }
    }
    
    private void SetCollidersState(bool active)
    {
        if (!disableCollidersWhenInactive && !active) return;
        
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = active ? originalColliderStates[i] : false;
            }
        }
    }
    
    void OnDestroy()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
    }
}
