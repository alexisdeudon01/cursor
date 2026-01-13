using UnityEngine;

/// <summary>
/// Minimal context required by player commands.
/// Keeps commands decoupled from concrete game container implementations.
/// </summary>
public interface IPlayerCommandContext
{
    /// <summary>
    /// Returns the player's pawn GameObject for the given client id (or null if missing).
    /// </summary>
    GameObject GetPlayerPawnObject(ulong clientId);
}

/// <summary>
/// Implemented by pawns that can be moved on the server.
/// Commands use this to avoid depending on concrete pawn types.
/// </summary>
public interface IServerMovablePawn
{
    void Move(Vector2 direction);
}
