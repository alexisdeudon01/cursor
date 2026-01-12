using System;
using UnityEngine;

namespace Core.Maps
{
    [CreateAssetMenu(fileName = "GridMap", menuName = "Maps/Grid Map")]
    public sealed class GridMapAsset : ScriptableObject
    {
        [Header("Config")]
        [SerializeField] private string mapId = "square-game";
        [SerializeField, Min(1)] private int width = 8;
        [SerializeField, Min(1)] private int height = 6;
        [SerializeField, Min(0.01f)] private float cellSize = 1f;

        [Header("Cells (row-major)")]
        [SerializeField] private GridCellType[] cells;

        [Header("Static game elements")]
        [SerializeField] private GameElementData[] gameElements;

        public string MapId => mapId;
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public GridCellType[] Cells => cells;
        public GameElementData[] GameElements => gameElements;

        public GridMapData ToData(Vector3 worldOffset)
        {
            var data = GridMapData.CreateEmpty(mapId, width, height, cellSize, worldOffset);
            if (cells != null && cells.Length == data.CellCount)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    data.cells[i].type = cells[i];
                }
            }

            if (gameElements != null && gameElements.Length > 0)
            {
                data.gameElements = (GameElementData[])gameElements.Clone();
                data.ApplyGameElementsToCells(true);
            }
            else
            {
                data.gameElements = Array.Empty<GameElementData>();
            }

            return data;
        }

        public string ToJson(bool prettyPrint = true)
        {
            var data = ToData(Vector3.zero);
            return JsonUtility.ToJson(data, prettyPrint);
        }

        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            cellSize = Mathf.Max(0.01f, cellSize);

            var expected = width * height;
            if (cells == null || cells.Length != expected)
            {
                cells = new GridCellType[expected];
            }
        }
    }
}
