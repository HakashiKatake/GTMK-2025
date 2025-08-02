using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    [Header("Game State")]
    public GameState currentState = GameState.Sailing;

    [Header("Player References")]
    public GameObject playerSailor;
    public GameObject playerSpirit;
    public Transform shipSpawnPoint;
    public Transform spiritSpawnPoint;

    [Header("Spirit Settings")]
    public GameObject[] spiritPrefabs;
    public int spiritsToSpawn = 3;
    public float spiritSpawnRadius = 10f;
    public LayerMask spiritSpawnLayer = 1;

    [Header("Transition Settings")]
    public float transitionDelay = 2f;
    public float possessionRange = 3f;

    [Header("UI References")]
    public GameObject sailingUI;
    public GameObject spiritUI;
    public GameObject transitionUI;

    // Events
    public System.Action<GameState> OnStateChanged;

    // Private variables
    private Health playerHealth;
    private bool isTransitioning = false;

    public enum GameState
    {
        Sailing,
        Drowning,
        Spirit,
        Possessing,
        Possessed
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGameState();
    }

    void InitializeGameState()
    {
        // Get player health component
        if (playerSailor != null)
        {
            playerHealth = playerSailor.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath.AddListener(OnPlayerDeath);
            }
        }

        // Set initial state
        SetGameState(GameState.Sailing);
    }

    public void SetGameState(GameState newState)
    {
        if (isTransitioning) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"Game state changed from {previousState} to {newState}");

        // Handle state transitions
        switch (newState)
        {
            case GameState.Sailing:
                HandleSailingState();
                break;
            case GameState.Drowning:
                HandleDrowningState();
                break;
            case GameState.Spirit:
                HandleSpiritState();
                break;
            case GameState.Possessing:
                HandlePossessingState();
                break;
            case GameState.Possessed:
                HandlePossessedState();
                break;
        }

        // Update audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnGameStateChanged(newState.ToString());
        }

        // Notify listeners
        OnStateChanged?.Invoke(newState);
    }

    void HandleSailingState()
    {
        // Enable sailor player
        if (playerSailor != null)
            playerSailor.SetActive(true);
        
        // Disable spirit player
        if (playerSpirit != null)
            playerSpirit.SetActive(false);

        // Update UI
        SetUIState(sailingUI, true);
        SetUIState(spiritUI, false);
        SetUIState(transitionUI, false);

        // Spawn new ship if needed
        // (This would be handled by your ship spawning system)
    }

    void HandleDrowningState()
    {
        isTransitioning = true;
        StartCoroutine(DrowningSequence());
    }

    void HandleSpiritState()
    {
        // Disable sailor player
        if (playerSailor != null)
            playerSailor.SetActive(false);
        
        // Enable spirit player
        if (playerSpirit != null)
        {
            playerSpirit.SetActive(true);
            if (spiritSpawnPoint != null)
                playerSpirit.transform.position = spiritSpawnPoint.position;
        }

        // Update UI
        SetUIState(sailingUI, false);
        SetUIState(spiritUI, true);
        SetUIState(transitionUI, false);

        // Spawn enemy spirits
        SpawnEnemySpirits();
    }

    void HandlePossessingState()
    {
        // This state is for the possession animation/transition
        isTransitioning = true;
        StartCoroutine(PossessionSequence());
    }

    void HandlePossessedState()
    {
        // Player becomes sailor again but with spirit influence
        // This transitions back to sailing state
        SetGameState(GameState.Sailing);
    }

    IEnumerator DrowningSequence()
    {
        // Show drowning effects
        SetUIState(transitionUI, true);
        
        // Play drowning audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Drowning");
        }

        // Wait for transition
        yield return new WaitForSeconds(transitionDelay);

        // Transition to spirit state
        isTransitioning = false;
        SetGameState(GameState.Spirit);
    }

    IEnumerator PossessionSequence()
    {
        // Show possession effects
        SetUIState(transitionUI, true);
        
        // Play possession audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Possession");
        }

        // Wait for transition
        yield return new WaitForSeconds(transitionDelay);

        // Transition to possessed state (which goes back to sailing)
        isTransitioning = false;
        SetGameState(GameState.Possessed);
    }

    void SpawnEnemySpirits()
    {
        if (spiritPrefabs.Length == 0) return;

        for (int i = 0; i < spiritsToSpawn; i++)
        {
            // Random position around spirit spawn point
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 spawnPosition = spiritSpawnPoint.position + (Vector3)(randomDirection * spiritSpawnRadius);

            // Choose random spirit prefab
            GameObject spiritPrefab = spiritPrefabs[Random.Range(0, spiritPrefabs.Length)];
            
            // Spawn spirit
            GameObject newSpirit = Instantiate(spiritPrefab, spawnPosition, Quaternion.identity);
            
            // Set up spirit target (if not player spirit)
            Spirit spiritComponent = newSpirit.GetComponent<Spirit>();
            if (spiritComponent != null && playerSailor != null)
            {
                spiritComponent.SetTarget(playerSailor.transform);
            }
        }
    }

    void SetUIState(GameObject uiElement, bool active)
    {
        if (uiElement != null)
            uiElement.SetActive(active);
    }

    // Called when player dies
    void OnPlayerDeath()
    {
        if (currentState == GameState.Sailing)
        {
            SetGameState(GameState.Drowning);
        }
    }

    // Called when spirit player gets close to another sailor
    public void TryPossession(GameObject target)
    {
        if (currentState != GameState.Spirit) return;

        // Check if target is valid for possession
        if (target.CompareTag("Player") || target.CompareTag("Sailor"))
        {
            float distance = Vector3.Distance(playerSpirit.transform.position, target.transform.position);
            if (distance <= possessionRange)
            {
                // Start possession
                SetGameState(GameState.Possessing);
                
                // Move sailor to spirit position
                if (playerSailor != null)
                {
                    playerSailor.transform.position = target.transform.position;
                }
            }
        }
    }

    // Public methods for external triggers
    public bool IsInState(GameState state)
    {
        return currentState == state;
    }

    public void ForceStateChange(GameState newState)
    {
        SetGameState(newState);
    }

    // Debug methods
    void Update()
    {
        // Debug key presses (remove in final build)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetGameState(GameState.Sailing);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetGameState(GameState.Spirit);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            OnPlayerDeath();
    }

    void OnDrawGizmosSelected()
    {
        // Draw possession range
        if (playerSpirit != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerSpirit.transform.position, possessionRange);
        }

        // Draw spirit spawn area
        if (spiritSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spiritSpawnPoint.position, spiritSpawnRadius);
        }
    }
}
