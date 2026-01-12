using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Core.Games;
using Core.StateSync;
using Networking.Shared;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Networking.Server
{
    /// <summary>
    /// Dedicated server bootstrap for headless server builds.
    /// Handles server initialization, command-line arguments, and server lifecycle.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class ServerBootstrap : MonoBehaviour
    {
        private static ServerBootstrap _instance;
        public static ServerBootstrap Instance => _instance;

        private NetworkManager _networkManager;
        private UnityTransport _transport;
        private ConnectionController _connectionController;
        private ushort _serverPort = 7777;
        private int _maxPlayers = 32;
        private float _serverStartTime;
        private NetworkObject _sessionRpcHubPrefab;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeServerLogging();
            ParseCommandLine();
            Initialize();
        }

        private void InitializeServerLogging()
        {
            // Disable stack traces for cleaner server logs
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Debug.Log("[ServerBootstrap] Server logging initialized");
        }

        private void ParseCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                if ((arg == "-port" || arg == "--port") && i + 1 < args.Length)
                {
                    if (ushort.TryParse(args[i + 1], out ushort port))
                    {
                        _serverPort = port;
                        Debug.Log($"[ServerBootstrap] Port set to {port} from command line");
                    }
                }
                else if ((arg == "-maxplayers" || arg == "--maxplayers") && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int max))
                    {
                        _maxPlayers = max;
                        Debug.Log($"[ServerBootstrap] Max players set to {max} from command line");
                    }
                }
            }
        }

        private void Initialize()
        {
            Debug.Log("[ServerBootstrap] Initializing dedicated server...");

            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();

            if (_networkManager == null)
            {
                Debug.LogError("[ServerBootstrap] NetworkManager missing!");
                return;
            }

            if (_transport == null)
            {
                Debug.LogError("[ServerBootstrap] UnityTransport missing!");
                return;
            }

            // Configure network manager for server
            _networkManager.NetworkConfig.EnableSceneManagement = false;
            _networkManager.NetworkConfig.ForceSamePrefabs = false;

            // Configure transport
            _transport.UseWebSockets = false;
            _transport.UseEncryption = false;
            _transport.ConnectionData.Address = "0.0.0.0";
            _transport.ConnectionData.Port = _serverPort;

            // Hook network events
            _networkManager.OnServerStarted += OnServerStarted;
            _networkManager.OnServerStopped += OnServerStopped;
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

            // Initialize server components
            EnsureSessionManager();
            EnsureConnectionController();

            // Start server
            StartServer();
        }

        private void EnsureSessionManager()
        {
            if (GameSessionManager.Instance != null)
            {
                Debug.Log("[ServerBootstrap] GameSessionManager already active");
                return;
            }

            GameObject go = new GameObject("GameSessionManager");
            go.AddComponent<GameSessionManager>();
            Debug.Log("[ServerBootstrap] GameSessionManager created");

            GlobalRegistryHub.EnsureInstance();
        }

        private void EnsureConnectionController()
        {
            _connectionController = ConnectionController.Instance;
            Debug.Log("[ServerBootstrap] ConnectionController initialized");
        }

        private void StartServer()
        {
            if (_networkManager == null)
            {
                Debug.LogError("[ServerBootstrap] Cannot start server: NetworkManager missing");
                return;
            }

            if (!IsPortFree(_serverPort))
            {
                Debug.LogError($"[ServerBootstrap] Port {_serverPort} is not available");
                return;
            }

            Debug.Log($"[ServerBootstrap] Starting server on port {_serverPort} (max players: {_maxPlayers})");
            _serverStartTime = Time.realtimeSinceStartup;

            bool started = _networkManager.StartServer();
            if (!started)
            {
                Debug.LogError("[ServerBootstrap] Failed to start server");
                _networkManager.Shutdown();
                return;
            }

            Debug.Log("[ServerBootstrap] Server started successfully");
            SpawnSessionRpcHub();
            StartCoroutine(LogServerStats());
        }

        private void SpawnSessionRpcHub()
        {
            if (_networkManager == null || !_networkManager.IsServer)
            {
                Debug.LogWarning("[ServerBootstrap] Cannot spawn SessionRpcHub: not server");
                return;
            }

            // Try to find SessionRpcHub prefab from NetworkPrefabs
            if (!FindSessionRpcHubPrefab())
            {
                Debug.LogWarning("[ServerBootstrap] SessionRpcHub prefab not found in NetworkPrefabs");
                return;
            }

            if (_sessionRpcHubPrefab == null)
            {
                Debug.LogWarning("[ServerBootstrap] SessionRpcHub prefab is null");
                return;
            }

            NetworkObject hub = Instantiate(_sessionRpcHubPrefab);
            hub.Spawn(true);
            Debug.Log("[ServerBootstrap] SessionRpcHub spawned");
        }

        private bool FindSessionRpcHubPrefab()
        {
            if (_sessionRpcHubPrefab != null)
                return true;

            if (_networkManager == null || _networkManager.NetworkConfig?.Prefabs == null)
                return false;

            var prefabs = _networkManager.NetworkConfig.Prefabs;
            var lists = prefabs.NetworkPrefabsLists;

            if (lists == null || lists.Count == 0)
                return false;

            foreach (var list in lists)
            {
                if (list?.PrefabList == null)
                    continue;

                foreach (var entry in list.PrefabList)
                {
                    if (entry?.Prefab == null)
                        continue;

                    var hub = entry.Prefab.GetComponent<SessionRpcHub>();
                    if (hub != null)
                    {
                        var netObject = entry.Prefab.GetComponent<NetworkObject>();
                        if (netObject != null)
                        {
                            _sessionRpcHubPrefab = netObject;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsPortFree(ushort port)
        {
            try
            {
                using (UdpClient udp = new UdpClient())
                {
                    udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                }
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ServerBootstrap] Port check error: {ex.Message}");
                return false;
            }
        }

        private IEnumerator LogServerStats()
        {
            while (_networkManager != null && _networkManager.IsServer)
            {
                yield return new WaitForSeconds(60f);
                int players = _networkManager.ConnectedClientsIds.Count;
                float uptime = Time.realtimeSinceStartup - _serverStartTime;
                Debug.Log($"[ServerBootstrap] Stats - Players: {players}/{_maxPlayers} | Uptime: {uptime:F0}s");
            }
        }

        // Network event handlers
        private void OnServerStarted()
        {
            Debug.Log("[ServerBootstrap] Server started event");
        }

        private void OnServerStopped(bool isHost)
        {
            Debug.Log($"[ServerBootstrap] Server stopped (isHost: {isHost})");
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ServerBootstrap] Client {clientId} connected");
            _connectionController?.OnClientConnected(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[ServerBootstrap] Client {clientId} disconnected");
            _connectionController?.HandleClientDisconnect(clientId);
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnServerStarted -= OnServerStarted;
                _networkManager.OnServerStopped -= OnServerStopped;
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }
    }
}
