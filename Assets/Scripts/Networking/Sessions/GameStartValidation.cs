using Unity.Netcode;

/// <summary>
/// Error codes for game start failures.
/// Used to provide specific feedback to clients.
/// </summary>
public enum GameStartFailureReason
{
    None,
    SessionNotFound,
    NotSessionHost,
    NotEnoughPlayers,
    NotAllPlayersReady,
    GameAlreadyStarted,
    InvalidGameType,
    AuthorizationFailed,
    ServerError
}

/// <summary>
/// Result of game start validation.
/// </summary>
public struct GameStartValidation
{
    public bool IsValid;
    public string ErrorMessage;
    public GameStartFailureReason Reason;
    
    public static GameStartValidation Success()
    {
        return new GameStartValidation
        {
            IsValid = true,
            ErrorMessage = string.Empty,
            Reason = GameStartFailureReason.None
        };
    }
    
    public static GameStartValidation Failure(string message, GameStartFailureReason reason)
    {
        return new GameStartValidation
        {
            IsValid = false,
            ErrorMessage = message,
            Reason = reason
        };
    }
}
