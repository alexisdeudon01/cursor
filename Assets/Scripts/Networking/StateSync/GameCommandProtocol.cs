using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Networking.StateSync
{
    public enum GameCommandType
    {
        Unknown = 0,
        MapConfig = 1,
        SpawnEntity = 2,
        UpdateEntity = 3,
        RemoveEntity = 4,
        MoveInput = 5,
        ResyncRequest = 6
    }

    public enum GridDirection : byte
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    /// <summary>
    /// Single command DTO used to replicate state between server and clients.
    /// 
    /// Data-oriented note:
    /// - Pawns are NOT NetworkObjects.
    /// - Server replicates only data (entity id, owner id, transform, etc.).
    /// </summary>
    public struct GameCommandDto : INetworkSerializable
    {
        public GameCommandType Type;
        public int Version;
        public FixedString64Bytes SessionUid;

        // -------- Entity fields --------
        public FixedString64Bytes EntityId;
        public FixedString64Bytes EntityType;

        /// <summary>
        /// Stable logical uid (optional).
        /// </summary>
        public FixedString64Bytes OwnerClientUid;

        /// <summary>
        /// Netcode client id (required for identifying the local pawn on the client).
        /// </summary>
        public ulong OwnerClientId;

        public FixedString64Bytes DisplayName;
        public int ColorIndex;
        public byte PrefabType;

        // -------- Map fields --------
        public FixedString64Bytes MapName;
        public Vector3 MapSize;
        public Vector3 WorldOffset;
        public byte MapShape;
        public float CircleRadius;
        public int GridWidth;
        public int GridHeight;
        public float CellSize;
        public int Seed;

        // -------- Input fields --------
        public GridDirection Direction;

        // -------- Grid fields --------
        public int CellX;
        public int CellY;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Always: discriminator + versioning + routing
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Version);
            serializer.SerializeValue(ref SessionUid);
        
            // Conditional payload serialization to reduce bandwidth/CPU.
            switch (Type)
            {
                case GameCommandType.MapConfig:
                    serializer.SerializeValue(ref MapName);
                    serializer.SerializeValue(ref MapSize);
                    serializer.SerializeValue(ref WorldOffset);
                    serializer.SerializeValue(ref MapShape);
                    serializer.SerializeValue(ref CircleRadius);
                    serializer.SerializeValue(ref GridWidth);
                    serializer.SerializeValue(ref GridHeight);
                    serializer.SerializeValue(ref CellSize);
                    serializer.SerializeValue(ref Seed);
                    break;
        
                case GameCommandType.SpawnEntity:
                    serializer.SerializeValue(ref EntityId);
                    serializer.SerializeValue(ref OwnerClientId);
                    serializer.SerializeValue(ref OwnerClientUid);
                    serializer.SerializeValue(ref DisplayName);
                    serializer.SerializeValue(ref ColorIndex);
                    serializer.SerializeValue(ref PrefabType);
                    serializer.SerializeValue(ref CellX);
                    serializer.SerializeValue(ref CellY);
                    break;
        
                case GameCommandType.UpdateEntity:
                    serializer.SerializeValue(ref EntityId);
                    serializer.SerializeValue(ref CellX);
                    serializer.SerializeValue(ref CellY);
                    break;
        
                case GameCommandType.RemoveEntity:
                    serializer.SerializeValue(ref EntityId);
                    break;
        
                case GameCommandType.MoveInput:
                    serializer.SerializeValue(ref Direction);
                    break;
        
                case GameCommandType.ResyncRequest:
                default:
                    // No extra fields
                    break;
            }
        }
    }

    public static class GameCommandFactory
    {
        public static GameCommandDto CreateMapConfig(string sessionUid, MapConfigData config)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.MapConfig,
                Version = 0,
                SessionUid = sessionUid,
                MapName = config != null ? config.mapName : string.Empty,
                MapSize = config != null ? config.mapSize : Vector3.zero,
                WorldOffset = config != null ? config.worldOffset : Vector3.zero,
                MapShape = config != null ? (byte)config.shape : (byte)MapShape.Rect,
                CircleRadius = config != null ? config.circleRadius : 0f,
                GridWidth = config != null ? config.gridWidth : 0,
                GridHeight = config != null ? config.gridHeight : 0,
                CellSize = config != null ? config.cellSize : 1f,
                Seed = config != null ? config.seed : 0
            };
        }

        public static GameCommandDto CreateSpawnEntity(string sessionUid, GameEntityState entity, int version)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.SpawnEntity,
                Version = version,
                SessionUid = sessionUid,
                EntityId = entity != null ? entity.entityId : string.Empty,
                EntityType = entity != null ? entity.entityType : string.Empty,
                OwnerClientUid = entity != null ? entity.ownerClientUid : string.Empty,
                OwnerClientId = entity != null ? entity.ownerClientId : 0,
                DisplayName = entity != null ? entity.displayName : string.Empty,
                ColorIndex = entity != null ? entity.colorIndex : 0,
                PrefabType = entity != null ? entity.prefabType : (byte)PawnPrefabType.Square,
                CellX = entity != null ? entity.cellX : 0,
                CellY = entity != null ? entity.cellY : 0
            };
        }

        public static GameCommandDto CreateUpdateEntity(string sessionUid, GameEntityState entity, int version)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.UpdateEntity,
                Version = version,
                SessionUid = sessionUid,
                EntityId = entity != null ? entity.entityId : string.Empty,
                EntityType = entity != null ? entity.entityType : string.Empty,
                OwnerClientUid = entity != null ? entity.ownerClientUid : string.Empty,
                OwnerClientId = entity != null ? entity.ownerClientId : 0,
                DisplayName = entity != null ? entity.displayName : string.Empty,
                ColorIndex = entity != null ? entity.colorIndex : 0,
                PrefabType = entity != null ? entity.prefabType : (byte)PawnPrefabType.Square,
                CellX = entity != null ? entity.cellX : 0,
                CellY = entity != null ? entity.cellY : 0
            };
        }

        public static GameCommandDto CreateRemoveEntity(string sessionUid, string entityId, int version)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.RemoveEntity,
                Version = version,
                SessionUid = sessionUid,
                EntityId = entityId ?? string.Empty
            };
        }

        public static GameCommandDto CreateMoveInput(string sessionUid, GridDirection direction, int clientVersion)
        {
            var uid = (FixedString64Bytes)(sessionUid ?? string.Empty);
            return CreateMoveInput(uid, direction, clientVersion);
        }

public static GameCommandDto CreateResyncRequest(string sessionUid, int clientVersion)
{
    var uid = (FixedString64Bytes)(sessionUid ?? string.Empty);
    return CreateResyncRequest(uid, clientVersion);
}

        public static List<GameCommandDto> BuildSpawnCommands(string sessionUid, GameStateSnapshot snapshot)
        {
            var commands = new List<GameCommandDto>();
            if (snapshot == null)
            {
                return commands;
            }

            foreach (var entity in snapshot.entities)
            {
                commands.Add(CreateSpawnEntity(sessionUid, entity, snapshot.version));
            }

            return commands;
        }

        public static List<GameCommandDto> BuildUpdateCommands(string sessionUid, GameStateDiff diff)
        {
            var commands = new List<GameCommandDto>();
            if (diff == null)
            {
                return commands;
            }

            foreach (var entity in diff.updatedEntities)
            {
                commands.Add(CreateUpdateEntity(sessionUid, entity, diff.toVersion));
            }

            foreach (var removedId in diff.removedEntityIds)
            {
                commands.Add(CreateRemoveEntity(sessionUid, removedId, diff.toVersion));
            }

            return commands;
        }
        public static GameCommandDto CreateResyncRequest(FixedString64Bytes sessionUid, int clientVersion)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.ResyncRequest,
                Version = clientVersion,
                SessionUid = sessionUid
            };
        }

        public static GameCommandDto CreateMoveInput(FixedString64Bytes sessionUid, GridDirection direction, int clientVersion)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.MoveInput,
                Version = clientVersion,
                SessionUid = sessionUid,
                Direction = direction
            };
        }

    }
}
