using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Client
{
    /// <summary>
    /// Client bootstrap for client builds.
    /// Handles client connection to server and UI initialization.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(UnityTransport))]
    public class ClientBootstrap : MonoBehaviour
    {
        private static ClientBootstrap _instance;
        public static ClientBootstrap Instance => _instance;

        private NetworkManager _networkManager;
        private UnityTransport _transport;
        private string _serverIp = "127.0.0.1";
        private ushort _serverPort = 7777;
        private string _menuSceneName = SceneNames.Menu;

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnConnectionFailed;

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
            Initialize();
        }

        private void Initialize()
        {
            Debug.Log("[ClientBootstrap] Initializing client...");

            _networkManager = GetComponent<NetworkManager>();
            _transport = GetComponent<UnityTransport>();

            if (_networkManager == null)
            {
                Debug.LogError("[ClientBootstrap] NetworkManager missing!");
                return;
            }

            if (_transport == null)
            {
                Debug.LogError("[ClientBootstrap] UnityTransport missing!");
                return;
            }

            // Configure network manager for client
            _networkManager.NetworkConfig.EnableSceneManagement = false;
            _networkManager.NetworkConfig.ForceSamePrefabs = false;

            // Configure transport
            _transport.UseWebSockets = false;
            _transport.UseEncryption = false;

            // Hook network events
            _networkManager.OnClientConnectedCallback += OnClientConnected;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            _networkManager.OnClientStarted += OnClientStarted;
            _networkManager.OnClientStopped += OnClientStopped;
            _networkManager.OnTransportFailure += OnTransportFailure;

            Debug.Log("[ClientBootstrap] Client initialized");
        }

        /// <summary>
        /// Connect to server with default IP and port.
        /// </summary>
        public void Connect()
        {
            Connect(_serverIp, _serverPort);
        }

        /// <summary>
        /// Connect to server with specific IP and port.
        /// </summary>
        public void Connect(string ip, ushort port)
        {
            if (_networkManager == null)
            {
                OnConnectionFailed?.Invoke("NetworkManager missing");
                return;
            }

            if (_networkManager.IsClient)
            {
                Debug.LogWarning("[ClientBootstrap] Client already connected");
                return;
            }

            Debug.Log($"[ClientBootstrap] Connecting to {ip}:{port}...");

            // Update transport
            _transport.ConnectionData.Address = ip;
            _transport.ConnectionData.Port = port;

            // Start client
            bool started = _networkManager.StartClient();
            if (!started)
            {
                Debug.LogError("[ClientBootstrap] Failed to start client");
                OnConnectionFailed?.Invoke("Failed to start client");
                _networkManager.Shutdown();
            }
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        public void Disconnect()
        {
            if (_networkManager != null && _networkManager.IsClient)
            {
                _networkManager.Shutdown();
            }
        }

        /// <summary>
        /// Set server connection parameters.
        /// </summary>
        public void SetServerAddress(string ip, ushort port)
        {
            _serverIp = ip;
            _serverPort = port;
        }

        // Network event handlers
        private void OnClientConnected(ulong clientId)
        {
            if (_networkManager.IsClient && clientId == _networkManager.LocalClientId)
            {
                Debug.Log("[ClientBootstrap] Connected to server");
                OnConnected?.Invoke();
                LoadMenuScene();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (_networkManager.IsClient && clientId == _networkManager.LocalClientId)
            {
                Debug.Log("[ClientBootstrap] Disconnected from server");
                OnDisconnected?.Invoke();
            }
        }

        private void OnClientStarted()
        {
            Debug.Log("[ClientBootstrap] Client started");
        }

        private void OnClientStopped(bool isServer)
        {
            Debug.Log($"[ClientBootstrap] Client stopped (isServer: {isServer})");
        }

        private void OnTransportFailure()
        {
            Debug.LogError("[ClientBootstrap] Transport failure");
            OnConnectionFailed?.Invoke("Transport failure");
        }

        private void LoadMenuScene()
        {
            if (string.IsNullOrEmpty(_menuSceneName))
                return;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene.Equals(_menuSceneName, StringComparison.OrdinalIgnoreCase))
                return;

            SceneManager.LoadScene(_menuSceneName);
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
                _networkManager.OnClientStarted -= OnClientStarted;
                _networkManager.OnClientStopped -= OnClientStopped;
                _networkManager.OnTransportFailure -= OnTransportFailure;
            }
        }
    }
}
