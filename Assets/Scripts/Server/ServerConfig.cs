using UnityEngine;

namespace CursorServer
{
    /// <summary>
    /// Configuration settings for the fully authorized server
    /// </summary>
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "Cursor/Server Configuration")]
    public class ServerConfig : ScriptableObject
    {
        [Header("Server Settings")]
        [Tooltip("Port number for server connections")]
        public int Port = 7777;

        [Tooltip("Maximum number of concurrent connections")]
        public int MaxConnections = 100;

        [Header("Authorization Settings")]
        [Tooltip("Require full authorization for all connections")]
        public bool RequireFullAuthorization = true;

        [Tooltip("Token expiration time in seconds")]
        public int TokenExpirationSeconds = 3600;

        [Header("Data Oriented Settings")]
        [Tooltip("Enable data-oriented processing")]
        public bool EnableDataOrientedProcessing = true;

        [Tooltip("Data sync interval in milliseconds")]
        public int DataSyncIntervalMs = 100;

        [Tooltip("Maximum data packet size in bytes")]
        public int MaxDataPacketSize = 4096;

        public void ValidateConfig()
        {
            if (Port < 1024 || Port > 65535)
            {
                Debug.LogWarning("Port should be between 1024 and 65535");
            }

            if (MaxConnections < 1)
            {
                Debug.LogWarning("MaxConnections should be at least 1");
            }
        }
    }
}
