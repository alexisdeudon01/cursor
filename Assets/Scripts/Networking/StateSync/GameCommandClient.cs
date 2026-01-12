using Core.StateSync;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.StateSync
{
    /// <summary>
    /// Client-side receiver for replicated game commands.
    ///
    /// Data-oriented refactor (Option 1):
    /// - Entities are NOT NetworkObjects.
    /// - Spawn/Update/Remove are applied to local view objects via EntityViewWorld.
    /// - Commands are buffered until the Game scene is active.
    /// </summary>
    public class GameCommandClient : MonoBehaviour
    {
        public static GameCommandClient Instance { get; private set; }

        public static event Action<MapConfigData> OnMapConfigApplied;

        public MapConfigData CurrentConfig { get; private set; }
        public FixedString64Bytes CurrentSessionUid { get; private set; }
        public int LastAppliedVersion { get; private set; }

        // Buffer commands until the Game scene is active (prevents spawning into menu scenes).
        private readonly Queue<GameCommandDto> bufferedCommands = new Queue<GameCommandDto>();
        private bool readyForVisuals;

        public static GameCommandClient EnsureInstance()
        {
            if (Instance != null) return Instance;

            var go = new GameObject("GameCommandClient");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameCommandClient>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            EvaluateReadyForVisuals();

            // Ensure the view world exists on the client.
            EntityViewWorld.EnsureInstance();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            }
        }

        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            EvaluateReadyForVisuals();
            FlushBufferedCommandsIfReady();
        }

        private void EvaluateReadyForVisuals()
        {
            // The project uses a "Game" scene for gameplay.
            var active = SceneManager.GetActiveScene();
            readyForVisuals = active.IsValid() && active.name == "Game";
        }

        private void FlushBufferedCommandsIfReady()
        {
            if (!readyForVisuals)
                return;

            while (bufferedCommands.Count > 0)
            {
                var cmd = bufferedCommands.Dequeue();
                ApplyCommandInternal(cmd);
            }
        }

        public void ApplyCommand(GameCommandDto command)
        {
            if (!readyForVisuals)
            {
                bufferedCommands.Enqueue(command);
                return;
            }

            ApplyCommandInternal(command);
        }

        private void ApplyCommandInternal(GameCommandDto command)
        {
            // Capture session uid as soon as we see it (even before MapConfig arrives).
            if (CurrentSessionUid.IsEmpty && !command.SessionUid.IsEmpty)
            {
                CurrentSessionUid = command.SessionUid;
            }

            switch (command.Type)
            {
                case GameCommandType.MapConfig:
                    ApplyMapConfig(command);
                    break;
                case GameCommandType.SpawnEntity:
                    ApplySpawn(command);
                    break;
                case GameCommandType.UpdateEntity:
                    ApplyUpdate(command);
                    break;
                case GameCommandType.RemoveEntity:
                    ApplyRemove(command);
                    break;
            }
        }

        private void ApplyMapConfig(GameCommandDto command)
        {
            CurrentSessionUid = command.SessionUid;
            CurrentConfig = new MapConfigData
            {
                mapName = command.MapName.ToString(),
                shape = (MapShape)command.MapShape,
                mapSize = command.MapSize,
                circleRadius = command.CircleRadius,
                gridWidth = command.GridWidth,
                gridHeight = command.GridHeight,
                cellSize = command.CellSize,
                seed = command.Seed,
                worldOffset = command.WorldOffset
            };

            if (CurrentConfig.gridWidth <= 0 || CurrentConfig.gridHeight <= 0)
            {
                float size = Mathf.Max(0.01f, CurrentConfig.cellSize);
                CurrentConfig.gridWidth = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(CurrentConfig.mapSize.x) / size));
                CurrentConfig.gridHeight = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(CurrentConfig.mapSize.z) / size));
            }

            if (CurrentConfig.mapSize.sqrMagnitude <= 0.0001f)
            {
                CurrentConfig.mapSize = new Vector3(
                    CurrentConfig.gridWidth * CurrentConfig.cellSize,
                    0f,
                    CurrentConfig.gridHeight * CurrentConfig.cellSize);
            }

            // Reset versioning for a new snapshot.
            LastAppliedVersion = 0;

            // Clear old entities (new map == new world).
            EntityViewWorld.Instance?.ClearAll();

            OnMapConfigApplied?.Invoke(CurrentConfig);
        }

        private void ApplySpawn(GameCommandDto command)
        {
            // SpawnEntity commands are treated as snapshot chunks.
            if (!ShouldApplySnapshot(command.Version))
                return;

            EntityViewWorld.EnsureInstance().ApplySpawn(command);
        }

        private void ApplyUpdate(GameCommandDto command)
        {
            if (!ShouldApplyUpdate(command.Version))
                return;

            EntityViewWorld.Instance?.ApplyUpdate(command);
        }

        private void ApplyRemove(GameCommandDto command)
        {
            if (!ShouldApplyUpdate(command.Version))
                return;

            EntityViewWorld.Instance?.ApplyRemove(command);
        }

        private bool ShouldApplySnapshot(int version)
        {
            if (version <= 0)
                return false;

            // Allow snapshot chunks with the same version.
            if (LastAppliedVersion > version)
                return false;

            LastAppliedVersion = version;
            return true;
        }

        private bool ShouldApplyUpdate(int version)
        {
            if (version <= 0)
                return false;

            // First non-snapshot update requires that we have applied a snapshot.
            if (LastAppliedVersion == 0)
            {
                RequestResync("Update before snapshot");
                return false;
            }

            // If we missed a version, request resync.
            if (version > LastAppliedVersion + 1)
            {
                RequestResync($"Missing versions: have {LastAppliedVersion}, received {version}");
                return false;
            }

            if (version <= LastAppliedVersion)
                return false;

            LastAppliedVersion = version;
            return true;
        }

/// <summary>
/// Public helper to request a full snapshot from the server (useful on scene load / late join).
/// </summary>
public void RequestResyncNow(string sessionNameFallback = null, string reason = null)
{
    if (NetworkManager.Singleton == null)
        return;

    FixedString64Bytes uid = CurrentSessionUid;
    if (uid.IsEmpty && !string.IsNullOrEmpty(sessionNameFallback))
    {
        uid = new FixedString64Bytes(sessionNameFallback);
    }

    if (uid.IsEmpty)
        return;

    Debug.LogWarning($"[GameCommandClient] Requesting resync: {reason ?? "manual"}");

    var cmd = GameCommandFactory.CreateResyncRequest(uid.ToString());
    SessionRpcHub.Instance?.SendGameCommandServerRpc(cmd);
}



        private void RequestResync(string reason)
        {
            RequestResyncNow(null, reason);
        }

        public bool TryGetLocalPawnTransform(out Transform transform)
        {
            transform = null;
            if (EntityViewWorld.Instance == null)
                return false;

            return EntityViewWorld.Instance.TryGetLocalPawnTransform(out transform);
        }
    }
}
