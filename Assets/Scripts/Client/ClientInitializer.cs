using UnityEngine;
using CursorClient;
using CursorShared;

/// <summary>
/// Client initialization script - loads when client build starts
/// Manages connection to authorized server
/// </summary>
public class ClientInitializer : MonoBehaviour
{
#if !UNITY_SERVER && !SERVER_BUILD
    private void Awake()
    {
        Debug.Log("===========================================");
        Debug.Log("CURSOR CLIENT");
        Debug.Log("===========================================");
        
        CursorUtilities.LogBuildInfo();
        InitializeClient();
    }

    private void InitializeClient()
    {
        // Initialize network manager
        ClientNetworkManager networkManager = ClientNetworkManager.Instance;
        
        // Subscribe to authorization events
        networkManager.OnAuthorized += OnClientAuthorized;
        networkManager.OnAuthorizationFailed += OnAuthorizationFailed;
        
        Debug.Log("Client initialization complete");
        Debug.Log("Ready to connect to server");
    }

    private void OnClientAuthorized()
    {
        Debug.Log("Client successfully authorized by server");
        Debug.Log("Client is ready to send/receive data");
    }

    private void OnAuthorizationFailed()
    {
        Debug.LogError("Failed to authorize with server");
    }

    private void OnDestroy()
    {
        if (ClientNetworkManager.Instance != null)
        {
            ClientNetworkManager.Instance.OnAuthorized -= OnClientAuthorized;
            ClientNetworkManager.Instance.OnAuthorizationFailed -= OnAuthorizationFailed;
        }
    }
#endif
}
