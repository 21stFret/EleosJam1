using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Volume Labels (Optional)")]
    public TMP_Text musicVolumeLabel;
    public TMP_Text sfxVolumeLabel;
    
    [Header("Settings")]
    public bool updateLabels = true;
    public string labelFormat = "{0:0%}"; // Shows as percentage
    
    void Start()
    {
        InitializeSliders();
        SetupSliderListeners();
        UpdateUI();
    }
    
    void InitializeSliders()
    {
        // Wait for AudioManager to initialize
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Make sure AudioManager exists in the scene.");
            return;
        }
        
        // Set slider values to current AudioManager volumes
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.musicVolume;
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
        }
    }
    
    void SetupSliderListeners()
    {
        // Add listeners to sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }
    
    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateMusicLabel();
        }
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            UpdateSFXLabel();
        }
    }
    
    void UpdateUI()
    {
        UpdateMusicLabel();
        UpdateSFXLabel();
    }
    
    void UpdateMusicLabel()
    {
        if (updateLabels && musicVolumeLabel != null && musicVolumeSlider != null)
        {
            musicVolumeLabel.text = string.Format(labelFormat, musicVolumeSlider.value);
        }
    }
    
    void UpdateSFXLabel()
    {
        if (updateLabels && sfxVolumeLabel != null && sfxVolumeSlider != null)
        {
            sfxVolumeLabel.text = string.Format(labelFormat, sfxVolumeSlider.value);
        }
    }
    
    // Public method to refresh UI (useful if AudioManager values change externally)
    public void RefreshUI()
    {
        if (AudioManager.Instance != null)
        {
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
            }
            
            UpdateUI();
        }
    }
    
    // Utility methods for buttons or other UI elements
    public void ResetMusicVolume()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 1f;
        }
    }
    
    public void ResetSFXVolume()
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 1f;
        }
    }
    
    public void ResetAllVolumes()
    {
        ResetMusicVolume();
        ResetSFXVolume();
    }
    
    public void MuteMusicToggle()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolumeSlider.value > 0 ? 0f : 1f;
        }
    }
    
    public void MuteSFXToggle()
    {
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolumeSlider.value > 0 ? 0f : 1f;
        }
    }
    
    // Context menu for testing
    [ContextMenu("Refresh UI")]
    private void ContextRefreshUI()
    {
        RefreshUI();
    }
    
    [ContextMenu("Test Reset Volumes")]
    private void ContextResetVolumes()
    {
        ResetAllVolumes();
    }
    
    void OnValidate()
    {
        // Update labels in editor when slider values change
        if (Application.isPlaying)
        {
            UpdateUI();
        }
    }
}
