using System;
using System.Collections.Generic;

/// <summary>
/// Service interface for session/lobby operations.
/// Provides a unified API for both client and server implementations.
/// </summary>
/// <remarks>
/// <para><b>Design Pattern:</b> Strategy Pattern - allows swapping client/server implementations.</para>
/// <para><b>Usage Flow:</b></para>
/// <list type="number">
///   <item>Client enters name → UI shows session list</item>
///   <item>Client calls <see cref="RefreshSessions"/> → <see cref="SessionsChanged"/> fires</item>
///   <item>Client calls <see cref="CreateSession"/> or <see cref="JoinSession"/></item>
///   <item>Client in lobby → <see cref="SetReady"/> when ready</item>
///   <item>Host calls <see cref="StartGame"/> when threshold met</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Subscribe to events
/// sessionService.SessionsChanged += OnSessionsUpdated;
/// sessionService.CurrentSessionChanged += OnLobbyUpdated;
/// 
/// // Create a session
/// sessionService.CreateSession("My Room");
/// 
/// // Toggle ready
/// sessionService.SetReady(true);
/// </code>
/// </example>
public interface ISessionService
{
    #region Events

    /// <summary>
    /// Fired when the list of available sessions changes.
    /// Subscribe in UI to update session list display.
    /// </summary>
    /// <remarks>
    /// <b>Threading:</b> Always invoked on main thread.
    /// <b>Payload:</b> Array of all available sessions (may be empty).
    /// </remarks>
    event Action<GameSession[]> SessionsChanged;

    /// <summary>
    /// Fired when current session details change (players join/leave, ready state changes).
    /// Subscribe in UI to update lobby display.
    /// </summary>
    /// <remarks>
    /// <b>Threading:</b> Always invoked on main thread.
    /// <b>Payload:</b> Details of the session the local player is in.
    /// </remarks>
    event Action<SessionDetails> CurrentSessionChanged;

    /// <summary>
    /// Fired when an error occurs during session operations.
    /// </summary>
    /// <remarks>
    /// <b>Use cases:</b> Session name taken, failed to join, network error.
    /// </remarks>
    event Action<SessionError> ErrorOccurred;

    /// <summary>
    /// Fired when the game is starting for the current session.
    /// </summary>
    /// <remarks>
    /// <b>Client:</b> Should transition to game scene.
    /// <b>Server:</b> Should spawn pawns for all players.
    /// </remarks>
    event Action<GameStartInfo> GameStarting;

    #endregion

    #region State Properties

    /// <summary>
    /// The name of the session the local player is currently in.
    /// Null if not in any session.
    /// </summary>
    string CurrentSessionName { get; }

    /// <summary>
    /// True if the local player is currently in a session.
    /// </summary>
    bool IsInSession { get; }

    /// <summary>
    /// True if the local player is the host/creator of the current session.
    /// </summary>
    bool IsHost { get; }

    /// <summary>
    /// The local player's ready state in the current session.
    /// </summary>
    bool IsReady { get; }

    #endregion

    #region Commands

    /// <summary>
    /// Request the server to send the current list of available sessions.
    /// Result will be delivered via <see cref="SessionsChanged"/> event.
    /// </summary>
    /// <remarks>
    /// <b>Call when:</b> UI panel opens, player connects, periodic refresh.
    /// <b>Rate limit:</b> Avoid calling more than once per second.
    /// </remarks>
    void RefreshSessions();

    /// <summary>
    /// Create a new session with the given name.
    /// The creating player automatically joins and becomes the host.
    /// </summary>
    /// <param name="sessionName">
    /// Unique name for the session. Will be trimmed.
    /// Must not be empty or already taken.
    /// </param>
    /// <remarks>
    /// <b>On success:</b> <see cref="CurrentSessionChanged"/> fires, <see cref="IsInSession"/> becomes true.
    /// <b>On failure:</b> <see cref="ErrorOccurred"/> fires with <see cref="SessionErrorCode.SessionNameTaken"/> or <see cref="SessionErrorCode.InvalidSessionName"/>.
    /// </remarks>
    void CreateSession(string sessionName);

    /// <summary>
    /// Join an existing session.
    /// </summary>
    /// <param name="sessionName">Name of the session to join.</param>
    /// <remarks>
    /// <b>On success:</b> <see cref="CurrentSessionChanged"/> fires.
    /// <b>On failure:</b> <see cref="ErrorOccurred"/> fires with <see cref="SessionErrorCode.SessionNotFound"/> or <see cref="SessionErrorCode.SessionFull"/>.
    /// </remarks>
    void JoinSession(string sessionName);

    /// <summary>
    /// Leave the current session.
    /// </summary>
    /// <remarks>
    /// <b>If host leaves:</b> Session may be destroyed or transferred.
    /// <b>After call:</b> <see cref="IsInSession"/> becomes false.
    /// </remarks>
    void LeaveSession();

    /// <summary>
    /// Set the local player's ready state.
    /// </summary>
    /// <param name="ready">True if ready to start, false otherwise.</param>
    /// <remarks>
    /// <b>Precondition:</b> Must be in a session (<see cref="IsInSession"/> == true).
    /// <b>Effect:</b> <see cref="CurrentSessionChanged"/> fires for all players in session.
    /// </remarks>
    void SetReady(bool ready);

    /// <summary>
    /// Request to start the game for the current session.
    /// Only the host can start the game.
    /// </summary>
    /// <remarks>
    /// <b>Precondition:</b> <see cref="IsHost"/> == true and minimum players ready.
    /// <b>On success:</b> <see cref="GameStarting"/> fires for all players in session.
    /// <b>On failure:</b> <see cref="ErrorOccurred"/> fires with <see cref="SessionErrorCode.NotEnoughPlayers"/> or <see cref="SessionErrorCode.NotHost"/>.
    /// </remarks>
    void StartGame();

    /// <summary>
    /// Request updated details for the current session.
    /// Result delivered via <see cref="CurrentSessionChanged"/> event.
    /// </summary>
    void RefreshCurrentSession();

    #endregion
}

#region Supporting Types

/// <summary>
/// Information about a game starting.
/// </summary>
[Serializable]
public struct GameStartInfo
{
    /// <summary>Name of the session starting.</summary>
    public string sessionName;

    /// <summary>Scene to load for the game.</summary>
    public string sceneName;

    /// <summary>List of player client IDs in the game.</summary>
    public List<ulong> playerIds;
}

/// <summary>
/// Session operation error information.
/// </summary>
[Serializable]
public struct SessionError
{
    /// <summary>Error code for programmatic handling.</summary>
    public SessionErrorCode code;

    /// <summary>Human-readable error message.</summary>
    public string message;

    public SessionError(SessionErrorCode code, string message)
    {
        this.code = code;
        this.message = message;
    }

    public override string ToString() => $"[{code}] {message}";
}

/// <summary>
/// Error codes for session operations.
/// </summary>
public enum SessionErrorCode
{
    /// <summary>Unknown error.</summary>
    Unknown = 0,

    /// <summary>Session name is empty or invalid.</summary>
    InvalidSessionName = 1,

    /// <summary>Session name is already taken.</summary>
    SessionNameTaken = 2,

    /// <summary>Session does not exist.</summary>
    SessionNotFound = 3,

    /// <summary>Session is full.</summary>
    SessionFull = 4,

    /// <summary>Not enough players ready to start.</summary>
    NotEnoughPlayers = 5,

    /// <summary>Only the host can perform this action.</summary>
    NotHost = 6,

    /// <summary>Player is not in a session.</summary>
    NotInSession = 7,

    /// <summary>Network connection lost.</summary>
    NetworkError = 8
}

#endregion
