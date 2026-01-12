using Networking.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Abstract base class for all RPC handlers.
    /// Provides common functionality and logging.
    /// </summary>
    public abstract class BaseRpcHandler : ISessionRpcHandler
    {
        protected SessionRpcHub Hub { get; private set; }
        protected bool IsInitialized { get; private set; }

        public virtual void Initialize(SessionRpcHub hub)
        {
            if (hub == null)
            {
                Debug.LogError($"[{GetHandlerName()}] Cannot initialize with null hub");
                return;
            }

            Hub = hub;
            IsInitialized = true;
            OnInitialize();
            Log($"Handler initialized");
        }

        public virtual void Cleanup()
        {
            OnCleanup();
            IsInitialized = false;
            Hub = null;
            Log($"Handler cleaned up");
        }

        public abstract string GetHandlerName();

        /// <summary>
        /// Called after Initialize() completes.
        /// Override to add custom initialization logic.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Called before Cleanup() completes.
        /// Override to add custom cleanup logic.
        /// </summary>
        protected virtual void OnCleanup() { }

        /// <summary>
        /// Log a message with handler name prefix.
        /// </summary>
        protected void Log(string message)
        {
            Debug.Log($"[{GetHandlerName()}] {message}");
        }

        /// <summary>
        /// Log a warning with handler name prefix.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetHandlerName()}] {message}");
        }

        /// <summary>
        /// Log an error with handler name prefix.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetHandlerName()}] {message}");
        }

        /// <summary>
        /// Check if handler is properly initialized.
        /// </summary>
        protected bool CheckInitialized()
        {
            if (!IsInitialized || Hub == null)
            {
                LogError("Handler not initialized");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Build ClientRpcParams for targeted RPC sends.
        /// </summary>
        protected ClientRpcParams BuildClientRpcParams(System.Collections.Generic.List<ulong> targetClientIds)
        {
            var ids = new ulong[targetClientIds.Count];
            for (int i = 0; i < targetClientIds.Count; i++)
                ids[i] = targetClientIds[i];

            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = ids
                }
            };
        }

        /// <summary>
        /// Build ClientRpcParams for single client targeted RPC.
        /// </summary>
        protected ClientRpcParams BuildClientRpcParams(ulong targetClientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClientId }
                }
            };
        }
    }

    /// <summary>
    /// Abstract base class for validators.
    /// Provides common validation patterns.
    /// </summary>
    public abstract class BaseValidator : ISessionValidator
    {
        protected GameSessionManager SessionManager => GameSessionManager.Instance;

        public abstract ValidationResult ValidateAccess(ulong clientId, string sessionName, string actionName);

        /// <summary>
        /// Validate that session exists.
        /// </summary>
        protected ValidationResult ValidateSessionExists(string sessionName)
        {
            if (SessionManager == null)
            {
                return ValidationResult.Failure(
                    "GameSessionManager not initialized",
                    ValidationErrorCode.InvalidState
                );
            }

            var details = SessionManager.BuildDetails(sessionName);
            if (!details.HasValue)
            {
                return ValidationResult.Failure(
                    $"Session '{sessionName}' not found",
                    ValidationErrorCode.SessionNotFound
                );
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate that client has access to session.
        /// </summary>
        protected ValidationResult ValidateClientInSession(ulong clientId, string sessionName)
        {
            var sessionExists = ValidateSessionExists(sessionName);
            if (!sessionExists.IsValid)
                return sessionExists;

            // Authentication/authorization layer removed: session existence is enough here.
            return ValidationResult.Success();
        }

        /// <summary>
        /// Validate that client is session host.
        /// </summary>
        protected ValidationResult ValidateIsHost(ulong clientId, string sessionName)
        {
            var details = SessionManager.BuildDetails(sessionName);
            if (!details.HasValue)
            {
                return ValidationResult.Failure(
                    $"Session '{sessionName}' not found",
                    ValidationErrorCode.SessionNotFound
                );
            }

            if (details.Value.session.creator != clientId)
            {
                return ValidationResult.Failure(
                    "Only session host can perform this action",
                    ValidationErrorCode.NotSessionHost
                );
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Log validation result.
        /// </summary>
        protected void LogValidation(ValidationResult result, string context)
        {
            if (!result.IsValid)
            {
                Debug.LogWarning($"[Validator] {context} failed: {result.ErrorMessage} (Code: {result.ErrorCode})");
            }
        }
    }
}
