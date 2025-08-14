using UnityEngine;

[System.Serializable]
public class paralaxBackground : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform;
    
    [Header("Parallax Layers")]
    public ParallaxLayer[] parallaxLayers;
    
    [Header("Global Settings")]
    public bool followCameraX = true;
    public bool followCameraY = false;
    public float smoothDamping = 0.1f;
    
    // Private variables
    private Vector3 lastCameraPosition;
    private Vector3 velocity;
    
    void Start()
    {
        // Get camera reference if not assigned
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogError("No camera assigned and no main camera found!");
                return;
            }
        }
        
        // Initialize parallax layers
        InitializeLayers();
        
        // Store initial camera position
        lastCameraPosition = cameraTransform.position;
    }
    
    void InitializeLayers()
    {
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            if (parallaxLayers[i].spriteRenderer == null)
            {
                Debug.LogWarning($"Parallax layer {i} has no sprite renderer assigned!");
                continue;
            }
            
            // Store initial position
            parallaxLayers[i].startPosition = parallaxLayers[i].spriteRenderer.transform.position;
            
            // Calculate sprite dimensions for infinite scrolling
            if (parallaxLayers[i].infiniteScroll)
            {
                Bounds bounds = parallaxLayers[i].spriteRenderer.bounds;
                parallaxLayers[i].spriteWidth = bounds.size.x;
                parallaxLayers[i].spriteHeight = bounds.size.y;
            }
        }
    }
    
    void LateUpdate()
    {
        if (cameraTransform == null) return;
        
        // Calculate camera movement
        Vector3 cameraMovement = cameraTransform.position - lastCameraPosition;
        
        // Update each parallax layer
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            if (layer.spriteRenderer == null) continue;
            
            UpdateParallaxLayer(layer, cameraMovement);
        }
        
        // Update last camera position
        lastCameraPosition = cameraTransform.position;
    }
    
    void UpdateParallaxLayer(ParallaxLayer layer, Vector3 cameraMovement)
    {
        Transform layerTransform = layer.spriteRenderer.transform;
        Vector3 currentPosition = layerTransform.position;
        Vector3 targetPosition = currentPosition;
        
        // Calculate parallax movement
        if (followCameraX)
        {
            float parallaxX = cameraMovement.x * layer.parallaxSpeed;
            targetPosition.x += parallaxX;
        }
        
        if (followCameraY && layer.verticalParallax)
        {
            float parallaxY = cameraMovement.y * layer.verticalSpeed;
            targetPosition.y += parallaxY;
        }
        
        // Apply smooth movement
        if (smoothDamping > 0)
        {
            layerTransform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothDamping);
        }
        else
        {
            layerTransform.position = targetPosition;
        }
        
        // Handle infinite scrolling
        if (layer.infiniteScroll)
        {
            HandleInfiniteScroll(layer);
        }
    }
    
    void HandleInfiniteScroll(ParallaxLayer layer)
    {
        Transform layerTransform = layer.spriteRenderer.transform;
        Vector3 currentPosition = layerTransform.position;
        
        // Horizontal infinite scroll
        if (followCameraX)
        {
            float distanceFromCamera = cameraTransform.position.x - currentPosition.x;
            
            if (distanceFromCamera > layer.spriteWidth)
            {
                currentPosition.x += layer.spriteWidth * 2f;
                layerTransform.position = currentPosition;
            }
            else if (distanceFromCamera < -layer.spriteWidth)
            {
                currentPosition.x -= layer.spriteWidth * 2f;
                layerTransform.position = currentPosition;
            }
        }
        
        // Vertical infinite scroll (if enabled)
        if (followCameraY && layer.verticalParallax)
        {
            float distanceFromCameraY = cameraTransform.position.y - currentPosition.y;
            
            if (distanceFromCameraY > layer.spriteHeight)
            {
                currentPosition.y += layer.spriteHeight * 2f;
                layerTransform.position = currentPosition;
            }
            else if (distanceFromCameraY < -layer.spriteHeight)
            {
                currentPosition.y -= layer.spriteHeight * 2f;
                layerTransform.position = currentPosition;
            }
        }
    }
    
    // Public methods for runtime control
    public void SetParallaxSpeed(int layerIndex, float speed)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Length)
        {
            parallaxLayers[layerIndex].parallaxSpeed = Mathf.Clamp01(speed);
        }
    }
    
    public void SetVerticalParallaxSpeed(int layerIndex, float speed)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Length)
        {
            parallaxLayers[layerIndex].verticalSpeed = Mathf.Clamp01(speed);
        }
    }
    
    public void ToggleInfiniteScroll(int layerIndex, bool enable)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Length)
        {
            parallaxLayers[layerIndex].infiniteScroll = enable;
        }
    }
    
    public void ResetLayerPosition(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Length)
        {
            parallaxLayers[layerIndex].spriteRenderer.transform.position = parallaxLayers[layerIndex].startPosition;
        }
    }
    
    public void ResetAllLayers()
    {
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            ResetLayerPosition(i);
        }
        lastCameraPosition = cameraTransform.position;
    }
    
}
