using UnityEngine;

namespace CursorShared
{
    /// <summary>
    /// Shared network protocol definitions for client and server
    /// </summary>
    public static class NetworkProtocol
    {
        public const int PROTOCOL_VERSION = 1;
        public const int DEFAULT_PORT = 7777;
        public const int MAX_PACKET_SIZE = 4096;
        
        // Message Types
        public const byte MSG_CONNECT = 0x01;
        public const byte MSG_DISCONNECT = 0x02;
        public const byte MSG_AUTH_REQUEST = 0x03;
        public const byte MSG_AUTH_RESPONSE = 0x04;
        public const byte MSG_DATA_STORE = 0x05;
        public const byte MSG_DATA_RETRIEVE = 0x06;
        public const byte MSG_DATA_DELETE = 0x07;
        public const byte MSG_HEARTBEAT = 0x08;

        public static string GetMessageTypeName(byte messageType)
        {
            switch (messageType)
            {
                case MSG_CONNECT: return "Connect";
                case MSG_DISCONNECT: return "Disconnect";
                case MSG_AUTH_REQUEST: return "Auth Request";
                case MSG_AUTH_RESPONSE: return "Auth Response";
                case MSG_DATA_STORE: return "Data Store";
                case MSG_DATA_RETRIEVE: return "Data Retrieve";
                case MSG_DATA_DELETE: return "Data Delete";
                case MSG_HEARTBEAT: return "Heartbeat";
                default: return "Unknown";
            }
        }
    }

    /// <summary>
    /// Shared utility functions
    /// </summary>
    public static class CursorUtilities
    {
        public static string GetBuildTarget()
        {
#if UNITY_EDITOR
            return "Editor";
#elif UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_STANDALONE_OSX
            return "MacOS";
#elif UNITY_STANDALONE_LINUX
            return "Linux";
#elif UNITY_SERVER
            return "Server";
#else
            return "Unknown";
#endif
        }

        public static bool IsServerBuild()
        {
#if UNITY_SERVER || SERVER_BUILD
            return true;
#else
            return false;
#endif
        }

        public static bool IsClientBuild()
        {
#if UNITY_SERVER || SERVER_BUILD
            return false;
#else
            return true;
#endif
        }

        public static void LogBuildInfo()
        {
            Debug.Log($"Build Target: {GetBuildTarget()}");
            Debug.Log($"Is Server: {IsServerBuild()}");
            Debug.Log($"Is Client: {IsClientBuild()}");
        }
    }
}
