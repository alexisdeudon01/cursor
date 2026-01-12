using System;
using UnityEngine;

namespace CursorClient
{
    /// <summary>
    /// Manages client-side networking and communication with authorized server
    /// </summary>
    public class ClientNetworkManager : MonoBehaviour
    {
        private static ClientNetworkManager instance;
        public static ClientNetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ClientNetworkManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ClientNetworkManager");
                        instance = go.AddComponent<ClientNetworkManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Connection Settings")]
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;

        private string clientId;
        private string authToken;
        private bool isConnected = false;
        private bool isAuthorized = false;

        public bool IsConnected => isConnected;
        public bool IsAuthorized => isAuthorized;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            clientId = GenerateClientId();
        }

        public void ConnectToServer()
        {
            Debug.Log($"Connecting to server at {serverAddress}:{serverPort}");
            
            // Simulate connection (in real implementation, this would use actual networking)
            isConnected = true;
            RequestAuthorization();
        }

        private void RequestAuthorization()
        {
            if (!isConnected)
            {
                Debug.LogWarning("Cannot request authorization - not connected to server");
                return;
            }

            // Simulate authorization request
            string credentials = GenerateCredentials();
            Debug.Log($"Requesting authorization for client {clientId}");
            
            // In real implementation, this would send actual network request
            OnAuthorizationReceived(true, GenerateSecureToken());
        }

        private void OnAuthorizationReceived(bool success, string token)
        {
            if (success)
            {
                isAuthorized = true;
                authToken = token;
                Debug.Log("Client authorized successfully");
                OnAuthorized?.Invoke();
            }
            else
            {
                Debug.LogError("Authorization failed");
                OnAuthorizationFailed?.Invoke();
            }
        }

        public void SendData(string key, object data)
        {
            if (!isAuthorized)
            {
                Debug.LogWarning("Cannot send data - client not authorized");
                return;
            }

            Debug.Log($"Sending data with key {key} to server");
            // In real implementation, this would send actual network data
        }

        public void RequestData(string key)
        {
            if (!isAuthorized)
            {
                Debug.LogWarning("Cannot request data - client not authorized");
                return;
            }

            Debug.Log($"Requesting data with key {key} from server");
            // In real implementation, this would send actual network request
        }

        public void DisconnectFromServer()
        {
            if (isConnected)
            {
                Debug.Log("Disconnecting from server");
                isConnected = false;
                isAuthorized = false;
                authToken = null;
            }
        }

        private string GenerateClientId()
        {
            return $"Client_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        private string GenerateCredentials()
        {
            return $"Credentials_{clientId}_{DateTime.UtcNow.Ticks}";
        }

        private string GenerateSecureToken()
        {
            return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("X");
        }

        // Events
        public event Action OnAuthorized;
        public event Action OnAuthorizationFailed;
        public event Action OnDisconnected;
    }
}
