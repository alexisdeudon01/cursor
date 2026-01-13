using System;
using UnityEngine;

namespace Core.Maps
{
    public static class GridMapRepository
    {
        [Serializable]
        private sealed class LegacyGridMapData
        {
            public GridMapConfig config;
            public GridCellType[] cells;

            public GridMapData ToData()
            {
                var data = GridMapData.CreateEmpty(config.mapId, config.width, config.height, config.cellSize, config.worldOffset);
                if (cells != null && cells.Length == data.CellCount)
                {
                    for (int i = 0; i < cells.Length; i++)
                    {
                        data.cells[i].type = cells[i];
                    }
                }

                return data;
            }
        }

        public static GridMapData LoadFromResources(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                return null;
            }

            var text = Resources.Load<TextAsset>($"Maps/{mapId}");
            if (text == null || string.IsNullOrWhiteSpace(text.text))
            {
                return null;
            }

            var data = JsonUtility.FromJson<GridMapData>(text.text);
            if (data == null || data.cells == null || data.cells.Length == 0)
            {
                var legacy = JsonUtility.FromJson<LegacyGridMapData>(text.text);
                if (legacy != null && legacy.cells != null && legacy.cells.Length > 0)
                {
                    data = legacy.ToData();
                }
            }

            if (data == null)
            {
                return null;
            }

            data.EnsureSize(data.config.width, data.config.height, data.config.cellSize);
            data.EnsureElementDefaults();
            data.ApplyGameElementsToCellsIfEmpty();
            return data;
        }

        public static GridMapData LoadOrCreateFallback(string mapId, int width, int height, float cellSize, Vector3 worldOffset)
        {
            var data = LoadFromResources(mapId);
            if (data == null)
            {
                data = GridMapData.CreateEmpty(mapId, width, height, cellSize, worldOffset);
            }
            else
            {
                data.EnsureSize(width, height, cellSize);
                data.ApplyWorldOffset(worldOffset);
                if (string.IsNullOrEmpty(data.config.mapId))
                {
                    data.config.mapId = mapId ?? string.Empty;
                }
            }

            data.EnsureElementDefaults();
            data.ApplyGameElementsToCellsIfEmpty();
            return data;
        }
    }
}
