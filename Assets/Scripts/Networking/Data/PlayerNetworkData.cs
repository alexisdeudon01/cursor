using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Complete player data container for network client.
/// Consolidates all player information in one place.
/// </summary>
public class PlayerNetworkData
{
    // ============================================================
    // IDENTIFICATION
    // ============================================================

    /// <summary>Network client ID (unique per connection)</summary>
    public ulong ClientId { get; set; }

    /// <summary>Player display name</summary>
    public string PlayerName { get; set; }

    /// <summary>Current session ID (null if not in session)</summary>
    public string CurrentSessionId { get; set; }

    // ============================================================
    // NETWORK OBJECTS
    // ============================================================

    /// <summary>DefaultPlayer NetworkObject (persistent per connection)</summary>
    public NetworkObject PlayerObject { get; set; }

    /// <summary>Current game pawn NetworkObject (spawned in game)</summary>
    public NetworkObject CurrentPawn { get; set; }

    // ============================================================
    // SESSION STATE
    // ============================================================

    /// <summary>Is the player ready to start game?</summary>
    public bool IsReady { get; set; }

    /// <summary>Is the player currently in an active game?</summary>
    public bool IsInGame { get; set; }

    /// <summary>Is the player session host?</summary>
    public bool IsSessionHost { get; set; }

    // ============================================================
    // GAME STATE (in-game only)
    // ============================================================

    /// <summary>Current position in world space</summary>
    public Vector3 Position { get; set; }

    /// <summary>Current score (game-specific)</summary>
    public int Score { get; set; }

    /// <summary>Is the player alive? (for games with death)</summary>
    public bool IsAlive { get; set; }

    /// <summary>Player team/faction (game-specific)</summary>
    public int Team { get; set; }

    // ============================================================
    // STATISTICS
    // ============================================================

    /// <summary>When the client connected</summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>Last activity timestamp</summary>
    public DateTime LastActivity { get; set; }

    /// <summary>Total messages received from this client</summary>
    public int MessagesReceived { get; set; }

    /// <summary>Total messages sent to this client</summary>
    public int MessagesSent { get; set; }

    /// <summary>Total game sessions played</summary>
    public int GamesPlayed { get; set; }

    /// <summary>Total wins across all games</summary>
    public int TotalWins { get; set; }

    // ============================================================
    // COMPUTED PROPERTIES
    // ============================================================

    /// <summary>Is the player in a session?</summary>
    public bool IsInSession => !string.IsNullOrEmpty(CurrentSessionId);

    /// <summary>Time since last activity</summary>
    public TimeSpan IdleTime => DateTime.UtcNow - LastActivity;

    /// <summary>Total connection time</summary>
    public TimeSpan TotalPlayTime => DateTime.UtcNow - ConnectedAt;

    /// <summary>Has spawned pawn?</summary>
    public bool HasPawn => CurrentPawn != null && CurrentPawn.IsSpawned;

    // ============================================================
    // METHODS
    // ============================================================

    /// <summary>
    /// Record player activity (updates LastActivity timestamp).
    /// </summary>
    public void RecordActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    /// <summary>
    /// Reset game-specific state when leaving a game.
    /// </summary>
    public void ResetGameState()
    {
        CurrentPawn = null;
        IsInGame = false;
        IsAlive = false;
        Position = Vector3.zero;
        Score = 0;
        Team = 0;
    }

    /// <summary>
    /// Reset session-specific state when leaving a session.
    /// </summary>
    public void ResetSessionState()
    {
        CurrentSessionId = null;
        IsReady = false;
        IsSessionHost = false;
        ResetGameState();
    }

    /// <summary>
    /// Initialize game state when entering a game.
    /// </summary>
    public void InitializeGameState(NetworkObject pawn, Vector3 spawnPosition)
    {
        CurrentPawn = pawn;
        IsInGame = true;
        IsAlive = true;
        Position = spawnPosition;
        Score = 0;
    }

    /// <summary>
    /// Constructor with required fields.
    /// </summary>
    public PlayerNetworkData(ulong clientId, string playerName)
    {
        ClientId = clientId;
        PlayerName = playerName ?? $"Player {clientId}";
        ConnectedAt = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
        IsAlive = true;
    }

    /// <summary>
    /// Get a summary string for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"[Player {ClientId}] {PlayerName} " +
               $"| Session: {CurrentSessionId ?? "None"} " +
               $"| Ready: {IsReady} " +
               $"| InGame: {IsInGame} " +
               $"| HasPawn: {HasPawn}";
    }
}
