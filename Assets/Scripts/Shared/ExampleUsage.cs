using UnityEngine;
using CursorClient;
using CursorServer;

/// <summary>
/// Example script demonstrating how to use the Cursor client-server system
/// This file provides usage examples for developers
/// </summary>
public class ExampleUsage : MonoBehaviour
{
    // CLIENT USAGE EXAMPLES
    
    /// <summary>
    /// Example: Connecting to server from client
    /// </summary>
    public void ExampleConnectToServer()
    {
#if !UNITY_SERVER && !SERVER_BUILD
        // Subscribe to authorization events
        ClientNetworkManager.Instance.OnAuthorized += OnClientAuthorized;
        ClientNetworkManager.Instance.OnAuthorizationFailed += OnAuthFailed;
        
        // Connect to server
        ClientNetworkManager.Instance.ConnectToServer();
#endif
    }
    
    /// <summary>
    /// Example: Sending data to server (requires authorization)
    /// </summary>
    public void ExampleSendData()
    {
#if !UNITY_SERVER && !SERVER_BUILD
        if (ClientNetworkManager.Instance.IsAuthorized)
        {
            ClientNetworkManager.Instance.SendData("playerPosition", transform.position);
            ClientNetworkManager.Instance.SendData("playerHealth", 100);
        }
        else
        {
            Debug.LogWarning("Cannot send data - not authorized");
        }
#endif
    }
    
    /// <summary>
    /// Example: Loading different scenes
    /// </summary>
    public void ExampleLoadScenes()
    {
#if !UNITY_SERVER && !SERVER_BUILD
        // Load main menu
        SceneController.Instance.LoadMainMenu();
        
        // Load gameplay scene
        SceneController.Instance.LoadGameplay();
        
        // Load settings
        SceneController.Instance.LoadSettings();
        
        // Load any scene by name
        SceneController.Instance.LoadScene("MainMenu");
#endif
    }
    
    // SERVER USAGE EXAMPLES
    
    /// <summary>
    /// Example: Initializing server with configuration
    /// </summary>
    public void ExampleInitializeServer()
    {
#if UNITY_SERVER || SERVER_BUILD
        // Initialize server
        ServerAuthManager.Instance.InitializeServer();
        
        Debug.Log("Server initialized and ready for connections");
#endif
    }
    
    /// <summary>
    /// Example: Authorizing a client
    /// </summary>
    public void ExampleAuthorizeClient(string clientId, string credentials)
    {
#if UNITY_SERVER || SERVER_BUILD
        bool authorized = ServerAuthManager.Instance.AuthorizeClient(clientId, credentials);
        
        if (authorized)
        {
            Debug.Log($"Client {clientId} authorized successfully");
        }
        else
        {
            Debug.LogError($"Failed to authorize client {clientId}");
        }
#endif
    }
    
    /// <summary>
    /// Example: Storing data for an authorized client
    /// </summary>
    public void ExampleStoreData(string clientId)
    {
#if UNITY_SERVER || SERVER_BUILD
        // Check if client is authorized
        if (ServerAuthManager.Instance.IsAuthorized(clientId))
        {
            // Store player position
            Vector3 playerPos = new Vector3(10, 0, 5);
            AuthorizedDataManager.Instance.StoreData(clientId, "playerPosition", playerPos);
            
            // Store player score
            int score = 1000;
            AuthorizedDataManager.Instance.StoreData(clientId, "playerScore", score);
            
            Debug.Log($"Data stored for client {clientId}");
        }
        else
        {
            Debug.LogWarning($"Cannot store data - client {clientId} not authorized");
        }
#endif
    }
    
    /// <summary>
    /// Example: Retrieving data for an authorized client
    /// </summary>
    public void ExampleRetrieveData(string clientId)
    {
#if UNITY_SERVER || SERVER_BUILD
        if (ServerAuthManager.Instance.IsAuthorized(clientId))
        {
            // Retrieve player position
            object posData = AuthorizedDataManager.Instance.RetrieveData(clientId, "playerPosition");
            if (posData != null && posData is Vector3)
            {
                Vector3 position = (Vector3)posData;
                Debug.Log($"Player position: {position}");
            }
            
            // Retrieve player score
            object scoreData = AuthorizedDataManager.Instance.RetrieveData(clientId, "playerScore");
            if (scoreData != null && scoreData is int)
            {
                int score = (int)scoreData;
                Debug.Log($"Player score: {score}");
            }
        }
#endif
    }
    
    /// <summary>
    /// Example: Revoking client authorization
    /// </summary>
    public void ExampleRevokeAuthorization(string clientId)
    {
#if UNITY_SERVER || SERVER_BUILD
        ServerAuthManager.Instance.RevokeAuthorization(clientId);
        Debug.Log($"Authorization revoked for client {clientId}");
#endif
    }
    
    /// <summary>
    /// Example: Queuing data operations
    /// </summary>
    public void ExampleQueueOperations(string clientId)
    {
#if UNITY_SERVER || SERVER_BUILD
        // Create a data operation
        var operation = new AuthorizedDataManager.DataOperation
        {
            Type = AuthorizedDataManager.OperationType.Store,
            ClientId = clientId,
            Key = "gameState",
            Data = "active"
        };
        
        // Queue the operation for processing
        AuthorizedDataManager.Instance.QueueDataOperation(operation);
        
        Debug.Log("Data operation queued for processing");
#endif
    }
    
    // EVENT HANDLERS
    
    private void OnClientAuthorized()
    {
        Debug.Log("✓ Client successfully authorized by server");
        Debug.Log("You can now send and receive data");
    }
    
    private void OnAuthFailed()
    {
        Debug.LogError("✗ Failed to authorize with server");
        Debug.LogError("Please check server connection and credentials");
    }
    
    private void OnDestroy()
    {
#if !UNITY_SERVER && !SERVER_BUILD
        // Clean up event subscriptions
        if (ClientNetworkManager.Instance != null)
        {
            ClientNetworkManager.Instance.OnAuthorized -= OnClientAuthorized;
            ClientNetworkManager.Instance.OnAuthorizationFailed -= OnAuthFailed;
        }
#endif
    }
}
