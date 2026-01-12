using System.Collections.Generic;
using Core.StateSync;
using Unity.Netcode;

namespace Core.Networking
{
    /// <summary>
    /// Interface for sending game commands to clients.
    /// Implemented by Networking layer to avoid circular dependency.
    /// </summary>
    public interface IGameCommandSender
    {
        void SendGameCommandClientRpc(GameCommandDto command, ClientRpcParams rpcParams);
        void SendGameCommandBatchClientRpc(GameCommandDto[] commands, ClientRpcParams rpcParams);
    }

    /// <summary>
    /// Interface for accessing client registry.
    /// Implemented by Networking layer to avoid circular dependency.
    /// </summary>
    public interface IClientRegistry
    {
        string GetClientUid(ulong clientId);
    }
}
