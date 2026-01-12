using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Handles session query operations: RequestSessions, RequestSessionDetails.
    /// Extracted from SessionRpcHub to reduce complexity.
    /// </summary>
    public class SessionQueryHandler : BaseRpcHandler
    {
        public override string GetHandlerName() => "SessionQueryHandler";

        // ============================================================
        // REQUEST SESSIONS
        // ============================================================

        public void HandleRequestSessions()
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant");
                return;
            }

            var sessions = GameSessionManager.Instance.GetSessionsSnapshot();
            Log($"Broadcasting {sessions.Length} sessions");

            SessionRpcHub.TryBroadcastSessions(sessions);
        }

        // ============================================================
        // REQUEST SESSION DETAILS
        // ============================================================

        public void HandleRequestSessionDetails(string sessionName, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant");
                return;
            }

            var details = GameSessionManager.Instance.BuildDetails(sessionName);
            if (details.HasValue)
            {
                Log($"Sending details for session '{sessionName}' to client {clientId}");
                Hub.SendSessionDetailsClientRpc(details.Value, BuildClientRpcParams(clientId));
            }
            else
            {
                LogWarning($"Session '{sessionName}' not found for details request");
            }
        }
    }
}
