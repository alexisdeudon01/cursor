using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Presenter/ViewModel for session UI.
/// Mediates between UI views and <see cref="ISessionService"/>.
/// Provides UI-friendly properties and commands.
/// </summary>
/// <remarks>
/// <para><b>Design Pattern:</b> MVP (Model-View-Presenter) / MVVM-like.</para>
/// <para><b>Purpose:</b> Decouple UI from network layer, enable testability.</para>
/// <para><b>Usage:</b> UI binds to events and calls methods on this presenter.</para>
/// </remarks>
/// <example>
/// <code>
/// // UI Setup
/// presenter.SessionListChanged += UpdateSessionListUI;
/// presenter.LobbyStateChanged += UpdateLobbyUI;
/// presenter.ErrorReceived += ShowErrorPopup;
/// 
/// // UI Actions
/// submitButton.clicked += () => presenter.SubmitPlayerName(nameField.value);
/// createButton.clicked += () => presenter.CreateSession(sessionNameField.value);
/// readyButton.clicked += () => presenter.ToggleReady();
/// </code>
/// </example>
public class SessionPresenter : MonoBehaviour
{
    #region Inspector

    [Header("Configuration")]
    [Tooltip("Auto-refresh session list interval (0 = disabled)")]
    [SerializeField] private float autoRefreshInterval = 5f;

    #endregion

    #region Events for UI Binding

    /// <summary>Fired when the current UI panel should change.</summary>
    public event Action<SessionUIPanel> PanelChanged;

    /// <summary>Fired when the session list is updated.</summary>
    public event Action<List<SessionListItem>> SessionListChanged;

    /// <summary>Fired when lobby state changes (players, ready states).</summary>
    public event Action<LobbyState> LobbyStateChanged;

    /// <summary>Fired when an error should be displayed.</summary>
    public event Action<string> ErrorReceived;

    /// <summary>Fired when game is starting.</summary>
    public event Action<string> GameStarting;

    #endregion

    #region State Properties

    /// <summary>The local player's display name.</summary>
    public string PlayerName { get; private set; }

    /// <summary>Current session name (null if not in session).</summary>
    public string CurrentSessionName => _sessionService?.CurrentSessionName;

    /// <summary>True if in a session.</summary>
    public bool IsInSession => _sessionService?.IsInSession ?? false;

    /// <summary>True if local player is host.</summary>
    public bool IsHost => _sessionService?.IsHost ?? false;

    /// <summary>True if local player is ready.</summary>
    public bool IsReady => _sessionService?.IsReady ?? false;

    /// <summary>Current UI panel being displayed.</summary>
    public SessionUIPanel CurrentPanel { get; private set; } = SessionUIPanel.Name;

    #endregion

    #region Private State

    private ISessionService _sessionService;
    private float _lastRefreshTime;
    private readonly List<SessionListItem> _cachedSessionList = new List<SessionListItem>();
    private LobbyState _cachedLobbyState;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        Debug.Log("[SessionPresenter] Initialized");
    }

    private void OnEnable()
    {
        // Service will be injected via SetService or found via ServiceLocator
        if (_sessionService != null)
        {
            SubscribeToService();
        }
    }

    private void OnDisable()
    {
        if (_sessionService != null)
        {
            UnsubscribeFromService();
        }
    }

    private void Update()
    {
        // Auto-refresh session list when on sessions panel
        if (CurrentPanel == SessionUIPanel.Sessions && autoRefreshInterval > 0)
        {
            if (Time.time - _lastRefreshTime > autoRefreshInterval)
            {
                RefreshSessions();
            }
        }
    }

    #endregion

    #region Service Injection

    /// <summary>
    /// Inject the session service dependency.
    /// </summary>
    /// <param name="service">Session service implementation.</param>
    public void SetService(ISessionService service)
    {
        if (_sessionService != null)
        {
            UnsubscribeFromService();
        }

        _sessionService = service;

        if (_sessionService != null && enabled)
        {
            SubscribeToService();
        }

        Debug.Log($"[SessionPresenter] Service set: {service?.GetType().Name ?? "null"}");
    }

    private void SubscribeToService()
    {
        _sessionService.SessionsChanged += OnSessionsChanged;
        _sessionService.CurrentSessionChanged += OnCurrentSessionChanged;
        _sessionService.ErrorOccurred += OnError;
        _sessionService.GameStarting += OnGameStarting;
    }

    private void UnsubscribeFromService()
    {
        _sessionService.SessionsChanged -= OnSessionsChanged;
        _sessionService.CurrentSessionChanged -= OnCurrentSessionChanged;
        _sessionService.ErrorOccurred -= OnError;
        _sessionService.GameStarting -= OnGameStarting;
    }

    #endregion

    #region UI Commands

    /// <summary>
    /// Submit player name and proceed to session list.
    /// </summary>
    /// <param name="name">Player display name.</param>
    public void SubmitPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorReceived?.Invoke("Please enter a valid name");
            return;
        }

        PlayerName = name.Trim();
        Debug.Log($"[SessionPresenter] Player name set: {PlayerName}");

        NavigateTo(SessionUIPanel.Sessions);
        RefreshSessions();
    }

    /// <summary>
    /// Create a new session with the given name.
    /// </summary>
    /// <param name="sessionName">Session name.</param>
    public void CreateSession(string sessionName)
    {
        if (!ValidateService()) return;

        if (string.IsNullOrWhiteSpace(sessionName))
        {
            ErrorReceived?.Invoke("Please enter a session name");
            return;
        }

        Debug.Log($"[SessionPresenter] Creating session: {sessionName}");
        _sessionService.CreateSession(sessionName.Trim());
    }

    /// <summary>
    /// Join an existing session.
    /// </summary>
    /// <param name="sessionName">Session to join.</param>
    public void JoinSession(string sessionName)
    {
        if (!ValidateService()) return;

        Debug.Log($"[SessionPresenter] Joining session: {sessionName}");
        _sessionService.JoinSession(sessionName);
    }

    /// <summary>
    /// Leave the current session.
    /// </summary>
    public void LeaveSession()
    {
        if (!ValidateService()) return;

        Debug.Log("[SessionPresenter] Leaving session");
        _sessionService.LeaveSession();
        NavigateTo(SessionUIPanel.Sessions);
    }

    /// <summary>
    /// Toggle the local player's ready state.
    /// </summary>
    public void ToggleReady()
    {
        if (!ValidateService()) return;

        bool newReady = !IsReady;
        Debug.Log($"[SessionPresenter] Toggling ready: {newReady}");
        _sessionService.SetReady(newReady);
    }

    /// <summary>
    /// Start the game (host only).
    /// </summary>
    public void StartGame()
    {
        if (!ValidateService()) return;

        if (!IsHost)
        {
            ErrorReceived?.Invoke("Only the host can start the game");
            return;
        }

        Debug.Log("[SessionPresenter] Starting game");
        _sessionService.StartGame();
    }

    /// <summary>
    /// Refresh the session list.
    /// </summary>
    public void RefreshSessions()
    {
        if (!ValidateService()) return;

        _lastRefreshTime = Time.time;
        _sessionService.RefreshSessions();
    }

    /// <summary>
    /// Navigate back to name entry panel.
    /// </summary>
    public void BackToNameEntry()
    {
        if (IsInSession)
        {
            _sessionService?.LeaveSession();
        }
        NavigateTo(SessionUIPanel.Name);
    }

    /// <summary>
    /// Navigate back to session list.
    /// </summary>
    public void BackToSessionList()
    {
        if (IsInSession)
        {
            _sessionService?.LeaveSession();
        }
        NavigateTo(SessionUIPanel.Sessions);
        RefreshSessions();
    }

    #endregion

    #region Service Event Handlers

    private void OnSessionsChanged(GameSession[] sessions)
    {
        _cachedSessionList.Clear();

        foreach (var session in sessions)
        {
            _cachedSessionList.Add(new SessionListItem
            {
                name = session.name.ToString(),
                playerCount = session.playerCount,
                isJoinable = true // Could add max player check here
            });
        }

        Debug.Log($"[SessionPresenter] Sessions updated: {_cachedSessionList.Count}");
        SessionListChanged?.Invoke(_cachedSessionList);
    }

    private void OnCurrentSessionChanged(SessionDetails details)
    {
        _cachedLobbyState = new LobbyState
        {
            sessionName = details.session.name.ToString(),
            playerCount = details.session.playerCount,
            readyCount = details.session.readyCount,
            isHost = IsHost,
            isReady = IsReady,
            canStart = IsHost && details.session.readyCount >= details.session.playerCount && details.session.playerCount > 0,
            players = new List<LobbyPlayerInfo>()
        };

        foreach (var player in details.players)
        {
            _cachedLobbyState.players.Add(new LobbyPlayerInfo
            {
                clientId = player.clientId,
                displayName = player.name.IsEmpty ? $"Player {player.clientId}" : player.name.ToString(),
                isReady = player.ready,
                isHost = player.clientId == details.session.creator
            });
        }

        Debug.Log($"[SessionPresenter] Lobby updated: {_cachedLobbyState.sessionName} ({_cachedLobbyState.readyCount}/{_cachedLobbyState.playerCount} ready)");

        // Navigate to lobby panel if we just joined
        if (CurrentPanel == SessionUIPanel.Sessions)
        {
            NavigateTo(SessionUIPanel.Lobby);
        }

        LobbyStateChanged?.Invoke(_cachedLobbyState);
    }

    private void OnError(SessionError error)
    {
        Debug.LogWarning($"[SessionPresenter] Error: {error}");
        ErrorReceived?.Invoke(error.message);
    }

    private void OnGameStarting(GameStartInfo info)
    {
        Debug.Log($"[SessionPresenter] Game starting: {info.sessionName}");
        NavigateTo(SessionUIPanel.Loading);
        GameStarting?.Invoke(info.sceneName);
    }

    #endregion

    #region Navigation

    private void NavigateTo(SessionUIPanel panel)
    {
        if (CurrentPanel == panel) return;

        Debug.Log($"[SessionPresenter] Navigating: {CurrentPanel} → {panel}");
        CurrentPanel = panel;
        PanelChanged?.Invoke(panel);
    }

    #endregion

    #region Helpers

    private bool ValidateService()
    {
        if (_sessionService == null)
        {
            ErrorReceived?.Invoke("Session service not available");
            Debug.LogError("[SessionPresenter] Session service is null!");
            return false;
        }
        return true;
    }

    #endregion
}

#region UI Data Types

/// <summary>
/// UI panels in the session flow.
/// </summary>
public enum SessionUIPanel
{
    /// <summary>Name entry panel.</summary>
    Name,

    /// <summary>Session list panel.</summary>
    Sessions,

    /// <summary>Lobby/waiting room panel.</summary>
    Lobby,

    /// <summary>Loading/transition panel.</summary>
    Loading
}

/// <summary>
/// Session list item for UI display.
/// </summary>
[Serializable]
public class SessionListItem
{
    public string name;
    public int playerCount;
    public bool isJoinable;
}

/// <summary>
/// Lobby state for UI display.
/// </summary>
[Serializable]
public class LobbyState
{
    public string sessionName;
    public int playerCount;
    public int readyCount;
    public bool isHost;
    public bool isReady;
    public bool canStart;
    public List<LobbyPlayerInfo> players;
}

/// <summary>
/// Player info for lobby UI.
/// </summary>
[Serializable]
public class LobbyPlayerInfo
{
    public ulong clientId;
    public string displayName;
    public bool isReady;
    public bool isHost;
}

#endregion
