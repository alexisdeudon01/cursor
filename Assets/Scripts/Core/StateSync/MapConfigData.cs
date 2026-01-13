using System;
using UnityEngine;

namespace Core.StateSync
{
    /// <summary>
    /// Minimal map configuration replicated from server to clients.
    /// 
    /// The client uses this to build the arena visuals (floor, borders) in a deterministic way.
    /// </summary>
    public enum MapShape : byte
    {
        Rect = 0,
        Circle = 1
    }

    [Serializable]
    public sealed class MapConfigData
    {
        public string mapName;

        /// <summary>
        /// Arena shape.
        /// </summary>
        public MapShape shape = MapShape.Rect;

        /// <summary>
        /// For Rect: size.x = width, size.z = height.
        /// For Circle: often contains diameter in x/z for convenience.
        /// </summary>
        public Vector3 mapSize;

        /// <summary>
        /// Circle radius (only used when shape == Circle).
        /// </summary>
        public float circleRadius;

        /// <summary>
        /// Grid width in cells (for grid-based movement).
        /// </summary>
        public int gridWidth;

        /// <summary>
        /// Grid height in cells (for grid-based movement).
        /// </summary>
        public int gridHeight;

        /// <summary>
        /// Size of one cell in world units.
        /// </summary>
        public float cellSize = 1f;

        /// <summary>
        /// Deterministic seed (optional).
        /// </summary>
        public int seed;

        /// <summary>
        /// World-space offset for session isolation.
        /// </summary>
        public Vector3 worldOffset;
    }
}
