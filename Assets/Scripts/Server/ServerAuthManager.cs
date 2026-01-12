using System;
using System.Collections.Generic;
using UnityEngine;

namespace CursorServer
{
    /// <summary>
    /// Manages server-side authorization for all client connections
    /// Ensures fully authorized access control for data-oriented operations
    /// </summary>
    public class ServerAuthManager : MonoBehaviour
    {
        private static ServerAuthManager instance;
        public static ServerAuthManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ServerAuthManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ServerAuthManager");
                        instance = go.AddComponent<ServerAuthManager>();
                    }
                }
                return instance;
            }
        }

        [SerializeField] private ServerConfig serverConfig;
        
        private Dictionary<string, AuthorizedClient> authorizedClients = new Dictionary<string, AuthorizedClient>();
        private Dictionary<string, string> authTokens = new Dictionary<string, string>();
        private bool isServerRunning = false;

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

        public void InitializeServer()
        {
            if (serverConfig == null)
            {
                Debug.LogError("ServerConfig is not assigned!");
                return;
            }

            isServerRunning = true;
            Debug.Log($"Server initialized on port {serverConfig.Port} with authorization enabled");
        }

        public bool AuthorizeClient(string clientId, string credentials)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(credentials))
            {
                Debug.LogWarning("Invalid client credentials");
                return false;
            }

            // Generate secure token
            string token = GenerateSecureToken();
            
            AuthorizedClient client = new AuthorizedClient
            {
                ClientId = clientId,
                AuthToken = token,
                ConnectedAt = DateTime.UtcNow,
                IsAuthorized = true
            };

            authorizedClients[clientId] = client;
            authTokens[token] = clientId;

            Debug.Log($"Client {clientId} authorized successfully");
            return true;
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            if (authTokens.TryGetValue(token, out string clientId))
            {
                if (authorizedClients.TryGetValue(clientId, out AuthorizedClient client))
                {
                    return client.IsAuthorized;
                }
            }

            return false;
        }

        public void RevokeAuthorization(string clientId)
        {
            if (authorizedClients.TryGetValue(clientId, out AuthorizedClient client))
            {
                client.IsAuthorized = false;
                authTokens.Remove(client.AuthToken);
                Debug.Log($"Authorization revoked for client {clientId}");
            }
        }

        public bool IsAuthorized(string clientId)
        {
            return authorizedClients.TryGetValue(clientId, out AuthorizedClient client) && client.IsAuthorized;
        }

        private string GenerateSecureToken()
        {
            return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("X");
        }

        public void ShutdownServer()
        {
            isServerRunning = false;
            authorizedClients.Clear();
            authTokens.Clear();
            Debug.Log("Server shutdown complete");
        }

        [Serializable]
        public class AuthorizedClient
        {
            public string ClientId;
            public string AuthToken;
            public DateTime ConnectedAt;
            public bool IsAuthorized;
        }
    }
}
