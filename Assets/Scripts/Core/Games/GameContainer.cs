using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Container for a single game instance within a session.
/// Encapsulates game-specific logic: map, camera, player pawns, and game state.
/// Loaded in dedicated Game scene (not Client scene).
/// </summary>
public class GameContainer : IPlayerCommandContext
{
    public string SessionName { get; private set; }
    public string GameId { get; private set; }
    public Vector3 WorldOffset { get; private set; }
    public Scene GameScene { get; private set; }

    // Game-specific components
    private Camera gameCamera;
    private Transform mapRoot;
    private readonly Dictionary<ulong, NetworkObject> playerPawns = new Dictionary<ulong, NetworkObject>();
    private readonly Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    // Command pattern for player actions
    private readonly CommandInvoker commandInvoker = new CommandInvoker();

    public GameContainer(string sessionName, string gameId, Vector3 worldOffset)
    {
        SessionName = sessionName;
        GameId = gameId;
        WorldOffset = worldOffset;

        Debug.Log($"[GameContainer] Created for session '{sessionName}', game '{gameId}', offset {worldOffset}");
    }

    /// <summary>
    /// Initialize game scene components after scene load.
    /// </summary>
    public void InitializeGameScene(Scene scene, Camera camera, Transform mapRoot)
    {
        GameScene = scene;
        this.gameCamera = camera;
        this.mapRoot = mapRoot;

        Debug.Log($"[GameContainer:{SessionName}] Game scene initialized: {scene.name}");
    }

    /// <summary>
    /// Register a player pawn in this game instance.
    /// </summary>
    public void RegisterPawn(ulong clientId, NetworkObject pawn, string playerName = "")
    {
        if (playerPawns.ContainsKey(clientId))
        {
            Debug.LogWarning($"[GameContainer:{SessionName}] Pawn already registered for client {clientId}");
            return;
        }

        playerPawns[clientId] = pawn;
        if (!string.IsNullOrEmpty(playerName))
            playerNames[clientId] = playerName;

        Debug.Log($"[GameContainer:{SessionName}] Registered pawn for client {clientId}");
    }

    /// <summary>
    /// Unregister a player pawn.
    /// </summary>
    public void UnregisterPawn(ulong clientId)
    {
        if (playerPawns.Remove(clientId))
        {
            playerNames.Remove(clientId);
            Debug.Log($"[GameContainer:{SessionName}] Unregistered pawn for client {clientId}");
        }
    }

    /// <summary>
    /// Execute a command for a specific player (Command Pattern).
    /// </summary>
    public void ExecutePlayerCommand(IPlayerCommand command)
    {
        commandInvoker.ExecuteCommand(command);
    }

    /// <summary>
    /// Get player's pawn NetworkObject.
    /// </summary>
    public NetworkObject GetPlayerPawn(ulong clientId)
    {
        return playerPawns.TryGetValue(clientId, out var pawn) ? pawn : null;
    }

    /// <inheritdoc />
    public GameObject GetPlayerPawnObject(ulong clientId)
    {
        var pawn = GetPlayerPawn(clientId);
        return pawn != null ? pawn.gameObject : null;
    }

    /// <summary>
    /// Get player's name.
    /// </summary>
    public string GetPlayerName(ulong clientId)
    {
        return playerNames.TryGetValue(clientId, out var name) ? name : null;
    }

    /// <summary>
    /// Focus camera on specific player pawn.
    /// </summary>
    public void FocusCameraOnPlayer(ulong clientId)
    {
        if (gameCamera == null)
        {
            Debug.LogWarning($"[GameContainer:{SessionName}] No game camera available");
            return;
        }

        var pawn = GetPlayerPawn(clientId);
        if (pawn != null)
        {
            gameCamera.transform.position = new Vector3(
                pawn.transform.position.x,
                pawn.transform.position.y,
                gameCamera.transform.position.z
            );

            Debug.Log($"[GameContainer:{SessionName}] Camera focused on player {clientId}");
        }
    }

    /// <summary>
    /// Update game logic (call from Update loop).
    /// </summary>
    public void Update()
    {
        // Process command queue
        commandInvoker.ProcessQueue();
    }

    /// <summary>
    /// Cleanup game resources.
    /// </summary>
    public void Cleanup()
    {
        Debug.Log($"[GameContainer:{SessionName}] Cleaning up game container");

        // Destroy all pawns
        foreach (var pawn in playerPawns.Values)
        {
            if (pawn != null && pawn.gameObject != null)
            {
                Object.Destroy(pawn.gameObject);
            }
        }

        playerPawns.Clear();
        playerNames.Clear();

        gameCamera = null;
        mapRoot = null;
    }
}
