using Core.StateSync;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Networking.StateSync
{
    public sealed class GameInstanceEntry
    {
        public string gameInstanceUid;
        public string gameTypeUid;
        public string sessionUid;

        /// <summary>
        /// Legacy runtime state (snapshot/diff). Not used by the new server-authoritative sim loop,
        /// but kept for tooling and incremental migration.
        /// </summary>
        public GameRuntimeState runtimeState;
    }

    public sealed class GameInstanceRegister
    {
        private readonly Dictionary<string, GameInstanceEntry> entries = new Dictionary<string, GameInstanceEntry>();
        private readonly Dictionary<string, string> bySessionUid = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, GameInstanceEntry> Entries => entries;

        public GameInstanceEntry Create(string gameTypeUid, string sessionUid, GameRuntimeState runtimeState)
        {
            if (string.IsNullOrEmpty(gameTypeUid) || string.IsNullOrEmpty(sessionUid))
            {
                return null;
            }

            var instanceUid = Guid.NewGuid().ToString("N");
            var entry = new GameInstanceEntry
            {
                gameInstanceUid = instanceUid,
                gameTypeUid = gameTypeUid,
                sessionUid = sessionUid,
                runtimeState = runtimeState
            };

            entries[instanceUid] = entry;
            bySessionUid[sessionUid] = instanceUid;
            return entry;
        }

        public GameInstanceEntry GetByInstanceUid(string gameInstanceUid)
        {
            if (string.IsNullOrEmpty(gameInstanceUid))
            {
                return null;
            }

            return entries.TryGetValue(gameInstanceUid, out var entry) ? entry : null;
        }

        public GameInstanceEntry GetBySessionUid(string sessionUid)
        {
            if (string.IsNullOrEmpty(sessionUid))
            {
                return null;
            }

            return bySessionUid.TryGetValue(sessionUid, out var instanceUid)
                ? GetByInstanceUid(instanceUid)
                : null;
        }

        public bool RemoveBySessionUid(string sessionUid)
        {
            if (string.IsNullOrEmpty(sessionUid))
            {
                return false;
            }

            if (!bySessionUid.TryGetValue(sessionUid, out var instanceUid))
            {
                return false;
            }

            bySessionUid.Remove(sessionUid);
            return entries.Remove(instanceUid);
        }
    }

    [Serializable]
    public sealed class GameEntityState
    {
        public string entityId;
        public string entityType;

        /// <summary>
        /// Stable logical uid (optional).
        /// </summary>
        public string ownerClientUid;

        /// <summary>
        /// Netcode client id.
        /// </summary>
        public ulong ownerClientId;

        public string displayName;
        public int colorIndex;
        public byte prefabType;

        public int cellX;
        public int cellY;

        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public sealed class GameStateSnapshot
    {
        public int version;
        public List<GameEntityState> entities = new List<GameEntityState>();

        public GameStateSnapshot Clone()
        {
            var clone = new GameStateSnapshot { version = version };
            foreach (var entity in entities)
            {
                clone.entities.Add(CloneEntity(entity));
            }
            return clone;
        }

        public GameEntityState FindEntity(string entityId)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].entityId == entityId)
                {
                    return entities[i];
                }
            }

            return null;
        }

        private static GameEntityState CloneEntity(GameEntityState source)
        {
            if (source == null)
            {
                return null;
            }

            return new GameEntityState
            {
                entityId = source.entityId,
                entityType = source.entityType,
                ownerClientUid = source.ownerClientUid,
                ownerClientId = source.ownerClientId,
                displayName = source.displayName,
                colorIndex = source.colorIndex,
                prefabType = source.prefabType,
                cellX = source.cellX,
                cellY = source.cellY,
                position = source.position,
                rotation = source.rotation
            };
        }
    }

    [Serializable]
    public sealed class GameStateDiff
    {
        public int fromVersion;
        public int toVersion;
        public List<GameEntityState> updatedEntities = new List<GameEntityState>();
        public List<string> removedEntityIds = new List<string>();
    }

    public interface IGameStateCommand
    {
        void Apply(GameStateSnapshot snapshot);
    }

    public sealed class MoveEntityCommand : IGameStateCommand
    {
        private readonly string entityId;
        private readonly Vector3 position;

        public MoveEntityCommand(string entityId, Vector3 position)
        {
            this.entityId = entityId;
            this.position = position;
        }

        public void Apply(GameStateSnapshot snapshot)
        {
            var entity = snapshot.FindEntity(entityId);
            if (entity == null)
            {
                return;
            }

            entity.position = position;
        }
    }

    /// <summary>
    /// Legacy snapshot/diff runtime state.
    /// 
    /// The data-oriented server simulation uses a separate SoA world (GameInstanceManager).
    /// This remains as a convenient representation for JSON/debugging.
    /// </summary>
    public sealed class GameRuntimeState
    {
        private GameStateSnapshot baseState;
        private GameStateSnapshot workingState;

        public GameRuntimeState(GameStateSnapshot initialState)
        {
            baseState = initialState ?? new GameStateSnapshot();
            workingState = baseState.Clone();
        }

        public string BuildFullStateJson()
        {
            return JsonUtility.ToJson(baseState);
        }

        public GameStateDiff ApplyCommand(IGameStateCommand command)
        {
            if (command == null)
            {
                return null;
            }

            workingState = baseState.Clone();
            command.Apply(workingState);
            var diff = CreateDiff(baseState, workingState);
            Commit();
            return diff;
        }

        public GameStateDiff ApplyMove(string entityId, Vector3 newPosition)
        {
            return ApplyCommand(new MoveEntityCommand(entityId, newPosition));
        }

        public GameStateSnapshot GetBaseSnapshot()
        {
            return baseState.Clone();
        }

        public List<GameCommandDto> BuildSpawnCommands(string sessionUid)
        {
            var snapshot = GetBaseSnapshot();
            var commands = new List<GameCommandDto>();
            
            foreach (var entity in snapshot.entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.entityId))
                    continue;
                    
                // Convert Networking.StateSync.GameEntityState to Core.StateSync.GameEntityState format
                var coreEntity = new Core.StateSync.GameEntityState
                {
                    id = new Unity.Collections.FixedString64Bytes(entity.entityId ?? ""),
                    type = new Unity.Collections.FixedString64Bytes(entity.entityType ?? ""),
                    ownerClientUid = new Unity.Collections.FixedString64Bytes(entity.ownerClientUid ?? ""),
                    ownerClientId = entity.ownerClientId,
                    displayName = new Unity.Collections.FixedString64Bytes(entity.displayName ?? ""),
                    colorIndex = entity.colorIndex,
                    prefabType = entity.prefabType,
                    mapSize = Vector3.zero, // Not stored in Networking.StateSync version
                    worldOffset = Vector3.zero, // Not stored in Networking.StateSync version
                    cellX = entity.cellX,
                    cellY = entity.cellY
                };
                
                commands.Add(GameCommandFactory.CreateSpawnEntity(sessionUid, coreEntity, snapshot.version));
            }
            
            return commands;
        }

        public List<GameCommandDto> BuildUpdateCommands(string sessionUid, GameStateDiff diff)
        {
            var commands = new List<GameCommandDto>();
            
            foreach (var entity in diff.updatedEntities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.entityId))
                    continue;
                    
                // Convert Networking.StateSync.GameEntityState to Core.StateSync.GameEntityState format
                var coreEntity = new Core.StateSync.GameEntityState
                {
                    id = new Unity.Collections.FixedString64Bytes(entity.entityId ?? ""),
                    type = new Unity.Collections.FixedString64Bytes(entity.entityType ?? ""),
                    ownerClientUid = new Unity.Collections.FixedString64Bytes(entity.ownerClientUid ?? ""),
                    ownerClientId = entity.ownerClientId,
                    displayName = new Unity.Collections.FixedString64Bytes(entity.displayName ?? ""),
                    colorIndex = entity.colorIndex,
                    prefabType = entity.prefabType,
                    mapSize = Vector3.zero, // Not stored in Networking.StateSync version
                    worldOffset = Vector3.zero, // Not stored in Networking.StateSync version
                    cellX = entity.cellX,
                    cellY = entity.cellY
                };
                
                commands.Add(GameCommandFactory.CreateUpdateEntity(sessionUid, coreEntity, diff.toVersion));
            }
            
            foreach (var entityId in diff.removedEntityIds)
            {
                if (string.IsNullOrEmpty(entityId))
                    continue;
                    
                commands.Add(GameCommandFactory.CreateRemoveEntity(sessionUid, entityId));
            }
            
            return commands;
        }

        private void Commit()
        {
            baseState = workingState.Clone();
            baseState.version += 1;
            workingState = baseState.Clone();
        }

        private static GameStateDiff CreateDiff(GameStateSnapshot previous, GameStateSnapshot current)
        {
            var diff = new GameStateDiff
            {
                fromVersion = previous.version,
                toVersion = current.version + 1
            };

            var previousMap = new Dictionary<string, GameEntityState>();
            foreach (var entity in previous.entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.entityId))
                {
                    continue;
                }

                previousMap[entity.entityId] = entity;
            }

            var currentMap = new Dictionary<string, GameEntityState>();
            foreach (var entity in current.entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.entityId))
                {
                    continue;
                }

                currentMap[entity.entityId] = entity;
            }

            foreach (var pair in currentMap)
            {
                if (!previousMap.TryGetValue(pair.Key, out var prev))
                {
                    diff.updatedEntities.Add(CloneEntity(pair.Value));
                    continue;
                }

                if (HasEntityChanged(prev, pair.Value))
                {
                    diff.updatedEntities.Add(CloneEntity(pair.Value));
                }
            }

            foreach (var pair in previousMap)
            {
                if (!currentMap.ContainsKey(pair.Key))
                {
                    diff.removedEntityIds.Add(pair.Key);
                }
            }

            return diff;
        }

        private static bool HasEntityChanged(GameEntityState a, GameEntityState b)
        {
            if (a == null || b == null)
            {
                return true;
            }

            return a.position != b.position
                || a.rotation != b.rotation
                || a.ownerClientUid != b.ownerClientUid
                || a.ownerClientId != b.ownerClientId
                || a.entityType != b.entityType
                || a.displayName != b.displayName
                || a.colorIndex != b.colorIndex
                || a.prefabType != b.prefabType;
        }

        private static GameEntityState CloneEntity(GameEntityState source)
        {
            if (source == null)
            {
                return null;
            }

            return new GameEntityState
            {
                entityId = source.entityId,
                entityType = source.entityType,
                ownerClientUid = source.ownerClientUid,
                ownerClientId = source.ownerClientId,
                displayName = source.displayName,
                colorIndex = source.colorIndex,
                prefabType = source.prefabType,
                position = source.position,
                rotation = source.rotation
            };
        }
    }
}
