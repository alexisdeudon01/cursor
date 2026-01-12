
using Networking.StateSync;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Core.Games;
using Core.Patterns;
using Core.Maps;

/// <summary>
/// Circle Game (data-oriented, Option 1).
/// 
/// - Server runs an authoritative simulation in a circular arena.
/// - Player pawns are NOT NetworkObjects.
/// - Client renders a circle pawn view for each entity.
/// </summary>
[CreateAssetMenu(fileName = "CircleGame", menuName = "Games/Circle Game")]
public class CircleGameDefinition : GameDefinitionAsset
{
    [Header("Arena Settings")]
    [SerializeField, Min(1f)] private float arenaRadius = 12f;
    [SerializeField, Min(0.01f)] private float cellSize = 1f;

    // Provide defaults even if asset isn't configured.
    public override string GameId => string.IsNullOrEmpty(gameId) ? "circle-game" : gameId;
    public override string DisplayName => string.IsNullOrEmpty(displayName) ? "Circle Game" : displayName;

    public override MapConfigData CreateMapConfig(Vector3 worldOffset, int seed)
    {
        float diameter = Mathf.Max(1f, arenaRadius * 2f);
        int gridSize = Mathf.Max(1, Mathf.CeilToInt(diameter / Mathf.Max(0.01f, cellSize)));
        return new MapConfigData
        {
            mapName = GameId,
            shape = MapShape.Circle,
            mapSize = new Vector3(diameter, 0f, diameter),
            circleRadius = arenaRadius,
            gridWidth = gridSize,
            gridHeight = gridSize,
            cellSize = Mathf.Max(0.01f, cellSize),
            seed = seed,
            worldOffset = worldOffset
        };
    }

    public override Vector3 GetSpawnPosition(int playerIndex, int totalPlayers, MapConfigData config)
    {
        var offset = config != null ? config.worldOffset : Vector3.zero;
        float radius = config != null && config.circleRadius > 0f ? config.circleRadius : arenaRadius;

        float safeTotal = Mathf.Max(1, totalPlayers);
        float angle = (playerIndex / safeTotal) * Mathf.PI * 2f;
        float spawnRadius = radius * 0.6f;

        var world = offset + new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0f,
            Mathf.Sin(angle) * spawnRadius
        );
        if (config == null)
        {
            // Fallback: if no map config is provided, use the computed world position directly.
            return world;
        }
        var cell = GridMapUtils.WorldToCell(config, world);
        return GridMapUtils.CellToWorld(config, cell.x, cell.y);
    }

    public override void SetupClientVisuals(MapConfigData config)
    {
        // Map visuals are built by MapConfigSceneBuilder via the replicated MapConfig command.
        // Extra per-mode visuals could be added here.
    }

    public override void CleanupGame()
    {
        // Nothing to clean up for this mode.
    }
}
