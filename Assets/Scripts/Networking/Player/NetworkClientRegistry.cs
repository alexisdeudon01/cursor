using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Central registry for all connected network clients.
/// Tracks client data, session membership, and provides efficient lookups.
/// </summary>
public class NetworkClientRegistry : MonoBehaviour
{
    public static NetworkClientRegistry Instance { get; private set; }

    // Main registry: clientId → ClientNetworkData
    private readonly Dictionary<ulong, ClientNetworkData> clients = new Dictionary<ulong, ClientNetworkData>();

    // Index: sessionId → Set of clientIds (for fast session queries)
    private readonly Dictionary<string, HashSet<ulong>> sessionClients = new Dictionary<string, HashSet<ulong>>();

    // Lock for thread safety
    private readonly object registryLock = new object();

    // Events
    public event Action<ulong> OnClientRegistered;
    public event Action<ulong> OnClientUnregistered;
    public event Action<ulong, string> OnClientJoinedSession;
    public event Action<ulong, string> OnClientLeftSession;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to network events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        Debug.Log("[NetworkClientRegistry] Initialized");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (Instance == this)
            Instance = null;
    }

    // ============================================================
    //  NETWORK CALLBACKS
    // ============================================================

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        RegisterClient(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        UnregisterClient(clientId);
    }

    // ============================================================
    //  CLIENT MANAGEMENT
    // ============================================================

    /// <summary>
    /// Register a new client in the registry.
    /// </summary>
    public void RegisterClient(ulong clientId, string initialName = null)
    {
        lock (registryLock)
        {
            if (clients.ContainsKey(clientId))
            {
                Debug.LogWarning($"[NetworkClientRegistry] Client {clientId} already registered");
                return;
            }

            var clientData = new ClientNetworkData(clientId);
            if (!string.IsNullOrEmpty(initialName))
                clientData.PlayerName = initialName;

            clients[clientId] = clientData;

            Debug.Log($"[NetworkClientRegistry] Registered client {clientId}");
            OnClientRegistered?.Invoke(clientId);
        }
    }

    /// <summary>
    /// Unregister a client from the registry.
    /// </summary>
    public void UnregisterClient(ulong clientId)
    {
        lock (registryLock)
        {
            if (!clients.TryGetValue(clientId, out var clientData))
                return;

            // Remove from session index if in a session
            if (clientData.IsInSession)
            {
                ClearClientSession(clientId);
            }

            clients.Remove(clientId);

            Debug.Log($"[NetworkClientRegistry] Unregistered client {clientId}");
            OnClientUnregistered?.Invoke(clientId);
        }
    }

    /// <summary>
    /// Update client player name.
    /// </summary>
    public void SetClientName(ulong clientId, string playerName)
    {
        lock (registryLock)
        {
            if (clients.TryGetValue(clientId, out var clientData))
            {
                clientData.PlayerName = playerName;
                Debug.Log($"[NetworkClientRegistry] Updated name for client {clientId}: {playerName}");
            }
        }
    }

    /// <summary>
    /// Get client data.
    /// </summary>
    public ClientNetworkData GetClient(ulong clientId)
    {
        lock (registryLock)
        {
            return clients.TryGetValue(clientId, out var data) ? data : null;
        }
    }

    /// <summary>
    /// Get all registered clients.
    /// </summary>
    public List<ClientNetworkData> GetAllClients()
    {
        lock (registryLock)
        {
            return new List<ClientNetworkData>(clients.Values);
        }
    }

    /// <summary>
    /// Check if client is registered.
    /// </summary>
    public bool IsClientRegistered(ulong clientId)
    {
        lock (registryLock)
        {
            return clients.ContainsKey(clientId);
        }
    }

    // ============================================================
    //  SESSION MANAGEMENT
    // ============================================================

    /// <summary>
    /// Assign client to a session.
    /// </summary>
    public void SetClientSession(ulong clientId, string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.LogWarning($"[NetworkClientRegistry] Cannot set empty session for client {clientId}");
            return;
        }

        lock (registryLock)
        {
            if (!clients.TryGetValue(clientId, out var clientData))
            {
                Debug.LogWarning($"[NetworkClientRegistry] Client {clientId} not found");
                return;
            }

            // Remove from old session if any
            if (clientData.IsInSession)
            {
                RemoveClientFromSessionIndex(clientId, clientData.CurrentSessionId);
            }

            // Assign new session
            clientData.CurrentSessionId = sessionId;
            clientData.IsReady = false; // Reset ready state

            // Add to session index
            if (!sessionClients.ContainsKey(sessionId))
                sessionClients[sessionId] = new HashSet<ulong>();

            sessionClients[sessionId].Add(clientId);

            Debug.Log($"[NetworkClientRegistry] Client {clientId} joined session '{sessionId}'");
            OnClientJoinedSession?.Invoke(clientId, sessionId);
        }
    }

    /// <summary>
    /// Remove client from their current session.
    /// </summary>
    public void ClearClientSession(ulong clientId)
    {
        lock (registryLock)
        {
            if (!clients.TryGetValue(clientId, out var clientData))
                return;

            if (!clientData.IsInSession)
                return;

            string oldSession = clientData.CurrentSessionId;
            RemoveClientFromSessionIndex(clientId, oldSession);

            clientData.CurrentSessionId = null;
            clientData.IsReady = false;

            Debug.Log($"[NetworkClientRegistry] Client {clientId} left session '{oldSession}'");
            OnClientLeftSession?.Invoke(clientId, oldSession);
        }
    }

    private void RemoveClientFromSessionIndex(ulong clientId, string sessionId)
    {
        if (sessionClients.TryGetValue(sessionId, out var clients))
        {
            clients.Remove(clientId);

            // Clean up empty session entries
            if (clients.Count == 0)
                sessionClients.Remove(sessionId);
        }
    }

    /// <summary>
    /// Get current session of a client.
    /// </summary>
    public string GetClientSession(ulong clientId)
    {
        lock (registryLock)
        {
            return clients.TryGetValue(clientId, out var data) ? data.CurrentSessionId : null;
        }
    }

    /// <summary>
    /// Check if client is in a specific session.
    /// </summary>
    public bool IsClientInSession(ulong clientId, string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return false;

        lock (registryLock)
        {
            return clients.TryGetValue(clientId, out var data) && data.CurrentSessionId == sessionId;
        }
    }

    /// <summary>
    /// Get all clients in a session.
    /// </summary>
    public List<ClientNetworkData> GetClientsInSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return new List<ClientNetworkData>();

        lock (registryLock)
        {
            if (!sessionClients.TryGetValue(sessionId, out var clientIds))
                return new List<ClientNetworkData>();

            return clientIds.Select(id => clients.TryGetValue(id, out var data) ? data : null)
                           .Where(d => d != null)
                           .ToList();
        }
    }

    /// <summary>
    /// Get client IDs in a session.
    /// </summary>
    public List<ulong> GetClientIdsInSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return new List<ulong>();

        lock (registryLock)
        {
            return sessionClients.TryGetValue(sessionId, out var ids)
                ? new List<ulong>(ids)
                : new List<ulong>();
        }
    }

    /// <summary>
    /// Get count of clients in a session.
    /// </summary>
    public int GetSessionClientCount(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return 0;

        lock (registryLock)
        {
            return sessionClients.TryGetValue(sessionId, out var ids) ? ids.Count : 0;
        }
    }

    // ============================================================
    //  STATE MANAGEMENT
    // ============================================================

    /// <summary>
    /// Set client ready state.
    /// </summary>
    public void SetClientReady(ulong clientId, bool ready)
    {
        lock (registryLock)
        {
            if (clients.TryGetValue(clientId, out var clientData))
            {
                clientData.IsReady = ready;
                Debug.Log($"[NetworkClientRegistry] Client {clientId} ready: {ready}");
            }
        }
    }

    /// <summary>
    /// Record client activity.
    /// </summary>
    public void RecordClientActivity(ulong clientId)
    {
        lock (registryLock)
        {
            if (clients.TryGetValue(clientId, out var clientData))
            {
                clientData.RecordActivity();
            }
        }
    }

    // ============================================================
    //  STATISTICS
    // ============================================================

    /// <summary>
    /// Get total registered clients count.
    /// </summary>
    public int GetTotalClientCount()
    {
        lock (registryLock)
        {
            return clients.Count;
        }
    }

    /// <summary>
    /// Get total active sessions count.
    /// </summary>
    public int GetActiveSessionCount()
    {
        lock (registryLock)
        {
            return sessionClients.Count;
        }
    }

    /// <summary>
    /// Get clients without a session.
    /// </summary>
    public List<ClientNetworkData> GetUnassignedClients()
    {
        lock (registryLock)
        {
            return clients.Values.Where(c => !c.IsInSession).ToList();
        }
    }
}
