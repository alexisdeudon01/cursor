using Core.Networking;
using Networking.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(UnityTransport))]
[RequireComponent(typeof(PrefabIdentity))]
[DisallowMultipleComponent]
public class NetworkBootstrap : MonoBehaviour
{
    public static NetworkBootstrap Instance { get; private set; }

    private NetworkManager networkManager;
    private UnityTransport transport;
    private AppNetworkConfig cfg;
    [SerializeField] private NetworkObject sessionRpcHubPrefab;

    [Header("Bootstrap UI (client only)")]
    [SerializeField] private MonoBehaviour progressViewMonoBehaviour; // Serialized as MonoBehaviour, cast to INetworkBootstrapProgressView
    private INetworkBootstrapProgressView progressView;
    [SerializeField] private bool showProgressUiOnClient = true;
    [SerializeField] private bool loadMenuAfterBootstrap = true;
    [SerializeField] private string menuSceneName = SceneNames.Menu;
    [SerializeField] private bool verboseBootstrapLogs = true;
    [SerializeField] private float minBootstrapUiSeconds = 0f; // No delay

    private int bootstrapStepIndex;
    private int bootstrapStepCount;
    private bool bootstrapCompleted;
    private ISceneServiceSync sceneService;

    private bool networkStarted;
    private float bootstrapUiStartTime;
    private Coroutine bootstrapUiCoroutine;
    private bool eventsHooked;

    // Events for UI and other systems
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionFailed;

    private float serverStartTime;

    private void OnEnable()
    {
        Debug.Log("[NetworkBootstrap] OnEnable start");

        try
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Debug.Log("[NetworkBootstrap] Checking duplicate prefab");
            if (DestroyIfDuplicatePrefab())
            {
                Debug.Log("[NetworkBootstrap] Duplicate prefab removed - skipping initialization (singleton already exists)");
                // Don't call Destroy(gameObject) here - DestroyIfDuplicatePrefab already handled it
                return;
            }

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.gameObject != gameObject)
            {
                Debug.LogWarning("[NetworkBootstrap] Duplicate NetworkManager detected - destroy");
                Destroy(gameObject);
                return;
            }

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[NetworkBootstrap] Duplicate NetworkBootstrap - destroy");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Debug.Log("[NetworkBootstrap] Active instance id=" + GetInstanceID() + ", scene=" + gameObject.scene.name);

            Initialize();
            DontDestroyOnLoad(gameObject);
        }
        catch (Exception ex)
        {
            ReportBootstrapError("OnEnable", "Unhandled error", ex);
        }
    }

    private void Start()
    {
        try
        {
            Debug.Log("[NetworkBootstrap] Start running AutoStartBySceneName");
            
            // Initialize server-specific settings if in batchmode
            if (Application.isBatchMode)
            {
                ParseCommandLine();
                InitializeServerLogging();
            }

            RegisterRequiredNetworkPrefabs();
            AutoStartBySceneName();
        }
        catch (Exception ex)
        {
            ReportBootstrapError("Start", "AutoStart failed", ex);
        }
    }

    private void ParseCommandLine()
    {
        if (cfg == null) cfg = NetworkConfigProvider.Config;
        if (cfg == null) return;

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLower();
            if ((arg == "-port" || arg == "--port") && i + 1 < args.Length)
            {
                if (ushort.TryParse(args[i + 1], out ushort port))
                {
                    cfg.port = port;
                }
            }
            else if ((arg == "-maxplayers" || arg == "--maxplayers") && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int max))
                {
                    cfg.maxPlayers = max;
                }
            }
        }
    }

    private void InitializeServerLogging()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        
        Debug.Log("========================================");
        Debug.Log("  DEDICATED SERVER INITIALIZED");
        Debug.Log($"  Port: {cfg.port}");
        Debug.Log($"  Max Players: {cfg.maxPlayers}");
        Debug.Log("========================================");
    }

    private void OnDestroy()
    {
        Debug.Log("[NetworkBootstrap] OnDestroy");

        try
        {
            UnhookNetworkEvents();
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetworkBootstrap] Exception in OnDestroy");
            Debug.LogException(ex);
        }
    }

    private bool DestroyIfDuplicatePrefab()
    {
        try
        {
            PrefabIdentity identity = GetComponent<PrefabIdentity>();
            if (!Application.isPlaying)
            {
                return false;
            }

            if (identity == null)
            {
                Debug.LogWarning("[NetworkBootstrap] PrefabIdentity missing, skip duplicate check");
                return false;
            }

            PrefabIdentity[] allPrefabs = FindObjectsByType<PrefabIdentity>(FindObjectsSortMode.None);
            List<PrefabIdentity> matches = new List<PrefabIdentity>();

            for (int i = 0; i < allPrefabs.Length; i++)
            {
                PrefabIdentity item = allPrefabs[i];
                if (item != null && item.PrefabId == identity.PrefabId)
                {
                    matches.Add(item);
                }
            }

            PrefabIdentity[] all = matches.ToArray();

            Debug.Log("[NetworkBootstrap] Prefab '" + identity.PrefabId + "' instances = " + all.Length);

            if (all.Length <= 1)
            {
                return false;
            }

            PrefabIdentity keeper = all[0];

            if (keeper.gameObject != gameObject)
            {
                Debug.LogWarning(
                    "[NetworkBootstrap] Duplicate NetworkManagerRoot - destroy\n"
                    + "ThisInstanceID=" + GetInstanceID() + ", Scene=" + gameObject.scene.name + "\n"
                    + "KeptInstanceID=" + keeper.gameObject.GetInstanceID() + ", Scene=" + keeper.gameObject.scene.name
                );

                Destroy(gameObject);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetworkBootstrap] Exception in DestroyIfDuplicatePrefab");
            Debug.LogException(ex);
        }

        return false;
    }

    private void Initialize()
    {
        try
        {
            BeginBootstrapProgress(6);

            ReportBootstrapStep("Initialize", "Read config");
            cfg = NetworkConfigProvider.Config;

            if (cfg == null)
            {
                ReportBootstrapError("Initialize", "Config missing");
                return;
            }

            ReportBootstrapStep("Initialize", "Resolve components");
            networkManager = GetComponent<NetworkManager>();
            transport = GetComponent<UnityTransport>();

            if (networkManager == null)
            {
                ReportBootstrapError("Initialize", "NetworkManager missing");
                return;
            }

            if (transport == null)
            {
                ReportBootstrapError("Initialize", "UnityTransport missing");
                return;
            }

            ReportBootstrapStep("Initialize", "Disable scene management");
            networkManager.NetworkConfig.EnableSceneManagement = false;
            
            // Disable strict prefab matching to allow connection during development
            // Server and Client may have different builds
            networkManager.NetworkConfig.ForceSamePrefabs = false;

            ReportBootstrapStep("Initialize", "Apply transport config");
            ApplyTransportConfig();

            ReportBootstrapStep("Initialize", "Hook network events");
            HookNetworkEvents();

            ReportBootstrapStep("Initialize", "Complete");
            CompleteBootstrapProgress();
        }
        catch (Exception ex)
        {
            ReportBootstrapError("Initialize", "Unhandled error", ex);
        }
    }

    private void ApplyTransportConfig()
    {
        AddNetworkEvent("Transport", "Apply config", false);

        if (transport == null || cfg == null)
        {
            ReportBootstrapError("Transport", "Missing transport or config");
            return;
        }

        try
        {
            // Force UDP (no WebSockets/TLS) for the game transport.
            transport.UseWebSockets = false;
            transport.UseEncryption = false;

            transport.ConnectionData.Address = cfg.ipAddress;
            transport.ConnectionData.Port = cfg.port;

            AddNetworkEvent("Transport", "Configured " + cfg.ipAddress + ":" + cfg.port, false);
        }
        catch (Exception ex)
        {
            ReportBootstrapError("Transport", "Apply config failed", ex);
        }
    }

    private void HookNetworkEvents()
    {
        AddNetworkEvent("Network", "Register events", false);

        if (networkManager == null)
        {
            ReportBootstrapError("Network", "NetworkManager missing");
            return;
        }

        try
        {
            if (eventsHooked)
            {
                AddNetworkEvent("Network", "Events already registered", false);
                return;
            }

            networkManager.OnClientConnectedCallback += HandleClientConnected;
            networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            networkManager.OnServerStarted += OnServerStarted;
            networkManager.OnClientStarted += OnClientStarted;
            networkManager.OnServerStopped += OnServerStopped;
            networkManager.OnClientStopped += OnClientStopped;
            networkManager.OnTransportFailure += OnTransportFailure;
            eventsHooked = true;

            AddNetworkEvent("Network", "Events registered", false);
        }
        catch (Exception ex)
        {
            ReportBootstrapError("Network", "Register events failed", ex);
        }
    }

    private void UnhookNetworkEvents()
    {
        if (!eventsHooked || networkManager == null)
            return;

        networkManager.OnClientConnectedCallback -= HandleClientConnected;
        networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        networkManager.OnServerStarted -= OnServerStarted;
        networkManager.OnClientStarted -= OnClientStarted;
        networkManager.OnServerStopped -= OnServerStopped;
        networkManager.OnClientStopped -= OnClientStopped;
        networkManager.OnTransportFailure -= OnTransportFailure;
        eventsHooked = false;
    }

    private void HandleClientConnected(ulong clientId)
    {
        AddNetworkEvent("Client connected", "ClientId=" + clientId, false);
        
        if (networkManager.IsClient && clientId == networkManager.LocalClientId)
        {
            OnConnected?.Invoke();
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        AddNetworkEvent("Client disconnected", "ClientId=" + clientId, false);
        
        if (networkManager.IsClient && clientId == networkManager.LocalClientId)
        {
            OnDisconnected?.Invoke();
        }
    }

    private void OnServerStarted()
    {
        AddNetworkEvent("Server started", "Server is active", false);
    }

    private void OnClientStarted()
    {
        AddNetworkEvent("Client started", "Client is active", false);
    }

    private void OnServerStopped(bool isHost)
    {
        AddNetworkEvent("Server stopped", "IsHost=" + isHost, false);
    }

    private void OnClientStopped(bool isServer)
    {
        AddNetworkEvent("Client stopped", "IsServer=" + isServer, false);
    }

    private void OnTransportFailure()
    {
        AddNetworkEvent("Transport failure", "Connection error", true);
        OnConnectionFailed?.Invoke("Transport failure");
    }

    private void AutoStartBySceneName()
    {
        try
        {
            string scene = SceneManager.GetActiveScene().name;
            Debug.Log("[NetworkBootstrap] AutoStart check activeScene='" + scene + "'");

            if (networkStarted)
            {
                Debug.Log("[NetworkBootstrap] Network already started - skip");
                return;
            }

            // If we are in the Server scene, start the server
            if (scene.Equals(SceneNames.Server, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("[NetworkBootstrap] Server scene detected - starting server");
                StartServer();
                networkStarted = true;
                return;
            }

            if (scene.Equals(SceneNames.Server, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("[NetworkBootstrap] Server scene detected - starting server");
                StartServer();
                networkStarted = true;
            }
            else if (scene.Equals(SceneNames.Client, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("[NetworkBootstrap] Client scene detected - auto-starting client");
                StartClient();
                networkStarted = true;
            }
            else
            {
                Debug.Log("[NetworkBootstrap] Menu or other scene - waiting for manual connection");
            }
        }
        catch (Exception ex)
        {
            ReportBootstrapError("AutoStart", "AutoStart failed", ex);
        }
    }

    public bool IsNetworkRunning()
    {
        return networkManager != null && (networkManager.IsServer || networkManager.IsClient);
    }

    public void StartClient()
    {
        if (cfg == null)
        {
            OnConnectionFailed?.Invoke("Config missing");
            return;
        }

        StartClient(cfg.ipAddress, cfg.port);
    }

    public void StartClient(string ip, ushort port)
    {
        try
        {
            RegisterRequiredNetworkPrefabs();
            if (networkManager == null)
            {
                AddNetworkEvent("StartClient", "NetworkManager missing", true);
                OnConnectionFailed?.Invoke("NetworkManager missing");
                return;
            }

            if (networkStarted && networkManager.IsClient)
            {
                AddNetworkEvent("StartClient", "Already started", false);
                return;
            }

            // Update transport with specific IP/Port
            if (transport != null)
            {
                transport.ConnectionData.Address = ip;
                transport.ConnectionData.Port = port;
            }

            AddNetworkEvent("StartClient", $"Starting client to {ip}:{port}", false);
            if (networkManager.StartClient())
            {
                networkStarted = true;
            }
            else
            {
                OnConnectionFailed?.Invoke("Failed to start client");
            }
        }
        catch (Exception ex)
        {
            ReportBootstrapError("StartClient", "Start failed", ex);
            OnConnectionFailed?.Invoke(ex.Message);
        }
    }

    public void Disconnect()
    {
        if (networkManager != null && (networkManager.IsClient || networkManager.IsServer))
        {
            networkManager.Shutdown();
            networkStarted = false;
        }
    }

    public void TestConnection(string ip, ushort port, Action<bool, string> callback)
    {
        StartCoroutine(TestConnectionCoroutine(ip, port, callback));
    }

    private IEnumerator TestConnectionCoroutine(string ip, ushort port, Action<bool, string> callback)
    {
        Debug.Log($"[NetworkBootstrap] Testing connection to {ip}:{port}...");
        bool success = false;
        string message = "Timeout";

        System.Net.Sockets.UdpClient udpClient = null;
        try
        {
            udpClient = new System.Net.Sockets.UdpClient();
            udpClient.Client.ReceiveTimeout = 2000;
            var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port);
            byte[] pingData = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            udpClient.Send(pingData, pingData.Length, endpoint);
            success = true;
            message = "UDP packet sent";
        }
        catch (Exception ex)
        {
            success = false;
            message = ex.Message;
        }
        finally
        {
            udpClient?.Close();
        }

        yield return null;
        callback?.Invoke(success, message);
    }

    public void StartServer()
    {
        try
        {
            if (cfg == null)
            {
                AddNetworkEvent("StartServer", "Config missing", true);
                return;
            }

            if (networkManager == null)
            {
                AddNetworkEvent("StartServer", "NetworkManager missing", true);
                return;
            }

            if (networkStarted && networkManager.IsServer)
            {
                AddNetworkEvent("StartServer", "Already started", false);
                return;
            }

            serverStartTime = Time.realtimeSinceStartup;
            EnsureSessionManager();

            if (!IsPortFree(cfg.port))
            {
                AddNetworkEvent("StartServer", "Port unavailable: " + cfg.port, true);
                return;
            }

            AddNetworkEvent("StartServer", "Starting server on port " + cfg.port, false);
            bool started = networkManager.StartServer();

            if (!started)
            {
                AddNetworkEvent("StartServer", "Start failed", true);
                networkManager.Shutdown();
                return;
            }

            networkStarted = true;
            SpawnSessionRpcHub();
            StartCoroutine(LogServerStats());
        }
        catch (Exception ex)
        {
            ReportBootstrapError("StartServer", "Start failed", ex);
        }
    }

    private IEnumerator LogServerStats()
    {
        while (networkManager != null && networkManager.IsServer)
        {
            yield return new WaitForSeconds(60f);
            int players = networkManager.ConnectedClientsIds.Count;
            float uptime = Time.realtimeSinceStartup - serverStartTime;
            Debug.Log($"[STATS] Players: {players}/{cfg.maxPlayers} | Uptime: {uptime:F0}s");
        }
    }

    private void EnsureSessionManager()
    {
        if (GameSessionManager.Instance != null)
        {
            Debug.Log("[NetworkBootstrap] GameSessionManager already active - skip");
            return;
        }

        GameObject go = new GameObject("GameSessionManager");
        go.AddComponent<GameSessionManager>();
        Debug.Log("[NetworkBootstrap] GameSessionManager created for session sync");

        Networking.StateSync.GlobalRegistryHub.EnsureInstance();
    }

    private void SpawnSessionRpcHub()
    {
        try
        {
            if (networkManager == null || !networkManager.IsServer)
            {
                Debug.LogWarning("[NetworkBootstrap] SpawnSessionRpcHub ignored because not server");
                return;
            }

            if (SessionRpcHub.Instance != null && SessionRpcHub.Instance.IsSpawned)
            {
                Debug.Log("[NetworkBootstrap] SessionRpcHub already active - skip spawn");
                return;
            }

            if (!EnsureSessionRpcHubPrefab())
            {
                Debug.LogWarning("[NetworkBootstrap] SessionRpcHub prefab missing");
                return;
            }

            if (sessionRpcHubPrefab == null)
            {
                Debug.LogWarning("[NetworkBootstrap] SessionRpcHub prefab missing");
                return;
            }

            NetworkObject hub = Instantiate(sessionRpcHubPrefab);
            hub.Spawn(true);

            Debug.Log("[NetworkBootstrap] SessionRpcHub spawned on server");
        }
        catch (Exception ex)
        {
            ReportBootstrapError("SessionRpcHub", "Spawn failed", ex);
        }
    }

    private bool EnsureSessionRpcHubPrefab()
    {
        if (sessionRpcHubPrefab != null)
        {
            return true;
        }

        if (networkManager == null)
        {
            AddNetworkEvent("SessionRpcHub", "NetworkManager missing", true);
            return false;
        }

        NetworkConfig config = networkManager.NetworkConfig;
        if (config == null)
        {
            AddNetworkEvent("SessionRpcHub", "NetworkConfig missing", true);
            return false;
        }

        NetworkPrefabs prefabs = config.Prefabs;
        if (prefabs == null)
        {
            AddNetworkEvent("SessionRpcHub", "NetworkPrefabs missing", true);
            return false;
        }

        List<NetworkPrefabsList> lists = prefabs.NetworkPrefabsLists;
        if (lists == null || lists.Count == 0)
        {
            AddNetworkEvent("SessionRpcHub", "NetworkPrefabs list missing", true);
            return false;
        }

        for (int i = 0; i < lists.Count; i++)
        {
            NetworkPrefabsList list = lists[i];
            if (list == null)
            {
                continue;
            }

            IReadOnlyList<NetworkPrefab> entries = list.PrefabList;
            if (entries == null)
            {
                continue;
            }

            for (int j = 0; j < entries.Count; j++)
            {
                NetworkPrefab entry = entries[j];
                if (entry == null)
                {
                    continue;
                }

                GameObject prefabObject = entry.Prefab;
                if (prefabObject == null && entry.OverridingTargetPrefab != null)
                {
                    prefabObject = entry.OverridingTargetPrefab;
                }

                if (prefabObject == null)
                {
                    continue;
                }

                SessionRpcHub hub = prefabObject.GetComponent<SessionRpcHub>();
                if (hub == null)
                {
                    continue;
                }

                NetworkObject netObject = prefabObject.GetComponent<NetworkObject>();
                if (netObject == null)
                {
                    AddNetworkEvent("SessionRpcHub", "SessionRpcHub prefab missing NetworkObject", true);
                    return false;
                }

                sessionRpcHubPrefab = netObject;
                AddNetworkEvent("SessionRpcHub", "Prefab resolved from NetworkPrefabs", false);
                return true;
            }
        }

        AddNetworkEvent("SessionRpcHub", "Prefab not found in NetworkPrefabs", true);
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
        catch (SocketException ex)
        {
            Debug.LogWarning("[NetworkBootstrap] Port check failed for " + port + ": " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[NetworkBootstrap] Port check error for " + port + ": " + ex.Message);
            return false;
        }
    }

    private void BeginBootstrapProgress(int totalSteps)
    {
        bootstrapStepCount = totalSteps;
        bootstrapStepIndex = 0;
        bootstrapCompleted = false;

        if (!ShouldShowBootstrapUi())
        {
            return;
        }

        EnsureProgressView();
        if (progressView != null)
        {
            bootstrapUiStartTime = Time.realtimeSinceStartup;
            if (bootstrapUiCoroutine != null)
            {
                StopCoroutine(bootstrapUiCoroutine);
                bootstrapUiCoroutine = null;
            }

            progressView.Initialize();
            progressView.ClearError();
            progressView.Show();
            progressView.SetProgress(0f, "Initialize", "Start");
            progressView.AddEntry("Initialize - Start", false);
        }
    }

    private void ReportBootstrapStep(string step, string subStep)
    {
        bootstrapStepIndex = bootstrapStepIndex + 1;

        float progressValue = 0f;
        if (bootstrapStepCount > 0)
        {
            progressValue = (float)bootstrapStepIndex / (float)bootstrapStepCount;
            progressValue = Mathf.Clamp01(progressValue) * 100f;
        }

        if (verboseBootstrapLogs)
        {
            Debug.Log("[NetworkBootstrap] " + step + " - " + subStep + " (" + progressValue.ToString("0") + "%)");
        }

        if (progressView != null && ShouldShowBootstrapUi())
        {
            progressView.SetProgress(progressValue, step, subStep);
            progressView.AddEntry(step + " - " + subStep, false);
        }
    }

    private void CompleteBootstrapProgress()
    {
        if (bootstrapCompleted)
        {
            return;
        }

        bootstrapCompleted = true;

        if (progressView != null && ShouldShowBootstrapUi() && minBootstrapUiSeconds > 0f)
        {
            float elapsed = Time.realtimeSinceStartup - bootstrapUiStartTime;
            float remaining = minBootstrapUiSeconds - elapsed;

            if (remaining > 0f)
            {
                if (bootstrapUiCoroutine != null)
                {
                    StopCoroutine(bootstrapUiCoroutine);
                }

                bootstrapUiCoroutine = StartCoroutine(FinalizeBootstrapAfterDelay(remaining));
                return;
            }
        }

        FinalizeBootstrap();
    }

    private IEnumerator FinalizeBootstrapAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);
        }

        FinalizeBootstrap();
    }

    private void FinalizeBootstrap()
    {
        if (progressView != null && ShouldShowBootstrapUi())
        {
            progressView.SetProgress(100f, "Initialize", "Done");
            progressView.AddEntry("Initialize - Done", false);
            progressView.Hide();
        }

        LoadMenuAfterBootstrap();
    }

    private void LoadMenuAfterBootstrap()
    {
        if (!loadMenuAfterBootstrap)
        {
            return;
        }

        if (!ShouldShowBootstrapUi())
        {
            return;
        }

        if (string.IsNullOrEmpty(menuSceneName))
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Equals(menuSceneName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ISceneServiceSync service = ResolveSceneService();
        if (service != null)
        {
            service.LoadSceneIfNeeded(menuSceneName);
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void ReportBootstrapError(string step, string detail)
    {
        ReportBootstrapError(step, detail, null);
    }

    private void ReportBootstrapError(string step, string detail, Exception ex)
    {
        string message = step + " - " + detail;

        Debug.LogError("[NetworkBootstrap] " + message);
        if (ex != null)
        {
            Debug.LogException(ex);
        }

        if (progressView != null && ShouldShowBootstrapUi())
        {
            progressView.AddEntry("ERROR " + message, true);
            progressView.SetError(message);
        }
    }

    private void AddNetworkEvent(string title, string detail, bool isError)
    {
        string message = title;
        if (!string.IsNullOrEmpty(detail))
        {
            message = title + " - " + detail;
        }

        if (isError)
        {
            Debug.LogError("[NetworkBootstrap] " + message);
        }
        else
        {
            Debug.Log("[NetworkBootstrap] " + message);
        }

        if (progressView != null && ShouldShowBootstrapUi())
        {
            progressView.AddEntry(message, isError);
            if (isError)
            {
                progressView.SetError(message);
            }
        }
    }

    private bool ShouldShowBootstrapUi()
    {
        if (!showProgressUiOnClient)
        {
            return false;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals(SceneNames.Server, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private void EnsureProgressView()
    {
        // 1) Si déjà set et dans la scène, ok
        if (progressView != null)
            return;

        // 2) Initialiser depuis le MonoBehaviour sérialisé
        if (progressViewMonoBehaviour != null)
        {
            progressView = progressViewMonoBehaviour as INetworkBootstrapProgressView;
            if (progressView != null)
                return;
        }

        // 3) Sinon cherche dans la scène un progress view configuré
        var found = FindProgressViewInScene();
        if (found != null)
        {
            progressView = found;
            return;
        }

        // 4) Sinon: erreur claire (au lieu de créer un truc inutilisable)
        Debug.LogError("[NetworkBootstrap] No INetworkBootstrapProgressView found in scene. " +
                       "Add NetworkBootstrapUI prefab to the scene and assign it.");
    }

    private INetworkBootstrapProgressView FindProgressViewInScene()
    {
        // Chercher tous les MonoBehaviour et vérifier s'ils implémentent l'interface
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (allMonoBehaviours == null || allMonoBehaviours.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < allMonoBehaviours.Length; i++)
        {
            if (allMonoBehaviours[i] is INetworkBootstrapProgressView view && 
                allMonoBehaviours[i].gameObject.scene.IsValid())
            {
                return view;
            }
        }

        return null;
    }

    private ISceneServiceSync ResolveSceneService()
    {
        if (sceneService != null)
        {
            return sceneService;
        }

        // Find any MonoBehaviour that implements ISceneServiceSync
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (allMonoBehaviours != null && allMonoBehaviours.Length > 0)
        {
            for (int i = 0; i < allMonoBehaviours.Length; i++)
            {
                if (allMonoBehaviours[i] is ISceneServiceSync service && 
                    allMonoBehaviours[i].gameObject.scene.IsValid())
                {
                    sceneService = service;
                    return sceneService;
                }
            }
        }

        return null;
    }


/// <summary>
/// Dedicated-server full authoritative mode:
/// ensure required NetworkPrefabs are registered (currently SessionRpcHub only).
/// Pawns are client-side views and MUST NOT be registered as network prefabs.
/// </summary>
    private void RegisterRequiredNetworkPrefabs()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
            return;

        if (sessionRpcHubPrefab == null)
            return;

        var config = nm.NetworkConfig;
        if (config == null || config.Prefabs == null)
            return;

        var prefabObject = sessionRpcHubPrefab.gameObject;
        if (prefabObject == null)
            return;

        if (config.Prefabs.Contains(prefabObject))
            return;

        nm.AddNetworkPrefab(prefabObject);
    }

    #region Logging

    public static void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string formatted = $"[{timestamp}] {message}";
        Debug.Log(formatted);
    }

    public static void LogWarning(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string formatted = $"[{timestamp}] [WARN] {message}";
        Debug.LogWarning(formatted);
    }

    public static void LogError(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string formatted = $"[{timestamp}] [ERROR] {message}";
        Debug.LogError(formatted);
    }

    public static void LogSession(string action, string sessionName, ulong clientId = 0)
    {
        if (clientId > 0)
            Log($"[SESSION] {action}: {sessionName} (Client {clientId})");
        else
            Log($"[SESSION] {action}: {sessionName}");
    }

    public static void LogGame(string action, string sessionName, int playerCount = 0)
    {
        if (playerCount > 0)
            Log($"[GAME] {action}: {sessionName} ({playerCount} players)");
        else
            Log($"[GAME] {action}: {sessionName}");
    }

    #endregion
}
