using Core.Games;
using Core.Patterns;
using Core.Utilities;
using Networking.StateSync;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class GameSessionManager : Singleton<GameSessionManager>
{
    private readonly Dictionary<string, SessionState> sessions = new Dictionary<string, SessionState>();
    private readonly Dictionary<ulong, string> clientDisplayNames = new Dictionary<ulong, string>();
    private NetworkManager networkManager;

    // Secure container manager for session isolation
    private SessionContainerManager containerManager;

    protected override void OnInitialize()
    {
        networkManager = NetworkManager.Singleton;

        // Initialize container manager for session isolation
        containerManager = new SessionContainerManager();

        if (networkManager != null)
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        GlobalRegistryHub.EnsureInstance();

        NetworkLogger.Info("GameSessionManager", "Initialized with session isolation support");
    }

    protected override void OnCleanup()
    {
        if (networkManager != null)
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;

        if (containerManager != null)
        {
            containerManager.Dispose();
            containerManager = null;
        }

        NetworkLogger.Info("GameSessionManager", "Cleaned up all resources");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        RemoveSessionsForClient(clientId);

        // Clean up container authorization
        containerManager?.HandleClientDisconnect(clientId);

        clientDisplayNames.Remove(clientId);
    }



    // ============================
    //   AJOUT DE SESSION
    // ============================
    public bool TryAddSession(ulong clientId, string sessionName)
    {
        return TryAddSession(clientId, sessionName, null);
    }

    public bool TryAddSession(ulong clientId, string sessionName, string playerName)
    {
        if (networkManager == null || !networkManager.IsServer)
        {
            Debug.LogError("[GameSessionManager] ❌ TryAddSession appelé côté client !");
            return false;
        }

        SetClientDisplayName(clientId, playerName);

        sessionName = sessionName?.Trim();

        if (string.IsNullOrEmpty(sessionName))
        {
            Debug.LogWarning("[GameSessionManager] Session sans nom refusée");
            return false;
        }

        if (sessions.ContainsKey(sessionName))
        {
            Debug.LogWarning($"[GameSessionManager] Session '{sessionName}' existe déjà");
            return false;
        }

        var state = new SessionState
        {
            Name = sessionName,
            Creator = clientId,
            SelectedGameId = "square-game" // Default game type
        };
        state.Players.Add(clientId);

        sessions[sessionName] = state;

        // Create secure container for this session
        var container = containerManager?.CreateSession(sessionName, 4, clientId);
        if (container == null)
        {
            Debug.LogWarning($"[GameSessionManager] Failed to create secure container for '{sessionName}'");
            sessions.Remove(sessionName);
            return false;
        }

        // Host already added by SessionContainerManager.CreateSession.

        Debug.Log($"[SERVER] Secure container created for session '{sessionName}'");

        Debug.Log($"[SERVER] Session ajoutée → {sessionName} (client {clientId})");
        NetworkBootstrap.LogSession("CREATED", sessionName, clientId);

        var registryHub = GlobalRegistryHub.Instance ?? GlobalRegistryHub.EnsureInstance();
        if (registryHub != null)
        {
            registryHub.GameRegisterTemplate.EnsureLoadedFromGameRegistry();
            registryHub.ClientRegistry.TryGetClientUidByNetClientId(clientId, out var clientUid);
            var sessionEntry = registryHub.SessionRegistry.Create(sessionName, clientUid);
            var representation = registryHub.GameRegisterTemplate.GetByGameId(state.SelectedGameId);
            if (sessionEntry != null && representation != null)
            {
                sessionEntry.GameTypeUid = representation.gameTypeUid;
            }
        }

        BroadcastSessions();
        return true;
    }

    public bool TryJoinSession(ulong clientId, string sessionName)
    {
        return TryJoinSession(clientId, sessionName, null);
    }

    public bool TryJoinSession(ulong clientId, string sessionName, string playerName)
    {
        if (networkManager == null || !networkManager.IsServer)
            return false;

        SetClientDisplayName(clientId, playerName);

        sessionName = sessionName?.Trim();
        if (string.IsNullOrEmpty(sessionName))
            return false;

        var existingSession = containerManager?.GetClientSession(clientId);
        if (!string.IsNullOrEmpty(existingSession) && existingSession != sessionName)
        {
            Debug.LogWarning($"[GameSessionManager] Client {clientId} already in session '{existingSession}'");
            return false;
        }

        if (!sessions.TryGetValue(sessionName, out var state))
        {
            Debug.LogWarning($"[GameSessionManager] Session '{sessionName}' introuvable");
            return false;
        }

        // Authorize and add to secure container (also records membership)
        var resolvedName = ResolvePlayerName(clientId);
        bool addedToContainer = containerManager != null && containerManager.JoinSession(sessionName, clientId, resolvedName);
        if (!addedToContainer)
        {
            Debug.LogWarning($"[GameSessionManager] Container rejected join for '{sessionName}' (client {clientId})");
            return false;
        }

        state.Players.Add(clientId);
        state.Ready.Remove(clientId); // reset ready on join

        Debug.Log($"[SERVER] Player {clientId} joined session '{sessionName}'");
        NetworkBootstrap.LogSession("JOINED", sessionName, clientId);

        var registryHub = GlobalRegistryHub.Instance;
        if (registryHub != null)
        {
            if (registryHub.ClientRegistry.TryGetClientUidByNetClientId(clientId, out var clientUid))
            {
                registryHub.SessionRegistry.AddClient(sessionName, clientUid);
            }
        }

        // If there's an active game, add the player to it
        if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(sessionName))
        {
            if (GameInstanceManager.Instance.AddPlayerToGame(sessionName, clientId, resolvedName))
            {
                Debug.Log($"[SERVER] Added late joiner {clientId} to active game in session '{sessionName}'");

                // Notify the client to setup game visuals
                string gameId = GameInstanceManager.Instance.GetGameId(sessionName);
                if (!string.IsNullOrEmpty(gameId))
                {
                    Vector3 worldOffset = Vector3.zero;
                    GameInstanceManager.Instance.TryGetWorldOffset(sessionName, out worldOffset);
                    SessionRpcHub.Instance?.NotifyLateJoinerClientRpc(sessionName, gameId, worldOffset, clientId);
                }
            }
        }

        BroadcastSessions();
        return true;
    }

    private string ResolvePlayerName(ulong clientId)
    {
        if (clientDisplayNames.TryGetValue(clientId, out var stored) && !string.IsNullOrWhiteSpace(stored))
        {
            return stored;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
        {
            foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                var player = obj.GetComponent<DefaultPlayer>();
                if (player != null && player.OwnerClientId == clientId && !player.NameAgent.Value.IsEmpty)
                {
                    return player.NameAgent.Value.ToString();
                }
            }
        }
        return $"Player {clientId}";
    }

    private void SetClientDisplayName(ulong clientId, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        playerName = playerName.Trim();
        if (playerName.Length > 20)
        {
            playerName = playerName.Substring(0, 20);
        }

        clientDisplayNames[clientId] = playerName;
    }

    public bool SetReady(ulong clientId, string sessionName, bool ready)
    {
        if (networkManager == null || !networkManager.IsServer)
            return false;

        if (!sessions.TryGetValue(sessionName, out var state))
            return false;

        if (!state.Players.Contains(clientId))
            return false;

        if (ready)
            state.Ready.Add(clientId);
        else
            state.Ready.Remove(clientId);

        BroadcastSessions();
        return true;
    }

    /// <summary>
    /// [SERVER] Set the game type for a session.
    /// Only the creator can change this.
    /// </summary>
    public bool SetGameType(ulong clientId, string sessionName, string gameId)
    {
        if (networkManager == null || !networkManager.IsServer)
            return false;

        if (!sessions.TryGetValue(sessionName, out var state))
            return false;

        // Only creator can change game type
        if (state.Creator != clientId)
        {
            Debug.LogWarning($"[GameSessionManager] Client {clientId} cannot change game type (not creator)");
            return false;
        }

        // Validate game exists
        if (!GameRegistry.HasGame(gameId))
        {
            Debug.LogWarning($"[GameSessionManager] Game '{gameId}' not found in registry");
            return false;
        }

        state.SelectedGameId = gameId;
        Debug.Log($"[GameSessionManager] Session '{sessionName}' game set to '{gameId}'");

        var registryHub = GlobalRegistryHub.Instance;
        if (registryHub != null)
        {
            registryHub.GameRegisterTemplate.EnsureLoadedFromGameRegistry();
            var entry = registryHub.SessionRegistry.GetByName(sessionName);
            var representation = registryHub.GameRegisterTemplate.GetByGameId(gameId);
            if (entry != null && representation != null)
            {
                entry.GameTypeUid = representation.gameTypeUid;
            }
        }

        BroadcastSessions();
        return true;
    }

    /// <summary>
    /// Get the selected game ID for a session.
    /// </summary>
    public string GetSelectedGameId(string sessionName)
    {
        if (sessions.TryGetValue(sessionName, out var state))
            return state.SelectedGameId;
        return null;
    }

    /// <summary>
    /// [SERVER] Leave a specific session without disconnecting.
    /// </summary>
    public void LeaveSession(ulong clientId, string sessionName)
    {
        if (networkManager == null || !networkManager.IsServer)
            return;

        if (!sessions.TryGetValue(sessionName, out var state))
        {
            Debug.LogWarning($"[GameSessionManager] LeaveSession: session '{sessionName}' not found");
            return;
        }

        bool changed = false;

        // If creator leaves, destroy the session
        if (state.Creator == clientId)
        {
            Debug.Log($"[SERVER] Session '{sessionName}' destroyed (creator {clientId} left)");
            NetworkBootstrap.LogSession("DESTROYED", sessionName);
            sessions.Remove(sessionName);

            // Destroy secure container
            containerManager?.DestroySession(sessionName);

            // Also destroy any active game
            if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(sessionName))
            {
                GameInstanceManager.Instance.DestroyGame(sessionName);
            }

            changed = true;
        }
        else
        {
            // Remove player from session
            if (state.Players.Remove(clientId) | state.Ready.Remove(clientId))
            {
                Debug.Log($"[SERVER] Player {clientId} left session '{sessionName}'");
                NetworkBootstrap.LogSession("LEFT", sessionName, clientId);

                // Remove from secure container
                var container = containerManager?.GetSession(sessionName);
                container?.RemovePlayer(clientId);

                // Data-oriented refactor (Option 1): also remove the client's simulated entity.
                if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(sessionName))
                {
                    GameInstanceManager.Instance.RemovePlayerFromGame(sessionName, clientId);
                }

                changed = true;

                // If session is now empty, destroy it
                if (state.Players.Count == 0)
                {
                    Debug.Log($"[SERVER] Session '{sessionName}' destroyed (empty)");
                    NetworkBootstrap.LogSession("DESTROYED", sessionName);
                    sessions.Remove(sessionName);

                    // Destroy secure container
                    containerManager?.DestroySession(sessionName);

                    // Also destroy any active game
                    if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(sessionName))
                    {
                        GameInstanceManager.Instance.DestroyGame(sessionName);
                    }
                }
            }
        }

        var registryHub = GlobalRegistryHub.Instance;
        if (registryHub != null)
        {
            if (registryHub.ClientRegistry.TryGetClientUidByNetClientId(clientId, out var clientUid))
            {
                registryHub.SessionRegistry.RemoveClient(sessionName, clientUid);
            }

            if (!sessions.ContainsKey(sessionName))
            {
                var entry = registryHub.SessionRegistry.GetByName(sessionName);
                if (entry != null)
                {
                    registryHub.GameInstanceRegister.RemoveBySessionUid(entry.SessionUid);
                    registryHub.SessionRegistry.Remove(entry.SessionUid);
                }
            }
        }

        if (changed)
        {
            BroadcastSessions();
        }

        // Ensure mapping cleanup even if session still exists
        containerManager?.HandleClientDisconnect(clientId);
    }

    // ============================
    //   NETTOYAGE
    // ============================
    public void RemoveSessionsForClient(ulong clientId)
    {
        if (networkManager == null || !networkManager.IsServer)
            return;

        bool changed = false;
        foreach (var key in sessions.Keys.ToList())
        {
            var s = sessions[key];
            if (s.Creator == clientId)
            {
                Debug.Log($"[SERVER] Session '{key}' destroyed (creator {clientId} left)");
                NetworkBootstrap.LogSession("DESTROYED", key);
                sessions.Remove(key);
                containerManager?.DestroySession(key);
                if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(key))
                {
                    GameInstanceManager.Instance.DestroyGame(key);
                }
                changed = true;
                continue;
            }

            if (s.Players.Remove(clientId) | s.Ready.Remove(clientId))
            {
                Debug.Log($"[SERVER] Player {clientId} left session '{key}'");
                NetworkBootstrap.LogSession("LEFT", key, clientId);
                changed = true;
                containerManager?.LeaveSession(clientId);

                // Data-oriented refactor (Option 1): remove their simulated entity if a game is active.
                if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(key))
                {
                    GameInstanceManager.Instance.RemovePlayerFromGame(key, clientId);
                }
                if (s.Players.Count == 0)
                {
                    Debug.Log($"[SERVER] Session '{key}' destroyed (empty)");
                    NetworkBootstrap.LogSession("DESTROYED", key);
                    sessions.Remove(key);
                    containerManager?.DestroySession(key);
                    if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(key))
                    {
                        GameInstanceManager.Instance.DestroyGame(key);
                    }
                }
            }
        }

        var registryHub = GlobalRegistryHub.Instance;
        if (registryHub != null)
        {
            if (registryHub.ClientRegistry.TryGetClientUidByNetClientId(clientId, out var clientUid))
            {
                foreach (var entry in registryHub.SessionRegistry.Sessions.Values)
                {
                    entry.ClientUids.Remove(clientUid);
                }
            }

            var removedSessions = new List<string>();
            foreach (var entry in registryHub.SessionRegistry.Sessions.Values)
            {
                if (!sessions.ContainsKey(entry.Name))
                {
                    removedSessions.Add(entry.SessionUid);
                }
            }

            foreach (var sessionUid in removedSessions)
            {
                registryHub.GameInstanceRegister.RemoveBySessionUid(sessionUid);
                registryHub.SessionRegistry.Remove(sessionUid);
            }
        }

        if (changed)
        {
            Debug.Log($"[GameSessionManager] Sessions mises à jour après départ {clientId}");
            BroadcastSessions();
        }

        containerManager?.HandleClientDisconnect(clientId);
    }

    // ============================
    //   RÉCUPÉRER TOUTES LES SESSIONS
    // ============================
    public GameSession[] GetSessionsSnapshot()
    {
        return sessions.Values
            .Select(s => new GameSession(s.Name, s.Creator, s.Players.Count, s.Ready.Count))
            .ToArray();
    }

    public List<ulong> GetPlayers(string sessionName)
    {
        if (sessions.TryGetValue(sessionName, out var state))
            return state.Players.ToList();

        return new List<ulong>();
    }

    public void BroadcastSessions()
    {
        if (networkManager == null || !networkManager.IsServer)
            return;

        if (!SessionRpcHub.TryBroadcastSessions(GetSessionsSnapshot()))
        {
            Debug.LogWarning("[GameSessionManager] Impossible de diffuser les sessions (pas de SessionRpcHub serveur)");
        }
    }

    public SessionDetails? BuildDetails(string sessionName)
    {
        if (!sessions.TryGetValue(sessionName, out var state))
            return null;

        var details = new SessionDetails
        {
            session = new GameSession(state.Name, state.Creator, state.Players.Count, state.Ready.Count),
            players = new List<SessionPlayerInfo>()
        };

        foreach (var clientId in state.Players)
        {
            string name = ResolvePlayerName(clientId);
            bool ready = state.Ready.Contains(clientId);
            bool isCreator = clientId == state.Creator;
            details.players.Add(new SessionPlayerInfo(clientId, name, ready, isCreator));
        }

        return details;
    }

    // ============================
    //   DEBUG SERVEUR
    // ============================
    public void DebugPrintSessions()
    {
        Debug.Log("=== LISTE DES SESSIONS ===");

        foreach (var kvp in sessions)
        {
            var s = kvp.Value;
            Debug.Log($"- Name={s.Name}, Creator={s.Creator}, Players={s.Players.Count}, Ready={s.Ready.Count}");
        }

        if (sessions.Count == 0)
            Debug.Log("(aucune session)");
    }

    // ============================
    //   SECURE CONTAINER ACCESS
    // ============================

    /// <summary>
    /// Get secure container for a session (server only).
    /// </summary>
    public SessionContainer GetSecureContainer(string sessionName)
    {
        return containerManager?.GetSession(sessionName);
    }

    /// <summary>
    /// Backward-compatible alias for retrieving a session container.
    /// </summary>
    public SessionContainer GetSession(string sessionName)
    {
        return GetSecureContainer(sessionName);
    }

    /// <summary>
    /// Get the session name for a client.
    /// </summary>
    public string GetClientSession(ulong clientId)
    {
        return containerManager?.GetClientSession(clientId);
    }

    /// <summary>
    /// Validate a movement request against session bounds.
    /// </summary>
    public bool ValidateMovement(ulong clientId, string sessionName, Vector3 position)
    {
        var container = containerManager?.GetSession(sessionName);
        if (container == null) return false;

        return container.ValidatePositionInBounds(position);
    }

    /// <summary>
    /// Get container manager statistics.
    /// </summary>
    public (int sessions, int clients) GetContainerStats()
    {
        if (containerManager == null) return (0, 0);
        return (containerManager.GetSessionCount(), containerManager.GetTotalClientCount());
    }
}
