using UnityEngine;

[System.Serializable]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Layer Settings")]
    public SpriteRenderer spriteRenderer;
    [Range(0f, 1f)]
    public float parallaxSpeed = 0.5f;
    public bool infiniteScroll = true;
    
    [Header("Optional Settings")]
    public bool verticalParallax = false;
    [Range(0f, 1f)]
    public float verticalSpeed = 0.2f;
    
    // Internal variables
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public float spriteWidth;
    [HideInInspector] public float spriteHeight;
}