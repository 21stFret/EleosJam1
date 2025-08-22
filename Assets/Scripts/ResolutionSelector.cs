using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResolutionSelector : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Text currentResolutionText;
    
    [Header("Settings")]
    public bool fullscreenOnly = false;
    public bool sortByResolution = true;
    public bool showRefreshRate = true;
    
    private Resolution[] availableResolutions;
    private List<Resolution> filteredResolutions;
    
    void Start()
    {
        SetupResolutionSelector();
    }
    
    void SetupResolutionSelector()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("Resolution dropdown is not assigned!");
            return;
        }
        
        // Get available resolutions
        availableResolutions = Screen.resolutions;
        
        // Filter resolutions (remove duplicates with different refresh rates if needed)
        FilterResolutions();
        
        // Populate dropdown
        PopulateDropdown();
        
        // Set current resolution
        SetCurrentResolution();
        
        // Add listener for dropdown changes
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        
        // Update current resolution text
        UpdateCurrentResolutionText();
    }
    
    void FilterResolutions()
    {
        filteredResolutions = new List<Resolution>();
        
        if (fullscreenOnly)
        {
            // Only include resolutions that work well in fullscreen
            foreach (Resolution resolution in availableResolutions)
            {
                // Filter out very small resolutions and duplicates
                if (resolution.width >= 800 && resolution.height >= 600)
                {
                    // Check if we already have this resolution (different refresh rate)
                    bool alreadyAdded = filteredResolutions.Any(r => 
                        r.width == resolution.width && r.height == resolution.height);
                    
                    if (!alreadyAdded || showRefreshRate)
                    {
                        filteredResolutions.Add(resolution);
                    }
                }
            }
        }
        else
        {
            // Include all resolutions
            foreach (Resolution resolution in availableResolutions)
            {
                if (!showRefreshRate)
                {
                    // Only add if we don't already have this width/height combination
                    bool alreadyAdded = filteredResolutions.Any(r => 
                        r.width == resolution.width && r.height == resolution.height);
                    
                    if (!alreadyAdded)
                    {
                        filteredResolutions.Add(resolution);
                    }
                }
                else
                {
                    filteredResolutions.Add(resolution);
                }
            }
        }
        
        // Sort resolutions if requested
        if (sortByResolution)
        {
            filteredResolutions = filteredResolutions.OrderByDescending(r => r.width * r.height)
                                                   .ThenByDescending(r => r.refreshRateRatio.value)
                                                   .ToList();
        }
    }
    
    void PopulateDropdown()
    {
        resolutionDropdown.ClearOptions();
        
        List<string> resolutionOptions = new List<string>();
        
        foreach (Resolution resolution in filteredResolutions)
        {
            string resolutionString;
            
            if (showRefreshRate)
            {
                resolutionString = $"{resolution.width} x {resolution.height} @ {resolution.refreshRateRatio.value:0}Hz";
            }
            else
            {
                resolutionString = $"{resolution.width} x {resolution.height}";
            }
            
            resolutionOptions.Add(resolutionString);
        }
        
        resolutionDropdown.AddOptions(resolutionOptions);
    }
    
    void SetCurrentResolution()
    {
        Resolution currentResolution = Screen.currentResolution;
        
        // Find the current resolution in our filtered list
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            Resolution resolution = filteredResolutions[i];
            
            if (resolution.width == currentResolution.width && 
                resolution.height == currentResolution.height)
            {
                // If we're showing refresh rates, match that too
                if (!showRefreshRate || 
                    Mathf.Approximately((float)resolution.refreshRateRatio.value, (float)currentResolution.refreshRateRatio.value))
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
        }
    }
    
    public void OnResolutionChanged(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < filteredResolutions.Count)
        {
            Resolution selectedResolution = filteredResolutions[resolutionIndex];
            
            // Apply the new resolution
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, FullScreenMode.Windowed);
            
            // Update the current resolution text
            UpdateCurrentResolutionText();
            
            // Save the resolution preference
            SaveResolutionPreference(selectedResolution);
            
            Debug.Log($"Resolution changed to: {selectedResolution.width}x{selectedResolution.height} @ {selectedResolution.refreshRateRatio.value}Hz");
        }
    }
    
    void UpdateCurrentResolutionText()
    {
        if (currentResolutionText != null)
        {
            Resolution current = Screen.currentResolution;
            string fullscreenText = Screen.fullScreen ? " (Fullscreen)" : " (Windowed)";
            currentResolutionText.text = $"Current: {current.width}x{current.height}{fullscreenText}";
        }
    }
    
    void SaveResolutionPreference(Resolution resolution)
    {
        PlayerPrefs.SetInt("ScreenWidth", resolution.width);
        PlayerPrefs.SetInt("ScreenHeight", resolution.height);
        PlayerPrefs.SetFloat("RefreshRate", (float)resolution.refreshRateRatio.value);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void LoadResolutionPreference()
    {
        if (PlayerPrefs.HasKey("ScreenWidth"))
        {
            int width = PlayerPrefs.GetInt("ScreenWidth");
            int height = PlayerPrefs.GetInt("ScreenHeight");
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            
            Screen.SetResolution(width, height, fullscreen);
        }
    }
    
    // Public methods for other UI elements
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateCurrentResolutionText();
        SaveResolutionPreference(Screen.currentResolution);
    }
    
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        UpdateCurrentResolutionText();
        SaveResolutionPreference(Screen.currentResolution);
    }
    
    public void RefreshResolutionList()
    {
        SetupResolutionSelector();
    }
    
    // Context menu for testing
    [ContextMenu("Refresh Resolution List")]
    private void ContextRefreshResolutions()
    {
        RefreshResolutionList();
    }
    
    [ContextMenu("Log Current Resolution")]
    private void LogCurrentResolution()
    {
        Resolution current = Screen.currentResolution;
        Debug.Log($"Current Resolution: {current.width}x{current.height} @ {current.refreshRateRatio.value}Hz, Fullscreen: {Screen.fullScreen}");
    }
    
    [ContextMenu("Apply Native Resolution")]
    private void ApplyNativeResolution()
    {
        Resolution native = Screen.currentResolution;
        Screen.SetResolution(native.width, native.height, true);
        SetCurrentResolution();
        UpdateCurrentResolutionText();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Update resolution text when returning to the application
            UpdateCurrentResolutionText();
        }
    }
}
