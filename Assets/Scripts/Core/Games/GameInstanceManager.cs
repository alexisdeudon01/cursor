using Core.Maps;
using Core.Simulation;
using Core.StateSync;
using Core.Networking;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.Games
{
/// <summary>
/// Manages active game instances.
/// 
/// Data-oriented refactor (Option 1):
/// - Server runs an authoritative simulation (SimWorld).
/// - Player pawns are NOT NetworkObjects.
/// - The server replicates state via GameCommandDto messages.
/// </summary>
public class GameInstanceManager : MonoBehaviour
{
    public static GameInstanceManager Instance { get; private set; }

    /// <summary>
    /// Injected command sender (set by Networking layer to avoid circular dependency).
    /// </summary>
    public static IGameCommandSender CommandSender { get; set; }

    /// <summary>
    /// Injected client registry (set by Networking layer to avoid circular dependency).
    /// </summary>
    public static IClientRegistry ClientRegistry { get; set; }

    /// <summary>
    /// Event fired when a game instance is created.
    /// </summary>
    public static event Action<string, string> GameCreated; // sessionName, gameId

    /// <summary>
    /// Event fired when a game instance is destroyed.
    /// </summary>
    public static event Action<string> GameDestroyed; // sessionName

    [Header("Server Tick Rates")]
    [SerializeField] private float simulationTickRate = 60f;
    [SerializeField] private float replicationTickRate = 20f;

    // Active games
    private readonly Dictionary<string, GameInstance> activeBySessionName = new Dictionary<string, GameInstance>();
    private readonly Dictionary<string, GameInstance> activeBySessionUid = new Dictionary<string, GameInstance>();
    private readonly object activeLock = new object();
    private readonly List<GameInstance> activeInstancesSnapshot = new List<GameInstance>(16);

    // Scratch lists to avoid allocations
    private readonly List<int> scratchEntityIds = new List<int>(64);
    private readonly List<GameCommandDto> scratchCommands = new List<GameCommandDto>(128);

    private float simAccumulator;
    private float repAccumulator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        // Server only simulation & replication
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (activeBySessionName.Count == 0)
        {
            return;
        }

        var dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        simAccumulator += dt;
        repAccumulator += dt;

        var simStep = simulationTickRate > 0.01f ? (1f / simulationTickRate) : (1f / 60f);
        var repStep = replicationTickRate > 0.01f ? (1f / replicationTickRate) : (1f / 20f);

        activeInstancesSnapshot.Clear();
        lock (activeLock)
        {
            foreach (var kv in activeBySessionName)
            {
                if (kv.Value != null)
                {
                    activeInstancesSnapshot.Add(kv.Value);
                }
            }
        }

        int maxStepsPerFrame = 5;
        int steps = 0;
        while (simAccumulator >= simStep && steps < maxStepsPerFrame)
        {
            for (int i = 0; i < activeInstancesSnapshot.Count; i++)
            {
                activeInstancesSnapshot[i].Step(simStep);
            }

            simAccumulator -= simStep;
            steps++;
        }

        while (repAccumulator >= repStep)
        {
            for (int i = 0; i < activeInstancesSnapshot.Count; i++)
            {
                ReplicateDirty(activeInstancesSnapshot[i]);
            }

            repAccumulator -= repStep;
        }
    }

    /// <summary>
    /// [SERVER] Create a new game instance for a session.
    /// </summary>
    public bool CreateGame(
        string sessionName,
        string sessionUid,
        string gameId,
        List<(ulong clientId, string name)> playerData,
        Vector3 worldOffset)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[GameInstanceManager] CreateGame called on client!");
            return false;
        }

        if (string.IsNullOrEmpty(sessionName) || string.IsNullOrEmpty(sessionUid) || string.IsNullOrEmpty(gameId))
        {
            Debug.LogError("[GameInstanceManager] CreateGame missing required arguments");
            return false;
        }

        lock (activeLock)
        {
            if (activeBySessionName.ContainsKey(sessionName) || activeBySessionUid.ContainsKey(sessionUid))
            {
                Debug.LogWarning($"[GameInstanceManager] Session '{sessionName}' already has an active game");
                return false;
            }
        }

        var gameDef = GameRegistry.GetGame(gameId);
        if (gameDef == null)
        {
            Debug.LogError($"[GameInstanceManager] Game '{gameId}' not found in registry");
            return false;
        }

        var seed = ComputeSeed(sessionUid, worldOffset);
        var mapConfig = gameDef.CreateMapConfig(worldOffset, seed);
        if (mapConfig == null)
        {
            Debug.LogError($"[GameInstanceManager] Game '{gameId}' returned null MapConfigData");
            return false;
        }

        var gridData = GridMapRepository.LoadOrCreateFallback(
            mapConfig.mapName,
            mapConfig.gridWidth,
            mapConfig.gridHeight,
            mapConfig.cellSize,
            mapConfig.worldOffset);

        if (mapConfig.shape == MapShape.Circle)
        {
            GridMapUtils.ApplyCircleMask(gridData, mapConfig.circleRadius);
        }

        var instance = new GameInstance(sessionName, sessionUid, gameId, gameDef, mapConfig, gridData);

        // Spawn player entities
        var totalPlayers = playerData?.Count ?? 0;
        for (int i = 0; i < totalPlayers; i++)
        {
            var (clientId, playerName) = playerData[i];

            var clientUid = ClientRegistry?.GetClientUid(clientId) ?? string.Empty;
            var spawnPos = gameDef.GetSpawnPosition(i, totalPlayers, mapConfig);
            var spawnCell = GridMapUtils.WorldToCell(mapConfig, spawnPos);

            if (!GridMapUtils.InBounds(mapConfig, spawnCell.x, spawnCell.y))
            {
                spawnCell = new Vector2Int(mapConfig.gridWidth / 2, mapConfig.gridHeight / 2);
            }
            else if (gridData.GetCell(spawnCell.x, spawnCell.y) == GridCellType.Wall)
            {
                spawnCell = GridMapUtils.FindFirstEmptyCell(gridData);
            }

            instance.AddPlayer(clientId, playerName, clientUid, spawnCell.x, spawnCell.y, i);
        }

        lock (activeLock)
        {
            if (activeBySessionName.ContainsKey(sessionName) || activeBySessionUid.ContainsKey(sessionUid))
            {
                Debug.LogWarning($"[GameInstanceManager] Session '{sessionName}' already has an active game");
                return false;
            }

            activeBySessionName[sessionName] = instance;
            activeBySessionUid[sessionUid] = instance;
        }

        Debug.Log($"[GameInstanceManager] Created game '{gameId}' for session '{sessionName}' with {totalPlayers} players");
        GameCreated?.Invoke(sessionName, gameId);
        return true;
    }

    /// <summary>
    /// [SERVER] Destroy a game instance.
    /// </summary>
    public void DestroyGame(string sessionName)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        GameInstance instance;
        lock (activeLock)
        {
            if (!activeBySessionName.TryGetValue(sessionName, out instance))
            {
                return;
            }

            activeBySessionName.Remove(sessionName);
            if (!string.IsNullOrEmpty(instance.SessionUid))
            {
                activeBySessionUid.Remove(instance.SessionUid);
            }
        }

        instance.Cleanup();

        Debug.Log($"[GameInstanceManager] Destroyed game for session '{sessionName}'");
        GameDestroyed?.Invoke(sessionName);
    }

    public bool HasActiveGame(string sessionName)
    {
        return activeBySessionName.ContainsKey(sessionName);
    }

    public bool TryGetInstance(string sessionName, out GameInstance instance)
    {
        return activeBySessionName.TryGetValue(sessionName, out instance);
    }

    public bool TryGetInstanceBySessionUid(string sessionUid, out GameInstance instance)
    {
        return activeBySessionUid.TryGetValue(sessionUid, out instance);
    }

    public string GetGameId(string sessionName)
    {
        return activeBySessionName.TryGetValue(sessionName, out var inst) ? inst.GameId : null;
    }

    public List<ulong> GetPlayerIds(string sessionName)
    {
        return activeBySessionName.TryGetValue(sessionName, out var inst)
            ? new List<ulong>(inst.PlayerIds)
            : new List<ulong>();
    }

    public bool TryGetWorldOffset(string sessionName, out Vector3 worldOffset)
    {
        if (activeBySessionName.TryGetValue(sessionName, out var inst) && inst.MapConfig != null)
        {
            worldOffset = inst.MapConfig.worldOffset;
            return true;
        }

        worldOffset = Vector3.zero;
        return false;
    }

    /// <summary>
    /// [SERVER] Set movement input for a player in a given session.
    /// </summary>
    public void SetPlayerInput(string sessionName, ulong clientId, GridDirection direction)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (!activeBySessionName.TryGetValue(sessionName, out var inst))
        {
            return;
        }

        inst.World.SetInput(clientId, direction);
    }

    /// <summary>
    /// [SERVER] Late joiner support.
    /// Spawns a new entity for the player and sends the current snapshot to the joining client.
    /// </summary>
    public bool AddPlayerToGame(string sessionName, ulong clientId, string playerName)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            return false;
        }

        if (!activeBySessionName.TryGetValue(sessionName, out var inst))
        {
            return false;
        }

        if (inst.PlayerIds.Contains(clientId))
        {
            Debug.LogWarning($"[GameInstanceManager] Player {clientId} already in game for session '{sessionName}'");
            return false;
        }

        var clientUid = ClientRegistry?.GetClientUid(clientId) ?? string.Empty;

        var totalPlayersAfterJoin = inst.PlayerIds.Count + 1;
        var joinIndex = inst.PlayerIds.Count;
        var spawnPos = inst.GameDefinition.GetSpawnPosition(joinIndex, totalPlayersAfterJoin, inst.MapConfig);
        var spawnCell = GridMapUtils.WorldToCell(inst.MapConfig, spawnPos);

        if (!GridMapUtils.InBounds(inst.MapConfig, spawnCell.x, spawnCell.y))
        {
            spawnCell = new Vector2Int(inst.MapConfig.gridWidth / 2, inst.MapConfig.gridHeight / 2);
        }
        else if (inst.GridMap != null && inst.GridMap.GetCell(spawnCell.x, spawnCell.y) == GridCellType.Wall)
        {
            spawnCell = GridMapUtils.FindFirstEmptyCell(inst.GridMap);
        }

        // Add to world
        inst.BumpVersion();
        var entityId = inst.AddPlayer(clientId, playerName, clientUid, spawnCell.x, spawnCell.y, joinIndex);

        // Notify existing players about the new entity
        var existingTargets = new List<ulong>(inst.PlayerIds);
        existingTargets.Remove(clientId);

        if (existingTargets.Count > 0 && CommandSender != null)
        {
            var rpcParams = BuildClientRpcParams(existingTargets);
            var spawnCmd = inst.BuildSpawnCommand(entityId, inst.Version);
            CommandSender.SendGameCommandClientRpc(spawnCmd, rpcParams);
        }

        // Send full snapshot to late joiner
        SendFullSnapshotToClient(sessionName, clientId);

        return true;
    }

    /// <summary>
    /// [SERVER] Remove a player entity from an active game.
    /// </summary>
    public bool RemovePlayerFromGame(string sessionName, ulong clientId)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
        {
            return false;
        }

        if (!activeBySessionName.TryGetValue(sessionName, out var inst))
        {
            return false;
        }

        if (!inst.PlayerIds.Contains(clientId))
        {
            return false;
        }

        if (!inst.World.TryGetEntityIdForOwner(clientId, out var entityId))
        {
            inst.PlayerIds.Remove(clientId);
            return true;
        }

        inst.BumpVersion();

        // Remove from sim world first
        inst.World.RemoveEntity(entityId);
        inst.PlayerIds.Remove(clientId);

        // Replicate removal to remaining players
        if (inst.PlayerIds.Count > 0)
        {
            var rpcParams = BuildClientRpcParams(inst.PlayerIds);
            var removeCmd = GameCommandFactory.CreateRemoveEntity(inst.SessionUid, ToFixedString(entityId).ToString());

            scratchCommands.Clear();
            scratchCommands.Add(removeCmd);
            CommandSender?.SendGameCommandBatchClientRpc(scratchCommands.ToArray(), rpcParams);
        }

        return true;
    }

    /// <summary>
    /// [SERVER] Send full game snapshot (map + spawns) to a specific client.
    /// This does not change the authoritative state/version.
    /// </summary>
public void SendFullSnapshotToClient(string sessionName, ulong targetClientId, bool includeMapConfig = true)
{
    if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
    {
        return;
    }

    if (!activeBySessionName.TryGetValue(sessionName, out var inst))
    {
        return;
    }

    var targets = new List<ulong> { targetClientId };
    SendFullSnapshotInternal(inst, targets, includeMapConfig);
}

/// <summary>
/// [SERVER] Send full game snapshot (map + spawns) to multiple clients.
/// Uses a batched client RPC to reduce RPC overhead.
/// </summary>
public void SendFullSnapshotToClients(string sessionName, IReadOnlyList<ulong> targetClientIds, bool includeMapConfig = true)
{
    if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
    {
        return;
    }

    if (!activeBySessionName.TryGetValue(sessionName, out var inst))
    {
        return;
    }

    if (targetClientIds == null || targetClientIds.Count == 0)
    {
        return;
    }

    var targets = new List<ulong>(targetClientIds);
    SendFullSnapshotInternal(inst, targets, includeMapConfig);
}

private void SendFullSnapshotInternal(GameInstance inst, List<ulong> targets, bool includeMapConfig)
{
    if (inst == null || inst.MapConfig == null || CommandSender == null)
    {
        return;
    }

    var rpcParams = BuildClientRpcParams(targets);

    scratchCommands.Clear();

    // Map config first (optional)
    if (includeMapConfig)
    {
        scratchCommands.Add(GameCommandFactory.CreateMapConfig(inst.SessionUid, inst.MapConfig));
    }

    // Then spawn all entities (snapshot)
    inst.World.CollectAllEntityIds(scratchEntityIds);
    for (int i = 0; i < scratchEntityIds.Count; i++)
    {
        var entityId = scratchEntityIds[i];
        scratchCommands.Add(inst.BuildSpawnCommand(entityId, inst.Version));
    }

    if (scratchCommands.Count > 0)
    {
        CommandSender.SendGameCommandBatchClientRpc(scratchCommands.ToArray(), rpcParams);
    }
}


private void ReplicateDirty(GameInstance inst)
{
    if (inst == null || inst.MapConfig == null)
    {
        return;
    }

    if (!inst.World.IsDirty)
    {
        return;
    }

    if (CommandSender == null)
    {
        return;
    }

    inst.Version++;

    inst.World.CollectDirtyEntityIds(scratchEntityIds);
    if (scratchEntityIds.Count == 0)
    {
        inst.World.ClearDirty();
        return;
    }

    var rpcParams = BuildClientRpcParams(inst.PlayerIds);

    scratchCommands.Clear();
    for (int i = 0; i < scratchEntityIds.Count; i++)
    {
        var entityId = scratchEntityIds[i];

        inst.World.GetEntityDataById(entityId,
            out var prefabType,
            out var ownerClientId,
            out var ownerClientUid,
            out var displayName,
            out var colorIndex,
            out var cellX,
            out var cellY);

        var cmd = GameCommandFactory.CreateUpdateEntity(inst.SessionUid, new GameEntityState 
        { 
            id = ToFixedString(entityId),
            cellX = cellX,
            cellY = cellY
        }, inst.Version);

        scratchCommands.Add(cmd);
    }

    CommandSender.SendGameCommandBatchClientRpc(scratchCommands.ToArray(), rpcParams);

    inst.World.ClearDirty();
}

    private static ClientRpcParams BuildClientRpcParams(IReadOnlyList<ulong> targets)
    {
        var sendParams = new ClientRpcSendParams
        {
            TargetClientIds = targets
        };

        return new ClientRpcParams
        {
            Send = sendParams
        };
    }

    public static FixedString64Bytes ToFixedString(int value)
    {
        var fs = new FixedString64Bytes();
        fs.Append(value);
        return fs;
    }

    private static int ComputeSeed(string sessionUid, Vector3 worldOffset)
    {
        unchecked
        {
            // FNV-1a 32-bit hash
            uint hash = 2166136261;

            if (!string.IsNullOrEmpty(sessionUid))
            {
                for (int i = 0; i < sessionUid.Length; i++)
                {
                    hash ^= sessionUid[i];
                    hash *= 16777619;
                }
            }

            hash ^= (uint)Mathf.RoundToInt(worldOffset.x * 10f);
            hash *= 16777619;
            hash ^= (uint)Mathf.RoundToInt(worldOffset.z * 10f);
            hash *= 16777619;

            return (int)hash;
        }
    }
}

/// <summary>
/// Server-side game instance state.
/// </summary>
public sealed class GameInstance
{
    public string SessionName { get; }
    public string SessionUid { get; }
    public string GameId { get; }
    public IGameDefinition GameDefinition { get; }
    public MapConfigData MapConfig { get; }
    public GridMapData GridMap { get; }

    public SimWorld World { get; } = new SimWorld();

    public List<ulong> PlayerIds { get; } = new List<ulong>(16);

    public int Version { get; internal set; } = 1;

    public GameInstance(string sessionName, string sessionUid, string gameId, IGameDefinition gameDefinition, MapConfigData mapConfig, GridMapData gridMap)
    {
        SessionName = sessionName;
        SessionUid = sessionUid;
        GameId = gameId;
        GameDefinition = gameDefinition;
        MapConfig = mapConfig;
        GridMap = gridMap;
    }

    public void Step(float dt)
    {
        World.Step(dt, GameDefinition.MoveSpeed, GridMap);
    }

    public int AddPlayer(ulong clientId, string playerName, string clientUid, int cellX, int cellY, int colorIndex)
    {
        PlayerIds.Add(clientId);

        var ownerUid = new FixedString64Bytes(clientUid ?? string.Empty);
        var displayName = new FixedString64Bytes(string.IsNullOrEmpty(playerName) ? $"Player {clientId}" : playerName);
        var prefabType = (byte)GameDefinition.PawnPrefabType;

        var entityId = World.SpawnPlayerEntity(
            ownerClientId: clientId,
            ownerClientUid: ownerUid,
            displayName: displayName,
            colorIndex: colorIndex,
            prefabType: prefabType,
            cellX: cellX,
            cellY: cellY);

        return entityId;
    }

    public void BumpVersion()
    {
        Version++;
    }

    public GameCommandDto BuildSpawnCommand(int entityId, int version)
    {
        World.GetEntityDataById(entityId,
            out var prefabType,
            out var ownerClientId,
            out var ownerClientUid,
            out var displayName,
            out var colorIndex,
            out var cellX,
            out var cellY);

        return new GameCommandDto
        {
            Type = GameCommandType.SpawnEntity,
            Version = version,
            SessionUid = SessionUid,
            EntityId = GameInstanceManager.ToFixedString(entityId),
            EntityType = "player",
            PrefabType = prefabType,
            OwnerClientId = ownerClientId,
            OwnerClientUid = ownerClientUid,
            DisplayName = displayName,
            ColorIndex = colorIndex,
            CellX = cellX,
            CellY = cellY
        };
    }

    public void Cleanup()
    {
        GameDefinition?.CleanupGame();
        PlayerIds.Clear();
        World.ClearDirty();
    }
}
}