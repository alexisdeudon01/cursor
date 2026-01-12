using Core.Utilities;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Core.Utilities
{
    /// <summary>
    /// Centralized utility for resolving player NetworkObjects, components, and metadata.
    /// Eliminates redundant player resolution logic across handlers and systems.
    /// 
    /// Usage:
    ///   string playerName = NetworkPlayerResolver.GetPlayerName(clientId);
    ///   var player = NetworkPlayerResolver.GetPlayerComponent<DefaultPlayer>(clientId);
    ///   var networkObj = NetworkPlayerResolver.GetPlayerNetworkObject(clientId);
    /// </summary>
    public static class NetworkPlayerResolver
    {
        #region Player Name Resolution

        /// <summary>
        /// Get player name for a client ID. Returns "PlayerXXX" fallback if not found.
        /// </summary>
        public static string GetPlayerName(ulong clientId)
        {
            var player = GetPlayerComponent<DefaultPlayer>(clientId);
            if (player != null)
            {
                // Check if NameAgent NetworkVariable is initialized
                if (!player.NameAgent.Value.IsEmpty)
                {
                    return player.NameAgent.Value.ToString();
                }
            }

            return $"Player{clientId}";
        }

        /// <summary>
        /// Try to get player name. Returns true if found, false if using fallback.
        /// </summary>
        public static bool TryGetPlayerName(ulong clientId, out string playerName)
        {
            var player = GetPlayerComponent<DefaultPlayer>(clientId);
            if (player != null && !player.NameAgent.Value.IsEmpty)
            {
                playerName = player.NameAgent.Value.ToString();
                return true;
            }

            playerName = $"Player{clientId}";
            return false;
        }

        #endregion

        #region Component Resolution

        /// <summary>
        /// Get a specific component from player's NetworkObject.
        /// Returns null if player not found or component not present.
        /// </summary>
        public static T GetPlayerComponent<T>(ulong clientId) where T : Component
        {
            var networkObject = GetPlayerNetworkObject(clientId);
            if (networkObject == null)
                return null;

            return networkObject.GetComponent<T>();
        }

        /// <summary>
        /// Get multiple components from player's NetworkObject.
        /// Returns empty array if player not found.
        /// </summary>
        public static T[] GetPlayerComponents<T>(ulong clientId) where T : Component
        {
            var networkObject = GetPlayerNetworkObject(clientId);
            if (networkObject == null)
                return new T[0];

            return networkObject.GetComponents<T>();
        }

        /// <summary>
        /// Get component in children of player's NetworkObject.
        /// Returns null if player not found or component not present.
        /// </summary>
        public static T GetPlayerComponentInChildren<T>(ulong clientId, bool includeInactive = false) where T : Component
        {
            var networkObject = GetPlayerNetworkObject(clientId);
            if (networkObject == null)
                return null;

            return networkObject.GetComponentInChildren<T>(includeInactive);
        }

        #endregion

        #region NetworkObject Resolution

        /// <summary>
        /// Get player's NetworkObject by client ID.
        /// Returns null if not found or NetworkManager unavailable.
        /// </summary>
        public static NetworkObject GetPlayerNetworkObject(ulong clientId)
        {
            if (!IsNetworkManagerAvailable())
            {
                NetworkLogger.Warning("NetworkPlayerResolver", "NetworkManager not available");
                return null;
            }

            var spawnManager = NetworkManager.Singleton.SpawnManager;
            if (spawnManager == null)
            {
                NetworkLogger.Warning("NetworkPlayerResolver", "SpawnManager not available");
                return null;
            }

            if (spawnManager.SpawnedObjects.TryGetValue(clientId, out var networkObject))
            {
                return networkObject;
            }

            NetworkLogger.DebugLog("NetworkPlayerResolver", $"Player NetworkObject not found for client {clientId}");
            return null;
        }

        /// <summary>
        /// Check if a player NetworkObject exists for the given client ID
        /// </summary>
        public static bool PlayerExists(ulong clientId)
        {
            return GetPlayerNetworkObject(clientId) != null;
        }

        /// <summary>
        /// Get all spawned player NetworkObjects
        /// </summary>
        public static NetworkObject[] GetAllPlayerNetworkObjects()
        {
            if (!IsNetworkManagerAvailable())
                return new NetworkObject[0];

            var spawnManager = NetworkManager.Singleton.SpawnManager;
            if (spawnManager == null)
                return new NetworkObject[0];

            var players = new System.Collections.Generic.List<NetworkObject>();
            foreach (var kvp in spawnManager.SpawnedObjects)
            {
                // Filter for player objects (assuming they have DefaultPlayer component)
                if (kvp.Value.GetComponent<DefaultPlayer>() != null)
                {
                    players.Add(kvp.Value);
                }
            }

            return players.ToArray();
        }

        #endregion

        #region Player Metadata

        /// <summary>
        /// Get player's GameObject
        /// </summary>
        public static GameObject GetPlayerGameObject(ulong clientId)
        {
            var networkObject = GetPlayerNetworkObject(clientId);
            return networkObject != null ? networkObject.gameObject : null;
        }

        /// <summary>
        /// Get player's Transform
        /// </summary>
        public static Transform GetPlayerTransform(ulong clientId)
        {
            var networkObject = GetPlayerNetworkObject(clientId);
            return networkObject != null ? networkObject.transform : null;
        }

        /// <summary>
        /// Get player's position
        /// </summary>
        public static Vector3 GetPlayerPosition(ulong clientId)
        {
            var transform = GetPlayerTransform(clientId);
            return transform != null ? transform.position : Vector3.zero;
        }

        /// <summary>
        /// Get player's rotation
        /// </summary>
        public static Quaternion GetPlayerRotation(ulong clientId)
        {
            var transform = GetPlayerTransform(clientId);
            return transform != null ? transform.rotation : Quaternion.identity;
        }

        #endregion

        #region Validation and Diagnostics

        /// <summary>
        /// Check if NetworkManager is available and ready
        /// </summary>
        public static bool IsNetworkManagerAvailable()
        {
            return NetworkManager.Singleton != null;
        }

        /// <summary>
        /// Check if player is connected and spawned
        /// </summary>
        public static bool IsPlayerConnected(ulong clientId)
        {
            if (!IsNetworkManagerAvailable())
                return false;

            return NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId);
        }

        /// <summary>
        /// Get count of connected players
        /// </summary>
        public static int GetConnectedPlayerCount()
        {
            if (!IsNetworkManagerAvailable())
                return 0;

            return NetworkManager.Singleton.ConnectedClients.Count;
        }

        /// <summary>
        /// Get list of all connected client IDs
        /// </summary>
        public static ulong[] GetAllConnectedClientIds()
        {
            if (!IsNetworkManagerAvailable())
                return new ulong[0];

            return NetworkManager.Singleton.ConnectedClients.Keys.ToArray();
        }

        /// <summary>
        /// Check if this is the server
        /// </summary>
        public static bool IsServer()
        {
            return IsNetworkManagerAvailable() && NetworkManager.Singleton.IsServer;
        }

        /// <summary>
        /// Check if this is a client
        /// </summary>
        public static bool IsClient()
        {
            return IsNetworkManagerAvailable() && NetworkManager.Singleton.IsClient;
        }

        /// <summary>
        /// Get local client ID
        /// </summary>
        public static ulong GetLocalClientId()
        {
            if (!IsNetworkManagerAvailable())
            {
                NetworkLogger.Warning("NetworkPlayerResolver", "Cannot get local client ID: NetworkManager unavailable");
                return 0;
            }

            return NetworkManager.Singleton.LocalClientId;
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Get player names for multiple client IDs
        /// </summary>
        public static string[] GetPlayerNames(params ulong[] clientIds)
        {
            string[] names = new string[clientIds.Length];
            for (int i = 0; i < clientIds.Length; i++)
            {
                names[i] = GetPlayerName(clientIds[i]);
            }
            return names;
        }

        /// <summary>
        /// Get components for multiple client IDs
        /// </summary>
        public static T[] GetPlayerComponents<T>(params ulong[] clientIds) where T : Component
        {
            var components = new System.Collections.Generic.List<T>();
            foreach (var clientId in clientIds)
            {
                var component = GetPlayerComponent<T>(clientId);
                if (component != null)
                {
                    components.Add(component);
                }
            }
            return components.ToArray();
        }

        #endregion

        #region Statistics

        private static int _resolutionAttempts = 0;
        private static int _resolutionFailures = 0;

        /// <summary>
        /// Get total resolution attempts
        /// </summary>
        public static int GetResolutionAttempts() => _resolutionAttempts;

        /// <summary>
        /// Get resolution failures
        /// </summary>
        public static int GetResolutionFailures() => _resolutionFailures;

        /// <summary>
        /// Get resolution success rate
        /// </summary>
        public static float GetSuccessRate()
        {
            if (_resolutionAttempts == 0) return 1f;
            return 1f - ((float)_resolutionFailures / _resolutionAttempts);
        }

        #endregion
    }
}
