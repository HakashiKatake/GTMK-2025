using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambienceSource;

    [Header("Audio Clips")]
    public Sound[] sounds;
    public AudioClip fishingMusic;
    public AudioClip battleMusic;
    public AudioClip undeadMusic;
    public AudioClip ambienceSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float ambienceVolume = 0.5f;

    private Dictionary<string, Sound> _soundDictionary = new();
    private AudioClip _currentMusic;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudio()
    {
        // Setup sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
        }

        // Set initial volumes
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        ambienceSource.volume = ambienceVolume;

        // Initialize sound dictionary
        foreach (Sound s in sounds)
        {
            _soundDictionary[s.name] = s;
        }

        // Start ambience if assigned
        if (ambienceSound != null)
        {
            PlayAmbience(ambienceSound);
        }
    }

    public void PlayMusic(AudioClip music)
    {
        if (_currentMusic == music) return;

        _currentMusic = music;
        if (!music)
        {
            musicSource.Stop();
            return;
        }

        musicSource.clip = music;
        musicSource.Play();
    }

    public void PlaySfx(string soundName)
    {
        if (_soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume);
        }
    }

    public void PlayAmbience(AudioClip ambience)
    {
        if (ambienceSource.clip == ambience && ambienceSource.isPlaying) return;

        ambienceSource.clip = ambience;
        ambienceSource.Play();
    }
}
