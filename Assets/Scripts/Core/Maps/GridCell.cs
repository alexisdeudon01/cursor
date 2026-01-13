using UnityEngine;

namespace Core.Maps
{
    public sealed class GridCell : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public GridCellType Type { get; private set; }
        public int GameElementIndex { get; private set; }

        public void Initialize(int x, int y, GridCellType type)
        {
            Initialize(x, y, new GridCellData
            {
                type = type,
                content = new GridCellContent { gameElementIndex = GridMapData.NoElement }
            });
        }

        public void Initialize(int x, int y, GridCellData data)
        {
            X = x;
            Y = y;
            Type = data.type;
            GameElementIndex = data.content.gameElementIndex;
        }
    }
}
