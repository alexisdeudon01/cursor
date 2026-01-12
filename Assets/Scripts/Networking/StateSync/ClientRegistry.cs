using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Networking.StateSync
{
    public sealed class ClientNode : NetworkClient
    {
        public string ClientUid { get; }
        public string DisplayName { get; set; }

        public ClientNode(string clientUid, ulong netClientId)
        {
            ClientUid = clientUid;
            ClientId = netClientId;
        }
    }

    public sealed class ClientRegistry
    {
        private readonly Dictionary<string, ClientNode> clients = new Dictionary<string, ClientNode>();

        public IReadOnlyDictionary<string, ClientNode> Clients => clients;

        public ClientNode Register(ulong netClientId, string displayName)
        {
            var existing = FindByNetClientId(netClientId);
            if (existing != null)
            {
                if (!string.IsNullOrEmpty(displayName))
                {
                    existing.DisplayName = displayName;
                }
                return existing;
            }

            var uid = Guid.NewGuid().ToString("N");
            var node = new ClientNode(uid, netClientId) { DisplayName = displayName ?? string.Empty };
            clients[uid] = node;
            return node;
        }

        public void RegisterExisting(NetworkManager networkManager)
        {
            if (networkManager == null)
            {
                return;
            }

            foreach (var clientId in networkManager.ConnectedClientsIds)
            {
                Register(clientId, null);
            }
        }

        public bool UnregisterByNetClientId(ulong netClientId)
        {
            string toRemove = null;
            foreach (var pair in clients)
            {
                if (pair.Value != null && pair.Value.ClientId == netClientId)
                {
                    toRemove = pair.Key;
                    break;
                }
            }

            if (toRemove == null)
            {
                return false;
            }

            return clients.Remove(toRemove);
        }

        public ClientNode GetByClientUid(string clientUid)
        {
            if (string.IsNullOrEmpty(clientUid))
            {
                return null;
            }

            return clients.TryGetValue(clientUid, out var node) ? node : null;
        }

        public ClientNode FindByNetClientId(ulong netClientId)
        {
            foreach (var pair in clients)
            {
                if (pair.Value != null && pair.Value.ClientId == netClientId)
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public bool TryGetClientUidByNetClientId(ulong netClientId, out string clientUid)
        {
            var node = FindByNetClientId(netClientId);
            if (node != null)
            {
                clientUid = node.ClientUid;
                return true;
            }

            clientUid = null;
            return false;
        }

        /// <summary>
        /// Convenience accessor: returns empty string if unknown.
        /// </summary>
        public string GetClientUid(ulong netClientId)
        {
            return TryGetClientUidByNetClientId(netClientId, out var uid) ? uid : string.Empty;
        }

    }
}