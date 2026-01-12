using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Main game manager for the Game scene.
/// Handles game initialization and state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool autoInitialize = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (autoInitialize)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        Debug.Log("[GameManager] Initializing game scene...");

        // Check if we're connected
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[GameManager] Running as server");
                InitializeServer();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("[GameManager] Running as client");
                InitializeClient();
            }
        }
        else
        {
            Debug.Log("[GameManager] No network - running offline");
            InitializeOffline();
        }
    }

    private void InitializeServer()
    {
        // Server-specific initialization
        Debug.Log("[GameManager] Server initialized");
    }

    private void InitializeClient()
    {
        // Client-specific initialization
        Debug.Log("[GameManager] Client initialized");
    }

    private void InitializeOffline()
    {
        // Offline/testing initialization
        Debug.Log("[GameManager] Offline mode initialized");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
