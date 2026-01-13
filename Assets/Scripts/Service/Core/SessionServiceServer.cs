using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-side implementation of <see cref="ISessionService"/>.
/// Directly accesses <see cref="GameSessionManager"/> without RPCs.
/// Used by host/server to perform session operations locally.
/// </summary>
/// <remarks>
/// <para><b>Architecture:</b> Direct access to managers, no network round-trip.</para>
/// <para><b>Threading:</b> All operations run on main thread.</para>
/// <para><b>Use case:</b> Dedicated server or host player.</para>
/// </remarks>
public class SessionServiceServer : MonoBehaviour, ISessionService
{
    #region Configuration

    [Header("Game Settings")]
    [Tooltip("Minimum number of players required to start a game")]
    [SerializeField] private int minPlayersToStart = 1;

    [Tooltip("Maximum players per session (0 = unlimited)")]
    [SerializeField] private int maxPlayersPerSession = 8;

    #endregion

    #region Events

    /// <inheritdoc/>
    public event Action<GameSession[]> SessionsChanged;

    /// <inheritdoc/>
    public event Action<SessionDetails> CurrentSessionChanged;

    /// <inheritdoc/>
    public event Action<SessionError> ErrorOccurred;

    /// <inheritdoc/>
    public event Action<GameStartInfo> GameStarting;

    #endregion

    #region State

    /// <inheritdoc/>
    public string CurrentSessionName { get; private set; }

    /// <inheritdoc/>
    public bool IsInSession => !string.IsNullOrEmpty(CurrentSessionName);

    /// <inheritdoc/>
    public bool IsHost => true; // Server is always host

    /// <inheritdoc/>
    public bool IsReady { get; private set; }

    /// <summary>
    /// The local server client ID (0 for dedicated server, or host's client ID).
    /// </summary>
    private ulong ServerClientId => NetworkManager.Singleton?.LocalClientId ?? 0;

    #endregion

    #region Dependencies

    private GameSessionManager SessionManager => GameSessionManager.Instance;

    #endregion

    #region Lifecycle

    private void OnEnable()
    {
        // SERVER ONLY: This service should only run on server/host
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[SessionServiceServer] Disabling - this service is server-only");
            enabled = false;
            return;
        }
        
        Debug.Log("[SessionServiceServer] Enabled - Server-side session service ready");
    }

    private void OnDisable()
    {
        Debug.Log("[SessionServiceServer] Disabled");
    }

    #endregion

    #region Commands

    /// <inheritdoc/>
    public void RefreshSessions()
    {
        var manager = GetSessionManager();
        if (manager == null) return;

        var sessions = manager.GetSessionsSnapshot();
        Debug.Log($"[SessionServiceServer] Refreshing sessions: {sessions.Length} found");
        SessionsChanged?.Invoke(sessions);
    }

    /// <inheritdoc/>
    public void CreateSession(string sessionName)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            RaiseError(SessionErrorCode.InvalidSessionName, "Session name cannot be empty");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        sessionName = sessionName.Trim();

        bool success = manager.TryAddSession(ServerClientId, sessionName);
        if (success)
        {
            Debug.Log($"[SessionServiceServer] Created session: {sessionName}");
            CurrentSessionName = sessionName;
            IsReady = false;

            // Broadcast to all clients
            BroadcastSessionsUpdate();

            // Notify local listeners
            RefreshCurrentSession();
        }
        else
        {
            RaiseError(SessionErrorCode.SessionNameTaken, $"Session '{sessionName}' already exists or invalid");
        }
    }

    /// <inheritdoc/>
    public void JoinSession(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            RaiseError(SessionErrorCode.InvalidSessionName, "Session name cannot be empty");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        sessionName = sessionName.Trim();

        // Check max players
        var players = manager.GetPlayers(sessionName);
        if (players != null && maxPlayersPerSession > 0 && players.Count >= maxPlayersPerSession)
        {
            RaiseError(SessionErrorCode.SessionFull, $"Session '{sessionName}' is full");
            return;
        }

        bool success = manager.TryJoinSession(ServerClientId, sessionName);
        if (success)
        {
            Debug.Log($"[SessionServiceServer] Joined session: {sessionName}");
            CurrentSessionName = sessionName;
            IsReady = false;

            BroadcastSessionsUpdate();
            RefreshCurrentSession();
        }
        else
        {
            RaiseError(SessionErrorCode.SessionNotFound, $"Session '{sessionName}' not found");
        }
    }

    /// <inheritdoc/>
    public void LeaveSession()
    {
        if (!IsInSession)
        {
            Debug.LogWarning("[SessionServiceServer] Not in a session");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        Debug.Log($"[SessionServiceServer] Leaving session: {CurrentSessionName}");
        manager.RemoveSessionsForClient(ServerClientId);

        CurrentSessionName = null;
        IsReady = false;

        BroadcastSessionsUpdate();
    }

    /// <inheritdoc/>
    public void SetReady(bool ready)
    {
        if (!IsInSession)
        {
            RaiseError(SessionErrorCode.NotInSession, "Must be in a session to set ready");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        bool success = manager.SetReady(ServerClientId, CurrentSessionName, ready);
        if (success)
        {
            Debug.Log($"[SessionServiceServer] Set ready: {ready} for session: {CurrentSessionName}");
            IsReady = ready;

            // Broadcast updated details to players in this session
            BroadcastSessionDetails(CurrentSessionName);
        }
    }

    /// <inheritdoc/>
    public void StartGame()
    {
        if (!IsInSession)
        {
            RaiseError(SessionErrorCode.NotInSession, "Must be in a session to start game");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        var players = manager.GetPlayers(CurrentSessionName);
        if (players == null || players.Count < minPlayersToStart)
        {
            RaiseError(SessionErrorCode.NotEnoughPlayers, 
                $"Need at least {minPlayersToStart} players to start (have {players?.Count ?? 0})");
            return;
        }

        // Check ready count
        var details = manager.BuildDetails(CurrentSessionName);
        if (details.HasValue)
        {
            if (details.Value.session.readyCount < details.Value.session.playerCount)
            {
                RaiseError(SessionErrorCode.NotEnoughPlayers, 
                    $"Not all players are ready ({details.Value.session.readyCount}/{details.Value.session.playerCount})");
                return;
            }
        }

        Debug.Log($"[SessionServiceServer] Starting game for session: {CurrentSessionName}");

        var info = new GameStartInfo
        {
            sessionName = CurrentSessionName,
            sceneName = SceneNames.Game,
            playerIds = new List<ulong>(players)
        };

        var hub = SessionRpcHub.Instance;
        if (hub != null && hub.IsServer)
        {
            hub.StartGameServerRpc(CurrentSessionName);
            if (GameInstanceManager.Instance != null && GameInstanceManager.Instance.HasActiveGame(CurrentSessionName))
            {
                GameStarting?.Invoke(info);
            }
        }
        else
        {
            Debug.LogWarning("[SessionServiceServer] SessionRpcHub missing - StartGame not dispatched over RPC");
            GameStarting?.Invoke(info);
        }
    }

    /// <inheritdoc/>
    public void RefreshCurrentSession()
    {
        if (!IsInSession)
        {
            Debug.LogWarning("[SessionServiceServer] Not in a session");
            return;
        }

        var manager = GetSessionManager();
        if (manager == null) return;

        var details = manager.BuildDetails(CurrentSessionName);
        if (details.HasValue)
        {
            Debug.Log($"[SessionServiceServer] Refreshed details for: {CurrentSessionName}");
            CurrentSessionChanged?.Invoke(details.Value);
        }
    }

    #endregion

    #region Server Broadcast Methods

    /// <summary>
    /// Broadcast session list update to all connected clients.
    /// </summary>
    public void BroadcastSessionsUpdate()
    {
        var manager = GetSessionManager();
        if (manager == null) return;

        var sessions = manager.GetSessionsSnapshot();
        
        // Use SessionRpcHub to broadcast
        SessionRpcHub.TryBroadcastSessions(sessions);
        
        // Also notify local listeners
        SessionsChanged?.Invoke(sessions);
    }

    /// <summary>
    /// Broadcast session details to all players in a specific session.
    /// </summary>
    /// <param name="sessionName">Session to broadcast details for.</param>
    public void BroadcastSessionDetails(string sessionName)
    {
        var manager = GetSessionManager();
        if (manager == null) return;

        var details = manager.BuildDetails(sessionName);
        if (details.HasValue)
        {
            var hub = SessionRpcHub.Instance;
            if (hub != null && hub.IsServer && hub.IsSpawned)
            {
                var ids = new ulong[details.Value.players.Count];
                for (int i = 0; i < details.Value.players.Count; i++)
                {
                    ids[i] = details.Value.players[i].clientId;
                }

                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = ids }
                };

                hub.SendSessionDetailsClientRpc(details.Value, rpcParams);
            }

            if (sessionName == CurrentSessionName)
            {
                CurrentSessionChanged?.Invoke(details.Value);
            }
        }
    }

    #endregion

    #region Helpers

    private GameSessionManager GetSessionManager()
    {
        var manager = SessionManager;
        if (manager == null)
        {
            RaiseError(SessionErrorCode.NetworkError, "GameSessionManager not available");
            return null;
        }
        return manager;
    }

    private void RaiseError(SessionErrorCode code, string message)
    {
        var error = new SessionError(code, message);
        Debug.LogWarning($"[SessionServiceServer] {error}");
        ErrorOccurred?.Invoke(error);
    }

    #endregion

    #region Public Configuration

    /// <summary>
    /// Get or set the minimum players required to start a game.
    /// </summary>
    public int MinPlayersToStart
    {
        get => minPlayersToStart;
        set => minPlayersToStart = Mathf.Max(1, value);
    }

    /// <summary>
    /// Get or set the maximum players per session (0 = unlimited).
    /// </summary>
    public int MaxPlayersPerSession
    {
        get => maxPlayersPerSession;
        set => maxPlayersPerSession = Mathf.Max(0, value);
    }

    #endregion
}
