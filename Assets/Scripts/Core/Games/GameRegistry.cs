using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry for all available game types.
/// Games register themselves here to be available for sessions.
/// 
/// <para><b>Usage:</b></para>
/// <list type="bullet">
///   <item>Call <see cref="Register"/> to add a game</item>
///   <item>Call <see cref="GetGame"/> to retrieve by ID</item>
///   <item>Use <see cref="AllGames"/> to list available games</item>
/// </list>
/// </summary>
public static class GameRegistry
{
    private static readonly Dictionary<string, IGameDefinition> games = new Dictionary<string, IGameDefinition>();
    private static bool initialized = false;

    /// <summary>
    /// Event fired when a new game is registered.
    /// </summary>
    public static event Action<IGameDefinition> GameRegistered;

    /// <summary>
    /// All registered games.
    /// </summary>
    public static IReadOnlyCollection<IGameDefinition> AllGames => games.Values;

    /// <summary>
    /// Number of registered games.
    /// </summary>
    public static int Count => games.Count;

    /// <summary>
    /// Register a game definition.
    /// </summary>
    /// <param name="game">The game to register</param>
    /// <returns>True if registered, false if ID already exists</returns>
    public static bool Register(IGameDefinition game)
    {
        if (game == null)
        {
            Debug.LogError("[GameRegistry] Cannot register null game");
            return false;
        }

        if (string.IsNullOrEmpty(game.GameId))
        {
            Debug.LogError("[GameRegistry] Game has null or empty GameId");
            return false;
        }

        if (games.ContainsKey(game.GameId))
        {
            Debug.LogWarning($"[GameRegistry] Game '{game.GameId}' already registered");
            return false;
        }

        games[game.GameId] = game;
        Debug.Log($"[GameRegistry] Registered game: {game.DisplayName} ({game.GameId})");
        GameRegistered?.Invoke(game);
        return true;
    }

    /// <summary>
    /// Unregister a game by ID.
    /// </summary>
    public static bool Unregister(string gameId)
    {
        if (games.Remove(gameId))
        {
            Debug.Log($"[GameRegistry] Unregistered game: {gameId}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get a game definition by ID.
    /// </summary>
    /// <param name="gameId">The game ID to look up</param>
    /// <returns>The game definition, or null if not found</returns>
    public static IGameDefinition GetGame(string gameId)
    {
        if (string.IsNullOrEmpty(gameId))
            return null;

        games.TryGetValue(gameId, out var game);
        return game;
    }

    /// <summary>
    /// Check if a game is registered.
    /// </summary>
    public static bool HasGame(string gameId)
    {
        return !string.IsNullOrEmpty(gameId) && games.ContainsKey(gameId);
    }

    /// <summary>
    /// Get all game IDs.
    /// </summary>
    public static IEnumerable<string> GetGameIds()
    {
        return games.Keys;
    }

    /// <summary>
    /// Clear all registered games.
    /// </summary>
    public static void Clear()
    {
        games.Clear();
        initialized = false;
        Debug.Log("[GameRegistry] Cleared all games");
    }

    /// <summary>
    /// Initialize registry by loading all GameDefinitionAsset from Resources.
    /// Call this once at startup.
    /// </summary>
    public static void Initialize()
    {
        if (initialized)
            return;

        // Load all GameDefinitionAsset from Resources/Games folder
        var assets = Resources.LoadAll<GameDefinitionAsset>("Games");
        foreach (var asset in assets)
        {
            Register(asset);
        }

        initialized = true;
        Debug.Log($"[GameRegistry] Initialized with {games.Count} games");
    }
}
