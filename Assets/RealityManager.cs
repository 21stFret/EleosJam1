using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RealityManager : MonoBehaviour
{
    [Header("Reality Settings")]
    public int currentReality = 0;
    public List<Reality> realities = new List<Reality>();
    
    [Header("Input")]
    public InputActionAsset inputActions;
    private InputAction switchRealityAction;
    
    [Header("Visual Settings")]
    public float inactiveOpacity = 0.2f;
    public float transitionSpeed = 2f;
    
    [Header("Audio (Optional)")]
    public AudioClip switchSound;
    private AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Set up input actions
        if (inputActions != null)
        {
            switchRealityAction = inputActions.FindAction("SwitchReality");
        }
    }
    
    void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
            
            if (switchRealityAction != null)
                switchRealityAction.performed += OnSwitchReality;
        }
    }
    
    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            
            if (switchRealityAction != null)
                switchRealityAction.performed -= OnSwitchReality;
        }
    }
    
    void Start()
    {
        // Initialize all realities
        InitializeRealities();
        
        // Set the starting reality
        SwitchToReality(1);
    }

    void InitializeRealities()
    {
        for (int i = 0; i < realities.Count; i++)
        {
            if (realities[i] != null)
            {
                realities[i].Initialize();
            }
        }
        currentReality = -1;
    }
    
    void OnSwitchReality(InputAction.CallbackContext context)
    {
        SwitchToNextReality();
    }
    
    public void SwitchToNextReality()
    {
        int nextReality = (currentReality + 1) % realities.Count;
        SwitchToReality(nextReality);
    }
    
    public void SwitchToReality(int realityIndex)
    {
        if (realityIndex >= realities.Count)
        {
            Debug.LogWarning($"Reality index {realityIndex} is out of range!");
            return;
        }
        
        if (realityIndex == currentReality) return;
        
        currentReality = realityIndex;
        
        // Update all realities
        for (int i = 0; i < realities.Count; i++)
        {
            bool isActive = (i == currentReality);
            realities[i].SetActive(isActive, inactiveOpacity, transitionSpeed);
        }
        
        // Play sound effect
        if (switchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchSound);
        }
        
        Debug.Log($"Switched to Reality {currentReality}");
    }
    
    // Method to add objects to a reality at runtime
    public void AddObjectToReality(int realityIndex, GameObject obj)
    {
        if (realityIndex >= 0 && realityIndex < realities.Count)
        {
            realities[realityIndex].AddObject(obj);
            
            // Update the object's state based on current reality
            bool isActive = (realityIndex == currentReality);
            realities[realityIndex].SetObjectState(obj, isActive, inactiveOpacity);
        }
    }
    
    // Method to remove objects from a reality at runtime
    public void RemoveObjectFromReality(int realityIndex, GameObject obj)
    {
        if (realityIndex >= 0 && realityIndex < realities.Count)
        {
            realities[realityIndex].RemoveObject(obj);
        }
    }
}

[System.Serializable]
public class Reality
{
    [Header("Reality Info")]
    public string realityName = "Reality";
    public Color realityColor = Color.white;
    
    [Header("Objects")]
    public List<GameObject> staticObjects = new List<GameObject>();
    public List<GameObject> activeObjects = new List<GameObject>();
    public List<GameObject> enemies = new List<GameObject>();
    
    // Runtime collections
    private List<RealityObject> allRealityObjects = new List<RealityObject>();
    
    public void Initialize()
    {
        // Clear existing reality objects
        allRealityObjects.Clear();
        
        // Add all objects to the reality object list
        AddObjectsToList(staticObjects);
        AddObjectsToList(activeObjects);
        AddObjectsToList(enemies);
    }
    
    private void AddObjectsToList(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                RealityObject realityObj = obj.GetComponent<RealityObject>();
                if (realityObj == null)
                {
                    realityObj = obj.AddComponent<RealityObject>();
                }
                allRealityObjects.Add(realityObj);
            }
        }
    }
    
    public void SetActive(bool isActive, float inactiveOpacity, float transitionSpeed)
    {
        foreach (RealityObject realityObj in allRealityObjects)
        {
            if (realityObj != null)
            {
                realityObj.SetState(isActive, inactiveOpacity, transitionSpeed);
            }
        }
    }
    
    public void AddObject(GameObject obj)
    {
        if (obj != null && !staticObjects.Contains(obj) && !activeObjects.Contains(obj) && !enemies.Contains(obj))
        {
            staticObjects.Add(obj);
            
            RealityObject realityObj = obj.GetComponent<RealityObject>();
            if (realityObj == null)
            {
                realityObj = obj.AddComponent<RealityObject>();
            }
            allRealityObjects.Add(realityObj);
        }
    }
    
    public void RemoveObject(GameObject obj)
    {
        staticObjects.Remove(obj);
        activeObjects.Remove(obj);
        enemies.Remove(obj);
        
        RealityObject realityObj = obj.GetComponent<RealityObject>();
        if (realityObj != null)
        {
            allRealityObjects.Remove(realityObj);
        }
    }
    
    public void SetObjectState(GameObject obj, bool isActive, float inactiveOpacity)
    {
        RealityObject realityObj = obj.GetComponent<RealityObject>();
        if (realityObj != null)
        {
            realityObj.SetState(isActive, inactiveOpacity, 2f);
        }
    }
}
