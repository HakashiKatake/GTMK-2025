using UnityEngine;
using UnityEngine.Events;

public class ManagerBehaviour : MonoBehaviour
{
    [Header("General Settings")]
    public bool useSingleton = true;
    public bool dontDestroyOnLoad = true;
    public bool enableDebugLogs = false;

    [Header("Optional Settings")]
    public float initializationDelay = 0f;
    public UnityEvent onInitialize;

    private static ManagerBehaviour _instance;

    void Awake()
    {
        if (useSingleton)
        {
            if (_instance != null && _instance != this)
            {
                if (enableDebugLogs)
                    Debug.Log("[ManagerBehaviour] Duplicate found, destroying self.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (initializationDelay > 0f)
            Invoke(nameof(Initialize), initializationDelay);
        else
            Initialize();
    }

    void Initialize()
    {
        if (enableDebugLogs)
            Debug.Log("[ManagerBehaviour] Initialization complete.");

        onInitialize?.Invoke();
    }

    // Optional: Static access to the singleton instance
    public static ManagerBehaviour Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogWarning("[ManagerBehaviour] Instance is null.");
            return _instance;
        }
    }
}
