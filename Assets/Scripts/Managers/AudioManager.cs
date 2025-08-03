using UnityEngine;
using System.Collections;

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

    [Header("Music")]
    public AudioClip[] musicTracks;
    public int startingMusicIndex = 0;
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;

    [Header("Sound Effects")]
    public AudioClipData[] soundEffects;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    public float fadeTime = 2f;

    // Private variables
    private int currentMusicIndex = -1;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            StartMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        // Create music source if not assigned
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
        }

        // Create SFX source if not assigned
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
        }

        // Configure sources
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void StartMusic()
    {
        if (musicTracks.Length > 0 && startingMusicIndex >= 0 && startingMusicIndex < musicTracks.Length)
        {
            PlayMusicTrack(startingMusicIndex, true);
        }
    }

    #region Music Control

    public void PlayMusicTrack(int trackIndex, bool fadeIn = true)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Length)
        {
            Debug.LogWarning($"Music track index {trackIndex} is out of range!");
            return;
        }

        if (currentMusicIndex == trackIndex && musicSource.isPlaying)
        {
            return; // Already playing this track
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        currentMusicIndex = trackIndex;
        fadeCoroutine = StartCoroutine(FadeToNewTrack(musicTracks[trackIndex], fadeIn));
    }

    public void FadeInMusic(int trackIndex)
    {
        PlayMusicTrack(trackIndex, true);
    }

    public void FadeOutMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutCurrentTrack());
    }

    public void StopMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        musicSource.Stop();
        currentMusicIndex = -1;
    }

    IEnumerator FadeToNewTrack(AudioClip newTrack, bool fadeIn)
    {
        // Fade out current music if playing
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeTime * 0.5f; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0, t / (fadeTime * 0.5f));
                yield return null;
            }
            musicSource.Stop();
        }

        // Start new track
        musicSource.clip = newTrack;
        musicSource.volume = 0;
        musicSource.Play();

        // Fade in new music
        if (fadeIn)
        {
            float targetVolume = musicVolume * masterVolume;
            for (float t = 0; t < fadeTime * 0.5f; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0, targetVolume, t / (fadeTime * 0.5f));
                yield return null;
            }
            musicSource.volume = targetVolume;
        }
        else
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        fadeCoroutine = null;
    }

    IEnumerator FadeOutCurrentTrack()
    {
        if (!musicSource.isPlaying) yield break;

        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0;
        currentMusicIndex = -1;
        fadeCoroutine = null;
    }

    #endregion

    #region Sound Effects

    public void PlaySFX(string sfxName)
    {
        AudioClipData sfxData = GetSFXData(sfxName);
        if (sfxData != null && sfxData.clip != null)
        {
            sfxSource.pitch = sfxData.pitch;
            sfxSource.PlayOneShot(sfxData.clip, sfxData.volume * sfxVolume * masterVolume);
        }
    }

    public void PlaySFX(string sfxName, float volumeScale)
    {
        AudioClipData sfxData = GetSFXData(sfxName);
        if (sfxData != null && sfxData.clip != null)
        {
            sfxSource.pitch = sfxData.pitch;
            sfxSource.PlayOneShot(sfxData.clip, sfxData.volume * volumeScale * sfxVolume * masterVolume);
        }
    }

    private AudioClipData GetSFXData(string sfxName)
    {
        foreach (var sfx in soundEffects)
        {
            if (sfx.name == sfxName)
            {
                return sfx;
            }
        }
        Debug.LogWarning($"Sound effect '{sfxName}' not found!");
        return null;
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
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

    private void UpdateMusicVolume()
    {
        if (musicSource.isPlaying)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    #endregion

    #region Public Utility Methods

    public bool IsMusicPlaying()
    {
        return musicSource.isPlaying;
    }

    public int GetCurrentMusicIndex()
    {
        return currentMusicIndex;
    }

    public void NextTrack()
    {
        if (musicTracks.Length > 0)
        {
            int nextIndex = (currentMusicIndex + 1) % musicTracks.Length;
            PlayMusicTrack(nextIndex, true);
        }
    }

    public void PreviousTrack()
    {
        if (musicTracks.Length > 0)
        {
            int prevIndex = currentMusicIndex - 1;
            if (prevIndex < 0) prevIndex = musicTracks.Length - 1;
            PlayMusicTrack(prevIndex, true);
        }
    }

    #endregion
}
