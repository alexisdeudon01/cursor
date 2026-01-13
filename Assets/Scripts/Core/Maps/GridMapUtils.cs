using UnityEngine;
using Core.StateSync;

namespace Core.Maps
{
    public static class GridMapUtils
    {
        public static Vector2Int FindFirstEmptyCell(GridMapData data)
        {
            if (data == null)
                return Vector2Int.zero;

            int width = Mathf.Max(1, data.config.width);
            int height = Mathf.Max(1, data.config.height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (data.GetCell(x, y) != GridCellType.Wall)
                        return new Vector2Int(x, y);
                }
            }

            return Vector2Int.zero;
        }
        public static void ApplyCircleMask(GridMapData data, float radius)
        {
            if (data == null)
            {
                return;
            }

            float r = Mathf.Max(0f, radius);
            float rSqr = r * r;
            int width = Mathf.Max(1, data.config.width);
            int height = Mathf.Max(1, data.config.height);
            float size = Mathf.Max(0.01f, data.config.cellSize);
            var half = new Vector3(width * size * 0.5f, 0f, height * size * 0.5f);
            var origin = data.config.worldOffset - half;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var world = origin + new Vector3((x + 0.5f) * size, 0f, (y + 0.5f) * size);
                    var delta = world - data.config.worldOffset;
                    if ((delta.x * delta.x) + (delta.z * delta.z) > rSqr)
                    {
                        data.SetCell(x, y, GridCellType.Wall);
                        data.ClearCellElement(x, y);
                    }
                }
            }
        }
        public static Vector3 GetOrigin(MapConfigData config)
        {
            if (config == null)
            {
                return Vector3.zero;
            }

            float width = Mathf.Max(1, config.gridWidth) * Mathf.Max(0.01f, config.cellSize);
            float height = Mathf.Max(1, config.gridHeight) * Mathf.Max(0.01f, config.cellSize);
            var half = new Vector3(width * 0.5f, 0f, height * 0.5f);
            return config.worldOffset - half;
        }

        public static Vector3 CellToWorld(MapConfigData config, int cellX, int cellY)
        {
            if (config == null)
            {
                return Vector3.zero;
            }

            float size = Mathf.Max(0.01f, config.cellSize);
            var origin = GetOrigin(config);
            return origin + new Vector3((cellX + 0.5f) * size, 0f, (cellY + 0.5f) * size);
        }

        public static Vector2Int WorldToCell(MapConfigData config, Vector3 world)
        {
            if (config == null)
            {
                return Vector2Int.zero;
            }

            float size = Mathf.Max(0.01f, config.cellSize);
            var origin = GetOrigin(config);
            int x = Mathf.FloorToInt((world.x - origin.x) / size);
            int y = Mathf.FloorToInt((world.z - origin.z) / size);
            return new Vector2Int(x, y);
        }

        public static bool InBounds(MapConfigData config, int x, int y)
        {
            if (config == null)
            {
                return false;
            }

            return x >= 0 && y >= 0 && x < config.gridWidth && y < config.gridHeight;
        }

        public static Vector2Int DirectionToDelta(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return new Vector2Int(0, 1);
                case GridDirection.Down:
                    return new Vector2Int(0, -1);
                case GridDirection.Left:
                    return new Vector2Int(-1, 0);
                case GridDirection.Right:
                    return new Vector2Int(1, 0);
                default:
                    return Vector2Int.zero;
            }
        }
    }
}
