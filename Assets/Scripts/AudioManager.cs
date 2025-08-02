using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Music Tracks")]
    public AudioClipData[] musicTracks;

    [Header("Sound Effects")]
    public AudioClipData[] soundEffects;

    [Header("Ambient Sounds")]
    public AudioClipData[] ambientSounds;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;

    [Header("Transition Settings")]
    public float musicFadeTime = 2f;
    public float ambientFadeTime = 1f;

    // Private variables
    private Dictionary<string, AudioClipData> musicDict;
    private Dictionary<string, AudioClipData> sfxDict;
    private Dictionary<string, AudioClipData> ambientDict;
    private string currentMusicTrack = "";
    private string currentAmbientTrack = "";
    private Coroutine musicFadeCoroutine;
    private Coroutine ambientFadeCoroutine;

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

    void InitializeAudioManager()
    {
        // Create dictionaries for quick lookup
        musicDict = new Dictionary<string, AudioClipData>();
        sfxDict = new Dictionary<string, AudioClipData>();
        ambientDict = new Dictionary<string, AudioClipData>();

        // Populate dictionaries
        foreach (var music in musicTracks)
        {
            if (!string.IsNullOrEmpty(music.name))
                musicDict[music.name] = music;
        }

        foreach (var sfx in soundEffects)
        {
            if (!string.IsNullOrEmpty(sfx.name))
                sfxDict[sfx.name] = sfx;
        }

        foreach (var ambient in ambientSounds)
        {
            if (!string.IsNullOrEmpty(ambient.name))
                ambientDict[ambient.name] = ambient;
        }

        // Setup audio sources
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }

        if (ambientSource == null)
        {
            GameObject ambientGO = new GameObject("Ambient Source");
            ambientGO.transform.SetParent(transform);
            ambientSource = ambientGO.AddComponent<AudioSource>();
        }

        // Configure audio sources
        musicSource.loop = true;
        ambientSource.loop = true;
        sfxSource.loop = false;
    }

    #region Music Control

    public void PlayMusic(string trackName, bool fadeIn = true)
    {
        if (!musicDict.ContainsKey(trackName))
        {
            Debug.LogWarning($"Music track '{trackName}' not found!");
            return;
        }

        AudioClipData trackData = musicDict[trackName];

        if (currentMusicTrack == trackName && musicSource.isPlaying)
        {
            return; // Already playing this track
        }

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeMusicTo(trackData, fadeIn));
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fadeOut)
        {
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
            currentMusicTrack = "";
        }
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    IEnumerator FadeMusicTo(AudioClipData newTrack, bool fadeIn)
    {
        float startVolume = musicSource.volume;

        // Fade out current music
        if (musicSource.isPlaying)
        {
            float fadeOutTime = musicFadeTime * 0.5f;
            for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeOutTime);
                yield return null;
            }
        }

        // Switch to new track
        musicSource.clip = newTrack.clip;
        musicSource.volume = 0;
        musicSource.pitch = newTrack.pitch;
        musicSource.Play();
        currentMusicTrack = newTrack.name;

        // Fade in new music
        if (fadeIn)
        {
            float targetVolume = newTrack.volume * musicVolume * masterVolume;
            float fadeInTime = musicFadeTime * 0.5f;
            for (float t = 0; t < fadeInTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / fadeInTime);
                yield return null;
            }
            musicSource.volume = targetVolume;
        }
        else
        {
            musicSource.volume = newTrack.volume * musicVolume * masterVolume;
        }

        musicFadeCoroutine = null;
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < musicFadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeTime);
            yield return null;
        }
        musicSource.Stop();
        currentMusicTrack = "";
        musicFadeCoroutine = null;
    }

    #endregion

    #region Sound Effects

    public void PlaySFX(string sfxName)
    {
        if (!sfxDict.ContainsKey(sfxName))
        {
            Debug.LogWarning($"Sound effect '{sfxName}' not found!");
            return;
        }

        AudioClipData sfxData = sfxDict[sfxName];
        sfxSource.pitch = sfxData.pitch;
        sfxSource.PlayOneShot(sfxData.clip, sfxData.volume * sfxVolume * masterVolume);
    }

    public void PlaySFX(string sfxName, float volumeScale)
    {
        if (!sfxDict.ContainsKey(sfxName))
        {
            Debug.LogWarning($"Sound effect '{sfxName}' not found!");
            return;
        }

        AudioClipData sfxData = sfxDict[sfxName];
        sfxSource.pitch = sfxData.pitch;
        sfxSource.PlayOneShot(sfxData.clip, sfxData.volume * volumeScale * sfxVolume * masterVolume);
    }

    #endregion

    #region Ambient Sounds

    public void PlayAmbient(string ambientName, bool fadeIn = true)
    {
        if (!ambientDict.ContainsKey(ambientName))
        {
            Debug.LogWarning($"Ambient sound '{ambientName}' not found!");
            return;
        }

        AudioClipData ambientData = ambientDict[ambientName];

        if (currentAmbientTrack == ambientName && ambientSource.isPlaying)
        {
            return; // Already playing this ambient
        }

        if (ambientFadeCoroutine != null)
        {
            StopCoroutine(ambientFadeCoroutine);
        }

        ambientFadeCoroutine = StartCoroutine(FadeAmbientTo(ambientData, fadeIn));
    }

    public void StopAmbient(bool fadeOut = true)
    {
        if (ambientFadeCoroutine != null)
        {
            StopCoroutine(ambientFadeCoroutine);
        }

        if (fadeOut)
        {
            ambientFadeCoroutine = StartCoroutine(FadeOutAmbient());
        }
        else
        {
            ambientSource.Stop();
            currentAmbientTrack = "";
        }
    }

    IEnumerator FadeAmbientTo(AudioClipData newAmbient, bool fadeIn)
    {
        float startVolume = ambientSource.volume;

        // Fade out current ambient
        if (ambientSource.isPlaying)
        {
            float fadeOutTime = ambientFadeTime * 0.5f;
            for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
            {
                ambientSource.volume = Mathf.Lerp(startVolume, 0, t / fadeOutTime);
                yield return null;
            }
        }

        // Switch to new ambient
        ambientSource.clip = newAmbient.clip;
        ambientSource.volume = 0;
        ambientSource.pitch = newAmbient.pitch;
        ambientSource.Play();
        currentAmbientTrack = newAmbient.name;

        // Fade in new ambient
        if (fadeIn)
        {
            float targetVolume = newAmbient.volume * ambientVolume * masterVolume;
            float fadeInTime = ambientFadeTime * 0.5f;
            for (float t = 0; t < fadeInTime; t += Time.deltaTime)
            {
                ambientSource.volume = Mathf.Lerp(0, targetVolume, t / fadeInTime);
                yield return null;
            }
            ambientSource.volume = targetVolume;
        }
        else
        {
            ambientSource.volume = newAmbient.volume * ambientVolume * masterVolume;
        }

        ambientFadeCoroutine = null;
    }

    IEnumerator FadeOutAmbient()
    {
        float startVolume = ambientSource.volume;
        for (float t = 0; t < ambientFadeTime; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, 0, t / ambientFadeTime);
            yield return null;
        }
        ambientSource.Stop();
        currentAmbientTrack = "";
        ambientFadeCoroutine = null;
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        UpdateAmbientVolume();
    }

    void UpdateAllVolumes()
    {
        UpdateMusicVolume();
        UpdateAmbientVolume();
    }

    void UpdateMusicVolume()
    {
        if (musicSource.isPlaying && !string.IsNullOrEmpty(currentMusicTrack))
        {
            AudioClipData currentTrack = musicDict[currentMusicTrack];
            musicSource.volume = currentTrack.volume * musicVolume * masterVolume;
        }
    }

    void UpdateAmbientVolume()
    {
        if (ambientSource.isPlaying && !string.IsNullOrEmpty(currentAmbientTrack))
        {
            AudioClipData currentAmbient = ambientDict[currentAmbientTrack];
            ambientSource.volume = currentAmbient.volume * ambientVolume * masterVolume;
        }
    }

    #endregion

    // Game state audio management
    public void OnGameStateChanged(string newState)
    {
        switch (newState.ToLower())
        {
            case "sailing":
                PlayMusic("SailingTheme");
                PlayAmbient("OceanWaves");
                break;
            case "spirit":
                PlayMusic("SpiritTheme");
                PlayAmbient("GhostlyWhispers");
                break;
            case "combat":
                PlayMusic("CombatTheme");
                StopAmbient();
                break;
            case "possessed":
                PlayMusic("PossessionTheme");
                PlayAmbient("EerieAmbient");
                break;
        }
    }
}
