using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Client-side implementation of <see cref="ISessionService"/>.
/// Sends RPCs to server and listens for responses via <see cref="SessionRpcHub"/> events.
/// </summary>
/// <remarks>
/// <para><b>Architecture:</b> This class acts as a facade over the RPC layer.</para>
/// <para><b>Threading:</b> All operations run on main thread. Events are invoked on main thread.</para>
/// <para><b>Lifecycle:</b> Should be created after NetworkManager is connected.</para>
/// </remarks>
/// <example>
/// <code>
/// var service = gameObject.AddComponent&lt;SessionServiceClient&gt;();
/// service.SessionsChanged += sessions => UpdateUI(sessions);
/// service.RefreshSessions();
/// </code>
/// </example>
public class SessionServiceClient : MonoBehaviour, ISessionService
{
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
    public bool IsHost { get; private set; }

    /// <inheritdoc/>
    public bool IsReady { get; private set; }

    private ulong LocalClientId => NetworkManager.Singleton?.LocalClientId ?? 0;
    private string pendingSessionName;
    private bool pendingIsHost;

    #endregion

    #region Lifecycle

    private void OnEnable()
    {
        SubscribeToRpcEvents();
        Debug.Log("[SessionServiceClient] Enabled and subscribed to RPC events");
    }

    private void OnDisable()
    {
        UnsubscribeFromRpcEvents();
        Debug.Log("[SessionServiceClient] Disabled and unsubscribed from RPC events");
    }

    private void SubscribeToRpcEvents()
    {
        SessionRpcHub.SessionsUpdated += HandleSessionsUpdated;
        SessionRpcHub.SessionDetailsUpdated += HandleSessionDetailsUpdated;
        SessionRpcHub.GameStart += HandleGameStart;
    }

    private void UnsubscribeFromRpcEvents()
    {
        SessionRpcHub.SessionsUpdated -= HandleSessionsUpdated;
        SessionRpcHub.SessionDetailsUpdated -= HandleSessionDetailsUpdated;
        SessionRpcHub.GameStart -= HandleGameStart;
    }

    #endregion

    #region Commands

    /// <inheritdoc/>
    public void RefreshSessions()
    {
        var hub = GetHub();
        if (hub == null) return;

        Debug.Log("[SessionServiceClient] Requesting sessions refresh");
        hub.RequestSessionsServerRpc();
    }

    /// <inheritdoc/>
    public void CreateSession(string sessionName)
    {
        // Client-side validation
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            RaiseError(SessionErrorCode.InvalidSessionName, "Session name cannot be empty");
            return;
        }

        var hub = GetHub();
        if (hub == null) return;

        Debug.Log($"[SessionServiceClient] Creating session: {sessionName}");
        pendingSessionName = sessionName.Trim();
        pendingIsHost = true;
        hub.CreateSessionServerRpc(sessionName, GetLocalPlayerName());
        hub.RequestSessionDetailsServerRpc(pendingSessionName);
    }

    /// <inheritdoc/>
    public void JoinSession(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            RaiseError(SessionErrorCode.InvalidSessionName, "Session name cannot be empty");
            return;
        }

        var hub = GetHub();
        if (hub == null) return;

        Debug.Log($"[SessionServiceClient] Joining session: {sessionName}");
        pendingSessionName = sessionName.Trim();
        pendingIsHost = false;
        IsReady = false;
        hub.JoinSessionServerRpc(sessionName, GetLocalPlayerName());

        // Request details after joining
        RefreshCurrentSession();
    }

    /// <inheritdoc/>
    public void LeaveSession()
    {
        if (!IsInSession)
        {
            Debug.LogWarning("[SessionServiceClient] Not in a session, cannot leave");
            return;
        }

        Debug.Log($"[SessionServiceClient] Leaving session: {CurrentSessionName}");

        var hub = GetHub();
        if (hub != null)
        {
            hub.LeaveSessionServerRpc(CurrentSessionName);
        }

        ClearSessionState();
        pendingSessionName = null;
    }

    private static string GetLocalPlayerName()
    {
        var playerName = PlayerPrefs.GetString("PlayerName", string.Empty)?.Trim();
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return "Player";
        }

        if (playerName.Length > 20)
        {
            playerName = playerName.Substring(0, 20);
        }

        return playerName;
    }

    /// <inheritdoc/>
    public void SetReady(bool ready)
    {
        if (!IsInSession)
        {
            RaiseError(SessionErrorCode.NotInSession, "Must be in a session to set ready state");
            return;
        }

        var hub = GetHub();
        if (hub == null) return;

        Debug.Log($"[SessionServiceClient] Setting ready: {ready} for session: {CurrentSessionName}");
        IsReady = ready;
        hub.SetReadyServerRpc(CurrentSessionName, ready);
    }

    /// <inheritdoc/>
    public void StartGame()
    {
        if (!IsInSession)
        {
            RaiseError(SessionErrorCode.NotInSession, "Must be in a session to start game");
            return;
        }

        if (!IsHost)
        {
            RaiseError(SessionErrorCode.NotHost, "Only the host can start the game");
            return;
        }

        var hub = GetHub();
        if (hub == null) return;

        Debug.Log($"[SessionServiceClient] Starting game for session: {CurrentSessionName}");
        hub.StartGameServerRpc(CurrentSessionName);
    }

    /// <inheritdoc/>
    public void RefreshCurrentSession()
    {
        var targetSession = CurrentSessionName ?? pendingSessionName;
        if (string.IsNullOrEmpty(targetSession))
        {
            Debug.LogWarning("[SessionServiceClient] Not in a session, cannot refresh details");
            return;
        }

        var hub = GetHub();
        if (hub == null) return;

        Debug.Log($"[SessionServiceClient] Requesting details for session: {targetSession}");
        hub.RequestSessionDetailsServerRpc(targetSession);
    }

    #endregion

    #region RPC Event Handlers

    private void HandleSessionsUpdated(GameSession[] sessions)
    {
        Debug.Log($"[SessionServiceClient] Received {sessions?.Length ?? 0} sessions");
        SessionsChanged?.Invoke(sessions ?? Array.Empty<GameSession>());
    }

    private void HandleSessionDetailsUpdated(SessionDetails details)
    {
        Debug.Log($"[SessionServiceClient] Received details for session: {details.session.name}");

        // Update local state from server
        var detailsSessionName = details.session.name.ToString();
        if (!string.IsNullOrEmpty(pendingSessionName) && detailsSessionName == pendingSessionName)
        {
            CurrentSessionName = pendingSessionName;
            pendingSessionName = null;
            IsHost = details.session.creator == LocalClientId || pendingIsHost;
            UpdateReadyStateFromDetails(details);
        }
        else if (detailsSessionName == CurrentSessionName)
        {
            // Check if we're still the host (creator)
            IsHost = details.session.creator == LocalClientId;

            // Find our ready state from the details
            UpdateReadyStateFromDetails(details);
        }

        CurrentSessionChanged?.Invoke(details);
    }

    private void HandleGameStart(string sessionName, List<ulong> playerIds, NetworkObject pawnPrefab)
    {
        Debug.Log($"[SessionServiceClient] Game starting for session: {sessionName}");

        if (sessionName != CurrentSessionName)
        {
            Debug.LogWarning($"[SessionServiceClient] Received game start for different session: {sessionName}");
            return;
        }

        var info = new GameStartInfo
        {
            sessionName = sessionName,
            sceneName = SceneNames.Game,
            playerIds = playerIds
        };

        GameStarting?.Invoke(info);
    }

    #endregion

    #region Helpers

    private SessionRpcHub GetHub()
    {
        var hub = SessionRpcHub.Instance;
        if (hub == null)
        {
            RaiseError(SessionErrorCode.NetworkError, "SessionRpcHub not available. Is the network connected?");
            return null;
        }

        if (!hub.IsSpawned)
        {
            RaiseError(SessionErrorCode.NetworkError, "SessionRpcHub is not spawned on network");
            return null;
        }

        return hub;
    }

    private void RaiseError(SessionErrorCode code, string message)
    {
        var error = new SessionError(code, message);
        Debug.LogWarning($"[SessionServiceClient] {error}");
        ErrorOccurred?.Invoke(error);
    }

    private void ClearSessionState()
    {
        CurrentSessionName = null;
        IsHost = false;
        IsReady = false;
    }

    private void UpdateReadyStateFromDetails(SessionDetails details)
    {
        // details.players contains SessionPlayerInfo array
        // We need to find our ready state
        foreach (var player in details.players)
        {
            if (player.clientId == LocalClientId)
            {
                IsReady = player.ready;
                break;
            }
        }
    }

    #endregion
}
