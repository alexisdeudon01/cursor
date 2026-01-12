using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Session Lobby UI Controller with Popup and Log Console.
/// </summary>
public class SessionLobbyUI : MonoBehaviour
{
    // Ajout : référence au gestionnaire de canvas de jeu
    [SerializeField] private GameCanvasManager gameCanvasManager;
    public static SessionLobbyUI Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int maxLogEntries = 50;

    // UI Elements - Sessions
    private VisualElement sessionsList;
    private TextField sessionNameField;
    private Button createSessionButton;
    private Button refreshButton;
    private VisualElement lobbyRoot;
    private VisualElement viewTabs;
    private VisualElement sessionsPanel;
    private VisualElement logPanel;
    private Button tabSessionsButton;
    private Button tabConsoleButton;

    // UI Elements - Log Console
    private VisualElement logContainer;
    private ScrollView logScroll;
    private Button clearLogsButton;

    // UI Elements - Popup
    private VisualElement popupOverlay;
    private Label popupSessionName;
    private Label popupReadyCount;
    private DropdownField popupGameType;
    private VisualElement popupPlayersList;
    private Button popupCloseButton;
    private Button popupReadyButton;
    private Button popupStartButton;
    private Button popupLeaveButton;

    // UI Elements - General
    private Label connectionStatus;
    private Button disconnectButton;
    private Label statusLabel;

    // UI Elements - Name Entry
    private VisualElement nameEntryPanel;
    private TextField playerNameField;
    private Button confirmNameButton;
    private Label nameValidationError;

    // State Machine
    public enum LobbyState
    {
        Disconnected,       // Not connected to server
        EnteringName,       // Entering player name (first screen)
        BrowsingSessions,   // In main lobby, viewing available sessions
        InSessionLobby,     // In a session, viewing lobby popup
        GameStarting,       // Game is starting (transition state)
        InGame              // In an active game session
    }

    private StateMachine<LobbyState> _stateMachine;

    private enum BrowserWindowState
    {
        Sessions,
        Console
    }

    private StateMachine<BrowserWindowState> _browserWindowStateMachine;
    private string currentSessionName;
    private bool isHost;
    private bool isReady;
    private bool bound;
    private bool pendingConnectedState;
    private GameSession? currentSession;
    private SessionDetails? currentSessionDetails;
    private string pendingSessionName; // waiting for server confirmation
    private Queue<LogEntry> logEntries = new Queue<LogEntry>();
    private readonly Queue<LogEntry> pendingLogEntries = new Queue<LogEntry>();
    private bool logFlushScheduled;

    private ulong LocalClientId => NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0;

    private struct LogEntry
    {
        public string message;
        public LogType type;
        public string timestamp;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // If the serialized reference points to a prefab asset (or is missing), try to resolve a scene instance.
        if (gameCanvasManager == null || !gameCanvasManager.gameObject.scene.IsValid())
        {
            gameCanvasManager = GameCanvasManager.Instance != null
                ? GameCanvasManager.Instance
                : UnityEngine.Object.FindFirstObjectByType<GameCanvasManager>();
        }

        // Initialize state machine - start in Disconnected state
        _stateMachine = new StateMachine<LobbyState>(LobbyState.Disconnected);
        ConfigureStateMachine();

        _browserWindowStateMachine = new StateMachine<BrowserWindowState>(BrowserWindowState.Sessions);
        _browserWindowStateMachine.OnStateChanged += (from, to) =>
        {
            Debug.Log($"[SessionLobbyUI] BrowserWindow: {from} -> {to}");
            ApplyBrowserWindow();
        };
    }

    /// <summary>
    /// Configure state machine transitions and callbacks.
    /// </summary>
    private void ConfigureStateMachine()
    {
        // EnteringName State (First Screen)
        _stateMachine.Configure(LobbyState.EnteringName)
            .OnEnter(() =>
            {
                ShowEnteringNameScreen();
                Debug.Log("[SessionLobbyUI] État: EnteringName");
            })
            .OnExit(() =>
            {
                if (nameEntryPanel != null) nameEntryPanel.style.display = DisplayStyle.None;
                if (lobbyRoot != null) lobbyRoot.style.display = DisplayStyle.Flex;
                if (sessionsList?.parent?.parent != null) sessionsList.parent.parent.style.display = DisplayStyle.Flex;
            });

        // Disconnected State
        _stateMachine.Configure(LobbyState.Disconnected)
            .OnEnter(() =>
            {
                if (nameEntryPanel != null) nameEntryPanel.style.display = DisplayStyle.None;
                if (sessionsList != null) sessionsList.parent.parent.style.display = DisplayStyle.None;
                if (viewTabs != null) viewTabs.style.display = DisplayStyle.None;
                if (sessionsPanel != null) sessionsPanel.style.display = DisplayStyle.None;
                if (logPanel != null) logPanel.style.display = DisplayStyle.None;
                if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
                if (connectionStatus != null) connectionStatus.text = "Déconnecté";
                DisableAllButtons();
                Debug.Log("[SessionLobbyUI] État: Disconnected");
            });

        // BrowsingSessions State
        _stateMachine.Configure(LobbyState.BrowsingSessions)
            .OnEnter(() =>
            {
                if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                    uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                if (lobbyRoot != null) lobbyRoot.style.display = DisplayStyle.Flex;
                if (viewTabs != null) viewTabs.style.display = DisplayStyle.Flex;
                if (connectionStatus != null) connectionStatus.text = "Connecté";
                if (GameDebugUI.Instance != null)
                    GameDebugUI.Instance.Hide();
                if (gameCanvasManager != null)
                    gameCanvasManager.HideGameCanvas();
                SetBrowserWindow(BrowserWindowState.Sessions);
                EnableSessionBrowserButtons();
                currentSessionName = null;
                isHost = false;
                isReady = false;
                Debug.Log("[SessionLobbyUI] État: BrowsingSessions");
            });

        // InSessionLobby State
        _stateMachine.Configure(LobbyState.InSessionLobby)
            .OnEnter(() =>
            {
                if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.Flex;
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                    uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                if (lobbyRoot != null) lobbyRoot.style.display = DisplayStyle.None;
                if (viewTabs != null) viewTabs.style.display = DisplayStyle.None;
                if (GameDebugUI.Instance != null)
                    GameDebugUI.Instance.Hide();
                if (gameCanvasManager != null)
                    gameCanvasManager.HideGameCanvas();
                DisableSessionBrowserButtons();
                Debug.Log($"[SessionLobbyUI] État: InSessionLobby (session={currentSessionName})");
            })
            .OnExit(() =>
            {
                if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
            });

        // GameStarting State (transition)
        _stateMachine.Configure(LobbyState.GameStarting)
            .OnEnter(() =>
            {
                DisableAllButtons();
                Debug.Log("[SessionLobbyUI] État: GameStarting");
            });

        // InGame State
        _stateMachine.Configure(LobbyState.InGame)
            .OnEnter(() =>
            {
                if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                    uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                Debug.Log("[SessionLobbyUI] État: InGame");
            })
            .OnExit(() =>
            {
                // Restore UI when leaving game
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                    uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            });
    }

    private void DisableAllButtons()
    {
        if (createSessionButton != null) createSessionButton.SetEnabled(false);
        if (refreshButton != null) refreshButton.SetEnabled(false);
        if (popupReadyButton != null) popupReadyButton.SetEnabled(false);
        if (popupStartButton != null) popupStartButton.SetEnabled(false);
        if (popupLeaveButton != null) popupLeaveButton.SetEnabled(false);
    }

    private void EnableSessionBrowserButtons()
    {
        if (createSessionButton != null) createSessionButton.SetEnabled(true);
        if (refreshButton != null) refreshButton.SetEnabled(true);
    }

    private void DisableSessionBrowserButtons()
    {
        if (createSessionButton != null) createSessionButton.SetEnabled(false);
        if (refreshButton != null) refreshButton.SetEnabled(false);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;

        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        bound = false;
        Invoke(nameof(TryBind), 0.1f);

        // Safety: make sure the UI stays visible when this component re-enables.
        if (uiDocument != null && uiDocument.rootVisualElement != null)
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        Unbind();
    }

    private void Start()
    {
        SessionRpcHub.SessionsUpdated += OnSessionsUpdated;
        SessionRpcHub.GameStart += OnGameStart;
        SessionRpcHub.GameStartFailed += OnGameStartFailed;
        SessionRpcHub.SessionDetailsUpdated += OnSessionDetailsUpdated;

        // Subscribe to connection events
        if (NetworkBootstrap.Instance != null)
        {
            NetworkBootstrap.Instance.OnConnected += HandleClientConnected;
            NetworkBootstrap.Instance.OnDisconnected += HandleClientDisconnected;

            // If we connected while still in the Menu scene, remember to apply the state once bound.
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                pendingConnectedState = true;
        }
        else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            // Fallback if bootstrap is missing but netcode reports a connection.
            pendingConnectedState = true;
        }
    }

    private void OnDestroy()
    {
        SessionRpcHub.SessionsUpdated -= OnSessionsUpdated;
        SessionRpcHub.GameStart -= OnGameStart;
        SessionRpcHub.GameStartFailed -= OnGameStartFailed;
        SessionRpcHub.SessionDetailsUpdated -= OnSessionDetailsUpdated;

        // Unsubscribe from connection events
        if (NetworkBootstrap.Instance != null)
        {
            NetworkBootstrap.Instance.OnConnected -= HandleClientConnected;
            NetworkBootstrap.Instance.OnDisconnected -= HandleClientDisconnected;
        }
    }

    private void OnGameStart(string sessionName, System.Collections.Generic.List<ulong> players, Unity.Netcode.NetworkObject pawn)
    {
        Debug.Log($"[SessionLobbyUI] OnGameStart received: session={sessionName}, current={currentSessionName}, players={players?.Count ?? 0}");

        if (sessionName == currentSessionName || string.IsNullOrEmpty(currentSessionName))
        {
            Debug.Log($"[SessionLobbyUI] Processing game start for session: {sessionName}");
            currentSessionName = sessionName; // Ensure it's set

            // Show progress indicator
            StartCoroutine(GameStartProgressSequence(sessionName));
        }
    }

    /// <summary>
    /// Shows a visual progress sequence when game starts.
    /// </summary>
    private System.Collections.IEnumerator GameStartProgressSequence(string sessionName)
    {
        // Phase 1: Initialization
        ProgressIndicator.Show("Démarrage de la partie", 0.1f);
        ProgressIndicator.UpdateProgress(0.1f, "Initialisation...");
        yield return new WaitForSeconds(0.3f);

        // Phase 2: Game systems
        ProgressIndicator.UpdateProgress(0.3f, "Chargement des systèmes de jeu...");

        // Initialize game systems
        GameDebugUI.EnsureInstance();
        if (GameDebugUI.Instance != null)
        {
            GameDebugUI.Instance.SetSession(sessionName);
            if (NetworkManager.Singleton != null)
                GameDebugUI.Instance.SetClientId(NetworkManager.Singleton.LocalClientId);
        }

        PlayerInputHandler.EnsureInstance();
        if (PlayerInputHandler.Instance != null)
        {
            PlayerInputHandler.Instance.SetSession(sessionName);
        }

        yield return new WaitForSeconds(0.3f);

        // Phase 3: UI Setup
        ProgressIndicator.UpdateProgress(0.6f, "Configuration de l'interface...");
        yield return new WaitForSeconds(0.2f);

        // Phase 4: Final preparations
        ProgressIndicator.UpdateProgress(0.9f, "Préparation finale...");
        SetStatus($"Partie démarrée: {sessionName}");
        yield return new WaitForSeconds(0.3f);

        // Phase 5: Complete
        ProgressIndicator.UpdateProgress(1f, "Terminé!");
        yield return new WaitForSeconds(0.5f);

        // Hide progress and switch to game
        ProgressIndicator.Hide();
        SetUIState(LobbyState.InGame);

        // Show success notification
        ToastNotification.Show("Partie démarrée!", ToastNotification.ToastType.Success, 2f);
    }

    private void TryBind()
    {
        if (bound) return;

        if (uiDocument == null)
        {
            Debug.LogError("[SessionLobbyUI] UIDocument missing");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Invoke(nameof(TryBind), 0.1f);
            return;
        }

        // Sessions Panel
        sessionsList = root.Q<VisualElement>("SessionsList");
        sessionNameField = root.Q<TextField>("SessionNameField");
        createSessionButton = root.Q<Button>("CreateSessionButton");
        refreshButton = root.Q<Button>("RefreshButton");
        lobbyRoot = root.Q<VisualElement>("lobby-root");
        viewTabs = root.Q<VisualElement>("view-tabs");
        sessionsPanel = root.Q<VisualElement>("sessions-panel");
        logPanel = root.Q<VisualElement>("log-panel");
        tabSessionsButton = root.Q<Button>("TabSessionsButton");
        tabConsoleButton = root.Q<Button>("TabConsoleButton");

        // Name Entry Panel
        nameEntryPanel = root.Q<VisualElement>("name-entry-panel");
        playerNameField = root.Q<TextField>("PlayerNameField");
        confirmNameButton = root.Q<Button>("ConfirmNameButton");
        nameValidationError = root.Q<Label>("name-validation-error");

        // Log Console
        logContainer = root.Q<VisualElement>("LogContainer");
        logScroll = root.Q<ScrollView>("log-scroll");
        clearLogsButton = root.Q<Button>("ClearLogsButton");

        // Popup
        popupOverlay = root.Q<VisualElement>("popup-overlay");
        popupSessionName = root.Q<Label>("PopupSessionName");
        popupReadyCount = root.Q<Label>("PopupReadyCount");
        popupGameType = root.Q<DropdownField>("PopupGameType");
        popupPlayersList = root.Q<VisualElement>("PopupPlayersList");
        popupCloseButton = root.Q<Button>("PopupCloseButton");
        popupReadyButton = root.Q<Button>("PopupReadyButton");
        popupStartButton = root.Q<Button>("PopupStartButton");
        popupLeaveButton = root.Q<Button>("PopupLeaveButton");

        // General
        connectionStatus = root.Q<Label>("connection-status");
        disconnectButton = root.Q<Button>("DisconnectButton");
        statusLabel = root.Q<Label>("StatusLabel");

        // Bind events
        if (createSessionButton != null) createSessionButton.clicked += OnCreateSession;
        if (refreshButton != null) refreshButton.clicked += OnRefresh;
        if (clearLogsButton != null) clearLogsButton.clicked += OnClearLogs;
        if (disconnectButton != null) disconnectButton.clicked += OnDisconnect;
        if (confirmNameButton != null) confirmNameButton.clicked += OnConfirmName;
        if (tabSessionsButton != null) tabSessionsButton.clicked += OnTabSessions;
        if (tabConsoleButton != null) tabConsoleButton.clicked += OnTabConsole;

        // Popup events - Back button should leave session, not just close popup
        if (popupCloseButton != null) popupCloseButton.clicked += OnLeaveSession;
        if (popupReadyButton != null) popupReadyButton.clicked += OnToggleReady;
        if (popupStartButton != null) popupStartButton.clicked += OnStartGame;
        if (popupLeaveButton != null) popupLeaveButton.clicked += OnLeaveSession;
        if (popupGameType != null) popupGameType.RegisterValueChangedCallback(OnGameTypeChanged);

        // Setup game types dropdown
        SetupGameTypes();

        bound = true;
        Debug.Log("[SessionLobbyUI] UI bound successfully");

        ApplyBrowserWindow();

        // Apply initial state according to current connection (covers the case where we connect in the Menu scene)
        ApplyInitialConnectionState();

        // Ensure entering-name overlay is visible as the first screen.
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsConnectedClient)
        {
            _stateMachine.TransitionTo(LobbyState.EnteringName);
        }

        // Request initial sessions when connected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            OnRefresh();
        }

        ScheduleLogFlush();
    }

    private void Unbind()
    {
        if (!bound) return;

        if (createSessionButton != null) createSessionButton.clicked -= OnCreateSession;
        if (refreshButton != null) refreshButton.clicked -= OnRefresh;
        if (clearLogsButton != null) clearLogsButton.clicked -= OnClearLogs;
        if (disconnectButton != null) disconnectButton.clicked -= OnDisconnect;
        if (popupCloseButton != null) popupCloseButton.clicked -= OnLeaveSession;
        if (popupReadyButton != null) popupReadyButton.clicked -= OnToggleReady;
        if (popupStartButton != null) popupStartButton.clicked -= OnStartGame;
        if (popupLeaveButton != null) popupLeaveButton.clicked -= OnLeaveSession;
        if (popupGameType != null) popupGameType.UnregisterValueChangedCallback(OnGameTypeChanged);
        if (tabSessionsButton != null) tabSessionsButton.clicked -= OnTabSessions;
        if (tabConsoleButton != null) tabConsoleButton.clicked -= OnTabConsole;

        bound = false;
    }

    private void OnTabSessions()
    {
        SetBrowserWindow(BrowserWindowState.Sessions);
    }

    private void OnTabConsole()
    {
        SetBrowserWindow(BrowserWindowState.Console);
    }

    private void SetBrowserWindow(BrowserWindowState state)
    {
        _browserWindowStateMachine?.TransitionTo(state);
        ApplyBrowserWindow();
    }

    private void ApplyBrowserWindow()
    {
        if (_browserWindowStateMachine == null)
        {
            return;
        }

        bool showSessions = _browserWindowStateMachine.IsInState(BrowserWindowState.Sessions);
        bool showConsole = _browserWindowStateMachine.IsInState(BrowserWindowState.Console);

        if (sessionsPanel != null) sessionsPanel.style.display = showSessions ? DisplayStyle.Flex : DisplayStyle.None;
        if (logPanel != null) logPanel.style.display = showConsole ? DisplayStyle.Flex : DisplayStyle.None;

        SetTabActive(tabSessionsButton, showSessions);
        SetTabActive(tabConsoleButton, showConsole);
    }

    private static void SetTabActive(Button button, bool active)
    {
        if (button == null) return;
        if (active) button.AddToClassList("active");
        else button.RemoveFromClassList("active");
    }

    private void SetupGameTypes()
    {
        if (popupGameType == null) return;

        var games = new List<string>();

        GameRegistry.Initialize();
        foreach (var gameDef in GameRegistry.AllGames)
        {
            games.Add(gameDef.GameId);
        }

        if (games.Count == 0)
        {
            games.Add("square-game");
            games.Add("circle-game");
        }

        popupGameType.choices = games;
        if (games.Count > 0)
            popupGameType.value = games[0];
    }

    // ============================================
    //   LOG CONSOLE
    // ============================================

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        // Filter Unity internal logs
        if (message.Contains("Mesh Deformation") ||
            message.Contains("Entities Graphics") ||
            message.Contains("UnityPlayer") ||
            message.Contains("mono-2.0"))
            return;

        var entry = new LogEntry
        {
            message = message,
            type = type,
            timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        logEntries.Enqueue(entry);

        while (logEntries.Count > maxLogEntries)
            logEntries.Dequeue();

        pendingLogEntries.Enqueue(entry);
        ScheduleLogFlush();
    }

    private void ScheduleLogFlush()
    {
        if (logContainer == null || logFlushScheduled || pendingLogEntries.Count == 0)
        {
            return;
        }

        logFlushScheduled = true;
        logContainer.schedule.Execute(FlushPendingLogs);
    }

    private void FlushPendingLogs()
    {
        logFlushScheduled = false;
        if (logContainer == null) return;

        while (pendingLogEntries.Count > 0)
        {
            AddLogEntryToUI(pendingLogEntries.Dequeue());
        }
    }

    private void AddLogEntryToUI(LogEntry entry)
    {
        if (logContainer == null) return;

        var label = new Label($"[{entry.timestamp}] {entry.message}");
        label.AddToClassList("log-entry");

        switch (entry.type)
        {
            case LogType.Warning:
                label.AddToClassList("log-entry-warning");
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                label.AddToClassList("log-entry-error");
                break;
            default:
                label.AddToClassList("log-entry-log");
                break;
        }

        logContainer.Add(label);

        // Auto-scroll to bottom
        logScroll?.schedule.Execute(() =>
        {
            logScroll.scrollOffset = new Vector2(0, float.MaxValue);
        });
    }

    private void OnClearLogs()
    {
        logEntries.Clear();
        pendingLogEntries.Clear();
        if (logContainer != null)
            logContainer.Clear();
    }

    // ============================================
    //   POPUP MANAGEMENT
    // ============================================

    private void OpenPopup(string sessionName, bool asHost)
    {
        currentSessionName = sessionName;
        isHost = asHost;
        isReady = false;
        currentSessionDetails = null;

        if (popupSessionName != null)
            popupSessionName.text = sessionName;

        // Show/hide host controls
        if (popupGameType != null)
            popupGameType.style.display = asHost ? DisplayStyle.Flex : DisplayStyle.None;

        // Request session refresh to get current state
        RequestSessionDetails(sessionName);

        UpdatePopupState();

        // Switch to popup state (handles showing/hiding)
        SetUIState(LobbyState.InSessionLobby);

        // Show success notification
        if (asHost)
        {
            ToastNotification.Show($"Session '{sessionName}' créée avec succès!", ToastNotification.ToastType.Success);
        }
        else
        {
            ToastNotification.Show($"Vous avez rejoint '{sessionName}'", ToastNotification.ToastType.Success);
        }
    }

    private void ClosePopup()
    {
        // Return to lobby state (handles hiding popup)
        SetUIState(LobbyState.BrowsingSessions);
    }

    /// <summary>
    /// Centralized UI state management - ensures only one view is visible at a time.
    /// </summary>
    private void SetUIState(LobbyState newState)
    {
        _stateMachine.TransitionTo(newState);
    }

    /// <summary>
    /// Public method to leave the current game and return to lobby.
    /// Can be called from GameDebugUI or other scripts.
    /// </summary>
    public void LeaveCurrentGame()
    {
        if (!_stateMachine.IsInState(LobbyState.InGame))
        {
            Debug.LogWarning("[SessionLobbyUI] LeaveCurrentGame called but not in game");
            return;
        }

        Debug.Log("[SessionLobbyUI] Leaving current game...");
        OnLeaveSession();
    }

    /// <summary>
    /// Returns true if currently in a game (lobby UI is hidden).
    /// </summary>
    public bool IsInGame => _stateMachine.IsInState(LobbyState.InGame);

    private void UpdatePopupState()
    {
        // Update ready button state even without session data
        if (popupReadyButton != null)
        {
            popupReadyButton.style.display = DisplayStyle.Flex; // Always visible
            popupReadyButton.text = isReady ? "Unready" : "Ready";
            if (isReady)
                popupReadyButton.AddToClassList("unready");
            else
                popupReadyButton.RemoveFromClassList("unready");
        }

        if (!currentSessionDetails.HasValue)
        {
            if (popupReadyCount != null)
                popupReadyCount.text = "Loading...";
            if (popupPlayersList != null)
                popupPlayersList.Clear();
            if (popupStartButton != null)
            {
                popupStartButton.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
                popupStartButton.SetEnabled(false);
                popupStartButton.text = isHost ? "Start game" : string.Empty;
            }
            return;
        }

        var details = currentSessionDetails.Value;
        var playersList = details.players ?? new List<SessionPlayerInfo>();

        // Sync host/ready flags from server data
        isHost = details.session.creator == LocalClientId;
        isReady = false;
        foreach (var p in playersList)
        {
            if (p.clientId == LocalClientId)
            {
                isReady = p.ready;
                break;
            }
        }

        int readyCount = 0;
        foreach (var p in playersList)
        {
            if (p.ready) readyCount++;
        }

        // Ready count with visual progress
        if (popupReadyCount != null)
        {
            popupReadyCount.text = $"{readyCount}/{details.session.playerCount} prêt";

            // Add color coding
            if (readyCount == details.session.playerCount && details.session.playerCount > 0)
            {
                popupReadyCount.style.color = new Color(0.2f, 0.8f, 0.3f); // Green - all ready
            }
            else if (readyCount >= details.session.playerCount / 2)
            {
                popupReadyCount.style.color = new Color(1f, 0.8f, 0.2f); // Orange - half ready
            }
            else
            {
                popupReadyCount.style.color = new Color(0.8f, 0.8f, 0.8f); // Gray - few ready
            }
        }

        // Players list with enhanced visuals
        if (popupPlayersList != null)
        {
            popupPlayersList.Clear();

            foreach (var p in playersList)
            {
                var playerCard = CreatePlayerCard(p);
                popupPlayersList.Add(playerCard);
            }
        }

        // Ready button
        if (popupReadyButton != null)
        {
            popupReadyButton.style.display = DisplayStyle.Flex; // Always visible
            popupReadyButton.text = isReady ? "Pas prêt" : "Prêt";
            if (isReady)
                popupReadyButton.AddToClassList("unready");
            else
                popupReadyButton.RemoveFromClassList("unready");
        }

        // Start button (host only, at least 1 ready)
        if (popupStartButton != null)
        {
            bool canStart = isHost &&
                           readyCount >= 1 &&
                           details.session.playerCount > 0;

            popupStartButton.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
            popupStartButton.SetEnabled(canStart);

            if (canStart)
            {
                popupStartButton.text = "🎮 Démarrer la partie";
                popupStartButton.style.backgroundColor = new Color(0.2f, 0.7f, 0.3f); // Green

                // Add pulse animation when ready to start
                StartCoroutine(PulseStartButton());
            }
            else
            {
                popupStartButton.text = isHost ? "En attente des joueurs..." : "";
                popupStartButton.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f); // Gray
            }
        }

        // Update ready progress bar (if exists)
        UpdateReadyProgressBar(readyCount, details.session.playerCount);
    }

    /// <summary>
    /// Updates or creates a visual progress bar showing ready status.
    /// </summary>
    private void UpdateReadyProgressBar(int readyCount, int totalPlayers)
    {
        if (popupOverlay == null) return;

        // Find or create progress bar container
        var progressContainer = popupOverlay.Q<VisualElement>("ready-progress-container");
        if (progressContainer == null)
        {
            progressContainer = new VisualElement();
            progressContainer.name = "ready-progress-container";
            progressContainer.style.marginTop = 8;
            progressContainer.style.marginBottom = 8;

            // Insert after popupReadyCount
            if (popupReadyCount != null && popupReadyCount.parent != null)
            {
                var parent = popupReadyCount.parent;
                var index = parent.IndexOf(popupReadyCount);
                parent.Insert(index + 1, progressContainer);
            }
        }

        progressContainer.Clear();

        // Progress bar background
        var progressBg = new VisualElement();
        progressBg.style.height = 6;
        progressBg.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        progressBg.style.borderTopLeftRadius = 3;
        progressBg.style.borderTopRightRadius = 3;
        progressBg.style.borderBottomLeftRadius = 3;
        progressBg.style.borderBottomRightRadius = 3;
        progressBg.style.overflow = Overflow.Hidden;

        // Progress fill
        var progressFill = new VisualElement();
        progressFill.style.height = Length.Percent(100);

        float progress = totalPlayers > 0 ? (float)readyCount / totalPlayers : 0f;
        progressFill.style.width = Length.Percent(progress * 100);

        // Color based on progress
        if (progress >= 1f)
        {
            progressFill.style.backgroundColor = new Color(0.2f, 0.8f, 0.3f); // Green - all ready
        }
        else if (progress >= 0.5f)
        {
            progressFill.style.backgroundColor = new Color(1f, 0.7f, 0.2f); // Orange - half ready
        }
        else
        {
            progressFill.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f); // Gray - few ready
        }

        progressBg.Add(progressFill);
        progressContainer.Add(progressBg);
    }

    /// <summary>
    /// Pulse animation for the start button when game is ready to begin.
    /// </summary>
    private System.Collections.IEnumerator PulseStartButton()
    {
        if (popupStartButton == null) yield break;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration && popupStartButton.enabledSelf)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed * Mathf.PI * 2f) * 0.05f;
            popupStartButton.style.scale = new Scale(new Vector3(scale, scale, 1f));
            yield return null;
        }

        if (popupStartButton != null)
        {
            popupStartButton.style.scale = new Scale(Vector3.one);
        }
    }

    // ============================================
    //   NAME ENTRY HANDLERS
    // ============================================

    private void OnConfirmName()
    {
        if (playerNameField == null) return;

        string playerName = playerNameField.value?.Trim();

        // Validation
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ShowNameValidationError("Please enter a name");
            return;
        }

        if (playerName.Length < 2)
        {
            ShowNameValidationError("Name must be at least 2 characters");
            return;
        }

        if (playerName.Length > 20)
        {
            ShowNameValidationError("Name must be 20 characters or less");
            return;
        }

        // Hide validation error
        if (nameValidationError != null)
        {
            nameValidationError.style.display = DisplayStyle.None;
        }

        // Store player name
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        Debug.Log($"[SessionLobbyUI] Player name set: {playerName}");

        // Transition to BrowsingSessions
        _stateMachine.TransitionTo(LobbyState.BrowsingSessions);

        ToastNotification.Show($"Welcome, {playerName}!", ToastNotification.ToastType.Success);

        // Refresh sessions list after entering name
        OnRefresh();
    }

    private void ShowNameValidationError(string message)
    {
        if (nameValidationError != null)
        {
            nameValidationError.text = message;
            nameValidationError.style.display = DisplayStyle.Flex;
        }
    }

    // ============================================
    //   EVENT HANDLERS
    // ============================================

    private void OnCreateSession()
    {
        string name = sessionNameField?.value ?? "Session";
        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("Entrez un nom de session");
            ToastNotification.Show("Le nom de la session ne peut pas être vide", ToastNotification.ToastType.Warning);
            return;
        }

        Debug.Log($"[SessionLobbyUI] Creating session: {name}");
        SetStatus($"Création de '{name}'...");
        ToastNotification.Show($"Création de la session '{name}'...", ToastNotification.ToastType.Info);

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.CreateSessionServerRpc(name, GetLocalPlayerName());
            pendingSessionName = name;
        }
    }

    private void OnRefresh()
    {
        Debug.Log("[SessionLobbyUI] Refreshing sessions...");
        SetStatus("Refreshing...");

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.RequestSessionsServerRpc();
        }

        // Refresh current session details if we have one
        if (!string.IsNullOrEmpty(currentSessionName))
        {
            RequestSessionDetails(currentSessionName);
        }
    }

    private void OnJoinSession(string sessionName)
    {
        Debug.Log($"[SessionLobbyUI] Joining session: {sessionName}");
        SetStatus($"Rejoindre '{sessionName}'...");
        ToastNotification.Show($"Connexion à '{sessionName}'...", ToastNotification.ToastType.Info);

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.JoinSessionServerRpc(sessionName, GetLocalPlayerName());
            pendingSessionName = sessionName;
        }
    }

    private string GetLocalPlayerName()
    {
        var playerName = PlayerPrefs.GetString("PlayerName", string.Empty)?.Trim();
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return $"Player {LocalClientId}";
        }

        if (playerName.Length > 20)
        {
            playerName = playerName.Substring(0, 20);
        }

        return playerName;
    }

    private void OnToggleReady()
    {
        if (string.IsNullOrEmpty(currentSessionName)) return;

        isReady = !isReady;
        Debug.Log($"[SessionLobbyUI] Ready: {isReady}");

        // Visual feedback
        string message = isReady ? "Vous êtes prêt!" : "Vous n'êtes plus prêt";
        var toastType = isReady ? ToastNotification.ToastType.Success : ToastNotification.ToastType.Info;
        ToastNotification.Show(message, toastType, 2f);

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.SetReadyServerRpc(currentSessionName, isReady);
        }

        RequestSessionDetails(currentSessionName);
    }

    private void OnGameTypeChanged(ChangeEvent<string> evt)
    {
        if (!isHost || string.IsNullOrEmpty(currentSessionName)) return;

        Debug.Log($"[SessionLobbyUI] Game type changed: {evt.newValue}");

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.SetGameTypeServerRpc(currentSessionName, evt.newValue);
        }
    }

    private void OnStartGame()
    {
        Debug.Log($"[SessionLobbyUI] OnStartGame button clicked: isHost={isHost}, session='{currentSessionName}'");

        if (!isHost || string.IsNullOrEmpty(currentSessionName))
        {
            Debug.LogWarning($"[SessionLobbyUI] Cannot start game: isHost={isHost}, currentSessionName='{currentSessionName}'");
            ToastNotification.Show("Seul l'hôte peut démarrer la partie", ToastNotification.ToastType.Warning);
            return;
        }

        Debug.Log($"[SessionLobbyUI] Sending StartGameServerRpc for session: {currentSessionName}");
        SetStatus("Démarrage de la partie...");

        // Disable start button to prevent double-click
        if (popupStartButton != null)
        {
            popupStartButton.SetEnabled(false);
            popupStartButton.text = "Démarrage...";
        }

        // Show loading feedback
        ToastNotification.Show("Lancement de la partie en cours...", ToastNotification.ToastType.Info);

        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.StartGameServerRpc(currentSessionName);
        }

        // Safety timeout - re-enable button after 10 seconds if no response
        StartCoroutine(GameStartTimeoutCoroutine());
    }

    private System.Collections.IEnumerator GameStartTimeoutCoroutine()
    {
        yield return new WaitForSeconds(10f);

        // If still in session lobby (not in-game), re-enable button
        if (_stateMachine.IsInState(LobbyState.InSessionLobby) && popupStartButton != null)
        {
            popupStartButton.SetEnabled(true);
            popupStartButton.text = "Démarrer";
            ToastNotification.Show("Le démarrage a échoué (timeout)", ToastNotification.ToastType.Error);
            SetStatus("Échec du démarrage");
        }
    }

    /// <summary>
    /// Called when game start fails on server.
    /// Re-enables UI and shows error feedback.
    /// </summary>
    public void OnGameStartFailed(string errorMessage, GameStartFailureReason reason)
    {
        Debug.LogWarning($"[SessionLobbyUI] Game start failed: {errorMessage}");

        // Re-enable start button
        if (popupStartButton != null)
        {
            popupStartButton.SetEnabled(true);
            popupStartButton.text = "Démarrer";
        }

        // Update status
        SetStatus($"Échec: {errorMessage}");

        // Additional feedback based on reason
        switch (reason)
        {
            case GameStartFailureReason.NotAllPlayersReady:
                ToastNotification.Show("Attendez que tous les joueurs soient prêts", ToastNotification.ToastType.Warning, 4f);
                break;
            case GameStartFailureReason.NotEnoughPlayers:
                ToastNotification.Show("Pas assez de joueurs pour cette partie", ToastNotification.ToastType.Warning, 4f);
                break;
            case GameStartFailureReason.NotSessionHost:
                ToastNotification.Show("Seul l'hôte peut démarrer", ToastNotification.ToastType.Error, 4f);
                break;
            default:
                ToastNotification.Show(errorMessage, ToastNotification.ToastType.Error, 4f);
                break;
        }
    }

    private void RequestSessionDetails(string sessionName)
    {
        if (SessionRpcHub.Instance != null && !string.IsNullOrEmpty(sessionName))
        {
            SessionRpcHub.Instance.RequestSessionDetailsServerRpc(sessionName);
        }
    }

    private void OnLeaveSession()
    {
        if (string.IsNullOrEmpty(currentSessionName)) return;

        Debug.Log($"[SessionLobbyUI] Leaving session: {currentSessionName} (inGame: {_stateMachine.IsInState(LobbyState.InGame)})");

        // Visual feedback
        ToastNotification.Show($"Vous avez quitté '{currentSessionName}'", ToastNotification.ToastType.Info);

        // Notify server we're leaving
        if (SessionRpcHub.Instance != null)
        {
            SessionRpcHub.Instance.LeaveSessionServerRpc(currentSessionName);
        }

        // Cleanup debug UI
        if (GameDebugUI.Instance != null)
        {
            GameDebugUI.Instance.Hide();
        }

        // Cleanup input handler session
        if (PlayerInputHandler.Instance != null)
        {
            PlayerInputHandler.Instance.SetSession(null);
        }

        // Cleanup pawn visibility (show all pawns again)
        if (SessionPawnVisibility.Instance != null)
        {
            SessionPawnVisibility.Instance.ClearLocalSession();
        }

        currentSessionName = null;
        isHost = false;
        isReady = false;
        currentSession = null;
        currentSessionDetails = null;
        pendingSessionName = null;

        // Return to lobby state (handles all UI visibility)
        SetUIState(LobbyState.BrowsingSessions);
        SetStatus("Left session");
        OnRefresh();
    }

    private void OnDisconnect()
    {
        Debug.Log("[SessionLobbyUI] Disconnecting...");

        if (NetworkBootstrap.Instance != null)
        {
            NetworkBootstrap.Instance.Disconnect();
        }
        else if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Menu);
    }

    // ============================================
    //   SESSION UPDATES
    // ============================================

    private void OnSessionsUpdated(GameSession[] sessions)
    {
        Debug.Log($"[SessionLobbyUI] Sessions updated: {sessions?.Length ?? 0}");
        UpdateSessionsList(sessions);

        // If waiting for a join/create, request details once the session appears
        if (!string.IsNullOrEmpty(pendingSessionName))
        {
            foreach (var s in sessions)
            {
                if (s.name.ToString() == pendingSessionName)
                {
                    RequestSessionDetails(pendingSessionName);
                    break;
                }
            }
        }

        // Update current session if in popup
        if (!string.IsNullOrEmpty(currentSessionName))
        {
            GameSession? found = null;
            foreach (var s in sessions)
            {
                if (s.name.ToString() == currentSessionName)
                {
                    found = s;
                    break;
                }
            }

            if (found.HasValue)
            {
                currentSession = found;
                RequestSessionDetails(currentSessionName);
            }
            else
            {
                // Session was destroyed
                currentSessionName = null;
                isHost = false;
                currentSession = null;
                currentSessionDetails = null;
                pendingSessionName = null;
                ClosePopup();
                SetStatus("Session ended");
            }
        }

        SetStatus("");
    }

    private void OnSessionDetailsUpdated(SessionDetails details)
    {
        string name = details.session.name.ToString();

        // Handle pending join/create
        if (!string.IsNullOrEmpty(pendingSessionName) && pendingSessionName == name)
        {
            pendingSessionName = null;
            currentSessionName = name;
            SetUIState(LobbyState.InSessionLobby);
        }

        if (!string.IsNullOrEmpty(currentSessionName) && currentSessionName != name)
            return;

        currentSessionName = name;
        currentSession = details.session;
        currentSessionDetails = details;
        UpdatePopupState();
    }

    private void UpdateSessionsList(GameSession[] sessions)
    {
        if (sessionsList == null) return;

        sessionsList.Clear();

        if (sessions == null || sessions.Length == 0)
        {
            var empty = new Label("No sessions available");
            empty.AddToClassList("empty-message");
            sessionsList.Add(empty);
            return;
        }

        foreach (var session in sessions)
        {
            var item = CreateSessionItem(session);
            sessionsList.Add(item);
        }
    }

    private VisualElement CreateSessionItem(GameSession session)
    {
        var item = new VisualElement();
        item.AddToClassList("session-item");

        var name = new Label(session.name.ToString());
        name.AddToClassList("session-name");
        item.Add(name);

        var players = new Label($"{session.playerCount} players");
        players.AddToClassList("session-players");
        item.Add(players);

        var joinBtn = new Button(() => OnJoinSession(session.name.ToString()));
        joinBtn.text = "Join";
        joinBtn.AddToClassList("join-btn");
        item.Add(joinBtn);

        if (session.name.ToString() == currentSessionName)
        {
            joinBtn.SetEnabled(false);
            joinBtn.text = "Joined";
            item.AddToClassList("selected");
        }

        return item;
    }

    /// <summary>
    /// Creates an enhanced player card with icons, badges, and color coding.
    /// </summary>
    private VisualElement CreatePlayerCard(SessionPlayerInfo playerInfo)
    {
        var playerCard = new VisualElement();
        playerCard.AddToClassList("popup-player-item");

        // Background color based on status
        if (playerInfo.ready)
        {
            playerCard.AddToClassList("popup-player-item-ready");
        }

        // Highlight local player
        bool isLocalPlayer = playerInfo.clientId == LocalClientId;
        if (isLocalPlayer)
        {
            playerCard.AddToClassList("popup-player-item-local");
        }

        // Status icon (left side)
        var statusIcon = new Label();
        statusIcon.AddToClassList("player-status-icon");

        if (playerInfo.ready)
        {
            statusIcon.text = "✓";
            statusIcon.AddToClassList("ready");
        }
        else
        {
            statusIcon.text = "○";
        }

        playerCard.Add(statusIcon);

        // Player name
        var nameLabel = new Label(playerInfo.name.ToString());
        nameLabel.AddToClassList("popup-player-name");

        if (isLocalPlayer)
        {
            nameLabel.text += " (Vous)";
            nameLabel.AddToClassList("local");
        }

        playerCard.Add(nameLabel);

        // Badges container (right side)
        var badgesContainer = new VisualElement();
        badgesContainer.AddToClassList("badges-container");

        // Host badge
        if (playerInfo.isCreator)
        {
            var hostBadge = new Label("👑");
            hostBadge.AddToClassList("host-badge");
            badgesContainer.Add(hostBadge);
        }

        playerCard.Add(badgesContainer);

        return playerCard;
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    private void ShowEnteringNameScreen()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        if (lobbyRoot != null) lobbyRoot.style.display = DisplayStyle.None;
        if (nameEntryPanel != null) nameEntryPanel.style.display = DisplayStyle.Flex;
        if (sessionsList?.parent?.parent != null) sessionsList.parent.parent.style.display = DisplayStyle.None;
        if (popupOverlay != null) popupOverlay.style.display = DisplayStyle.None;
        if (connectionStatus != null) connectionStatus.text = "Connexion...";
    }

    /// <summary>
    /// Handle client connection - transition to name entry.
    /// </summary>
    private void HandleClientConnected()
    {
        Debug.Log("[SessionLobbyUI] Client connected - transitioning to EnteringName");
        bool alreadyEntering = _stateMachine.IsInState(LobbyState.EnteringName);
        _stateMachine.TransitionTo(LobbyState.EnteringName);

        // If we were already in this state (e.g., event fired before binding), ensure the UI is visible.
        if (alreadyEntering)
            ShowEnteringNameScreen();
    }

    private void HandleClientDisconnected()
    {
        Debug.Log("[SessionLobbyUI] Client disconnected - returning to Disconnected state");
        currentSessionName = null;
        isHost = false;
        isReady = false;
        currentSession = null;
        currentSessionDetails = null;
        pendingSessionName = null;
        _stateMachine.TransitionTo(LobbyState.Disconnected);
    }

    private void ApplyInitialConnectionState()
    {
        bool connected = pendingConnectedState ||
                         (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient);

        pendingConnectedState = false;

        // Make sure the root is visible when landing in this scene after connecting elsewhere.
        if (uiDocument != null && uiDocument.rootVisualElement != null)
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        if (connected)
        {
            HandleClientConnected();
        }
        else
        {
            _stateMachine.TransitionTo(LobbyState.Disconnected);
        }
    }

    // ============================================
    //   UPDATE
    // ============================================

    private void Update()
    {
        if (connectionStatus != null)
        {
            bool connected = NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
            connectionStatus.text = connected ? "● Connected" : "○ Disconnected";
            connectionStatus.style.color = connected ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.8f, 0.4f, 0.4f);
        }
    }
}
