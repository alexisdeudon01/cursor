using Unity.Netcode;
using UnityEngine;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Base interface for all RPC handlers.
    /// Provides common contract for session-related RPC processing.
    /// </summary>
    public interface ISessionRpcHandler
    {
        /// <summary>
        /// Initialize the handler with SessionRpcHub reference.
        /// Called once when handler is created.
        /// </summary>
        void Initialize(SessionRpcHub hub);

        /// <summary>
        /// Cleanup handler resources.
        /// Called when handler is disposed or hub is destroyed.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Get handler name for logging purposes.
        /// </summary>
        string GetHandlerName();
    }

    /// <summary>
    /// Interface for session validation logic.
    /// Handlers can implement this to provide custom validation.
    /// </summary>
    public interface ISessionValidator
    {
        /// <summary>
        /// Validate if a client can perform an action on a session.
        /// </summary>
        /// <param name="clientId">Client requesting the action</param>
        /// <param name="sessionName">Target session name</param>
        /// <param name="actionName">Action being performed (for error messages)</param>
        /// <returns>Validation result with error message if failed</returns>
        ValidationResult ValidateAccess(ulong clientId, string sessionName, string actionName);
    }

    /// <summary>
    /// Interface for command-based handlers.
    /// Used for player actions that can be executed via Command Pattern.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Process a player command.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command was executed successfully</returns>
        bool ExecuteCommand(IPlayerCommand command);

        /// <summary>
        /// Check if handler can process a specific command type.
        /// </summary>
        /// <param name="commandType">Type of command to check</param>
        /// <returns>True if handler supports this command type</returns>
        bool CanHandleCommand(System.Type commandType);
    }

    /// <summary>
    /// Result of a validation operation.
    /// </summary>
    public struct ValidationResult
    {
        /// <summary>Is the validation successful?</summary>
        public bool IsValid { get; set; }

        /// <summary>Error message if validation failed</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Error code for programmatic handling</summary>
        public ValidationErrorCode ErrorCode { get; set; }

        public static ValidationResult Success()
        {
            return new ValidationResult
            {
                IsValid = true,
                ErrorMessage = string.Empty,
                ErrorCode = ValidationErrorCode.None
            };
        }

        public static ValidationResult Failure(string message, ValidationErrorCode code)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = message,
                ErrorCode = code
            };
        }
    }

    /// <summary>
    /// Validation error codes for programmatic handling.
    /// </summary>
    public enum ValidationErrorCode
    {
        None = 0,
        SessionNotFound = 1,
        AuthorizationFailed = 2,
        InvalidState = 3,
        NotSessionHost = 4,
        NotEnoughPlayers = 5,
        InvalidParameter = 6,
        RateLimited = 7,
        AlreadyInSession = 8,
        GameNotFound = 9
    }
}
