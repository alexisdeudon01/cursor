using Core.Maps;
using Networking.StateSync;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Core.Simulation
{
    /// <summary>
    /// Simple SoA-style world used by the server-authoritative simulation.
    /// 
    /// Entities are dense (swap-remove) and indexed by an int id.
    /// </summary>
    public sealed class SimWorld
    {
        private int nextEntityId = 1;

        // Dense entity storage (SoA)
        private readonly List<int> entityIds = new List<int>(16);
        private readonly List<byte> prefabTypes = new List<byte>(16);
        private readonly List<ulong> ownerClientIds = new List<ulong>(16);
        private readonly List<FixedString64Bytes> ownerClientUids = new List<FixedString64Bytes>(16);
        private readonly List<FixedString64Bytes> displayNames = new List<FixedString64Bytes>(16);
        private readonly List<int> colorIndices = new List<int>(16);
        private readonly List<int> cellXs = new List<int>(16);
        private readonly List<int> cellYs = new List<int>(16);
        private readonly List<Quaternion> rotations = new List<Quaternion>(16);
        private readonly List<GridDirection> inputs = new List<GridDirection>(16);
        private readonly List<float> moveProgress = new List<float>(16);

        // Lookups
        private readonly Dictionary<int, int> idToIndex = new Dictionary<int, int>(16);
        private readonly Dictionary<ulong, int> ownerToEntityId = new Dictionary<ulong, int>(16);

        // Dirty tracking (entity id)
        private readonly HashSet<int> dirty = new HashSet<int>();

        public int Count => entityIds.Count;

        public bool IsDirty => dirty.Count > 0;

        public void ClearDirty()
        {
            dirty.Clear();
        }

        public bool TryGetEntityIdForOwner(ulong ownerClientId, out int entityId)
        {
            return ownerToEntityId.TryGetValue(ownerClientId, out entityId);
        }

        public int SpawnPlayerEntity(
            ulong ownerClientId,
            FixedString64Bytes ownerClientUid,
            FixedString64Bytes displayName,
            int colorIndex,
            byte prefabType,
            int cellX,
            int cellY)
        {
            var entityId = nextEntityId++;
            var index = entityIds.Count;

            entityIds.Add(entityId);
            prefabTypes.Add(prefabType);
            ownerClientIds.Add(ownerClientId);
            ownerClientUids.Add(ownerClientUid);
            displayNames.Add(displayName);
            colorIndices.Add(colorIndex);
            cellXs.Add(cellX);
            cellYs.Add(cellY);
            rotations.Add(Quaternion.identity);
            inputs.Add(GridDirection.None);
            moveProgress.Add(0f);

            idToIndex[entityId] = index;
            ownerToEntityId[ownerClientId] = entityId;

            return entityId;
        }

        public bool RemoveEntity(int entityId)
        {
            if (!idToIndex.TryGetValue(entityId, out var index))
            {
                return false;
            }

            // Remove owner mapping
            var owner = ownerClientIds[index];
            if (ownerToEntityId.TryGetValue(owner, out var mappedId) && mappedId == entityId)
            {
                ownerToEntityId.Remove(owner);
            }

            var lastIndex = entityIds.Count - 1;
            if (index != lastIndex)
            {
                // Swap last -> index
                entityIds[index] = entityIds[lastIndex];
                prefabTypes[index] = prefabTypes[lastIndex];
                ownerClientIds[index] = ownerClientIds[lastIndex];
                ownerClientUids[index] = ownerClientUids[lastIndex];
                displayNames[index] = displayNames[lastIndex];
                colorIndices[index] = colorIndices[lastIndex];
                cellXs[index] = cellXs[lastIndex];
                cellYs[index] = cellYs[lastIndex];
                rotations[index] = rotations[lastIndex];
                inputs[index] = inputs[lastIndex];
                moveProgress[index] = moveProgress[lastIndex];

                // Fix lookup for swapped entity
                var swappedEntityId = entityIds[index];
                idToIndex[swappedEntityId] = index;

                // Fix owner mapping for swapped entity
                var swappedOwner = ownerClientIds[index];
                ownerToEntityId[swappedOwner] = swappedEntityId;
            }

            // Remove last
            entityIds.RemoveAt(lastIndex);
            prefabTypes.RemoveAt(lastIndex);
            ownerClientIds.RemoveAt(lastIndex);
            ownerClientUids.RemoveAt(lastIndex);
            displayNames.RemoveAt(lastIndex);
            colorIndices.RemoveAt(lastIndex);
            cellXs.RemoveAt(lastIndex);
            cellYs.RemoveAt(lastIndex);
            rotations.RemoveAt(lastIndex);
            inputs.RemoveAt(lastIndex);
            moveProgress.RemoveAt(lastIndex);

            idToIndex.Remove(entityId);
            dirty.Remove(entityId);

            return true;
        }

        public void SetInput(ulong ownerClientId, GridDirection direction)
        {
            if (!ownerToEntityId.TryGetValue(ownerClientId, out var entityId))
            {
                return;
            }

            if (!idToIndex.TryGetValue(entityId, out var index))
            {
                return;
            }

            inputs[index] = direction;
        }

        public void Step(float dt, float moveSpeed, GridMapData mapData)
        {
            if (mapData == null)
            {
                return;
            }

            float speed = Mathf.Max(0f, moveSpeed);

            for (int i = 0; i < cellXs.Count; i++)
            {
                var dir = inputs[i];
                if (dir == GridDirection.None)
                {
                    continue;
                }

                moveProgress[i] += speed * dt;
                while (moveProgress[i] >= 1f)
                {
                    moveProgress[i] -= 1f;

                    var delta = GridMapUtils.DirectionToDelta(dir);
                    int nextX = cellXs[i] + delta.x;
                    int nextY = cellYs[i] + delta.y;

                    if (!mapData.InBounds(nextX, nextY))
                    {
                        break;
                    }

                    if (mapData.GetCell(nextX, nextY) == GridCellType.Wall)
                    {
                        break;
                    }

                    if (nextX == cellXs[i] && nextY == cellYs[i])
                    {
                        break;
                    }

                    cellXs[i] = nextX;
                    cellYs[i] = nextY;
                    dirty.Add(entityIds[i]);
                }
            }
        }

        public void GetEntityDataById(int entityId,
            out byte prefabType,
            out ulong ownerClientId,
            out FixedString64Bytes ownerClientUid,
            out FixedString64Bytes displayName,
            out int colorIndex,
            out int cellX,
            out int cellY)
        {
            if (!idToIndex.TryGetValue(entityId, out var index))
            {
                prefabType = 0;
                ownerClientId = 0;
                ownerClientUid = default;
                displayName = default;
                colorIndex = 0;
                cellX = 0;
                cellY = 0;
                return;
            }

            prefabType = prefabTypes[index];
            ownerClientId = ownerClientIds[index];
            ownerClientUid = ownerClientUids[index];
            displayName = displayNames[index];
            colorIndex = colorIndices[index];
            cellX = cellXs[index];
            cellY = cellYs[index];
        }

        public void CollectDirtyEntityIds(List<int> results)
        {
            results.Clear();
            foreach (var entityId in dirty)
            {
                results.Add(entityId);
            }
        }

        public void CollectAllEntityIds(List<int> results)
        {
            results.Clear();
            for (int i = 0; i < entityIds.Count; i++)
            {
                results.Add(entityIds[i]);
            }
        }

        public static Vector3 ClampToMap(Vector3 position, MapConfigData mapConfig, float margin)
        {
            if (mapConfig == null)
            {
                return position;
            }

            var offset = mapConfig.worldOffset;

            if ((MapShape)mapConfig.shape == MapShape.Circle)
            {
                var radius = Mathf.Max(0f, mapConfig.circleRadius - margin);
                var dx = position.x - offset.x;
                var dz = position.z - offset.z;
                var dist = Mathf.Sqrt((dx * dx) + (dz * dz));

                if (dist > radius && dist > 0.0001f)
                {
                    var scale = radius / dist;
                    position.x = offset.x + (dx * scale);
                    position.z = offset.z + (dz * scale);
                }

                position.y = offset.y;
                return position;
            }

            // Rect (default)
            var size = mapConfig.mapSize;
            var halfW = Mathf.Max(0f, Mathf.Abs(size.x) * 0.5f - margin);
            var halfH = Mathf.Max(0f, Mathf.Abs(size.z) * 0.5f - margin);

            position.x = Mathf.Clamp(position.x, offset.x - halfW, offset.x + halfW);
            position.z = Mathf.Clamp(position.z, offset.z - halfH, offset.z + halfH);
            position.y = offset.y;
            return position;
        }
    }
}
