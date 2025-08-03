using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public enum GameState
    {
        Fishing,
        Battle,
        Undead
    }

    [Header("Game Settings")]
    public float fishingDuration = 120f; // 2 minutes
    public GameObject spiritPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 1f;

    [Header("UI References")]
    public GameObject fishingUI;
    public GameObject battleUI;
    public GameObject transitionUI;

    private GameState _currentState;
    private float _stateTimer;
    private float _nextSpawnTime;
    private bool _isTransitioning;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _currentState = GameState.Fishing;
        _stateTimer = fishingDuration;
        StartFishing();
        
        // Start with fishing music
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.fishingMusic);
        }
    }

    void Update()
    {
        if (_isTransitioning) return;

        switch (_currentState)
        {
            case GameState.Fishing:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0)
                {
                    SwitchToBattle();
                }
                break;

            case GameState.Battle:
                if (Time.time >= _nextSpawnTime)
                {
                    SpawnSpirit();
                    _nextSpawnTime = Time.time + spawnInterval;
                }
                break;
            case GameState.Undead:
                break;
        }
    }

    void SpawnSpirit()
    {
        GameObject spirit = Instantiate(spiritPrefab, spawnPoint.position + Vector3.one * Random.Range(-1f, 1f), Quaternion.identity);
        var spiritComponent = spirit.GetComponent<Spirit>();
        if (spiritComponent)
        {
            spiritComponent.enabled = true;
        }
    }

    public void DestroyAllSpirits()
    {
        Spirit[] spirits = FindObjectsOfType<Spirit>();
        foreach (Spirit spirit in spirits)
        {
            Destroy(spirit.gameObject);
        }
    }

    public void StartFishing()
    {
        _currentState = GameState.Fishing;
        _stateTimer = fishingDuration;
        SetUIState(fishingUI, true);
        SetUIState(battleUI, false);
        SetUIState(transitionUI, false);
        DestroyAllSpirits();
        
        // Play fishing music
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.fishingMusic);
        }
    }

    public GameState GetCurrentState()
    {
        return _currentState;
    }

    void SwitchToBattle()
    {
        StartCoroutine(StartTransitionSequence());
        _currentState = GameState.Battle;
        _nextSpawnTime = Time.time;
        SetUIState(fishingUI, false);
        SetUIState(battleUI, true);
        
        // Play battle music
        if (AudioManager.instance)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.battleMusic);
        }
    }

    public void SwitchToUndead()
    {
        StartCoroutine(StartTransitionSequence());
        _currentState = GameState.Undead;
        SetUIState(fishingUI, false);
        SetUIState(battleUI, false);
        
        // Play undead music
        if (AudioManager.instance)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.undeadMusic);
        }
        
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySfx("human drown");
        }
        //TODO:spawn bot
        DestroyAllSpirits();
    }

    private void SetUIState(GameObject ui, bool state)
    {
        if (ui)
            ui.SetActive(state);
    }

    public IEnumerator StartTransitionSequence()
    {
        _isTransitioning = true;
        SetUIState(transitionUI, true);

        // Pause current state functionality
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(2f);

        // Resume game
        Time.timeScale = previousTimeScale;
        SetUIState(transitionUI, false);
        _isTransitioning = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}