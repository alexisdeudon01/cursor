using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Networking.RpcHandlers
{
    /// <summary>
    /// Handles session lifecycle operations: Create, Join, Leave, SetReady.
    /// Extracted from SessionRpcHub to reduce complexity.
    /// </summary>
    public class SessionLifecycleHandler : BaseRpcHandler
    {
        public override string GetHandlerName() => "SessionLifecycleHandler";

        // ============================================================
        // CREATE SESSION
        // ============================================================

        public void HandleCreateSession(string sessionName, string playerName, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (string.IsNullOrWhiteSpace(sessionName))
            {
                SendError(clientId, "Impossible de créer la session (nom vide)");
                return;
            }

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant sur le serveur");
                return;
            }

            bool added = GameSessionManager.Instance.TryAddSession(clientId, sessionName, playerName);

            if (!added)
            {
                SendError(clientId, "Impossible de créer la session (nom déjà utilisé)");
            }
            else
            {
                Log($"Session '{sessionName}' created by client {clientId}");
            }
        }

        // ============================================================
        // JOIN SESSION
        // ============================================================

        public void HandleJoinSession(string sessionName, string playerName, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant");
                return;
            }

            Log($"Client {clientId} joining session '{sessionName}'");
            GameSessionManager.Instance.TryJoinSession(clientId, sessionName, playerName);
        }

        // ============================================================
        // LEAVE SESSION
        // ============================================================

        public void HandleLeaveSession(string sessionName, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant");
                return;
            }

            Log($"Client {clientId} leaving session '{sessionName}'");
            GameSessionManager.Instance.LeaveSession(clientId, sessionName);
        }

        // ============================================================
        // SET READY
        // ============================================================

        public void HandleSetReady(string sessionName, bool ready, ulong clientId)
        {
            if (!CheckInitialized())
                return;

            if (GameSessionManager.Instance == null)
            {
                LogWarning("GameSessionManager manquant");
                return;
            }

            Log($"Client {clientId} set ready={ready} in session '{sessionName}'");
            GameSessionManager.Instance.SetReady(clientId, sessionName, ready);
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        private void SendError(ulong clientId, string message)
        {
            LogWarning($"Sending error to client {clientId}: {message}");

            // Call ClientRpc on SessionRpcHub
            if (Hub != null && Hub.IsSpawned)
            {
                Hub.SendSessionErrorClientRpc(message, BuildClientRpcParams(clientId));
            }
        }
    }
}
