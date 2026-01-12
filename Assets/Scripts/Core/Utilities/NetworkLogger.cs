using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Log severity levels for network and game systems
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Centralized logging utility for consistent, filterable logging across the application.
    /// Provides subsystem-level filtering, log levels, and structured logging patterns.
    /// 
    /// Usage:
    ///   NetworkLogger.Log("PlayerMovement", "Player moved", LogLevel.Info);
    ///   NetworkLogger.LogSession("GameStart", sessionId, "Game started");
    ///   NetworkLogger.Error("SessionManager", "Session not found");
    /// </summary>
    public static class NetworkLogger
    {
        private static readonly Dictionary<string, bool> _enabledSubsystems = new Dictionary<string, bool>();
        private static readonly HashSet<string> _muteSubsystems = new HashSet<string>();
        private static LogLevel _globalMinLevel = LogLevel.Debug;

        // Statistics
        private static int _totalLogs = 0;
        private static int _suppressedLogs = 0;

        #region Configuration

        /// <summary>
        /// Set global minimum log level. Logs below this level will be suppressed.
        /// </summary>
        public static void SetMinimumLogLevel(LogLevel level)
        {
            _globalMinLevel = level;
        }

        /// <summary>
        /// Enable logging for a specific subsystem
        /// </summary>
        public static void EnableSubsystem(string subsystem)
        {
            if (string.IsNullOrEmpty(subsystem)) return;
            _enabledSubsystems[subsystem] = true;
            _muteSubsystems.Remove(subsystem);
        }

        /// <summary>
        /// Disable logging for a specific subsystem
        /// </summary>
        public static void DisableSubsystem(string subsystem)
        {
            if (string.IsNullOrEmpty(subsystem)) return;
            _enabledSubsystems[subsystem] = false;
            _muteSubsystems.Add(subsystem);
        }

        /// <summary>
        /// Check if a subsystem is enabled for logging
        /// </summary>
        public static bool IsEnabled(string subsystem)
        {
            if (string.IsNullOrEmpty(subsystem)) return true;
            if (_muteSubsystems.Contains(subsystem)) return false;

            // If no explicit config, enabled by default
            if (!_enabledSubsystems.ContainsKey(subsystem))
                return true;

            return _enabledSubsystems[subsystem];
        }

        /// <summary>
        /// Reset all subsystem filters
        /// </summary>
        public static void ResetFilters()
        {
            _enabledSubsystems.Clear();
            _muteSubsystems.Clear();
        }

        #endregion

        #region Core Logging Methods

        /// <summary>
        /// Log a message with specified subsystem and level
        /// </summary>
        public static void Log(string subsystem, string message, LogLevel level = LogLevel.Info)
        {
            _totalLogs++;

            if (!ShouldLog(subsystem, level))
            {
                _suppressedLogs++;
                return;
            }

            string formattedMessage = FormatMessage(subsystem, message);

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }

        /// <summary>
        /// Log session-specific message with session ID
        /// </summary>
        public static void LogSession(string subsystem, string sessionId, string message, LogLevel level = LogLevel.Info)
        {
            string subsystemWithSession = $"{subsystem}:{sessionId}";
            Log(subsystemWithSession, message, level);
        }

        /// <summary>
        /// Log client-specific message with client ID
        /// </summary>
        public static void LogClient(string subsystem, ulong clientId, string message, LogLevel level = LogLevel.Info)
        {
            string messageWithClient = $"[Client {clientId}] {message}";
            Log(subsystem, messageWithClient, level);
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// Log debug message (lowest priority)
        /// </summary>
        public static void DebugLog(string subsystem, string message)
        {
            Log(subsystem, message, LogLevel.Debug);
        }

        /// <summary>
        /// Log info message (normal priority)
        /// </summary>
        public static void Info(string subsystem, string message)
        {
            Log(subsystem, message, LogLevel.Info);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void Warning(string subsystem, string message)
        {
            Log(subsystem, message, LogLevel.Warning);
        }

        /// <summary>
        /// Log error message (highest priority)
        /// </summary>
        public static void Error(string subsystem, string message)
        {
            Log(subsystem, message, LogLevel.Error);
        }

        /// <summary>
        /// Log exception with full stack trace
        /// </summary>
        public static void Exception(string subsystem, Exception exception)
        {
            if (!ShouldLog(subsystem, LogLevel.Error))
            {
                _suppressedLogs++;
                return;
            }

            string formattedMessage = FormatMessage(subsystem, $"Exception: {exception.Message}");
            Debug.LogError(formattedMessage);
            Debug.LogException(exception);
        }

        #endregion

        #region Specialized Logging

        /// <summary>
        /// Log RPC call with parameters
        /// </summary>
        public static void LogRpc(string rpcName, ulong senderId, string parameters = "")
        {
            string message = string.IsNullOrEmpty(parameters)
                ? $"RPC '{rpcName}' from client {senderId}"
                : $"RPC '{rpcName}' from client {senderId} | {parameters}";
            Log("RPC", message, LogLevel.Debug);
        }

        /// <summary>
        /// Log network event (connection, disconnection, etc.)
        /// </summary>
        public static void LogNetworkEvent(string eventType, ulong clientId, string details = "")
        {
            string message = string.IsNullOrEmpty(details)
                ? $"{eventType} - Client {clientId}"
                : $"{eventType} - Client {clientId} | {details}";
            Log("Network", message, LogLevel.Info);
        }

        /// <summary>
        /// Log validation failure with reason
        /// </summary>
        public static void LogValidationFailure(string validator, string reason)
        {
            Log($"Validation:{validator}", $"Failed: {reason}", LogLevel.Warning);
        }

        #endregion

        #region Statistics and Diagnostics

        /// <summary>
        /// Get total number of log attempts
        /// </summary>
        public static int GetTotalLogs() => _totalLogs;

        /// <summary>
        /// Get number of suppressed logs (filtered out)
        /// </summary>
        public static int GetSuppressedLogs() => _suppressedLogs;

        /// <summary>
        /// Get efficiency ratio (0-1, higher is more suppression)
        /// </summary>
        public static float GetSuppressionRatio()
        {
            if (_totalLogs == 0) return 0f;
            return (float)_suppressedLogs / _totalLogs;
        }

        /// <summary>
        /// Log current statistics
        /// </summary>
        public static void LogStatistics()
        {
            Debug.Log($"[NetworkLogger] Stats: {_totalLogs} total, {_suppressedLogs} suppressed ({GetSuppressionRatio():P1})");
        }

        #endregion

        #region Internal Helpers

        private static bool ShouldLog(string subsystem, LogLevel level)
        {
            // Check global minimum level
            if (level < _globalMinLevel)
                return false;

            // Check subsystem filter
            if (!IsEnabled(subsystem))
                return false;

            return true;
        }

        private static string FormatMessage(string subsystem, string message)
        {
            if (string.IsNullOrEmpty(subsystem))
                return message;

            return $"[{subsystem}] {message}";
        }

        #endregion

        #region Build-Specific Configuration

        /// <summary>
        /// Configure logger for production build (suppress debug logs)
        /// </summary>
        public static void ConfigureForProduction()
        {
            SetMinimumLogLevel(LogLevel.Warning);
            DisableSubsystem("RPC");
            DisableSubsystem("Validation");
        }

        /// <summary>
        /// Configure logger for development (all logs enabled)
        /// </summary>
        public static void ConfigureForDevelopment()
        {
            SetMinimumLogLevel(LogLevel.Debug);
            ResetFilters();
        }

        #endregion
    }
}
