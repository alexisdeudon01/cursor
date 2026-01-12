using UnityEngine;
using CursorServer;

/// <summary>
/// Server initialization script - loads when server build starts
/// Manages fully authorized server with data-oriented processing
/// </summary>
public class ServerInitializer : MonoBehaviour
{
#if UNITY_SERVER || SERVER_BUILD
    [SerializeField] private ServerConfig serverConfig;

    private void Awake()
    {
        Debug.Log("===========================================");
        Debug.Log("CURSOR SERVER - Fully Authorized");
        Debug.Log("===========================================");
        
        InitializeServer();
    }

    private void InitializeServer()
    {
        // Initialize authorization manager
        ServerAuthManager authManager = ServerAuthManager.Instance;
        authManager.InitializeServer();

        // Initialize data manager
        AuthorizedDataManager dataManager = AuthorizedDataManager.Instance;
        
        Debug.Log("Server initialization complete");
        Debug.Log("Waiting for authorized client connections...");
    }

    private void OnApplicationQuit()
    {
        ServerAuthManager.Instance.ShutdownServer();
        Debug.Log("Server shutdown");
    }
#endif
}
