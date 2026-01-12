using Unity.Netcode;
using UnityEngine;

namespace Networking.StateSync
{
    public sealed class GlobalRegistryHub : MonoBehaviour
    {
        public static GlobalRegistryHub Instance { get; private set; }

        public ClientRegistry ClientRegistry { get; private set; }
        public SessionRegistry SessionRegistry { get; private set; }
        public GameRegisterTemplate GameRegisterTemplate { get; private set; }
        public GameInstanceRegister GameInstanceRegister { get; private set; }

        public static GlobalRegistryHub EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject("GlobalRegistryHub");
            return go.AddComponent<GlobalRegistryHub>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ClientRegistry = new ClientRegistry();
            SessionRegistry = new SessionRegistry();
            GameRegisterTemplate = new GameRegisterTemplate();
            GameInstanceRegister = new GameInstanceRegister();

            TryHookNetworkCallbacks();
        }

        private void Start()
        {
            TryHookNetworkCallbacks();
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void TryHookNetworkCallbacks()
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            ClientRegistry.RegisterExisting(NetworkManager.Singleton);
        }

        private void HandleClientConnected(ulong clientId)
        {
            ClientRegistry.Register(clientId, null);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            ClientRegistry.UnregisterByNetClientId(clientId);
        }
    }
}
