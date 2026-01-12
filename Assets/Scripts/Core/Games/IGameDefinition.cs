using Networking.StateSync;
using UnityEngine;

/// <summary>
/// Interface for pluggable game types.
/// 
/// Data-oriented refactor (Option 1): player pawns are NOT NetworkObjects.
/// The server runs an authoritative simulation and replicates entity state via DTO commands.
/// </summary>
public interface IGameDefinition
{
    /// <summary>
    /// Unique identifier for this game type (e.g., "square-game", "circle-game").
    /// </summary>
    string GameId { get; }

    /// <summary>
    /// Human-readable name shown in UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of the game for players.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Minimum players required to start.
    /// </summary>
    int MinPlayers { get; }

    /// <summary>
    /// Maximum players allowed.
    /// </summary>
    int MaxPlayers { get; }

    /// <summary>
    /// Default visual type for the pawn view in this game.
    /// (Client-only: used by the view factory.)
    /// </summary>
    PawnPrefabType PawnPrefabType { get; }

    /// <summary>
    /// Server-authoritative movement speed in units per second.
    /// </summary>
    float MoveSpeed { get; }

    /// <summary>
    /// [SERVER] Build the map/bounds configuration for this game instance.
    /// </summary>
    MapConfigData CreateMapConfig(Vector3 worldOffset, int seed);

    /// <summary>
    /// [SERVER] Get spawn position for a player.
    /// </summary>
    Vector3 GetSpawnPosition(int playerIndex, int totalPlayers, MapConfigData config);

    /// <summary>
    /// [CLIENT] Optional: add extra visuals specific to this mode after the map is configured.
    /// The base implementation can be a no-op.
    /// </summary>
    void SetupClientVisuals(MapConfigData config);

    /// <summary>
    /// Called when the game ends (server and/or client) to clean up game-specific state.
    /// </summary>
    void CleanupGame();
}

/// <summary>
/// Pawn view types understood by the client view factory.
/// </summary>
public enum PawnPrefabType : byte
{
    Square = 1,
    Circle = 2
}

/// <summary>
/// Base ScriptableObject for game definitions.
/// Create assets to register games in the editor.
/// </summary>
public abstract class GameDefinitionAsset : ScriptableObject, IGameDefinition
{
    [Header("Game Info")]
    [SerializeField] protected string gameId;
    [SerializeField] protected string displayName;
    [SerializeField, TextArea] protected string description;

    [Header("Player Settings")]
    [SerializeField] protected int minPlayers = 1;
    [SerializeField] protected int maxPlayers = 10;

    [Header("Simulation")]
    [SerializeField, Min(0.1f)] protected float moveSpeed = 5f;

    [Header("Presentation")]
    [SerializeField] protected PawnPrefabType pawnPrefabType = PawnPrefabType.Square;

    public virtual string GameId => gameId;
    public virtual string DisplayName => displayName;
    public virtual string Description => description;
    public virtual int MinPlayers => minPlayers;
    public virtual int MaxPlayers => maxPlayers;

    public virtual PawnPrefabType PawnPrefabType => pawnPrefabType;
    public virtual float MoveSpeed => moveSpeed;

    public abstract MapConfigData CreateMapConfig(Vector3 worldOffset, int seed);
    public abstract Vector3 GetSpawnPosition(int playerIndex, int totalPlayers, MapConfigData config);

    public virtual void SetupClientVisuals(MapConfigData config)
    {
        // Default: no extra visuals.
    }

    public abstract void CleanupGame();
}
