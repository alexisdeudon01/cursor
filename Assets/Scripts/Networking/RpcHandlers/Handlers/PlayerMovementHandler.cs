using Core.Games;
using Core.StateSync;
using Core.Utilities;
using Networking.StateSync;
using UnityEngine;

namespace Networking.RpcHandlers.Handlers
{
    /// <summary>
    /// Handles game-related commands coming from clients.
    ///
    /// Data-oriented refactor (Option 1):
    /// - Player pawns are NOT NetworkObjects.
    /// - Clients only send input (MoveInput) and optional ResyncRequest.
    /// - Server runs an authoritative simulation and replicates transforms via UpdateEntity commands.
    /// </summary>
    public class PlayerMovementHandler : BaseRpcHandler
    {
        public override string GetHandlerName() => nameof(PlayerMovementHandler);

        public bool HandleCommand(GameCommandDto command, ulong senderClientId)
        {
            return command.Type switch
            {
                GameCommandType.MoveInput => HandleMoveInput(command, senderClientId),
                GameCommandType.ResyncRequest => HandleResyncRequest(command, senderClientId),
                _ => false
            };
        }

        private bool HandleMoveInput(GameCommandDto command, ulong senderClientId)
        {
            // Resolve the sessionName from the sessionUid carried in the command.
            var sessionName = ResolveSessionName(command.SessionUid.ToString());
            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = command.SessionUid.ToString();
            }

            if (string.IsNullOrEmpty(sessionName))
            {
                NetworkLogger.Warning("MoveInput", $"Missing SessionUid for client {senderClientId}");
                return false;
            }

            if (GameInstanceManager.Instance == null)
                return false;

            // Feed the input into the server-authoritative sim.
            GameInstanceManager.Instance.SetPlayerInput(sessionName, senderClientId, command.Direction);
            return true;
        }

        private bool HandleResyncRequest(GameCommandDto command, ulong senderClientId)
        {
            var sessionName = ResolveSessionName(command.SessionUid.ToString());
            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = command.SessionUid.ToString();
            }

            if (string.IsNullOrEmpty(sessionName))
            {
                NetworkLogger.Warning("ResyncRequest", $"Missing SessionUid for client {senderClientId}");
                return false;
            }

            if (GameInstanceManager.Instance == null)
                return false;

            GameInstanceManager.Instance.SendFullSnapshotToClient(sessionName, senderClientId, includeMapConfig: true);
            return true;
        }

        internal static string ResolveSessionName(string sessionUidOrName)
        {
            var registry = GlobalRegistryHub.Instance?.SessionRegistry;
            if (registry == null)
                return null;

            // Prefer UID lookup, but gracefully fall back to name (e.g. if a client sends input
            // before it has received the MapConfig command containing the UID).
            var entry = registry.GetByUid(sessionUidOrName);
            if (entry != null)
                return entry.Name;

            var entryByName = registry.GetByName(sessionUidOrName);
            return entryByName?.Name;
        }
    }
}
