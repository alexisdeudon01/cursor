using System;
using UnityEngine;

namespace Core.Maps
{
    public enum GridCellType : byte
    {
        Empty = 0,
        Wall = 1,
        Spawn = 2,
        Goal = 3
    }

    [Serializable]
    public struct GridCellCoord
    {
        public int x;
        public int y;

        public GridCellCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public struct GameElementData
    {
        public string id;
        public GridCellCoord[] cells;
    }

    [Serializable]
    public struct GridCellContent
    {
        public int gameElementIndex;

        public bool HasGameElement => gameElementIndex >= 0;
    }

    [Serializable]
    public struct GridCellData
    {
        public GridCellType type;
        public GridCellContent content;
    }

    [Serializable]
    public struct GridMapConfig
    {
        public string mapId;
        public int width;
        public int height;
        public float cellSize;
        public Vector3 worldOffset;
    }

    [Serializable]
    public sealed class GridMapData
    {
        public const int NoElement = -1;

        public GridMapConfig config;
        public GridCellData[] cells;
        public GameElementData[] gameElements;

        public int CellCount => Mathf.Max(0, config.width) * Mathf.Max(0, config.height);

        public static GridMapData CreateEmpty(string mapId, int width, int height, float cellSize, Vector3 worldOffset)
        {
            var data = new GridMapData
            {
                config = new GridMapConfig
                {
                    mapId = mapId ?? string.Empty,
                    width = Mathf.Max(1, width),
                    height = Mathf.Max(1, height),
                    cellSize = Mathf.Max(0.01f, cellSize),
                    worldOffset = worldOffset
                }
            };

            data.cells = CreateCellArray(data.CellCount);
            data.gameElements = Array.Empty<GameElementData>();
            return data;
        }

        public void ApplyWorldOffset(Vector3 offset)
        {
            config.worldOffset = offset;
        }

        public void EnsureSize(int width, int height, float cellSize)
        {
            config.width = Mathf.Max(1, width);
            config.height = Mathf.Max(1, height);
            config.cellSize = Mathf.Max(0.01f, cellSize);

            var expected = CellCount;
            if (cells == null || cells.Length != expected)
            {
                var newCells = CreateCellArray(expected);
                if (cells != null && cells.Length > 0)
                {
                    Array.Copy(cells, newCells, Mathf.Min(cells.Length, newCells.Length));
                }

                cells = newCells;
            }
        }

        public int Index(int x, int y) => (y * config.width) + x;

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < config.width && y < config.height;
        }

        public GridCellData GetCellData(int x, int y)
        {
            if (!InBounds(x, y) || cells == null)
            {
                return new GridCellData
                {
                    type = GridCellType.Empty,
                    content = new GridCellContent { gameElementIndex = NoElement }
                };
            }

            return cells[Index(x, y)];
        }

        public GridCellType GetCell(int x, int y)
        {
            if (!InBounds(x, y) || cells == null)
            {
                return GridCellType.Empty;
            }

            return cells[Index(x, y)].type;
        }

        public void SetCell(int x, int y, GridCellType value)
        {
            if (!InBounds(x, y) || cells == null)
            {
                return;
            }

            int index = Index(x, y);
            var cell = cells[index];
            cell.type = value;
            cells[index] = cell;
        }

        public int GetCellElementIndex(int x, int y)
        {
            if (!InBounds(x, y) || cells == null)
            {
                return NoElement;
            }

            return cells[Index(x, y)].content.gameElementIndex;
        }

        public void SetCellElementIndex(int x, int y, int elementIndex)
        {
            if (!InBounds(x, y) || cells == null)
            {
                return;
            }

            int index = Index(x, y);
            var cell = cells[index];
            cell.content.gameElementIndex = elementIndex;
            cells[index] = cell;
        }

        public void ClearCellElement(int x, int y)
        {
            SetCellElementIndex(x, y, NoElement);
        }

        public void ClearAllCellElements()
        {
            if (cells == null)
            {
                return;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                cell.content.gameElementIndex = NoElement;
                cells[i] = cell;
            }
        }

        public void EnsureElementDefaults()
        {
            if (gameElements == null)
            {
                gameElements = Array.Empty<GameElementData>();
            }
            if (cells == null)
            {
                return;
            }

            if (gameElements.Length == 0)
            {
                ClearAllCellElements();
                return;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                int index = cell.content.gameElementIndex;
                if (index < 0 || index >= gameElements.Length)
                {
                    cell.content.gameElementIndex = NoElement;
                    cells[i] = cell;
                }
            }
        }

        public void ApplyGameElementsToCells(bool clearExisting = true)
        {
            if (cells == null || gameElements == null || gameElements.Length == 0)
            {
                return;
            }

            if (clearExisting)
            {
                ClearAllCellElements();
            }

            for (int i = 0; i < gameElements.Length; i++)
            {
                var element = gameElements[i];
                if (element.cells == null)
                {
                    continue;
                }

                for (int j = 0; j < element.cells.Length; j++)
                {
                    var coord = element.cells[j];
                    if (!InBounds(coord.x, coord.y))
                    {
                        continue;
                    }

                    SetCellElementIndex(coord.x, coord.y, i);
                }
            }
        }

        public void ApplyGameElementsToCellsIfEmpty()
        {
            if (gameElements == null || gameElements.Length == 0)
            {
                return;
            }

            if (!HasAnyElementAssignment())
            {
                ApplyGameElementsToCells(true);
            }
        }

        private bool HasAnyElementAssignment()
        {
            if (cells == null)
            {
                return false;
            }

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].content.gameElementIndex >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static GridCellData[] CreateCellArray(int cellCount)
        {
            var data = new GridCellData[Mathf.Max(0, cellCount)];
            for (int i = 0; i < data.Length; i++)
            {
                data[i].content.gameElementIndex = NoElement;
            }

            return data;
        }
    }
}
