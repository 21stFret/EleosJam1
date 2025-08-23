using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    // Mixer parameter names (must match your Audio Mixer exposed parameters)
    private const string MUSIC_VOLUME = "BGM";
    private const string SFX_VOLUME = "SFX";

    // Singleton instance
    public static AudioManager Instance { get; private set; }

    public List<AudioClip> musicClips;
    public List<AudioClip> sfxClips;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Load saved volumes
        LoadVolumeSettings();
        
        // Apply initial volumes
        ApplyAllVolumes();
    }
    
    void InitializeAudioManager()
    {        
        // Assign mixer groups if available
        if (audioMixer != null)
        {
            var musicGroups = audioMixer.FindMatchingGroups("BGM");
            var sfxGroups = audioMixer.FindMatchingGroups("SFX");
            
            if (musicGroups.Length > 0) musicSource.outputAudioMixerGroup = musicGroups[0];
            if (sfxGroups.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroups[0];
        }
    }
    
    #region Volume Control Methods
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SetMixerVolume(MUSIC_VOLUME, musicVolume);
        SaveVolumeSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SetMixerVolume(SFX_VOLUME, sfxVolume);
        SaveVolumeSettings();
    }
    
    private void SetMixerVolume(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            // Convert 0-1 range to decibel range (-80dB to 0dB)
            float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            
            // Try to set the parameter, catch error if it doesn't exist
            try
            {
                audioMixer.SetFloat(parameterName, dbVolume);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Audio Mixer parameter '{parameterName}' not found or not exposed. " +
                               $"Please check your Audio Mixer setup. Error: {e.Message}");
            }
        }
    }
    
    private void ApplyAllVolumes()
    {
        SetMixerVolume(MUSIC_VOLUME, musicVolume);
        SetMixerVolume(SFX_VOLUME, sfxVolume);
    }
    
    #endregion
    
    #region Audio Playback Methods
    
    public void PlayMusic(AudioClip clip, bool loop = true, float fadeInTime = 1f)
    {
        if (musicSource != null && clip != null)
        {
            if (fadeInTime > 0f)
            {
                StartCoroutine(FadeInMusic(clip, loop, fadeInTime));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }
    }
    
    public void StopMusic(float fadeOutTime = 0f)
    {
        if (musicSource != null)
        {
            if (fadeOutTime > 0f)
            {
                StartCoroutine(FadeOutMusic(fadeOutTime));
            }
            else
            {
                musicSource.Stop();
            }
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    #endregion
    
    #region Music Fade Effects
    
    private System.Collections.IEnumerator FadeInMusic(AudioClip clip, bool loop, float fadeTime)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();
        
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }
        
        musicSource.volume = 1f;
    }
    
    private System.Collections.IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }
        
        musicSource.volume = 0f;
        musicSource.Stop();
        musicSource.volume = startVolume;
    }
    
    #endregion
    
    #region Settings Persistence
    
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
    
    #endregion
    
    #region Utility Methods
    
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    public void ToggleMusicMute()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
    }
    
    public void ToggleSFXMute()
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !sfxSource.mute;
        }
    }
    
    // Context menu for testing in editor
    [ContextMenu("Reset All Volumes")]
    private void ResetVolumes()
    {
        SetMusicVolume(1f);
        SetSFXVolume(1f);
    }
    
    #endregion
}
