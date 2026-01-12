using Networking.RpcHandlers;
using Networking.RpcHandlers.Handlers;
using Networking.StateSync;
using Core.StateSync;
using Core.Networking;
using Core.Games;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Hub RPC global pour gérer les sessions, indépendant du player prefab.
// Met ce script sur un NetworkPrefab dédié (ex: TestRPC), spawné côté serveur une fois au démarrage.
// REFACTORED: Now delegates to specialized handlers (SessionLifecycleHandler, GameStartHandler, etc.)
public class SessionRpcHub : NetworkBehaviour, IGameCommandSender
{
    public static SessionRpcHub Instance { get; private set; }

    public static event Action<GameSession[]> SessionsUpdated;
    public static event Action<SessionDetails> SessionDetailsUpdated;

    public static event Action<string, List<ulong>, NetworkObject> GameStart;
    /// <summary>
    /// Fired on clients when a server-side game start request fails.
    /// UI and other client-side systems can listen without introducing a hard dependency
    /// from networking code to UI code.
    /// </summary>
    public static event Action<string, GameStartFailureReason> GameStartFailed;

    // Specialized handlers for different RPC categories
    private SessionLifecycleHandler lifecycleHandler;
    private GameStartHandler gameStartHandler;
    private PlayerMovementHandler movementHandler;
    private SceneLoadHandler sceneLoadHandler;
    private SessionQueryHandler queryHandler;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("SessionRpcHub Awake: NetworkManager.Singleton is null");
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Core interfaces to avoid circular dependency
        GameInstanceManager.CommandSender = this;

        // Initialize handlers
        InitializeHandlers();
    }

    private void InitializeHandlers()
    {
        lifecycleHandler = new SessionLifecycleHandler();
        gameStartHandler = new GameStartHandler();
        movementHandler = new PlayerMovementHandler();
        sceneLoadHandler = new SceneLoadHandler();
        queryHandler = new SessionQueryHandler();

        // Initialize all handlers with this hub reference
        lifecycleHandler.Initialize(this);
        gameStartHandler.Initialize(this);
        movementHandler.Initialize(this);
        sceneLoadHandler.Initialize(this);
        queryHandler.Initialize(this);

        Debug.Log("[SessionRpcHub] All handlers initialized");
    }




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("SessionRpcHub OnNetworkSpawn Server");
            // Force GameRegistry initialization on server startup
            GameRegistry.Initialize();
        }
        if (!IsServer)
        {
            Debug.Log("SessionRpcHub OnNetworkSpawn Client");
        }

        // BUG FIX: Previous condition "Instance == null && Instance != this" was always false
        // When Instance == null, the second part "Instance != this" is comparing null != this,
        // which is true, but the whole expression fails because we need Instance to be assigned.
        // Correct logic: Only set Instance if it's null (Awake may not have run on network spawn)
        if (Instance == null)
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private new void OnDestroy()
    {
        Debug.Log("SessionRpcHub OnDestroy");

        // Cleanup all handlers
        lifecycleHandler?.Cleanup();
        gameStartHandler?.Cleanup();
        movementHandler?.Cleanup();
        sceneLoadHandler?.Cleanup();
        queryHandler?.Cleanup();

        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Invoke GameStart event from anywhere while keeping invocation inside this declaring type.
    /// </summary>
    public static void InvokeGameStart(string sessionName, List<ulong> players, NetworkObject prefab)
    {
        GameStart?.Invoke(sessionName, players, prefab);
    }
    //Logique
    //Coté client, on a les non RPC, on les appeles qui ont appelerons les RPC
    // ============================================================
    //  SESSIONS
    // ============================================================

    [ServerRpc(RequireOwnership = false)]
    public void CreateSessionServerRpc(string sessionName, string playerName, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        lifecycleHandler.HandleCreateSession(sessionName, playerName, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void JoinSessionServerRpc(string sessionName, string playerName, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        lifecycleHandler.HandleJoinSession(sessionName, playerName, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LeaveSessionServerRpc(string sessionName, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        lifecycleHandler.HandleLeaveSession(sessionName, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(string sessionName, bool ready, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        lifecycleHandler.HandleSetReady(sessionName, ready, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSessionDetailsServerRpc(string sessionName, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        queryHandler.HandleRequestSessionDetails(sessionName, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc(string sessionName, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var clientId = serverRpcParams.Receive.SenderClientId;
        gameStartHandler.HandleStartGame(sessionName, clientId);
    }

    /// <summary>
    /// [ServerRpc] Set the game type for a session.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetGameTypeServerRpc(string sessionName, string gameId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        gameStartHandler.HandleSetGameType(sessionName, gameId, sender);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSessionsServerRpc()
    {
        if (!IsServer)
            return;

        queryHandler.HandleRequestSessions();
    }

    // ============================================================
    //  CLIENT RPCS (PUBLIC for handlers to call)
    // ============================================================
    [ClientRpc]
    private void SyncSessionsClientRpc(GameSession[] sessions)
    {
        SessionsUpdated?.Invoke(sessions ?? Array.Empty<GameSession>());
    }

    [ClientRpc]
    public void SendSessionDetailsClientRpc(SessionDetails details, ClientRpcParams clientRpcParams = default)
    {
        SessionDetailsUpdated?.Invoke(details);
    }

    [ClientRpc]
    public void SendSessionErrorClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        Debug.LogWarning($"[SessionRpcHub] Session error: {message}");
        // Toast notification removed - use events instead to avoid UI dependency
        // ToastNotification.Show(message, ToastNotification.ToastType.Error);
    }

    [ClientRpc]
    public void SendGameCommandClientRpc(GameCommandDto command, ClientRpcParams clientRpcParams = default)
    {
        GameCommandClient.EnsureInstance().ApplyCommand(command);
    }

/// <summary>
/// Batched variant: reduces RPC overhead by sending many commands at once.
/// </summary>
[ClientRpc]
public void SendGameCommandBatchClientRpc(GameCommandDto[] commands, ClientRpcParams clientRpcParams = default)
{
    if (commands == null || commands.Length == 0)
        return;

    var client = GameCommandClient.EnsureInstance();
    for (int i = 0; i < commands.Length; i++)
    {
        client.ApplyCommand(commands[i]);
    }
}


    [ServerRpc(RequireOwnership = false)]
    public void SendGameCommandServerRpc(GameCommandDto command, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer)
            return;

        var sender = serverRpcParams.Receive.SenderClientId;
        movementHandler.HandleCommand(command, sender);
    }

    /// <summary>
    /// [ClientRpc] Notify client that game start failed with detailed reason.
    /// </summary>
    [ClientRpc]
    public void SendGameStartFailedClientRpc(string errorMessage, GameStartFailureReason reason, ClientRpcParams clientRpcParams = default)
    {
        Debug.LogWarning($"[SessionRpcHub] Game start failed: {errorMessage} (Reason: {reason})");

        // Toast notification removed - use events instead to avoid UI dependency
        // ToastNotification.Show(errorMessage, ToastNotification.ToastType.Warning, 5f);

        // Notify listeners (UI, debug tools, etc.) without referencing UI classes directly.
        GameStartFailed?.Invoke(errorMessage, reason);
    }

    [ClientRpc]
    public void StartGameClientRpc(string sessionName, string gameId, Vector3 worldOffset, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[SessionRpcHub] StartGameClientRpc received: game={gameId}, session={sessionName}, IsServer={IsServer}");

        // Delegate to scene load handler
        StartCoroutine(sceneLoadHandler.LoadGameSceneAndInitialize(sessionName, gameId, worldOffset));

        // Fire event for other systems
        InvokeGameStart(sessionName, new List<ulong>(), null);
        Debug.Log($"[SessionRpcHub] GameStart event fired for session '{sessionName}'");
    }

    /// <summary>
    /// Notify a late joiner to setup their game visuals.
    /// </summary>
    [ClientRpc]
    public void NotifyLateJoinerClientRpc(string sessionName, string gameId, Vector3 worldOffset, ulong targetClientId)
    {
        sceneLoadHandler.HandleLateJoiner(sessionName, gameId, worldOffset, targetClientId);
    }

    // ============================================================
    //  STATIC DISPATCH
    // ============================================================
    public static bool TryBroadcastSessions(GameSession[] sessions)
    {
        if (Instance == null || !Instance.IsSpawned)
        {
            Debug.LogWarning("[SessionRpcHub] Aucun hub actif pour diffuser les sessions");
            return false;
        }

        Instance.SyncSessionsClientRpc(sessions ?? Array.Empty<GameSession>());
        return true;
    }

    // NOTE: legacy pawn sync RPCs removed (pawns are not NetworkObjects in Option 1).
}
