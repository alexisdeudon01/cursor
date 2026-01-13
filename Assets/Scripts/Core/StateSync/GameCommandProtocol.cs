using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.StateSync
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
                case GameCommandType.UpdateEntity:
                    serializer.SerializeValue(ref EntityId);
                    serializer.SerializeValue(ref EntityType);
                    serializer.SerializeValue(ref OwnerClientUid);
                    serializer.SerializeValue(ref OwnerClientId);
                    serializer.SerializeValue(ref DisplayName);
                    serializer.SerializeValue(ref ColorIndex);
                    serializer.SerializeValue(ref PrefabType);
                    serializer.SerializeValue(ref MapSize);
                    serializer.SerializeValue(ref WorldOffset);
                    serializer.SerializeValue(ref CellX);
                    serializer.SerializeValue(ref CellY);
                    break;

                case GameCommandType.RemoveEntity:
                    serializer.SerializeValue(ref EntityId);
                    break;

                case GameCommandType.MoveInput:
                    serializer.SerializeValue(ref EntityId);
                    serializer.SerializeValue(ref Direction);
                    serializer.SerializeValue(ref CellX);
                    serializer.SerializeValue(ref CellY);
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
                EntityId = entity.id,
                EntityType = entity.type,
                OwnerClientUid = entity.ownerClientUid,
                OwnerClientId = entity.ownerClientId,
                DisplayName = entity.displayName,
                ColorIndex = entity.colorIndex,
                PrefabType = entity.prefabType,
                MapSize = entity.mapSize,
                WorldOffset = entity.worldOffset,
                CellX = entity.cellX,
                CellY = entity.cellY
            };
        }

        public static GameCommandDto CreateUpdateEntity(string sessionUid, GameEntityState entity, int version)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.UpdateEntity,
                Version = version,
                SessionUid = sessionUid,
                EntityId = entity.id,
                EntityType = entity.type,
                OwnerClientUid = entity.ownerClientUid,
                OwnerClientId = entity.ownerClientId,
                DisplayName = entity.displayName,
                ColorIndex = entity.colorIndex,
                PrefabType = entity.prefabType,
                MapSize = entity.mapSize,
                WorldOffset = entity.worldOffset,
                CellX = entity.cellX,
                CellY = entity.cellY
            };
        }

        public static GameCommandDto CreateRemoveEntity(string sessionUid, string entityId)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.RemoveEntity,
                Version = 0,
                SessionUid = sessionUid,
                EntityId = entityId
            };
        }

        public static GameCommandDto CreateMoveInput(string sessionUid, string entityId, GridDirection direction, int cellX, int cellY)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.MoveInput,
                Version = 0,
                SessionUid = sessionUid,
                EntityId = entityId,
                Direction = direction,
                CellX = cellX,
                CellY = cellY
            };
        }

        public static GameCommandDto CreateResyncRequest(string sessionUid)
        {
            return new GameCommandDto
            {
                Type = GameCommandType.ResyncRequest,
                Version = 0,
                SessionUid = sessionUid
            };
        }

        // Note: These methods are defined here but implemented in Networking.StateSync namespace
        // to avoid circular dependencies. They are called from GameInstanceRegister.
        // The actual implementations convert between Networking.StateSync.GameEntityState 
        // and Core.StateSync.GameEntityState formats.
    }

    /// <summary>
    /// Entity state snapshot used for replication.
    /// </summary>
    public struct GameEntityState
    {
        public FixedString64Bytes id;
        public FixedString64Bytes type;
        public FixedString64Bytes ownerClientUid;
        public ulong ownerClientId;
        public FixedString64Bytes displayName;
        public int colorIndex;
        public byte prefabType;
        public Vector3 mapSize;
        public Vector3 worldOffset;
        public int cellX;
        public int cellY;
    }
}
