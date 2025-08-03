using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    [Header("SFX Settings")]
    public float masterVolume = 1f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip shotgunSFX;
    [SerializeField] private AudioClip humanHurtSFX;
    [SerializeField] private AudioClip humanDrownSFX;
    [SerializeField] private AudioClip spiritAttackSFX;
    
    [Header("Audio Source Pool")]
    [SerializeField] private int audioSourcePoolSize = 10;
    
    private Dictionary<string, AudioClip> sfxClips;
    private List<AudioSource> audioSourcePool;
    private int currentPoolIndex = 0;
    
    public static SFXManager instance;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeSFXManager()
    {
        // Initialize SFX dictionary
        sfxClips = new Dictionary<string, AudioClip>();
        
        // Add SFX clips to dictionary
        if (shotgunSFX != null) sfxClips.Add("shotgun", shotgunSFX);
        if (humanHurtSFX != null) sfxClips.Add("human hurt", humanHurtSFX);
        if (humanDrownSFX != null) sfxClips.Add("human drown", humanDrownSFX);
        if (spiritAttackSFX != null) sfxClips.Add("spirit attack", spiritAttackSFX);
        
        // Initialize audio source pool
        audioSourcePool = new List<AudioSource>();
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceGO = new GameObject($"SFX_AudioSource_{i}");
            audioSourceGO.transform.SetParent(transform);
            AudioSource audioSource = audioSourceGO.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = masterVolume;
            audioSourcePool.Add(audioSource);
        }
    }
    
    public void PlaySfx(string sfxName)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            AudioClip clipToPlay = sfxClips[sfxName];
            if (clipToPlay != null)
            {
                AudioSource availableSource = GetAvailableAudioSource();
                if (availableSource != null)
                {
                    availableSource.clip = clipToPlay;
                    availableSource.volume = masterVolume;
                    availableSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"SFXManager: Audio clip for '{sfxName}' is null!");
            }
        }
        else
        {
            Debug.LogWarning($"SFXManager: SFX '{sfxName}' not found in dictionary!");
        }
    }
    
    public void PlaySfx(string sfxName, float volume)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            AudioClip clipToPlay = sfxClips[sfxName];
            if (clipToPlay != null)
            {
                AudioSource availableSource = GetAvailableAudioSource();
                if (availableSource != null)
                {
                    availableSource.clip = clipToPlay;
                    availableSource.volume = volume * masterVolume;
                    availableSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"SFXManager: Audio clip for '{sfxName}' is null!");
            }
        }
        else
        {
            Debug.LogWarning($"SFXManager: SFX '{sfxName}' not found in dictionary!");
        }
    }
    
    AudioSource GetAvailableAudioSource()
    {
        // Find an available (not playing) audio source
        for (int i = 0; i < audioSourcePool.Count; i++)
        {
            if (!audioSourcePool[i].isPlaying)
            {
                return audioSourcePool[i];
            }
        }
        
        // If all sources are busy, use round-robin
        AudioSource source = audioSourcePool[currentPoolIndex];
        currentPoolIndex = (currentPoolIndex + 1) % audioSourcePool.Count;
        return source;
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        // Update volume for all audio sources
        foreach (AudioSource source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                source.volume = masterVolume;
            }
        }
    }
    
    public void StopAllSFX()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
    
    // Method to add new SFX clips at runtime
    public void AddSFXClip(string sfxName, AudioClip clip)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            sfxClips[sfxName] = clip;
        }
        else
        {
            sfxClips.Add(sfxName, clip);
        }
    }
    
    // Method to check if SFX exists
    public bool HasSFX(string sfxName)
    {
        return sfxClips.ContainsKey(sfxName);
    }
}
