using Core.Games;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Server
{
    /// <summary>
    /// Server-side connection controller for managing client connections and session mapping.
    /// Handles client-to-session assignment and connection lifecycle.
    /// </summary>
    public class ConnectionController : MonoBehaviour
    {
        private static ConnectionController _instance;
        public static ConnectionController Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ConnectionController");
                    _instance = go.AddComponent<ConnectionController>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private NetworkManager _networkManager;
        private SessionContainerManager _sessionContainerManager;
        private readonly Dictionary<ulong, bool> _connectedClients = new Dictionary<ulong, bool>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _networkManager = NetworkManager.Singleton;
            if (_networkManager == null)
            {
                Debug.LogError("[ConnectionController] NetworkManager not found!");
                return;
            }

            // Get or create SessionContainerManager
            var gameSessionManager = GameSessionManager.Instance;
            if (gameSessionManager != null)
            {
                // Access the container manager through GameSessionManager
                // We'll need to expose it or use a different approach
            }

            // Subscribe to network events
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback += OnClientConnected;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }

            Debug.Log("[ConnectionController] Initialized");
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        /// <summary>
        /// Try to assign a client to a session.
        /// Validates that the client is connected and not already in another session.
        /// </summary>
        public bool TryAssignToSession(ulong clientId, string sessionName)
        {
            if (!IsClientConnected(clientId))
            {
                Debug.LogWarning($"[ConnectionController] Client {clientId} is not connected");
                return false;
            }

            // Check if client is already in a different session
            var gameSessionManager = GameSessionManager.Instance;
            if (gameSessionManager != null)
            {
                var currentSession = gameSessionManager.GetClientSession(clientId);
                if (!string.IsNullOrEmpty(currentSession) && currentSession != sessionName)
                {
                    Debug.LogWarning($"[ConnectionController] Client {clientId} already in session '{currentSession}'");
                    return false;
                }
            }

            Debug.Log($"[ConnectionController] Assigned client {clientId} to session '{sessionName}'");
            return true;
        }

        /// <summary>
        /// Get the session name for a client.
        /// </summary>
        public string GetClientSession(ulong clientId)
        {
            var gameSessionManager = GameSessionManager.Instance;
            if (gameSessionManager != null)
            {
                return gameSessionManager.GetClientSession(clientId);
            }
            return null;
        }

        /// <summary>
        /// Handle client disconnection and cleanup.
        /// </summary>
        public void HandleClientDisconnect(ulong clientId)
        {
            _connectedClients.Remove(clientId);

            var gameSessionManager = GameSessionManager.Instance;
            if (gameSessionManager != null)
            {
                gameSessionManager.RemoveSessionsForClient(clientId);
            }

            Debug.Log($"[ConnectionController] Client {clientId} disconnected and cleaned up");
        }

        /// <summary>
        /// Check if a client is currently connected.
        /// </summary>
        public bool IsClientConnected(ulong clientId)
        {
            if (_networkManager == null || !_networkManager.IsServer)
                return false;

            return _networkManager.ConnectedClientsIds.Contains(clientId);
        }

        /// <summary>
        /// Get all connected client IDs.
        /// </summary>
        public List<ulong> GetConnectedClients()
        {
            if (_networkManager == null || !_networkManager.IsServer)
                return new List<ulong>();

            return new List<ulong>(_networkManager.ConnectedClientsIds);
        }

        // Event handlers (public for ServerBootstrap)
        public void OnClientConnected(ulong clientId)
        {
            _connectedClients[clientId] = true;
            Debug.Log($"[ConnectionController] Client {clientId} connected");
        }
    }
}
