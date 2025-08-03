using UnityEngine;

public class GameManager : MonoBehaviour
{
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

    private GameState _currentState;
    private float _stateTimer;
    private float _nextSpawnTime;

    void Start()
    {
        _currentState = GameState.Fishing;
        _stateTimer = fishingDuration;
        StartFishing();
    }

    void Update()
    {
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
                //TODO:add this
                break;
        }
    }

    public void StartFishing()
    {
        _currentState = GameState.Fishing;
        _stateTimer = fishingDuration;
    }

    void SwitchToBattle()
    {
        _currentState = GameState.Battle;
        _nextSpawnTime = Time.time;
    }

    void SpawnSpirit()
    {
        Instantiate(spiritPrefab, spawnPoint.position+Vector3.one*Random.Range(-1f,1f), Quaternion.identity);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}