using System;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Binds the ConnectionUI.uxml fields to connection actions.
/// Enables editing IP/port and wires buttons to NetworkBootstrap.
/// </summary>
public class ConnectionUIController : MonoBehaviour
{
    private static bool sceneEventsSubscribed;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private bool autoFocusIp = true;

    private TextField ipField;
    private TextField portField;
    private Button connectButton;
    private Button testButton;
    private Label statusLabel;

    private NetworkBootstrap bootstrap;
    private bool subscribed;
    private bool buttonsRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAttach()
    {
        if (sceneEventsSubscribed)
            return;

        sceneEventsSubscribed = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Equals(SceneNames.Menu, StringComparison.OrdinalIgnoreCase))
            return;

        var docs = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (var doc in docs)
        {
            var assetName = doc.visualTreeAsset != null ? doc.visualTreeAsset.name : string.Empty;
            if (assetName == "ConnectionUI")
            {
                if (doc.GetComponent<ConnectionUIController>() == null)
                    doc.gameObject.AddComponent<ConnectionUIController>();

                // Ensure the menu tag is present for overlay hiding
                if (!doc.CompareTag("Menu"))
                {
                    try { doc.tag = "Menu"; }
                    catch { /* tag might not exist in project; ignore */ }
                }
            }
            else if (doc.gameObject.name == "Menu")
            {
                // Disable legacy/unused UIDocuments that can block clicks
                doc.enabled = false;
            }
        }
    }

    private void Awake()
    {
        bootstrap = NetworkBootstrap.Instance;
    }

    private void OnEnable()
    {
        Initialize();
        SubscribeBootstrapEvents();
    }

    private void OnDisable()
    {
        UnsubscribeBootstrapEvents();
        UnregisterButtons();
    }

    private void OnDestroy()
    {
        UnsubscribeBootstrapEvents();
    }

    private void Initialize()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogError("[ConnectionUI] UIDocument missing on Connection UI object");
            enabled = false;
            return;
        }

        var root = uiDocument.rootVisualElement;
        root.pickingMode = PickingMode.Position;

        ipField = root.Q<TextField>("IPField");
        portField = root.Q<TextField>("PortField");
        connectButton = root.Q<Button>("ConnectButton");
        testButton = root.Q<Button>("TestButton");
        statusLabel = root.Q<Label>("StatusLabel");

        MakeEditable(ipField);
        MakeEditable(portField);

        ApplyDefaults();
        RegisterButtons();

        if (autoFocusIp && ipField != null)
            ipField.Focus();
    }

    private void ApplyDefaults()
    {
        var cfg = NetworkConfigProvider.Config;
        if (cfg != null)
        {
            if (ipField != null) ipField.value = cfg.ipAddress;
            if (portField != null) portField.value = cfg.port.ToString();
        }
        else
        {
            if (ipField != null && string.IsNullOrWhiteSpace(ipField.value))
                ipField.value = "127.0.0.1";
            if (portField != null && string.IsNullOrWhiteSpace(portField.value))
                portField.value = "7777";
        }
    }

    private void RegisterButtons()
    {
        if (buttonsRegistered)
            return;

        if (connectButton != null)
            connectButton.clicked += OnConnectClicked;
        if (testButton != null)
            testButton.clicked += OnTestClicked;
        buttonsRegistered = true;
    }

    private void UnregisterButtons()
    {
        if (connectButton != null)
            connectButton.clicked -= OnConnectClicked;
        if (testButton != null)
            testButton.clicked -= OnTestClicked;
        buttonsRegistered = false;
    }

    private void SubscribeBootstrapEvents()
    {
        if (subscribed)
            return;

        EnsureBootstrap();
        
        if (bootstrap == null)
            return;

        bootstrap.OnConnected += HandleConnected;
        bootstrap.OnDisconnected += HandleDisconnected;
        bootstrap.OnConnectionFailed += HandleConnectionFailed;
        subscribed = true;
    }

    private void UnsubscribeBootstrapEvents()
    {
        if (!subscribed || bootstrap == null)
            return;

        bootstrap.OnConnected -= HandleConnected;
        bootstrap.OnDisconnected -= HandleDisconnected;
        bootstrap.OnConnectionFailed -= HandleConnectionFailed;
        subscribed = false;
    }

    private void OnConnectClicked()
    {
        if (!TryReadInputs(out var ip, out var port))
            return;

        EnsureBootstrap();
        
        if (bootstrap == null)
        {
            SetStatus("NetworkBootstrap introuvable", true);
            return;
        }

        bootstrap.StartClient(ip, port);
        SetStatus($"Connexion à {ip}:{port}...", false);
    }

    private void OnTestClicked()
    {
        if (!TryReadInputs(out var ip, out var port))
            return;

        EnsureBootstrap();
        
        if (bootstrap == null)
        {
            SetStatus("NetworkBootstrap introuvable", true);
            return;
        }

        SetStatus("Test de connexion...", false);
        bootstrap.TestConnection(ip, port, (ok, message) =>
        {
            SetStatus(ok ? "Serveur joignable" : $"Échec: {message}", !ok);
        });
    }

    private void HandleConnected()
    {
        SetStatus("Connecté au serveur", false);

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name.Equals(SceneNames.Menu, StringComparison.OrdinalIgnoreCase))
        {
            SceneManager.LoadScene(SceneNames.Client);
        }
    }

    private void HandleDisconnected()
    {
        SetStatus("Déconnecté", true);
    }

    private void HandleConnectionFailed(string message)
    {
        SetStatus($"Connexion échouée: {message}", true);
    }

    private void EnsureBootstrap()
    {
        if (bootstrap != null)
            return;

        bootstrap = NetworkBootstrap.Instance;
        
        if (bootstrap == null)
        {
            // Try to find it even if inactive
            bootstrap = FindFirstObjectByType<NetworkBootstrap>(FindObjectsInactive.Include);
            if (bootstrap != null)
            {
                Debug.Log($"[ConnectionUI] Found inactive NetworkBootstrap on '{bootstrap.gameObject.name}', activating it.");
                bootstrap.gameObject.SetActive(true);
            }
        }

        if (bootstrap == null)
        {
            Debug.LogError("[ConnectionUI] NetworkBootstrap missing from scene! Ensure NetworkManagerRoot is present.");
        }
    }

    private bool TryReadInputs(out string ip, out ushort port)
    {
        ip = (ipField != null ? ipField.text : string.Empty)?.Trim();
        var portText = (portField != null ? portField.text : string.Empty)?.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            SetStatus("IP obligatoire", true);
            port = 0;
            return false;
        }

        if (!ushort.TryParse(portText, out port))
        {
            SetStatus("Port invalide", true);
            return false;
        }

        // Allow hostnames but flag obviously invalid IPs
        if (!IPAddress.TryParse(ip, out _) && ip.Contains(" "))
        {
            SetStatus("IP/host invalide", true);
            return false;
        }

        return true;
    }

    private void SetStatus(string message, bool isError)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
            statusLabel.style.color = isError ? new Color(1f, 0.6f, 0.6f) : new Color(0.8f, 0.9f, 1f);
        }
    }

    private static void MakeEditable(TextField field)
    {
        if (field == null)
            return;

        field.isReadOnly = false;
        field.focusable = true;
        field.pickingMode = PickingMode.Position;
        field.SetEnabled(true);
    }
}
