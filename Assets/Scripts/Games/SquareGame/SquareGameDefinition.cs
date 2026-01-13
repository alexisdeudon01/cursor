using Core.Games;
using Core.Patterns;
using Core.Utilities;
using Core.Maps;
using Networking.StateSync;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Square Game (data-oriented, Option 1).
/// 
/// - Server runs an authoritative simulation in a rectangular arena.
/// - Player pawns are NOT NetworkObjects.
/// - Client renders a square pawn view for each entity.
/// </summary>
[CreateAssetMenu(fileName = "SquareGame", menuName = "Games/Square Game")]
public class SquareGameDefinition : GameDefinitionAsset
{
    [Header("Map Settings")]
    [SerializeField] private Vector2 mapSize = new Vector2(20f, 15f);
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GridMapAsset gridMapAsset;

    // Provide defaults even if asset isn't configured.
    public override string GameId => string.IsNullOrEmpty(gameId) ? "square-game" : gameId;
    public override string DisplayName => string.IsNullOrEmpty(displayName) ? "Square Game" : displayName;

    public override MapConfigData CreateMapConfig(Vector3 worldOffset, int seed)
    {
        int width = gridMapAsset != null ? gridMapAsset.Width : Mathf.Max(1, Mathf.RoundToInt(mapSize.x));
        int height = gridMapAsset != null ? gridMapAsset.Height : Mathf.Max(1, Mathf.RoundToInt(mapSize.y));
        float size = gridMapAsset != null ? gridMapAsset.CellSize : Mathf.Max(0.01f, cellSize);

        return new MapConfigData
        {
            mapName = gridMapAsset != null && !string.IsNullOrEmpty(gridMapAsset.MapId) ? gridMapAsset.MapId : GameId,
            shape = MapShape.Rect,
            mapSize = new Vector3(width * size, 0f, height * size),
            circleRadius = 0f,
            gridWidth = width,
            gridHeight = height,
            cellSize = size,
            seed = seed,
            worldOffset = worldOffset
        };
    }

    public override Vector3 GetSpawnPosition(int playerIndex, int totalPlayers, MapConfigData config)
    {
        int width = config != null && config.gridWidth > 0 ? config.gridWidth : Mathf.Max(1, Mathf.RoundToInt(mapSize.x));
        int height = config != null && config.gridHeight > 0 ? config.gridHeight : Mathf.Max(1, Mathf.RoundToInt(mapSize.y));

        int maxX = width - 1;
        int maxY = height - 1;

        int cellX;
        int cellY;

        if (totalPlayers <= 4)
        {
            switch (playerIndex % 4)
            {
                case 0: cellX = 0; cellY = 0; break;
                case 1: cellX = maxX; cellY = 0; break;
                case 2: cellX = 0; cellY = maxY; break;
                default: cellX = maxX; cellY = maxY; break;
            }
        }
        else
        {
            float safeTotal = Mathf.Max(1, totalPlayers);
            float angle = (playerIndex / safeTotal) * Mathf.PI * 2f;
            float radiusX = Mathf.Max(1f, (width - 1) * 0.4f);
            float radiusY = Mathf.Max(1f, (height - 1) * 0.4f);
            cellX = Mathf.RoundToInt((width - 1) * 0.5f + Mathf.Cos(angle) * radiusX);
            cellY = Mathf.RoundToInt((height - 1) * 0.5f + Mathf.Sin(angle) * radiusY);
        }

        cellX = Mathf.Clamp(cellX, 0, maxX);
        cellY = Mathf.Clamp(cellY, 0, maxY);
        return GridMapUtils.CellToWorld(config, cellX, cellY);
    }

    public override void SetupClientVisuals(MapConfigData config)
    {
        // Map visuals are built by MapConfigSceneBuilder via the replicated MapConfig command.
        // Extra per-mode visuals could be added here.
    }

    public override void CleanupGame()
    {
        // Nothing to clean up for this mode (map & pawn views are managed by their own systems).
    }
}
